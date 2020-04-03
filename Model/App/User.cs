using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulluBackEnd.Model.App
{
    public class User
    {
        public int ID { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string mail { get; set; }
        public string phone { get; set; }
        public DateTime birthDate { get; set; }

        public string gender { get; set; }
        public string city { get; set; }
        public string profession { get; set; }
        public string balance { get; set; }
        public string earning { get; set; }

        public DateTime regDate { get; set; }
        
      
    }
}
