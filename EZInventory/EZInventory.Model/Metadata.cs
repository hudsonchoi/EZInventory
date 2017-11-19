using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EZInventory.Model
{
    [MetadataType(typeof(Item.MetaData))]
    public partial class Item
    {
        private sealed class MetaData
        {
            [Required(ErrorMessage = "Required")]
            public string sku { get; set; }

            [Required(ErrorMessage = "Required")]
            public string barcode { get; set; }

            [Required(ErrorMessage = "Required")]
            public string name { get; set; }

            [Required(ErrorMessage = "Required")]
            public int companyid { get; set; }

        }
    }

    [MetadataType(typeof(Inventory.MetaData))]
    public partial class Inventory
    {
        private sealed class MetaData
        {
            [Required(ErrorMessage = "Required")]
            public int companyid { get; set; }

            [Required(ErrorMessage = "Required")]
            public string sku { get; set; }

        }
    }

    [MetadataType(typeof(Order.MetatData))]
    public partial class Order
    {
        private sealed class MetatData
        {
            [Required(ErrorMessage = "Required")]
            public decimal rate { get; set; }
        }
    }

    [MetadataType(typeof(Location.MetaData))]
    public partial class Location
    {
        private sealed class MetaData
        {
            [Required(ErrorMessage = "Required")]
            public string id { get; set; }
        }
    }

    [MetadataType(typeof(Company.MetaData))]
    public partial class Company
    {
        private sealed class MetaData
        {
            [Required(ErrorMessage = "Required")]
            public string name { get; set; }

            [Required(ErrorMessage = "Required")]
            public string address { get; set; }

            [Required(ErrorMessage = "Required")]
            public string city { get; set; }

            [Required(ErrorMessage = "Required")]
            [StringLength(2, ErrorMessage="Up to 2 chars")]
            public string state { get; set; }

            [Required(ErrorMessage = "Required")]
            [StringLength(10, ErrorMessage = "Up to 10 chars")]
            public string zip { get; set; }

            [Required(ErrorMessage = "Required")]
            public string email { get; set; }

            [Required(ErrorMessage = "Required")]
            public string password { get; set; }

        }
    }
}
