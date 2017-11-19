using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EZInventory.Models
{
    public class OrderListModel
    {
        public int ID { get; set; }
        public string BillTo { get; set; }
        public string ShipTo { get; set; }
        public string StoreName { get; set; }
        public string Items { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public int TotalQuantity { get; set; }

    }
}