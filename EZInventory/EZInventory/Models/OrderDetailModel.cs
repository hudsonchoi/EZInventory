using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EZInventory.Models
{
    public class OrderDetailModel
    {
        public int ID { get; set; }
        public string CompanyName { get; set; }
        public string Items { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}