using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulluBackEnd.Model
{
    public class FirebaseUser
    {
        public String kind { get; set; }
        public String localId { get; set; }
        public String email { get; set; }
        public String displayName { get; set; }
        public String idToken { get; set; }
        public Boolean registered { get; set; }
        public string refreshToken { get; set; }
        public int expiresIn { get; set; }


    }
}
