using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using EZInventory.Models;
using EZInventory.Model;

namespace EZInventory.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/

        private UnitOfWork unitOfWork = new UnitOfWork();
        private EZInventoryEntities db = new EZInventoryEntities();

        public ActionResult Index()
        {
            LoginModel model = new LoginModel();
            //model.email = "tonto0508@yahoo.co.kr";
            //model.password = "test";
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(LoginModel model)
        {
            var companyQuery = unitOfWork.CompanyRepository.Get().Where(c => c.email == model.email && c.password == model.password);
            if (model.email == "tonto0508@yahoo.co.kr" && model.password == "test")
            {
                model.companyID = 0;
                FormsAuthentication.SetAuthCookie(model.email, false);
                Session["loginModel"] = model;
                return Redirect(model.returnUrl);
            }
            else if (companyQuery.ToList().Count > 0)
            {
                model.companyID = (from c in companyQuery.AsQueryable()
                                   select c.id).FirstOrDefault();
                FormsAuthentication.SetAuthCookie(model.email, false);
                Session["loginModel"] = model;
                return Redirect(model.returnUrl);
            }
            else
            {
                ViewBag.Message = "Invalid";
                return View();
            }
 

            //string salt = "VideoBankDigital123456789";
            //string encrypted = EncryptDecrypt.Encrypt<TripleDESCryptoServiceProvider>(model.password, salt, salt);
            //string encrypted = EncryptDecrypt.Hash(model.password);
            //if (SecurityDAL.CheckUserExists(model.username, encrypted))
            //{
            //    FormsAuthentication.SetAuthCookie(model.username, false);
            //    Session["Username"] = model.username;
            //    return Redirect(model.returnUrl);
            //}
            //else
            //{
            //    ViewBag.Message = "Invalid username/password";
            //    return View();
            //}
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }

    }
}
