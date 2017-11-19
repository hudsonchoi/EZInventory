using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EZInventory.Models
{
    public class LoginModel
    {
        public string email { get; set; }
        public string password { get; set; }
        public string returnUrl { get; set; }
        public int companyID { get; set; }
    }
}