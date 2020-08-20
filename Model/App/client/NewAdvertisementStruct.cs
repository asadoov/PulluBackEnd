using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PulluBackEnd.Model.App.client
{
    public class NewAdvertisementStruct
    {
        public string userToken { get; set; }
        public string requestToken { get; set; }
        public int isPaid { get; set; }
        public int aTypeID { get; set; }
        public int aCategoryID { get; set; }
        public int aMediaTypeID { get; set; }
        public string aTitle { get; set; }
        public int aTrfID { get; set; }
        public string aDescription { get; set; }
        public string aPrice { get; set; }
        public int aCountryId { get; set; }
        public int aCityId { get; set; }
        public int aGenderID { get; set; }
        public int aAgeRangeID { get; set; }
        public int aProfessionID { get; set; }
        public string aBackgroundUrl { get; set; }
        public List<IFormFile> files { get; set; }
       // public List<string> mediaBase64 { get; set; }

    }
}
