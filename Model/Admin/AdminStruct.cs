using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulluBackEnd.Model.Admin
{
    public class AdminStruct
    {
        public int ID { get; set; }
        public int managerTpID { get; set; }
        public string fullName { get; set; }
        public string mobile { get; set; }
        public DateTime cDate { get; set; } 
    }
}
