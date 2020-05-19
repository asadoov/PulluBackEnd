using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulluBackEnd.Model.App.server
{
    public class Advertisement
    {
        public int id { get; set; }
        public string name { get; set; }
        public int userID { get; set; }
        public string sellerFullName { get; set; }
        public string sellerPhone { get; set; }
        public string description { get; set; }
        public string price { get; set; }
        public int aTypeId { get; set; }
        public string aTypeName { get; set; }
        public int isPaid { get; set; }

        public int mediaTpId { get; set; }
        public int catId { get; set; }
        public string catName { get; set; }
        public DateTime cDate { get; set; }
        public List<string> photoUrl { get; set; }
        public int views { get; set; }

    }
}
