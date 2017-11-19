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
using System;


namespace EZInventory.Controllers
{
    public class LocationController : Controller
    {
        private UnitOfWork unitOfWork = new UnitOfWork();
        private EZInventoryEntities db = new EZInventoryEntities();
        //
        // GET: /Location/

        public ViewResult Index()
        {
            var locations = unitOfWork.LocationRepository.Get().Where(l=>l.deleted != true);
            var lvs = new List<LocationViewModel>();

            var ls = from l in locations
                         select l;

            foreach (var l in ls)
            {
                var lv = new LocationViewModel();
                lv.ID = l.id;
                lv.description = l.description;
                lv.capacity = l.InventoryDetails.Where(i => i.Inventory.deleted != true).Sum(s => s.capacity).Value;
                lv.inventoryDetails = l.InventoryDetails.Where(i=>i.Inventory.deleted != true).ToList();
                lvs.Add(lv);
            }


            return View(lvs);
        }

        public ActionResult Create(string description)
        {
            Location l = new Location();
            l.description = description;
            return View(l);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,description,deleted")]Location l)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Locations.Add(l);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return View(l);
        }


        public ViewResult Edit(string id)
        {
            var l = unitOfWork.LocationRepository.GetByID(id);
            return View(l);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id, description")]Location l)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(l).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return View(l);
        }

        // GET: /Course/Delete/5
        public ActionResult Delete(string id)
        {
            Location l = db.Locations.Find(id);
            if (l == null)
            {
                return HttpNotFound();
            }
            return View(l);
        }

        // POST: /Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Location l = db.Locations.Find(id);
            l.deleted = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
