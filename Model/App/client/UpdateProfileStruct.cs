using System;
namespace PulluBackEnd.Model.App.client
{
    public class UpdateProfileStruct
    {

        public string userToken { get; set; }
        public string requestToken { get; set; }

        public  string name { get; set; }
        public string surname { get; set; }
        
        public string newPass { get; set; }
       
      
        public string bDate { get; set; }
        public int genderID { get; set; }
        public int countryID { get; set; }
        public int cityID { get; set; }
        public int professionID { get; set; }
      
    }
}
