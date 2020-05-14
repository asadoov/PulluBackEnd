using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulluBackEnd.Model.App.server
{
    public class TariffStruct
    {
        public int ID { get; set; }
        public string measure { get; set; }
        public double price { get; set; }
        public int viewCount { get; set; }
    }
}
