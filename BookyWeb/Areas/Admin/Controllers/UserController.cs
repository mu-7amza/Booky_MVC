namespace BookyWeb.Areas.Admin.Controllers
{
    using Booky.DataAccess.Data;
    using Booky.DataAcess.Repositery.IRepositery;
    using Booky.Models;
    using Booky.Models.ViewModels;
    using Booky.Utility;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;

    namespace BookyWeb.Areas.Admin.Controllers
    {
        [Area("Admin")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        public class UserController : Controller
        {
            private readonly UserManager<IdentityUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly IUnitOfWork _unitOfWork;
            public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
            {
                _userManager = userManager;
                _roleManager = roleManager;
                _unitOfWork = unitOfWork;
            }
            public IActionResult Index()
            {
              
                return View();
            }

            public IActionResult RoleManagment(string userId)
            {

                RoleManagerVm RoleVm = new RoleManagerVm()
                {
                    ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId,includeProperities:"Company"),
                    RoleList = _roleManager.Roles.Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Name
                    }),
                    CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    })
                };
                
                RoleVm.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u=>u.Id==userId))
                    .GetAwaiter().GetResult().FirstOrDefault();
                return View(RoleVm);
            }

            [HttpPost]
            public IActionResult RoleManagment(RoleManagerVm RoleVm)
            {
                var oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == RoleVm.ApplicationUser.Id))
                    .GetAwaiter().GetResult().FirstOrDefault();

                ApplicationUser user = _unitOfWork.ApplicationUser.Get(u => u.Id == RoleVm.ApplicationUser.Id);

                if (!(RoleVm.ApplicationUser.Role == oldRole ))
                {
                    if(RoleVm.ApplicationUser.Role == SD.ROLE_COMPANY)
                    {
                        user.CompanyId = RoleVm.ApplicationUser.CompanyId;
                    }
                    if(oldRole == SD.ROLE_COMPANY)
                    {
                        user.CompanyId = null;
                    }
                    _unitOfWork.ApplicationUser.Update(user);
                    _unitOfWork.Save();

                    _userManager.RemoveFromRoleAsync(user, oldRole).GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(user, RoleVm.ApplicationUser.Role).GetAwaiter().GetResult();
                }
                else
                {
                    if(oldRole == SD.ROLE_COMPANY && user.CompanyId != RoleVm.ApplicationUser.CompanyId)
                    {
                        user.CompanyId= RoleVm.ApplicationUser.CompanyId;
                        _unitOfWork.ApplicationUser.Update(user);
                        _unitOfWork.Save();
                    }
                }

                return RedirectToAction(nameof(Index));
            }



            #region Api Calls
            [HttpGet]
            public IActionResult GetAll()
            {
                List<ApplicationUser> usersList = _unitOfWork.ApplicationUser.GetAll(includeProperities: "Company").ToList();

                foreach(var user in usersList) {

                    user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

                    if (user.Company == null)
                    {
                        user.Company = new() { Name = "" };
                    }
                }

                return Json(new { data = usersList });
            }

            [HttpPost]
            public IActionResult LockUnlock([FromBody] string id)
            {
                var appUser = _unitOfWork.ApplicationUser.Get(u=> u.Id == id);

                if(appUser == null)
                {
                    return Json(new { success = false , message = "Error while lock/unlock"});
                }

                if(appUser.LockoutEnd != null && appUser.LockoutEnd > DateTime.Now)
                {
                    appUser.LockoutEnd = DateTime.Now;
                }
                else
                {
                    appUser.LockoutEnd = DateTime.Now.AddYears(100);
                }
                _unitOfWork.ApplicationUser.Update(appUser);
                _unitOfWork.Save();

                return Json(new { success = true, message = "Operation Succesful" });


            }

            #endregion


        }
    }

}
