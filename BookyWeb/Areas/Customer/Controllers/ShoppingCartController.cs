using Booky.DataAcess.Repositery.IRepositery;
using Booky.Models;
using Booky.Models.ViewModels;
using Booky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookyWeb.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        [BindProperty]
        public ShoppingCartVm ShoppingCartVm { get; set; }
        private readonly IConfiguration _configuration;
        public ShoppingCartController(IUnitOfWork unitOfWork, IConfiguration configuration, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVm = new()
            {
                Items = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperities: "Product"),
                OrderHeader = new()

            };

            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();

            foreach(var cart in ShoppingCartVm.Items) 
            {
                cart.Product.ProductImages = productImages.Where(p => p.ProductId == cart.Product.Id).ToList();
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVm.OrderHeader.TotalOrder += cart.Price * cart.Count;
            }
            return View(ShoppingCartVm);
        }

        public IActionResult Summary()
        {

            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVm = new()
            {
                Items = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperities: "Product"),
                OrderHeader = new()

            };

            ShoppingCartVm.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(a => a.Id == userId);

            ShoppingCartVm.OrderHeader.FullName = ShoppingCartVm.OrderHeader.ApplicationUser.FullName;
            ShoppingCartVm.OrderHeader.PhoneNumber = ShoppingCartVm.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVm.OrderHeader.StreatAddress = ShoppingCartVm.OrderHeader.ApplicationUser.StreatAddress;
            ShoppingCartVm.OrderHeader.City = ShoppingCartVm.OrderHeader.ApplicationUser.City;
            ShoppingCartVm.OrderHeader.State = ShoppingCartVm.OrderHeader.ApplicationUser.State;
            ShoppingCartVm.OrderHeader.PostalCode = ShoppingCartVm.OrderHeader.ApplicationUser.PostalCode;


            foreach (var cart in ShoppingCartVm.Items)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVm.OrderHeader.TotalOrder += cart.Price * cart.Count;
            }
            return View(ShoppingCartVm);
        }

        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                ShoppingCartVm.Items = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                    includeProperities: "Product");

                ShoppingCartVm.OrderHeader.OrderDate = DateTime.Now;
                ShoppingCartVm.OrderHeader.ApplicationUserId = userId;

                ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(a => a.Id == userId);

                foreach (var cart in ShoppingCartVm.Items)
                {
                    cart.Price = GetPriceBasedOnQuantity(cart);
                    ShoppingCartVm.OrderHeader.TotalOrder += cart.Price * cart.Count;
                }

                if (applicationUser.CompanyId.GetValueOrDefault() == 0)
                {
                    // it's reguler user
                    ShoppingCartVm.OrderHeader.OrderStatus = SD.StatusPending;
                    ShoppingCartVm.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                }
                else
                {
                    ShoppingCartVm.OrderHeader.OrderStatus = SD.StatusApproved;
                    ShoppingCartVm.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                }

                _unitOfWork.OrderHeader.Add(ShoppingCartVm.OrderHeader);
                _unitOfWork.Save();

                foreach (var cart in ShoppingCartVm.Items)
                {
                    OrderDetail orderDetail = new()
                    {
                        ProductId = cart.ProductId,
                        OrderHeaderId = ShoppingCartVm.OrderHeader.Id,
                        Count = cart.Count,
                        Price = cart.Price,
                    };
                    _unitOfWork.OrderDetails.Add(orderDetail);
                    _unitOfWork.Save();

                }
                if (applicationUser.CompanyId.GetValueOrDefault() == 0)
                {
                    // it's reguler user , need payment
                    // stripe logic
                    string domain = "https://localhost:7147/";
                    var options = new SessionCreateOptions
                    {
                        SuccessUrl = domain + $"Customer/ShoppingCart/OrderConfirmation?id={ShoppingCartVm.OrderHeader.Id}",
                        CancelUrl = domain + "Customer/ShoppingCart/Index",
                        LineItems = new List<SessionLineItemOptions>(),
                        Mode = "payment",
                    };


                    foreach (var item in ShoppingCartVm.Items)
                    {
                        var sessionLineItem = new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(item.Price * 100),
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.Product.Title
                                }
                            },
                            Quantity = item.Count
                        };

                        options.LineItems.Add(sessionLineItem);
                    }

                    var service = new SessionService();
                    Session session = service.Create(options);
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVm.OrderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                    Response.Headers.Add("Location", session.Url);
                    return new StatusCodeResult(303);

                }

                return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVm.OrderHeader.Id });
            
		}

		public IActionResult OrderConfirmation(int id)
		{
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id , includeProperities:"ApplicationUser");

            if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                // it is a customer

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if(session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id,session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
                HttpContext.Session.Clear();
            }

            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "Booky - New Book",
                $"<h3>New Order Created : {orderHeader.Id}</h3>");

            List<ShoppingCart> Carts = _unitOfWork.ShoppingCart.
                GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(Carts);
            _unitOfWork.Save();

			return View(id);
		}


		public IActionResult Plus(int cartId)
        {
            ShoppingCart cart = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);
            cart.Count += 1;
            _unitOfWork.ShoppingCart.Update(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            ShoppingCart cart = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);
            if (cart.Count <= 1)
            {
                // remove cart 
                _unitOfWork.ShoppingCart.Remove(cart);
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == cart.ApplicationUserId).Count() - 1);
            }
            else
            {
                // update count
                cart.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cart);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
                ShoppingCart cart = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);
                _unitOfWork.ShoppingCart.Remove(cart);
            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == cart.ApplicationUserId).Count() - 1);
            _unitOfWork.Save();

                return RedirectToAction(nameof(Index));
        }


        private double GetPriceBasedOnQuantity(ShoppingCart cart)
        {
            if(cart.Count <= 50)
            {
                return cart.Product.Price;
            }
            else
            {
                if(cart.Count <= 100)
                {
                    return cart.Product.Price50;
                }
                else
                {
                    return cart.Product.Price100;
                }
            }
        }
    }
}
