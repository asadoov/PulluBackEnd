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
        public string birthDate { get; set; }
        public int genderID { get; set; }
        public int cityID { get; set; }
        public int countryID { get; set; }
        public int professionID { get; set; }
        public string balance { get; set; }
        public string earning { get; set; }
        public string pass { get; set; }
        public string newPass { get; set; }
        public DateTime regDate { get; set; }
        
      
    }
}
