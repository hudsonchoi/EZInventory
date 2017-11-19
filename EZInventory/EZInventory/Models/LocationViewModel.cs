using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EZInventory.Model;

namespace EZInventory.Models
{
    public class LocationViewModel
    {
        public string ID { get; set; }
        public int itemQty { get; set; }
        public int boxQty { get; set; }
        public string Company { get; set; }
        public string untCarton { get; set; }
        public string parCarton { get; set; }
        public Inventory InventoryView { get; set; }
        public string description { get; set; }
        public int capacity { get; set; }
        public string inventories { get; set; }
        public List<InventoryDetail> inventoryDetails { get; set; }
    }
}