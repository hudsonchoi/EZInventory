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
    
    public partial class CompanyOrder
    {
        public int id { get; set; }
        public byte type { get; set; }
        public int orderid { get; set; }
        public int companyid { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual Order Order { get; set; }
    }
}
