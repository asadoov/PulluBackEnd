using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulluBackEnd.Model.Database.Payment
{
    public class TransactionStruct
    {
        public string bundleID { get; set; }
        public int userID { get; set; }
        public double amount { get; set; }
        public string transactionID { get; set; }
        public DateTime transactionDate { get; set; }
      
       
    }
}
