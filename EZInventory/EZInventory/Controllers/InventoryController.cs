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
using PagedList;


namespace EZInventory.Controllers
{
    public class InventoryController : Controller
    {
        private UnitOfWork unitOfWork = new UnitOfWork();
        private EZInventoryEntities db = new EZInventoryEntities();
        //
        // GET: /Inventory/
        public ActionResult Index(int? page)
        {
            return GetResult("", "", page);
        }

        public ActionResult ExportToExcel(int? page)
        {
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=Inventories.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            return GetResult("", "", page);
        }

        [HttpPost]
        public ActionResult Index(string Keyword, string barcode)
        {
            return GetResult(Keyword, barcode);
        }

        public ActionResult GetResult(string Keyword, string barcode, int? page = null)
        {
            //var inventories = unitOfWork.InventoryRepository.Get(orderBy:i=>i.OrderByDescending(d=>d.inputdate));
            var inventories = unitOfWork.InventoryRepository.Get();
            var inventoryDetails = unitOfWork.InventoryDetailRepository.Get();
            var locations = unitOfWork.LocationRepository.Get();
            var companies = unitOfWork.CompanyRepository.Get();
            var products = unitOfWork.ItemRepository.Get();
            LoginModel lm = (LoginModel)Session["loginModel"];
            if (lm.companyID > 0)
                companies = companies.Where(c => c.id == lm.companyID || c.billtocid == lm.companyID);

            ViewBag.barcodeOnly = barcode;
            IQueryable<InventoryListModel> inventoryItems;
            if (barcode == "true")
            {
                inventoryItems = from t in
                                         (
                                            from inv in inventories.AsQueryable()
                                            join p in products.AsQueryable() on inv.sku equals p.sku
                                            join com in companies.AsQueryable() on inv.companyid equals com.id
                                            join invd in inventoryDetails.AsQueryable() on inv.id equals invd.inventoryid
                                            where p.barcode.ToUpper().IndexOf(Keyword.ToUpper()) >= 0 &&
                                            inv.deleted != true && p.deleted != true && com.deleted != true
                                            select new { inv, com, invd, p }
                                         )
                                     //select t;
                                     group t by new { t.inv.id, t.com.name, t.inv.sku, t.p.barcode, t.inv.inputdate } into grp
                                     select new InventoryListModel
                                     {
                                         ID = grp.Key.id,
                                         company = grp.Key.name,
                                         sku = grp.Key.sku,
                                         barcode = grp.Key.barcode,
                                         stockingDate = Convert.ToDateTime(grp.Key.inputdate),
                                         totalQty = Convert.ToInt32(grp.Sum(s => s.invd.boxqty * s.invd.itemperbox + s.invd.itemqty)),
                                         location = string.Join(",", grp.Select(s => s.invd.locationid)),
                                     };
                if (inventoryItems.ToList().Count == 0)//The item w/ this barcode does not exist
                {
                    var pQuery = products.AsQueryable().Where(p => p.barcode.ToUpper().IndexOf(Keyword.ToUpper())>=0);
                    if (pQuery.ToList().Count > 0)//Product exists
                    {
                        return Redirect("/inventory/create/" + Keyword);//Create the inventory
                    }
                    else
                    {
                        return Redirect("/product/create/" + Keyword);//Create an item
                    }
                    
                }
                else//This product exists let's check inventory
                {
                    var invs = from inv in unitOfWork.InventoryRepository.Get().AsQueryable()
                               join p in products.AsQueryable() on inv.sku equals p.sku
                               where p.barcode.ToUpper().IndexOf(Keyword.ToUpper()) >= 0 && inv.deleted != true && p.deleted != true
                               select inv;
                    if (invs.ToList().Count > 0)
                    {
                        var inv = invs.FirstOrDefault();
                        return Redirect("/Inventory/Edit/" + inv.id);
                    }
                    //else
                    //{
                    //    return Redirect("/Inventory/create/" + Keyword);
                    //}
                }

                //return View(inventoryItems.ToList());
            }
            else
            {
                inventoryItems = from t in
                                         (
                                            from inv in inventories.AsQueryable()
                                            join p in products.AsQueryable() on inv.sku equals p.sku
                                            join com in companies.AsQueryable() on inv.companyid equals com.id
                                            join invd in inventoryDetails.AsQueryable() on inv.id equals invd.inventoryid
                                            where (p.barcode.ToUpper().IndexOf(Keyword.ToUpper()) >= 0 ||
                                            com.name.ToUpper().IndexOf(Keyword.ToUpper()) >= 0 ||
                                            inv.sku.ToUpper().IndexOf(Keyword.ToUpper()) >= 0) && p.deleted != true && com.deleted != true && inv.deleted != true
                                            select new { inv, com, invd, p }
                                         )
                                     //select t;
                                     group t by new { t.inv.id, t.com.name, t.inv.sku, t.p.barcode, t.inv.inputdate } into grp
                                     select new InventoryListModel
                                     {
                                         ID = grp.Key.id,
                                         company = grp.Key.name,
                                         sku = grp.Key.sku,
                                         barcode = grp.Key.barcode,
                                         stockingDate = Convert.ToDateTime(grp.Key.inputdate),
                                         totalQty = Convert.ToInt32(grp.Sum(s => s.invd.boxqty * s.invd.itemperbox + s.invd.itemqty)),
                                         location = string.Join(",", grp.Select(s => s.invd.locationid)),
                                     };
                //return View(inventoryItems.ToList());
            }

            var pageNumber = page ?? 1;
            var onePageOfResults = inventoryItems.ToList().ToPagedList(pageNumber, 10);
            ViewBag.onePageOfResults = onePageOfResults;
            ViewBag.Message = Keyword;
            ViewBag.q = Keyword;
            //////return View();
            return View(onePageOfResults);
        }

