using System;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using Newtonsoft.Json;
using PulluBackEnd.Model.App;

namespace PulluBackEnd.Model.CommonScripts
{
    public class Communication
    {
        public async void sendNotification(string text, long userID)
        {
            try
            {
                string mail = "asadzade99@gmail.com";
                string pass = "123456";
                //string json = "{ \"method\" : \"guru.test\", \"params\" : [ \"Guru\" ], \"id\" : 123 }";
                //string json = "{ \"email\" : \"" + mail + "\", \"password\" : \"" + pass + "\", \" returnSecureToken\" :\"true\"}";
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    email = mail,
                    password = pass,
                    returnSecureToken = true
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                string url = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key=AIzaSyCwEuju_UmuNNPrYtxEhsuddOfCzqZQ8nI";
                HttpClient client = new HttpClient();
                var rslt = client.PostAsync(url, content);
                var resp = rslt.Result.Content.ReadAsStringAsync().Result;



                FirebaseUser deserializedUser = JsonConvert.DeserializeObject<FirebaseUser>(resp);


                string notifyJson = "{ \"title\" : \"" + text + "\", \"userID\" : \"" + userID + "\", \"seen\" : false }";

                var notifyContent = new StringContent(notifyJson, Encoding.UTF8, "application/json");
                string notifyUrl = $"https://pullu-2e3bb.firebaseio.com/users/{deserializedUser.localId}/notifications/{userID}.json?auth={deserializedUser.idToken}";
                HttpClient notifyClient = new HttpClient();
                var notifyRslt = notifyClient.PostAsync(notifyUrl, notifyContent);
                var notifyResp = notifyRslt.Result.RequestMessage;

                
            }
            catch
            {

               
            }

        }
        public async void sendMail(string body, string to)
        {
            MailMessage mailMsg = new MailMessage();
            using (SmtpClient SmtpServer = new SmtpClient("mail.pesekar.az"))
            {
                mailMsg.IsBodyHtml = true;
                mailMsg.From = new MailAddress("pullu@pesekar.az");
                mailMsg.To.Add($"{to}");
                mailMsg.Subject = "Pullu (Dəstək)";
                mailMsg.Body = body;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("pullu@pesekar.az", "pesekarpullu123");
                // SmtpServer.EnableSsl = true;

                SmtpServer.Send(mailMsg);
                SmtpServer.Dispose();
            }

        }

    }
}
