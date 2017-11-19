using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EZInventory.Model;
using System.ComponentModel.DataAnnotations;

namespace EZInventory.Models
{
    public class PackageModel
    {
        public int OrderID { get; set; }
        public string BillTo { get; set; }
        public string ShipTo { get; set; }
        public List<LocationViewModel> LocationViewModels { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime ShipDate { get; set; }
    }
}