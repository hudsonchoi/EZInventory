using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EZInventory.Models;

namespace EZInventory.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            //////LoginModel model = new LoginModel();
            //////model.companyID = 0;
            //////Session["loginModel"] = model;
            return View();
        }

    }
}
