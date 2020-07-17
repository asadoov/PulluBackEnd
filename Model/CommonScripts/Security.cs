using System;
using System.Text;

namespace PulluBackEnd.Model.CommonScripts
{
    public class Security
    {
       public string sha256(string randomString="")
        {
            string hashValue = "";
            if (!string.IsNullOrEmpty(randomString))
            {
                var crypt = new System.Security.Cryptography.SHA256Managed();
                var hash = new System.Text.StringBuilder();
                byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
                foreach (byte theByte in crypto)
                {
                    hash.Append(theByte.ToString("x2"));
                }
                hashValue = hash.ToString();
            }

            return hashValue;

        }
    }
}
