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
using System;
using System.Collections;


namespace EZInventory.Models
{
    public static class EZDropDown
    {
        public static SelectList Products(UnitOfWork unitOfWork, object selectedProduct = null)
        {
            //var productsQuery = unitOfWork.ItemRepository.Get(
            //    orderBy: q => q.OrderBy(d => d.sku));
            var productsQuery = unitOfWork.ItemRepository.Get().Where(p => p.deleted != true).OrderBy(c => c.sku);
            return new SelectList(productsQuery, "sku", "sku", selectedProduct);
        }

        public static List<Item> Products(UnitOfWork unitOfWork)
        {
            //return unitOfWork.ItemRepository.Get(
            //    orderBy: q => q.OrderBy(d => d.sku)).ToList();
            return unitOfWork.ItemRepository.Get().Where(p => p.deleted != true).OrderBy(c => c.sku).ToList();
        }

        public static SelectList Stores(UnitOfWork unitOfWork, object selectedStore = null)
        {
            var storeQuery = unitOfWork.StoreRepository.Get(
                orderBy: q => q.OrderBy(d => d.name));
            return new SelectList(storeQuery, "id", "name", selectedStore);
        }

        public static SelectList Inventories(UnitOfWork unitOfWork, object selectedInventory = null)
        {
            var inventoryQuery = unitOfWork.InventoryRepository.Get(orderBy: q => q.OrderBy(d => d.sku));
            return new SelectList(inventoryQuery, "id", "sku", selectedInventory);
        }

        public static SelectList InventoriesByCompany(UnitOfWork unitOfWork, int companyID)
        {
            var inventoryQuery = unitOfWork.InventoryRepository.Get(orderBy: q => q.OrderBy(d => d.sku)).Where(invd=>invd.companyid == companyID && invd.deleted != true && invd.Company.deleted != true);
            return new SelectList(inventoryQuery, "id", "sku");
        }

        public static SelectList Statuses(UnitOfWork unitOfWork, object selectedStatus = null)
        {
            var statusQuery = unitOfWork.StatusRepository.Get(orderBy: s => s.OrderBy(d => d.id));
            return new SelectList(statusQuery, "id", "description", selectedStatus);
            //var inventoryQuery = unitOfWork.InventoryRepository.Get(orderBy: q => q.OrderBy(d => d.sku));
            //return new SelectList(inventoryQuery, "id", "sku", selectedStatus);
        }

        public static SelectList Locations(UnitOfWork unitOfwork, object selectedLocation = null)
        {
            var locationQuery = unitOfwork.LocationRepository.Get();
            var inventoryDetailQuery = unitOfwork.InventoryDetailRepository.Get().Where(id=>id.Inventory.deleted != true);

            //from t in
            //    (
            //    from inv in inventories.AsQueryable()
            //    join com in companies.AsQueryable() on inv.companyid equals com.id
            //    join invd in inventoryDetails.AsQueryable() on inv.id equals invd.inventoryid
            //    select new { inv, com, invd }
            //    )
            ////select t;
            //group t by new { t.inv.id, t.com.name, t.inv.sku, t.inv.inputdate} into grp
            //select new InventoryListModel
            //                     {
            //                         ID = grp.Key.id,
            //                         company = grp.Key.name,
            //                         sku = grp.Key.sku,
            //                         stockingDate = Convert.ToDateTime(grp.Key.inputdate),
            //                         totalQty = Convert.ToInt32(grp.Sum(s => s.invd.itemqty)),
            //                         location = string.Join(",", grp.Select(s => s.invd.locationid)),
            //                     };

            var locationDDLQuery = from t in
                                       (
                                          from l in locationQuery.AsQueryable()
                                          join id in inventoryDetailQuery.AsQueryable() on l.id equals id.locationid into gj//Left join!!!
                                          from subid in gj.DefaultIfEmpty()
                                          select new { l, subid }
                                       )
                                   group t by new { t.l.id } into grp
                                   select new
                                   {
                                       id = grp.Key.id,
                                       idp = grp.Key.id + " : " + (grp.Sum(s=>(s.subid == null)? 0:s.subid.capacity) + "% filled")
                                   };

            //var locationDDLQuery = from l in locationQuery
            //                       select new
            //                       {
            //                           id = (string)l.id,
            //                           idp = (string)l.id + " : " + "0" + "% filled"
            //                       };
            return new SelectList(locationDDLQuery, "id", "idp", selectedLocation);
        }

        //public static List<LocationDropdownModel> Locations(UnitOfWork unitOfwork)
        //{
        //    var locationQuery = unitOfwork.LocationRepository.Get();
        //    var inventoryDetailQuery = unitOfwork.InventoryDetailRepository.Get();

        //    var locationDDLQuery = from t in
        //                               (
        //                                  from l in locationQuery.AsQueryable()
        //                                  join id in inventoryDetailQuery.AsQueryable() on l.id equals id.locationid into gj//Left join!!!
        //                                  from subid in gj.DefaultIfEmpty()
        //                                  select new { l, subid }
        //                               )
        //                           group t by new { t.l.id } into grp
        //                           select new LocationDropdownModel
        //                           {
        //                               id = grp.Key.id,
        //                               idp = grp.Key.id + " : " + (grp.Sum(s => (s.subid == null) ? 0 : s.subid.capacity) + "% filled")
        //                           };
        //    return locationDDLQuery.ToList();

        //}


        public static SelectList Percentage(object selectedPercentage = null)
        {
            ArrayList alPercentage = new ArrayList();
            for (int i = 1; i <= 100; i++)
            {
                alPercentage.Add(i);
            }
            var percentageQuery = from int p in alPercentage
                                  select new
                                  {
                                      value = p,
                                      item = p.ToString() + " %"
                                  };
            return new SelectList(percentageQuery, "value", "item", selectedPercentage);

        }

        public static SelectList Boxes(UnitOfWork unitOfWork, object selectedBox = null)
        {
            var boxQuery = unitOfWork.BoxRepository.Get(orderBy: box => box.OrderBy(d => d.id));
            return new SelectList(boxQuery, "id", "size", selectedBox);
        }

        public static SelectList Qunatity(int max, object selectedQuantity = null)
        {
            ArrayList alQuantity = new ArrayList();
            for (int i = 1; i <= max; i++)
            {
                alQuantity.Add(i);
            }
            var quantityQuery = from int q in alQuantity
                                  select new
                                  {
                                      value = q.ToString(),
                                      item = q.ToString()
                                  };
            return new SelectList(quantityQuery, "value", "item", selectedQuantity);

        }
        //public static SelectList Qtys(UnitOfWork unitOfWork, string sku = null)
        //{
        //    if (sku != null)
        //    {
        //    }
        //    else
        //    {
        //    }
        //    //var storeQuery = unitOfWork.StoreRepository.Get(
        //    //    orderBy: q => q.OrderBy(d => d.name));
        //    //return new SelectList(storeQuery, "id", "name", selectedStore);
        //}

        public static SelectList Companies(UnitOfWork unitOfWork, object selectedCompany = null)
        {
            var companyQuery = unitOfWork.CompanyRepository.Get().Where(c => c.billto == true && c.deleted != true).OrderBy(c=>c.name);
            return new SelectList(companyQuery, "id", "name", selectedCompany);
        }

    }
}