//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EZInventory.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Company
    {
        public Company()
        {
            this.CompanyOrders = new HashSet<CompanyOrder>();
            this.Items = new HashSet<Item>();
            this.Inventories = new HashSet<Inventory>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public Nullable<bool> billto { get; set; }
        public Nullable<bool> shipto { get; set; }
        public Nullable<int> billtocid { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public Nullable<bool> deleted { get; set; }
    
        public virtual ICollection<CompanyOrder> CompanyOrders { get; set; }
        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<Inventory> Inventories { get; set; }
    }
}
