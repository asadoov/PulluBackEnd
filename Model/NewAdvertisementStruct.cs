using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulluBackEnd.Model
{
    public class NewAdvertisementStruct
    {
        public string mail { get; set; }
        public string pass { get; set; }
        public int isPaid { get; set; }
        public int aTypeID { get; set; }
        public int aCategoryID { get; set; }
        public string title { get; set; }
        public int trfID { get; set; }
        public string description { get; set; }
        public double price { get; set; }
        public int countryId { get; set; }
        public int cityId { get; set; }
        public int ageRangeID { get; set; }
        public int professionID { get; set; }
        public List<string> mediaBase64 { get; set; }

    }
}
