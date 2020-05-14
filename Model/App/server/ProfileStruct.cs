using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulluBackEnd.Model.App.server
{
    public class ProfileStruct
    {
        public string name { get; set; }
        public string surname { get; set; }
        public string mail { get; set; }
        public string phone { get; set; }
        public DateTime bDate { get; set; }
        public string gender { get; set; }
        public string country { get; set; }
        public int countryID { get; set; }
        public string city { get; set; }
        public int cityID { get; set; }
        public string profession { get; set; }
        public int professionID { get; set; }
        public DateTime cDate { get; set; }
        
    }
}
