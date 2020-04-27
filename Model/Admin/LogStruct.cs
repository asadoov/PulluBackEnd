using System;
namespace PulluBackEnd.Model.Admin
{
    public class LogStruct
    {
     
            public int ID { get; set; }
        public string ipAdress { get; set; }
        public string log { get; set; }
        public string functionName { get; set; }
        public DateTime cdate { get; set; }
        
    }
}
