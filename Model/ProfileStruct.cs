using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulluBackEnd.Model
{
    public class ProfileStruct
    {
        public string name { get; set; }
        public string surname { get; set; }
        public string mail { get; set; }
        public string phone { get; set; }
        public DateTime bDate { get; set; }
        public string gender { get; set; }
        public string city { get; set; }
        public string profession { get; set; }
        public DateTime cDate { get; set; }
        public int response { get; set; }
    }
}
