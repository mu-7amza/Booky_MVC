namespace BookyWeb.Areas.Admin.Controllers
{
    using Booky.DataAcess.Repositery.IRepositery;
    using Booky.Models;
    using Booky.Utility;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    namespace BookyWeb.Areas.Admin.Controllers
    {
        [Area("Admin")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        public class CompanyController : Controller
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IWebHostEnvironment _webHost;
            public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHost)
            {
                _unitOfWork = unitOfWork;
                _webHost = webHost;
            }
            public IActionResult Index()
            {
                List<Company> companyList  = _unitOfWork.Company.GetAll().ToList();
                return View(companyList);
            }

            public IActionResult Upsert(int? id)
            {
                if (id == null || id == 0)
                {
                    return View(new Company());
                }
               
                else
                {
                    Company company = _unitOfWork.Company.Get(p => p.Id == id);
                    return View(company);
                }

            }

            [HttpPost]
            public IActionResult Upsert(Company company)
            {

                if (ModelState.IsValid)
                {
                   
                    if (company.Id == 0)
                    {
                        _unitOfWork.Company.Add(company);
                    }
                    else
                    {
                        _unitOfWork.Company.Update(company);
                    }

                    _unitOfWork.Save();
                    TempData["success"] = "Saved Successfully";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return View(company);
                }

            }

            #region Api Calls
            [HttpGet]
            public IActionResult GetAll()
            {
                var companyList = _unitOfWork.Company.GetAll().ToList();

                return Json(new { data = companyList });
            }

            [HttpDelete]
            public IActionResult Delete(int? id)
            {
                var companyToBeDeleted = _unitOfWork.Company.Get(c => c.Id == id);

                if (companyToBeDeleted == null)
                {
                    return Json(new { sucess = false, message = "Error while deleting" });
                }

                

                _unitOfWork.Company.Remove(companyToBeDeleted);
                _unitOfWork.Save();

                return Json(new { sucess = true, message = "Deleted Successfuly" });
            }
            #endregion


        }
    }

}
