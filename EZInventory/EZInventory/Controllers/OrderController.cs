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
    public class OrderController : Controller
    {
        private UnitOfWork unitOfWork = new UnitOfWork();
        private EZInventoryEntities db = new EZInventoryEntities();
        //
        // GET: /Order/

        public ViewResult Index(int? page)
        {
            return GetOrderList("", page);
        }

        [HttpPost]
        public ViewResult Index(string Keyword)
        {
            return GetOrderList(Keyword);
        }

        private ViewResult GetOrderList(string Keyword, int? page = null)
        {
            var orders = unitOfWork.OrderRepository.Get();
            var orderDetails = unitOfWork.OrderDetailRepository.Get();
            var inventories = unitOfWork.InventoryRepository.Get();
            LoginModel lm = (LoginModel)Session["loginModel"];
            var companies = unitOfWork.CompanyRepository.Get();
            var stores = unitOfWork.StoreRepository.Get();
            var items = unitOfWork.ItemRepository.Get();
            var statuses = unitOfWork.StatusRepository.Get();
            var companyOrders = unitOfWork.CompanyOrderRepository.Get();


            if (lm.companyID > 0)
                companies = companies.Where(c => c.id == lm.companyID || c.billtocid == lm.companyID);
            int orderID = 0;
            int.TryParse(Keyword, out orderID);


            var listItems = from t in
                                (
                                    from od in orderDetails.AsQueryable()
                                    join inv in inventories.AsQueryable() on od.inventoryid equals inv.id
                                    join o in orders.AsQueryable() on od.orderid equals o.id
                                    join st in statuses.AsQueryable() on o.statusid equals st.id
                                    //where o.id == orderID || inv.sku.IndexOf(Keyword)>=0
                                    where ((o.deleted != true) && (o.id == orderID ||
                                           inv.sku.ToUpper().Contains(Keyword.ToUpper()) ||
                                           (from c in companies.AsQueryable()
                                            join co in companyOrders.AsQueryable() on c.id equals co.companyid
                                            join _o in orders.AsQueryable() on co.orderid equals _o.id
                                            where (_o.id == o.id && co.type == 1 && c.name.ToUpper().Contains(Keyword.ToUpper())) && c.deleted != true && _o.deleted != true
                                            select c.name).Count() > 0 ||
                                           (from c in companies.AsQueryable()
                                            join co in companyOrders.AsQueryable() on c.id equals co.companyid
                                            join _o in orders.AsQueryable() on co.orderid equals _o.id
                                            where ((_o.id == o.id && co.type == 2 && c.name.ToUpper().Contains(Keyword.ToUpper())) || 
                                            (od.Inventory.Item.barcode == Keyword)) && _o.deleted != true
                                            select c.name).Count() > 0))
                                    //////select new { od, inv, o, c, st }
                                    select new { od, inv, o, st }
                                )
                            //////group t by new { t.o.id, t.c.name, t.o.date, t.st.description } into grp
                            group t by new { t.o.id, t.o.date, t.st.description } into grp
                            orderby grp.Key.date descending
                            select new OrderListModel
                            {
                                ID = grp.Key.id,
                                //////CompanyName = grp.Key.name.ToString(),
                                //CompanyName = string.Join(",", grp.Select(s => s.c.name)),
                                BillTo = (from c in companies.AsQueryable()
                                          join co in companyOrders.AsQueryable() on c.id equals co.companyid
                                          join o in orders.AsQueryable() on co.orderid equals o.id
                                          where o.id == grp.Key.id && co.type == 1 && c.deleted != true && o.deleted != true
                                          select c.name).FirstOrDefault(),
                                ShipTo = (from c in companies.AsQueryable()
                                          join co in companyOrders.AsQueryable() on c.id equals co.companyid
                                          join o in orders.AsQueryable() on co.orderid equals o.id
                                          where o.id == grp.Key.id && co.type == 2 && c.deleted != true && o.deleted != true
                                          select c.name).FirstOrDefault(),
                                Items = string.Join(",", grp.Select(s => s.inv.sku).Distinct()),
                                Date = Convert.ToDateTime(grp.Key.date),
                                Status = grp.Key.description.ToString(),
                                TotalQuantity = grp.Sum(s => s.od.qty)
                                //TotalQuantity = grp.Key.qty
                            };
            var pageNumber = page ?? 1;
            var onePageOfResults = listItems.ToList().Where(c => c.BillTo != null).ToPagedList(pageNumber, 10);
            //var onePageOfResults = listItems.ToList().Where(c => c.BillTo != null).ToPagedList(pageNumber, 10);
            ViewBag.onePageOfResults = onePageOfResults;
            ViewBag.Message = Keyword;
            ViewBag.q = Keyword;
            //////return View();
            return View(onePageOfResults);
        }

        public ViewResult GetOrder(int id)
        {
            var order = unitOfWork.OrderRepository.Get(null, null, "OrderDetails.Inventory, Status").FirstOrDefault(o => o.id == id && o.deleted != true);
            var companies = unitOfWork.CompanyRepository.Get();

            ViewData["BillTo"] = (from co in order.CompanyOrders.AsQueryable()
                                  join c in companies.AsQueryable() on co.companyid equals c.id
                                  where co.type == 1 && c.deleted != true
                                  select c.name).FirstOrDefault();

            ViewData["ShipTo"] = (from co in order.CompanyOrders.AsQueryable()
                                  join c in companies.AsQueryable() on co.companyid equals c.id
                                  where co.type == 2 && c.deleted != true
                                  select c.name).FirstOrDefault();
            //var order = unitOfWork.OrderRepository.GetByID(id);
            return View(order);
        }

        public ViewResult Details(int id)
        {
            return GetOrder(id);
        }

        public ViewResult Delete(int id)
        {
            return GetOrder(id);
        }

        // POST: /Course/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Order order)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(order).State = EntityState.Modified;
                    order.deleted = true;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return View(order);

        }

        public ViewResult Packing(int id, string view)
        {
            var order = unitOfWork.OrderRepository.Get(null, null, "OrderDetails.Inventory.InventoryDetails.Location, CompanyOrders.Company").FirstOrDefault(o => o.id == id);
            WorkOrderModel wom = new WorkOrderModel();

            wom.OrderID = id;
            wom.ShipDate = (order.shippingdate == null) ? DateTime.Now : (DateTime)order.shippingdate;

            Company billTo = order.CompanyOrders.AsQueryable().Include(co => co.Company).Where(co => co.type == 1).FirstOrDefault().Company;
            Company shipTo = order.CompanyOrders.AsQueryable().Include(co => co.Company).Where(co => co.type == 2).FirstOrDefault().Company;

            wom.ShipTo = shipTo.name;
            wom.ShipToAddress = shipTo.address;
            wom.ShipToCity = shipTo.city;
            wom.ShipToState = shipTo.state;
            wom.ShipToZip = shipTo.zip;

            wom.BillTo = billTo.name;
            wom.BillToAddress = billTo.address;
            wom.BillToCity = billTo.city;
            wom.BillToState = billTo.state;
            wom.BillToZip = billTo.zip;

            string tmp = string.Empty;
            List<LocationViewModel> locationViewModels = new List<LocationViewModel>();
            int totalCarton = 0;
            int totalQty = 0;
            int itemPerBox = 0;

            var packageDetailQry = from od in order.OrderDetails.AsQueryable()
                                   group od by new { od.orderid, od.inventoryid, od.Inventory } into grp
                                   select new OrderDetail
                                   {
                                       orderid = grp.Key.orderid,
                                       inventoryid = grp.Key.inventoryid,
                                       qty = grp.Sum(s => s.qty),
                                       boxqty = grp.Sum(s => s.boxqty),
                                       Inventory = grp.Key.Inventory                                   
                                   };

            foreach (var od in packageDetailQry.AsQueryable().Include(od => od.Inventory.InventoryDetails).ToList())
            {
                InventoryDetail ivd = od.Inventory.InventoryDetails.FirstOrDefault();
                itemPerBox = (int)ivd.itemperbox;

                if (locationViewModels.FindIndex(x => x.InventoryView.sku == od.Inventory.sku) < 0)
                {
                    LocationViewModel lvm = new LocationViewModel();
                    if ((int)od.qty < itemPerBox)
                    {
                        lvm.untCarton = "0 x 0";
                        lvm.parCarton = "1 x " + (int)od.qty;
                    }
                    else if ((int)od.qty % itemPerBox == 0)
                    {
                        lvm.untCarton = (int)od.boxqty + " x " + itemPerBox;
                        lvm.parCarton = "0 x 0";
                    }
                    else
                    {
                        lvm.untCarton = ((int)od.boxqty - 1) + " x " + itemPerBox;
                        lvm.parCarton = "1 x " + ((int)od.qty % itemPerBox);
                    }

                    lvm.itemQty = (int)od.qty;
                    lvm.InventoryView = od.Inventory;
                    totalCarton += (int)od.boxqty;
                    totalQty += (int)od.qty;
                    locationViewModels.Add(lvm);
                }
            }
            wom.LocationViewModels = locationViewModels;
            wom.TotalQty = totalQty;
            wom.TotalCarton = totalCarton;
            wom.Rate = (decimal)order.rate;
            return View(view, wom);
        }

        public ViewResult WorkOrder(int id)
        {
            var order = unitOfWork.OrderRepository.Get(null, null, "OrderDetails.Inventory.InventoryDetails.Location, CompanyOrders.Company").FirstOrDefault(o => o.id == id);
            WorkOrderModel wom = new WorkOrderModel();

            wom.OrderID = id;
            wom.ShipDate = (order.shippingdate == null) ? DateTime.Now : (DateTime)order.shippingdate;


            Company billTo = order.CompanyOrders.AsQueryable().Include(co => co.Company).Where(co => co.type == 1).FirstOrDefault().Company;
            Company shipTo = order.CompanyOrders.AsQueryable().Include(co => co.Company).Where(co => co.type == 2).FirstOrDefault().Company;

            wom.ShipTo = shipTo.name;
            wom.ShipToAddress = shipTo.address;
            wom.ShipToCity = shipTo.city;
            wom.ShipToState = shipTo.state;
            wom.ShipToZip = shipTo.zip;

            wom.BillTo = billTo.name;
            wom.BillToAddress = billTo.address;
            wom.BillToCity = billTo.city;
            wom.BillToState = billTo.state;
            wom.BillToZip = billTo.zip;

            string tmp = string.Empty;
            List<LocationViewModel> locationViewModels = new List<LocationViewModel>();
            int totalCarton = 0;
            int totalQty = 0;
            int itemPerBox = 0;
            foreach (var od in order.OrderDetails.AsQueryable().Include(od => od.Inventory.InventoryDetails).ToList())
            {
                InventoryDetail ivd = od.Inventory.InventoryDetails.FirstOrDefault();
                itemPerBox = (int)ivd.itemperbox;

                if (locationViewModels.FindIndex(x => x.ID == od.locationid) < 0)
                {
                    LocationViewModel lvm = new LocationViewModel();
                    lvm.ID = od.locationid;
                    if ((int)od.qty < itemPerBox)
                    {
                        lvm.untCarton = "0 x 0";
                        lvm.parCarton = "1 x " + (int)od.qty;
                    }
                    else if ((int)od.qty % itemPerBox == 0)
                    {
                        lvm.untCarton = (int)od.boxqty + " x " + itemPerBox;
                        lvm.parCarton = "0 x 0";
                    }
                    else
                    {
                        lvm.untCarton = ((int)od.boxqty - 1) + " x " + itemPerBox;
                        lvm.parCarton = "1 x " + ((int)od.qty % itemPerBox);
                    }
                    //lvm.boxQty = (int)ivd.boxqty;
                    lvm.itemQty = (int)od.qty;
                    lvm.InventoryView = od.Inventory;
                    totalCarton += (int)od.boxqty;
                    totalQty += (int)od.qty;
                    locationViewModels.Add(lvm);
                }
            }
            wom.LocationViewModels = locationViewModels;
            wom.TotalQty = totalQty;
            wom.TotalCarton = totalCarton;
            wom.Rate = (decimal)order.rate;
            return View(wom);
        }

        public ActionResult Create()
        {
            ViewData["billtoid"] = EZDropDown.Companies(unitOfWork);
            //ViewData["inventoryid"] = EZDropDown.Inventories(unitOfWork);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "date, statusid, rate, deleted")]Order order)
        {
            try
            {
                ViewData["billtoid"] = EZDropDown.Companies(unitOfWork);

                if (Request["billtoid"].ToString() == "")
                {
                    ModelState.AddModelError("CompanyOrders", "Required");
                }

                if (Request["shiptoid"]!=null && Request["shiptoid"].ToString() == "")
                {
                    ModelState.AddModelError("CompanyOrders", "Required");
                }

                //if (Request["hdnInventory"] == null)
                //{
                //    
                //}
                if ((Request["inventoryid"] == null) ||
                    (Request["qty"] == null) ||
                    (Request["inventoryid"] != null && Request["inventoryid"].ToString() == "") ||
                    (Request["qty"] != null && Request["qty"].ToString() == ""))
                {
                    ModelState.AddModelError("OrderDetails", "Inventory and Quantity are required");
                }
                if (ModelState.IsValid)
                {
                    int billToID = int.Parse(Request["billtoid"].ToString());
                    int shipToID = int.Parse(Request["shiptoid"].ToString());

                    CompanyOrder co = new CompanyOrder();
                    co.type = 1;//BillTo
                    co.companyid = billToID;
                    order.CompanyOrders.Add(co);

                    co = new CompanyOrder();
                    co.type = 2;//ShipTo
                    co.companyid = shipToID;
                    order.CompanyOrders.Add(co);

                    int inventoryID = int.Parse(Request["inventoryid"].ToString());
                    int itemTotalQty = int.Parse(Request["qty"].ToString());
                    int boxqty = 0;
                    int itemQtyLocation = 0;
                    var inventoryDetails = unitOfWork.InventoryDetailRepository.Get().Where(id => id.inventoryid == inventoryID).ToList();
                    foreach (InventoryDetail id in inventoryDetails)
                    {
                        int quantity = (int)id.boxqty * (int)id.itemperbox + (int)id.itemqty;
                        if (quantity < itemTotalQty)//Items are stored in multiple locations
                            itemQtyLocation = quantity;//Partial quantity in a location
                        else
                            itemQtyLocation = itemTotalQty;//The entire quantity

                        boxqty = (int)Math.Ceiling((decimal)(itemQtyLocation) / (int)(id.itemperbox));

                        OrderDetail od = new OrderDetail();
                        od.inventoryid = inventoryID;
                        od.qty = itemQtyLocation;
                        od.boxqty = boxqty;
                        od.locationid = id.locationid;
                        order.OrderDetails.Add(od);
                        if (quantity < itemTotalQty)//More items to ship so get it from a different location
                            itemTotalQty -= quantity;
                        else
                            break;
                    }
                    


                    
                    


                    //TO DO: Subtract item and box quantity!
                    //db.Entry(inventoryDetail).State = EntityState.Modified;
                    //inventoryDetail.boxqty -= boxqty;
                    //inventoryDetail.itemqty -= itemqty;

                    if (Request["hdnInventory"] != null)
                    {
                        string tmp = Request["hdnInventory"].ToString();
                        string[] aInvs = tmp.Split(',');
                        foreach (string inv in aInvs)
                        {
                            string[] aInv = inv.Split(':');
                            //od = new OrderDetail();
                            //od.inventoryid = int.Parse(aInv[0]);
                            //od.qty = int.Parse(aInv[1]);
                            //boxqty = (int)Math.Ceiling(decimal.Parse(aInv[1]) / (int)(inventoryDetail.itemperbox));
                            //od.boxqty = boxqty;
                            inventoryID = int.Parse(aInv[0]);
                            itemTotalQty = int.Parse(aInv[1]);
                            inventoryDetails = unitOfWork.InventoryDetailRepository.Get().Where(id => id.inventoryid == inventoryID).ToList();
                            foreach (InventoryDetail id in inventoryDetails)
                            {
                                int quantity = (int)id.boxqty * (int)id.itemperbox + (int)id.itemqty;
                                if (quantity < itemTotalQty)//Items are stored in multiple locations
                                    itemQtyLocation = quantity;//Partial quantity in a location
                                else
                                    itemQtyLocation = itemTotalQty;//The entire quantity
                                boxqty = (int)Math.Ceiling((decimal)(itemQtyLocation) / (int)(id.itemperbox));

                                OrderDetail od = new OrderDetail();
                                od.inventoryid = inventoryID;
                                od.qty = itemQtyLocation;
                                od.boxqty = boxqty;
                                od.locationid = id.locationid;
                                order.OrderDetails.Add(od);
                                if (quantity < itemTotalQty)//More items to ship so get it from a different location
                                    itemTotalQty -= quantity;
                                else
                                    break;
                            }

                            //order.OrderDetails.Add(od);
                        }
                    }

                    db.Orders.Add(order);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return View(order);
        }


        public ViewResult Edit(int id)
        {
            //var order = unitOfWork.OrderRepository.GetByID(id);
            var order = unitOfWork.OrderRepository.Get(null, null, "OrderDetails").FirstOrDefault(o => o.id == id);
            var companies = unitOfWork.CompanyRepository.Get();
            var inventories = unitOfWork.InventoryRepository.Get();

           ViewData["BillTo"] = (from co in order.CompanyOrders.AsQueryable()
                                 join c in companies.AsQueryable() on co.companyid equals c.id 
                                 where co.type == 1
                                 select c.name).FirstOrDefault();

           ViewData["ShipTo"] = (from co in order.CompanyOrders.AsQueryable()
                                 join c in companies.AsQueryable() on co.companyid equals c.id
                                 where co.type == 2
                                 select c.name).FirstOrDefault();
           
            int companyID = (from co in order.CompanyOrders.AsQueryable()
                            join c in companies.AsQueryable() on co.companyid equals c.id
                            where co.type == 1
                            select c.id).FirstOrDefault();

            var groupOrderDetails = from t in
                                   (
                                       from od in order.OrderDetails.AsQueryable()
                                       join inv in inventories.AsQueryable() on od.inventoryid equals inv.id
                                       select new { od, inv }
                                   )
                               group t by new { t.inv.sku, t.od.inventoryid } into grp
                               select new { inventoryid = grp.Key.inventoryid, sku = grp.Key.sku, qty = grp.Sum(s => s.od.qty) };
            
            ViewData["groupOrderDetails"] = groupOrderDetails.ToList();

                               
                
                
                //from od in order.OrderDetails.AsQueryable()
                //               group od by new { od.Inventory.sku, od.inventoryid } into grp
                //               select grp.Key.sku, grp.Sum(s => s.qty);


            ViewData["inventoryid"] = EZDropDown.InventoriesByCompany(unitOfWork, companyID);
            ViewData["statusid"] = EZDropDown.Statuses(unitOfWork, order.statusid);

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Order order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    
                    foreach (OrderDetail od in db.OrderDetails.Where(od => od.orderid == order.id))
                    {
                        db.Entry(od).State = EntityState.Deleted;
                    }


                    //Item item = db.Items.Find(sku);
                    //db.Items.Remove(item);
                    //db.SaveChanges();
                    //return RedirectToAction("Index");
                    //db.Entry(order).State = EntityState.Modified;

                    //foreach (OrderDetail od in order.OrderDetails)
                    //{
                    //    db.OrderDetails.Remove(od);
                    //}
                    //db.SaveChanges();


                    //order.OrderDetails.Remove(orderDetails.SingleOrDefault(s => s.orderid == order.id));
                    //db.SaveChanges();
                    int inventoryID = 0; 
                    int itemTotalQty = 0;
                    int boxqty = 0;
                    int itemQtyLocation = 0;
                    db.Entry(order).State = EntityState.Modified;
                    


                    if (Request["hdnInventory"] != null)
                    {
                        string tmp = Request["hdnInventory"].ToString();
                        string[] aInvs = tmp.Split(',');
                        foreach (string inv in aInvs)
                        {
                            string[] aInv = inv.Split(':');
                            inventoryID = int.Parse(aInv[0]);
                            itemTotalQty = int.Parse(aInv[1]);
                            var inventoryDetails = unitOfWork.InventoryDetailRepository.Get().Where(id => id.inventoryid == inventoryID).ToList();
                            foreach (InventoryDetail id in inventoryDetails)
                            {
                                int quantity = (int)id.boxqty * (int)id.itemperbox + (int)id.itemqty;
                                if (quantity < itemTotalQty)//Items are stored in multiple locations
                                    itemQtyLocation = quantity;//Partial quantity in a location
                                else
                                    itemQtyLocation = itemTotalQty;//The entire quantity

                                boxqty = (int)Math.Ceiling((decimal)(itemQtyLocation) / (int)(id.itemperbox));

                                OrderDetail od = new OrderDetail();
                                od.inventoryid = inventoryID;
                                od.qty = itemQtyLocation;
                                od.boxqty = boxqty;
                                od.locationid = id.locationid;
                                order.OrderDetails.Add(od);
                                if (quantity < itemTotalQty)//More items to ship so get it from a different location
                                    itemTotalQty -= quantity;
                                else
                                    break;
                            }
                            //OrderDetail od = new OrderDetail();
                            //od.inventoryid = int.Parse(aInv[0]);
                            //od.qty = int.Parse(aInv[1]);
                            ////od.qty = 3;
                            //order.OrderDetails.Add(od);
                        }
                    }

                    if (Request["inventoryid"].ToString() != "")
                    {
                        inventoryID = int.Parse(Request["inventoryid"].ToString());
                        itemTotalQty = int.Parse(Request["qty"].ToString());
                        var inventoryDetails = unitOfWork.InventoryDetailRepository.Get().Where(id => id.inventoryid == inventoryID).ToList();
                        foreach (InventoryDetail id in inventoryDetails)
                        {

                            int quantity = (int)id.boxqty * (int)id.itemperbox + (int)id.itemqty;
                            if (quantity < itemTotalQty)//Items are stored in multiple locations
                                itemQtyLocation = quantity;//Partial quantity in a location
                            else
                                itemQtyLocation = itemTotalQty;//The entire quantity

                            boxqty = (int)Math.Ceiling((decimal)(itemQtyLocation) / (int)(id.itemperbox));

                            OrderDetail orderDetail =  order.OrderDetails.Where(o => o.inventoryid == inventoryID).FirstOrDefault();

                            if (orderDetail == null)
                            {
                                OrderDetail od = new OrderDetail();
                                od.inventoryid = inventoryID;
                                od.qty = itemQtyLocation;
                                od.boxqty = boxqty;
                                od.locationid = id.locationid;
                                order.OrderDetails.Add(od);
                            }
                            else
                            {
                                orderDetail.qty += itemQtyLocation;
                                boxqty = (int)Math.Ceiling((decimal)(orderDetail.qty) / (int)(id.itemperbox));
                                orderDetail.boxqty = boxqty;
                            }

                            if (quantity < itemTotalQty)//More items to ship so get it from a different location
                                itemTotalQty -= quantity;
                            else
                                break;
                        }
                    }

                    var statusQuery = unitOfWork.StatusRepository.GetByID(Convert.ToInt32(Request["statusid"].ToString()));
                    
                    if (statusQuery.description.ToUpper() == "SHIPPED")
                    {
                        order.shippingdate = DateTime.Now;
                    }

                    db.SaveChanges();

                    if (statusQuery.description.ToUpper() == "SHIPPED")
                    {
                        if (Request["hdnInventory"] != null)
                        {
                            string tmp = Request["hdnInventory"].ToString();
                            string[] aInvs = tmp.Split(',');
                            foreach (string inv in aInvs)
                            {
                                string[] aInv = inv.Split(':');
                                inventoryID = int.Parse(aInv[0]);
                                itemTotalQty = int.Parse(aInv[1]);
                                foreach (InventoryDetail id in db.InventoryDetails.Where(id => id.inventoryid == inventoryID))
                                {
                                    int quantity = (int)id.boxqty * (int)id.itemperbox + (int)id.itemqty;
                                    if (quantity < itemTotalQty)//Items are stored in multiple locations
                                    {
                                        itemQtyLocation = quantity;//Partial quantity in a location
                                    }
                                    else
                                    {
                                        itemQtyLocation = itemTotalQty;//The entire quantity
                                    }

                                    db.Entry(id).State = EntityState.Modified;
                                    int boxQtyToReduce = (int)itemQtyLocation / (int)id.itemperbox;
                                    id.boxqty -= boxQtyToReduce;

                                    int partialQtytoReduce = (int)itemQtyLocation % (int)id.itemperbox;
                                    if (partialQtytoReduce > (int)id.itemqty)//Open box case
                                    {
                                        id.itemqty = (int)id.itemperbox + (int)id.itemqty - partialQtytoReduce;
                                        id.boxqty--;//An unit box was opened
                                    }
                                    else
                                    {
                                        id.itemqty -= partialQtytoReduce;
                                    }
                                    id.capacity = id.capacity * (quantity - itemQtyLocation) / quantity;



                                    //boxqty = (int)Math.Ceiling((decimal)(itemQtyLocation) / (int)(id.itemperbox));
                                    if (quantity < itemTotalQty)//More items to ship so get it from a different location
                                        itemTotalQty -= quantity;
                                    else
                                        break;
                                }

                                //foreach (InventoryDetail id in inventoryDetails)
                                //{

                                //}
                            }
                        }
                    }

                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return View(order);
        }

        // GET: /Course/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    Item item = db.Items.Find(sku);
        //    if (item == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(item);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Store, OrderDetails.Inventory")]Order order)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            //order.OrderDetails = null;
        //            string tmp = Request["hdnInventory"].ToString();
        //            string[] aInvs = tmp.Split(',');
        //            //foreach (string inv in aInvs)
        //            //{
        //            //    string[] aInv = inv.Split(':');
        //            //    OrderDetail od = new OrderDetail();
        //            //    od.inventoryid = int.Parse(aInv[0]);
        //            //    od.qty = int.Parse(aInv[1]);
        //            //    //od.qty = 3;
        //            //    order.OrderDetails.Add(od);

        //            //}

        //            unitOfWork.OrderRepository.Update(order);
        //            unitOfWork.Save();
        //            return RedirectToAction("Index");
        //        }
        //    }
        //    catch (RetryLimitExceededException /* dex */)
        //    {
        //        //Log the error (uncomment dex variable name and add a line here to write a log.)
        //        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
        //    }

        //    return View(order);
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "sku,name")]Item item)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            db.Entry(item).State = EntityState.Modified;
        //            db.SaveChanges();
        //            return RedirectToAction("Index");
        //        }
        //    }
        //    catch (RetryLimitExceededException /* dex */)
        //    {
        //        //Log the error (uncomment dex variable name and add a line here to write a log.)
        //        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
        //    }

        //    return View(item);
        //}


    }
}
