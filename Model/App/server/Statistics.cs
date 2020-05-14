using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulluBackEnd.Model.App.server
{
    public class Statistics
    {
        public int allUsers { get; set; }
        public int allUsersToday { get; set; }
        public int allAds { get; set; }
        public int myTodayViews { get; set; }
        public int allMyViews { get; set; }
        public int myPaidViews { get; set; }
        public int myNotPaidViews { get; set; }
        public int myAds { get; set; }
        public int myNotPaidAds { get; set; }
        public int myPaidAds { get; set; }


    }
}
