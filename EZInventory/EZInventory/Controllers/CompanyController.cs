using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Infrastructure;
using EZInventory.Model;
using EZInventory.Models;


namespace EZInventory.Controllers
{
    public class CompanyController : Controller
    {
        //
        // GET: /Company/
        private UnitOfWork unitOfWork = new UnitOfWork();
        private EZInventoryEntities db = new EZInventoryEntities();

        public ActionResult Index()
        {
            var companies = unitOfWork.CompanyRepository.Get().Where(c=>c.deleted != true);
            var cvms = new List<CompanyViewModel>();
            foreach (var c in companies)
            {
                CompanyViewModel cvm = new CompanyViewModel();
                cvm.ID = c.id;
                cvm.CompanyName = c.name;
                cvm.Billing = (bool)c.billto;
                cvm.Shipping = (bool)c.shipto;
                try
                {
                    cvm.BillToCompany = unitOfWork.CompanyRepository.Get().Where(_c=>_c.deleted != true && _c.id == c.billtocid).FirstOrDefault().name;
                }
                catch (Exception e) { }
                cvm.City = c.city;
                cvm.State = c.state;
                cvms.Add(cvm);
            }
            return View(cvms);
        }

        public ActionResult Create()
        {
            //ViewData["billtocid"] = EZDropDown.Companies(unitOfWork, null);
            ViewBag.CompanyIDs = unitOfWork.CompanyRepository.Get().Where(c => c.billto == true && c.deleted != true).OrderBy(c => c.name);

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "name, address, city, state, zip, billto, shipto, billtocid, email, password, deleted")]Company c, string selfBilling)
        {
            try
            {
                ViewBag.CompanyIDs = unitOfWork.CompanyRepository.Get().Where(cc => cc.billto == true && cc.deleted != true).OrderBy(cc => cc.name);

                    if (ModelState.IsValid)
                    {
                        if (selfBilling == "false" && c.billtocid == null)
                        {
                            ModelState.AddModelError("Required", "Required");
                        }
                        else
                        {
                            db.Companies.Add(c);
                            db.SaveChanges();
                            if (selfBilling == "true")
                            {
                                c.billtocid = c.id;
                                db.SaveChanges();
                            }
                            return RedirectToAction("Index");
                        }
                    }

            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return View(c);
        }


        public ViewResult Edit(int id)
        {
            var c = unitOfWork.CompanyRepository.GetByID(id);
            ViewData["billtocid"] = EZDropDown.Companies(unitOfWork, c.billtocid);
            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id, name, address, city, state, zip, billto, shipto, billtocid, email, password")]Company c)
        {
            try
            {
                ViewData["billtocid"] = EZDropDown.Companies(unitOfWork, c.billtocid);
                if (ModelState.IsValid)
                {
                    if (c.billtocid == null)
                    {
                        ModelState.AddModelError("Required", "Required");
                    }
                    else
                    {
                        db.Entry(c).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return View(c);
        }

        public ViewResult Details(int id)
        {
            var c = unitOfWork.CompanyRepository.GetByID(id);
            var c2 = unitOfWork.CompanyRepository.GetByID(c.billtocid);
            ViewBag.billToCompany = c2.name;
            return View(c);
        }

        // GET: /Course/Delete/5
        public ActionResult Delete(int id)
        {
            Company c = db.Companies.Find(id);
            if (c == null)
            {
                return HttpNotFound();
            }
            else
            {
                var c2 = unitOfWork.CompanyRepository.Get().Where(cc => cc.billtocid == id && cc.deleted != true).FirstOrDefault();
                try
                {
                    ViewBag.billToCompany = c2.name;
                }
                catch (Exception e) { }
            }
            return View(c);
        }

        // POST: /Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Company c = db.Companies.Find(id);
            c.deleted = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public JsonResult GetShipTosByCompany(int id)
        {
            List<SelectListItem> shiptos = new List<SelectListItem>();
            var shiptoQuery = unitOfWork.CompanyRepository.Get().Where(c => c.billtocid == id);
            foreach (Company c in shiptoQuery.ToList())
            {
                shiptos.Add(new SelectListItem { Text = c.name + " - " + c.city, Value = c.id.ToString() });
            }
            return Json(new SelectList(shiptos, "Value", "Text"));
        }

    }
}
