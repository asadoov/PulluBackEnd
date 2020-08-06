using System;
namespace PulluBackEnd.Model.Database.Payment
{
    public class WithdrawService
    {
        public long serviceID { get; set; }
        public long smartpayServiceID { get; set; }
        public string serviceName {get;set;}
        public long serviceCatID { get; set; }
        public string serviceCatName { get; set; }
        public string serviceImgURL { get; set; }

    }
}
