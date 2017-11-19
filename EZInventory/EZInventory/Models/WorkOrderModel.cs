using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EZInventory.Model;
using System.ComponentModel.DataAnnotations;

namespace EZInventory.Models
{
    public class WorkOrderModel
    {
        public int OrderID { get; set; }
        
        public string BillTo { get; set; }
        public string BillToAddress { get; set; }
        public string BillToCity { get; set; }
        public string BillToState { get; set; }
        public string BillToZip { get; set; }

        public string ShipTo { get; set; }
        public string ShipToAddress { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToState { get; set; }
        public string ShipToZip { get; set; }

        public List<LocationViewModel> LocationViewModels { get; set; }
        public int TotalCarton { get; set; }
        public int TotalQty { get; set; }

        public decimal Rate { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime ShipDate { get; set; }
    }
}