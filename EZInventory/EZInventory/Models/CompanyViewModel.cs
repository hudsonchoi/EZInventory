using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EZInventory.Model;

namespace EZInventory.Models
{
    public class CompanyViewModel
    {
        public int ID { get; set; }
        public string CompanyName { get; set; }
        public bool Billing { get; set; }
        public bool Shipping { get; set; }
        public string BillToCompany { get; set; }
        public string City { get; set; }
        public string State { get; set; }

    }
}