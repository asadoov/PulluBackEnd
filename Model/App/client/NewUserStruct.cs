using System;
using System.Collections.Generic;

namespace PulluBackEnd.Model.App.client
{
    public class NewUserStruct
    {
      public string mail { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string pass { get; set; }
        public string bDate { get; set; }
        public int gender { get; set; }
        public int country { get; set; }
        public int city { get; set; }
        public List<int> interestIds { get; set; }
        public Int64 phone { get; set; }
        public int otp { get; set; }
    }
}
