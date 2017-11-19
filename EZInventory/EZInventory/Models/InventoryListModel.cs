using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EZInventory.Models
{
    public class InventoryListModel
    {
        public int ID { get; set; }
        public string sku { get; set; }
        public string barcode { get; set; }
        public string company { get; set; }
        public DateTime stockingDate { get; set; }
        public int totalQty { get; set; }
        public string location { get; set; }

    }
}