        public ViewResult Edit(int id)
        {
            var inventory = unitOfWork.InventoryRepository.Get(null, null, "InventoryDetails").FirstOrDefault(inv => inv.id == id);
            var product = (from p in unitOfWork.ItemRepository.Get().AsQueryable()
                               where p.sku == inventory.sku
                                   select p).ToList().FirstOrDefault();
            ViewBag.Barcocde = product.barcode;

            ViewData["companyid"] = EZDropDown.Companies(unitOfWork, inventory.companyid);
            ViewData["sku"] = EZDropDown.Products(unitOfWork, inventory.sku);
            //ViewData["locationid"] = EZDropDown.Locations(unitOfWork, inventory.locationid);
            //Location l = unitOfWork.LocationRepository.GetByID(inventory.locationid);
            ViewData["locationid"] = EZDropDown.Locations(unitOfWork);
            //ViewData["boxid"] = EZDropDown.Boxes(unitOfWork);
            ViewData["boxqty"] = EZDropDown.Qunatity(100, null);
            ViewData["quantity"] = EZDropDown.Qunatity(100,null);
            ViewData["itembox"] = EZDropDown.Qunatity(100, null);
            return View(inventory);
        }

        public int GetQty(int id)
        {
            var inventoryDetails = unitOfWork.InventoryDetailRepository.Get().Where(invd=>invd.inventoryid == id);
            var inventoryItem = from invd in inventoryDetails
                                group invd by invd.inventoryid into grp
                                select new
                                {
                                    totalQty = (Convert.ToInt32(grp.Sum(s => s.boxqty * s.itemperbox + s.itemqty)))
                                };
            return (int)inventoryItem.FirstOrDefault().totalQty;
        }

        public JsonResult GetInventoriesByCompany(int id)
        {
            List<SelectListItem> inventories = new List<SelectListItem>();
            var inventoryQuery = unitOfWork.InventoryRepository.Get().Where(inv => inv.companyid == id && inv.deleted != true);
            foreach (Inventory inv in inventoryQuery.ToList())
            {
                inventories.Add(new SelectListItem{Text=inv.sku, Value=inv.id.ToString()});
            }
            return Json(new SelectList(inventories, "Value", "Text"));
        }


