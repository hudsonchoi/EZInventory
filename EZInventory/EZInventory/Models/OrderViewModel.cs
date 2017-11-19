using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EZInventory.Model;
using System.ComponentModel.DataAnnotations;

namespace EZInventory.Models
{
    public class OrderViewModel
    {
        public IEnumerable<Company> billtoid { get; set; }
        public IEnumerable<Company> shiptoid { get; set; }

    }
}