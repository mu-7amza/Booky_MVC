using Booky.DataAcess.Repositery.IRepositery;
using Booky.Models;
using Booky.Models.ViewModels;
using Booky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BookyWeb.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVm OrderVm { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        //[Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVm = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == orderId, includeProperities: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetails.GetAll(o => o.OrderHeaderId == orderId, includeProperities: "Product")
            };
            return View(OrderVm);
        }

        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
        public IActionResult UpdateOrderDetails()
        {
            var orderHeaderFormDb = _unitOfWork.OrderHeader.Get(o => o.Id == OrderVm.OrderHeader.Id);

            orderHeaderFormDb.FullName = OrderVm.OrderHeader.FullName;
            orderHeaderFormDb.PhoneNumber = OrderVm.OrderHeader.PhoneNumber;
            orderHeaderFormDb.City = OrderVm.OrderHeader.City;
            orderHeaderFormDb.State = OrderVm.OrderHeader.State;
            orderHeaderFormDb.StreatAddress = OrderVm.OrderHeader.StreatAddress;
            orderHeaderFormDb.PostalCode = OrderVm.OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(OrderVm.OrderHeader.Carrier))
            {
                orderHeaderFormDb.Carrier = OrderVm.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVm.OrderHeader.TrackingNumber))
            {
                orderHeaderFormDb.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.Update(orderHeaderFormDb);
            _unitOfWork.Save();

            TempData["success"] = "Order details is updated successfuly";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFormDb.Id });
        }
        
        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVm.OrderHeader.Id, SD.StatusInProcessing);
            _unitOfWork.Save();

            TempData["success"] = "Order Proccessed Successfuly";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });

        }

        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == OrderVm.OrderHeader.Id);
            orderHeader.Carrier = OrderVm.OrderHeader.Carrier;
            orderHeader.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            if(orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

            TempData["success"] = "Order Shipped Successfuly";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });

        }

        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
        public IActionResult CancelOrder(int? orderId)
        {
            if(orderId != null)
            {
				var orderBefore = _unitOfWork.OrderHeader.Get(u => u.Id == orderId);
				_unitOfWork.OrderHeader.UpdateStatus(orderBefore.Id, SD.StatusCancelled, SD.StatusCancelled);
				_unitOfWork.Save();
				TempData["success"] = "Order Canceled Successfuly";
				return RedirectToAction(nameof(Details), new { orderId = orderBefore.Id });
			}
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVm.OrderHeader.Id);

            if(orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason =  RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled,SD.StatusCancelled);
            }

            _unitOfWork.Save();
            TempData["success"] = "Order Canceled Successfuly";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
        }

        [HttpPost]
        [ActionName(nameof(Details))]
        public IActionResult Details_Pay_Now()
        {
            OrderVm.OrderHeader = _unitOfWork.OrderHeader
                .Get( u => u.Id == OrderVm.OrderHeader.Id , includeProperities: "ApplicationUser" );
            OrderVm.OrderDetails = _unitOfWork.OrderDetails
                .GetAll(u => u.OrderHeaderId == OrderVm.OrderHeader.Id, includeProperities: "Product");

            string domain = "https://localhost:7147/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVm.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?={OrderVm.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVm.OrderDetails)
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
            _unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVm.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);

                // it is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);

                if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
                {
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                }
                else
                {
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, SD.StatusApproved, SD.PaymentStatusApproved);
                }
                _unitOfWork.Save();
                }

          
            

            return View(orderHeaderId);
        }


        #region Api Calls
        [HttpGet]
        public IActionResult GetAll(string? status)
        {
            IEnumerable<OrderHeader> orderHeaderList;

            if (User.IsInRole(SD.ROLE_ADMIN) || User.IsInRole(SD.ROLE_EMPLOYEE))
            {
                orderHeaderList = _unitOfWork.OrderHeader
                    .GetAll(includeProperities: "ApplicationUser");
            }
            else
            {
                ClaimsIdentity claimsIdentity = (ClaimsIdentity) User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                orderHeaderList = _unitOfWork.OrderHeader
                    .GetAll(u => u.ApplicationUser.Id == userId,includeProperities: "ApplicationUser");

            }
            if (!string.IsNullOrEmpty(status))
            {
                switch (status)
                {
                    case "pending":
                        orderHeaderList = orderHeaderList.Where(o => o.PaymentStatus == SD.PaymentStatusDelayedPayment);
                        break;
                    case "inprocess":
                        orderHeaderList = orderHeaderList.Where(o => o.PaymentStatus == SD.StatusInProcessing);
                        break;
                    case "completed":
                        orderHeaderList = orderHeaderList.Where(o => o.PaymentStatus == SD.StatusShipped);
                        break;
                    case "approved":
                        orderHeaderList = orderHeaderList.Where(o => o.PaymentStatus == SD.StatusApproved);
                        break;
                    default:
                        break;
                }
            }
            return Json(new { data = orderHeaderList });
        }

        
        #endregion
    }
}
