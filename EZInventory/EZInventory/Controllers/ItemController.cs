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
using PagedList;
using EZInventory.Models;

namespace EZInventory.Controllers
{
    public class ItemController : Controller
    {

        private UnitOfWork unitOfWork = new UnitOfWork();
        private EZInventoryEntities db = new EZInventoryEntities();
        //
        // GET: /Product/

        public ActionResult Index(int? page)
        {
            return GetResult("","",page);
        }

        [HttpPost]
        public ActionResult Index(string Keyword, string barcode)
        {
            return GetResult(Keyword, barcode);
        }

        private ActionResult GetResult(string Keyword, string barcode, int? page = null)
        {
            ViewBag.barcodeOnly = barcode;
            var products = unitOfWork.ItemRepository.Get();
            if (barcode == "true")
            {
                var pQueried = from p in products.AsQueryable()
                               where p.barcode.ToUpper().IndexOf(Keyword.ToUpper()) >= 0
                               && p.deleted != true && p.Company.deleted != true
                               select p;
                if (pQueried.ToList().Count == 0)
                {
                    return Redirect("/product/create/" + Keyword);
                }
                else//This product exists let's check inventory
                {
                    var invs = from inv in unitOfWork.InventoryRepository.Get().AsQueryable()
                               join p in products.AsQueryable() on inv.sku equals p.sku
                               where p.barcode.ToUpper().IndexOf(Keyword.ToUpper()) >= 0 && p.deleted != true && p.Company.deleted != true && inv.deleted != true
                               select inv;
                    if (invs.ToList().Count > 0)
                    {
                        var inv = invs.FirstOrDefault();
                        return Redirect("/Inventory/Edit/" + inv.id);
                    }
                    else
                    {
                        return Redirect("/Inventory/create/" + Keyword);
                    }       
                }
            }
            else
            {
                var pQueried = from p in products.AsQueryable()
                               where (p.barcode.ToUpper().IndexOf(Keyword.ToUpper()) >= 0 ||
                               p.sku.ToUpper().IndexOf(Keyword.ToUpper()) >= 0 ||
                               p.name.ToUpper().IndexOf(Keyword.ToUpper()) >= 0) && p.deleted != true && p.Company.deleted != true
                               select p;
                var pageNumber = page ?? 1;
                var onePageOfResults = pQueried.ToList().ToPagedList(pageNumber, 10);
                ViewBag.onePageOfResults = onePageOfResults;
                ViewBag.Message = Keyword;
                ViewBag.q = Keyword;
                return View(onePageOfResults);
            }

            
        }

        public ViewResult Details(string sku)
        {
            var item = unitOfWork.ItemRepository.GetByID(sku);
            return View(item);
        }

        public ViewResult Edit(string sku)
        {
            var item = unitOfWork.ItemRepository.GetByID(sku);
            ViewData["companyid"] = EZDropDown.Companies(unitOfWork, item.companyid);
            return View(item);
        }

        public ActionResult Create(string barcode)
        {
            ViewBag.CompanyIDs = unitOfWork.CompanyRepository.Get().Where(c => c.billto == true && c.deleted != true).OrderBy(c => c.name);
            Item item = new Item();
            item.barcode = barcode;
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "sku,barcode,name,description,companyid")]Item item)
        {
            try
            {
                ViewBag.CompanyIDs = unitOfWork.CompanyRepository.Get().Where(c => c.billto == true && c.deleted != true).OrderBy(c => c.name);
                if (ModelState.IsValid)
                {
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "sku,name,description,barcode,companyid,deleted")]Item item)
        {
            try
            {
                ViewBag.CompanyIDs = unitOfWork.CompanyRepository.Get().Where(c => c.billto == true && c.deleted != true).OrderBy(c => c.name);
                if (ModelState.IsValid)
                {
                    db.Items.Add(item);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return View(item);
        }


        // GET: /Course/Delete/5
        public ActionResult Delete(string sku)
        {
            Item item = db.Items.Find(sku);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: /Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string sku)
        {
            Item item = db.Items.Find(sku);
            item.deleted = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public string GetBarcode(string sku)
        {
            var item = unitOfWork.ItemRepository.GetByID(sku);
            return item.barcode;
        }


    }
}