        public ActionResult Create(string barcode)
        {
            string sku = string.Empty;
            int companyID = 0;
            if (barcode != null)
            {
                var product = (from p in unitOfWork.ItemRepository.Get().AsQueryable()
                               where p.barcode == barcode && p.deleted != true
                               select p).ToList().FirstOrDefault();
                ViewData["sku"] = EZDropDown.Products(unitOfWork, product.sku);
                ViewData["companyid"] = EZDropDown.Companies(unitOfWork, (int)product.companyid);
            }
            else
            {
                ViewData["sku"] = EZDropDown.Products(unitOfWork, null);
                ViewData["companyid"] = EZDropDown.Companies(unitOfWork, null);
            }
            ViewData["locationid"] = EZDropDown.Locations(unitOfWork);
            ViewData["boxqty"] = EZDropDown.Qunatity(100,null);
            ViewData["itembox"] = EZDropDown.Qunatity(100, null);
            ViewBag.Barcode = barcode;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id, sku, lot, batch, expdate, qty, inputdate, companyid, locationid, capacity")]Inventory inventory)
        {
            try
            {
                ViewData["companyid"] = EZDropDown.Companies(unitOfWork, inventory.companyid);
                ViewData["sku"] = EZDropDown.Products(unitOfWork, inventory.sku);
                ViewData["locationid"] = EZDropDown.Locations(unitOfWork);
                ViewData["boxqty"] = EZDropDown.Qunatity(100, null);
                ViewData["itembox"] = EZDropDown.Qunatity(100, null);
                if (Request["companyid"] == "")
                {
                    ModelState.AddModelError("companyid", "Required");
                }
                if (Request["sku"] == "")
                {
                    ModelState.AddModelError("sku", "Required");
                }
                if (Request["hdnInventory"] == null)
                {
                    ModelState.AddModelError("InventoryDetails", "Please provide Carton Q'ty, Items/Carton, Total Item Q'ty, Location and Use");
                }
                if (ModelState.IsValid)
                {
                    foreach (InventoryDetail id in db.InventoryDetails.Where(id => id.inventoryid == inventory.id))
                    {
                        db.Entry(id).State = EntityState.Deleted;

                        //Location l = unitOfWork.LocationRepository.GetByID(id.locationid);
                        //l.capacity = int.Parse(aInv[4]);
                        //unitOfWork.LocationRepository.Update(l);
                        //unitOfWork.Save();
                    }

                    db.Entry(inventory).State = EntityState.Modified;
                    string tmp = Request["hdnInventory"].ToString();
                    string[] aInvs = tmp.Split(',');
                    foreach (string inv in aInvs)
                    {
                        string[] aInv = inv.Split(':');
                        //locationid:qty:boxid:quantity:newCapacity
                        //locationid:boxqty:itembox:qty:newCapacity
                        InventoryDetail id = new InventoryDetail();
                        id.locationid = aInv[0];
                        id.lot = aInv[1];
                        id.boxqty = int.Parse(aInv[2]);
                        id.itemperbox = int.Parse(aInv[3]);
                        id.itemqty = int.Parse(aInv[4]);
                        id.capacity = int.Parse(aInv[5]);
                        inventory.InventoryDetails.Add(id);

                        //Location l = unitOfWork.LocationRepository.GetByID(aInv[0]);
                        //l.capacity = int.Parse(aInv[4]);
                        //unitOfWork.LocationRepository.Update(l);
                        //unitOfWork.Save();
                    }

                    db.SaveChanges();

                    //var formerLocationID = Request["formerLocationID"].ToString();
                    //int formerCapacity = Convert.ToInt32(Request["formerCapacity"].ToString());
                    //unitOfWork.InventoryRepository.Update(inventory);
                    //unitOfWork.Save();
                    return RedirectToAction("Index");
                }
            }
            catch (DataException /* dex */)
            {
                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
                ViewBag.sku = EZDropDown.Products(unitOfWork, inventory.sku);
            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "sku, lot, batch, expdate, qty, inputdate, companyid, locationid, capacity")]Inventory inventory)
        {

            try
            {
                ViewBag.CompanyIDs = unitOfWork.CompanyRepository.Get().Where(c => c.billto == true && c.deleted != true).OrderBy(c => c.name);
                ViewBag.SKUs = unitOfWork.ItemRepository.Get().Where(i=>i.deleted != true).OrderBy(i=>i.sku);
                ViewData["locationid"] = EZDropDown.Locations(unitOfWork);
                ViewData["boxqty"] = EZDropDown.Qunatity(100, null);
                ViewData["itembox"] = EZDropDown.Qunatity(100, null);
                ViewBag.Barcode = Request["barcode"].ToString();
                if (Request["hdnInventory"] == null)
                {
                    ModelState.AddModelError("InventoryDetails", "Please provide Carton Q'ty, Items/Carton, Total Item Q'ty, Location and Use");
                }

                if (ModelState.IsValid)
                {
                    //TODO: Iterate the hdnInventory Values
                    //TODO: Update the capacity within a loop
                    string tmp = Request["hdnInventory"].ToString();
                    string[] aInvs = tmp.Split(',');
                    foreach (string inv in aInvs)
                    {
                        string[] aInv = inv.Split(':');
                        //locationid:boxqty:itembox:qty:newCapacity
                        InventoryDetail id = new InventoryDetail();
                        id.locationid = aInv[0];
                        id.lot = aInv[1];
                        id.boxqty = int.Parse(aInv[2]);
                        id.itemperbox = int.Parse(aInv[3]);
                        id.itemqty = int.Parse(aInv[4]);
                        id.capacity = int.Parse(aInv[5]);
                        inventory.InventoryDetails.Add(id);


                        //////Location l = unitOfWork.LocationRepository.GetByID(aInv[0]);
                        //////l.capacity = int.Parse(aInv[4]);
                        //////unitOfWork.LocationRepository.Update(l);
                        //////unitOfWork.Save();

                    }

                    db.Inventories.Add(inventory);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                

            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            //ViewData["sku"] = EZDropDown.Products(unitOfWork);
            return View(inventory);
        }


        public ViewResult Details(int id)
        {
            var inventory = unitOfWork.InventoryRepository.Get(null, null, "InventoryDetails").FirstOrDefault(inv => inv.id == id);
            var product = (from p in unitOfWork.ItemRepository.Get().AsQueryable()
                           where p.sku == inventory.sku
                           select p).ToList().FirstOrDefault();
            ViewBag.Barcocde = product.barcode;

            var company = unitOfWork.CompanyRepository.GetByID(inventory.companyid);
            ViewBag.Company = company.name;
            ViewData["sku"] = EZDropDown.Products(unitOfWork, inventory.sku);
            //ViewData["locationid"] = EZDropDown.Locations(unitOfWork, inventory.locationid);
            //Location l = unitOfWork.LocationRepository.GetByID(inventory.locationid);
            ViewData["locationid"] = EZDropDown.Locations(unitOfWork);
            //ViewData["boxid"] = EZDropDown.Boxes(unitOfWork);
            ViewData["boxqty"] = EZDropDown.Qunatity(100, null);
            ViewData["quantity"] = EZDropDown.Qunatity(100, null);
            ViewData["itembox"] = EZDropDown.Qunatity(100, null);
            return View(inventory);
        }

        // GET: /Course/Delete/5
        public ActionResult Delete(int id)
        {
            Inventory inv = db.Inventories.Find(id);
            var product = (from p in unitOfWork.ItemRepository.Get().AsQueryable()
                           where p.sku == inv.sku
                           select p).ToList().FirstOrDefault();
            ViewBag.Barcocde = product.barcode;

            var company = unitOfWork.CompanyRepository.GetByID(inv.companyid);
            ViewBag.Company = company.name;
            if (inv == null)
            {
                return HttpNotFound();
            }
            return View(inv);
        }

        // POST: /Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Inventory inv = db.Inventories.Find(id);
            inv.deleted = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
