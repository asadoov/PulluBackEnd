using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PulluBackEnd.Model.App;

namespace PulluBackEnd.Model.CommonScripts
{
    public class Communication
    {
        private readonly string ConnectionString;
        public IConfiguration Configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public Communication(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            ConnectionString = Configuration.GetSection("ConnectionStrings").GetSection("DefaultConnectionString").Value;
            _hostingEnvironment = hostingEnvironment;

        }
        public async Task sendMailAsync(string body, string to)
        {


            try
            {
                await Task.Run(() => sendMail(body, to));

            }
            catch (Exception ex)
            {

            }
        }
        public async Task sendSmsAsync(string smsText, int smsTel)
        {


            try
            {
                await Task.Run(() => sendSMS(smsText, smsTel));
            }
            catch (Exception ex)
            {

            }
        }
        public async Task sendNotificationAsync(string title,string body, long userID)
        {


            try
            {
                await Task.Run(() => sendNotification(title,body, userID));
            }
            catch (Exception ex)
            {

            }
        }


        public void sendNotification(string title,string body, long userID)
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


                string notifyJson = "{ \"title\" : \"" + title + "\",\"body\" : \"" + body + "\", \"userID\" : \"" + userID + "\", \"seen\" : false }";

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

        public async Task sendPushNotificationAsync(string title,string body,long userID=0)
        {
            if (!string.IsNullOrEmpty(title)&&!string.IsNullOrEmpty(body))
            {
                string appKey = "AAAA_D_IDDA:APA91bG5T8YbSwxyLo-q5lJ4BtAJxPexzKjFdZF3--56koZTVqWXR_C_raeGXOTXE9HgKAHmSECOh_sixFlZ65uxtJJfqkMulz3_GoXaU-AYLDT8noldworke8cKLRdXqRLx7tG2HXxr";
                try
                {


                    string pushNotifyJson = @"{
    ""to"": ""/topics/"",
    ""notification"": {
                    ""title"": ""/title/"",
      ""body"": ""/body/"",
      ""mutable_content"": true,
      ""sound"": ""Tri-tone""
      },

   ""data"": {
                    ""url"": ""<url of media image>"",
    ""dl"": ""<deeplink action on tap of notification>""
      }
            }";
                    switch (userID)
                    {
                        case 0:
                            pushNotifyJson = pushNotifyJson.Replace("/topics/", "/topics/admin");
                            break;
                        default:
                            pushNotifyJson = pushNotifyJson.Replace("/topics/", $"/topics/{userID.ToString()}");
                            break;
                    }

                    pushNotifyJson = pushNotifyJson.Replace("/title/", $"{title}");
                    pushNotifyJson = pushNotifyJson.Replace("/body/", $"{body.ToString()}");

                    var pushNotifyContent = new StringContent(pushNotifyJson, Encoding.UTF8, "application/json");
                    string pushNotifyUrl = $"https://fcm.googleapis.com/fcm/send";
                    HttpClient notifyClient = new HttpClient();
                    notifyClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={appKey}");
                    var notifyRslt = notifyClient.PostAsync(pushNotifyUrl, pushNotifyContent);
                    var notifyResp = notifyRslt.Result.RequestMessage;


                }
                catch
                {


                }

            }

        }



        public async Task sendMail(string body, string to)
        {
            try
            {
                MailMessage mailMsg = new MailMessage();
                using (SmtpClient SmtpServer = new SmtpClient("webmail.rabita.az"))
                {
                    mailMsg.IsBodyHtml = true;
                    mailMsg.From = new MailAddress("contact@pullu.az");
                    mailMsg.To.Add($"{to}");
                    mailMsg.Subject = "Pullu (Dəstək)";
                    mailMsg.Body = body;

                    SmtpServer.Port = 587;

                    SmtpServer.Credentials = new System.Net.NetworkCredential("contact@pullu.az", "88nf9uRf9b");
                    SmtpServer.EnableSsl = true;

                    await SmtpServer.SendMailAsync(mailMsg);
                    //SmtpServer.Dispose();
                }
            }
            catch { }

        }

        public void sendSMS(string smsText, int smsTel)
        {
            try
            {
                DateTime now = DateTime.Now;



                string smsXML = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
<soap12:Body>
<SendSms xmlns=""https://www.e-gov.az"">
<Authentication>
<RequestName>pullu</RequestName>
<RequestPassword>4l7E0yuLiquNrLp40bpr</RequestPassword>
<RequestSmsKey>!pulluRequestSms</RequestSmsKey>
</Authentication>
<Information>
<PhoneNumber>{smsTel}</PhoneNumber>
<Messages>{smsText}</Messages>
<SenderDate>{now.ToString("dd.MM.yyyy")}</SenderDate>
<SenderTime>{now.ToString("HH:mm:ss")}</SenderTime>
</Information>
</SendSms>
</soap12:Body>
</soap12:Envelope>";

                var smsContent = new StringContent(smsXML, Encoding.UTF8, "text/xml");
                //smsContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");

                string smsUrl = $"https://globalsms.rabita.az/ws/SmsWebServices.asmx";
                HttpClient smsClient = new HttpClient();


                var smsRslt = smsClient.PostAsync(smsUrl, smsContent).Result;
                // var smsResp =  smsRslt.Content.ReadAsStringAsync().Result;


            }
            catch
            {


            }

        }

        public async Task log(string log, string function_name, string ip)
        {

            DateTime now = DateTime.Now;

            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                using (MySqlCommand com = new MySqlCommand("Insert into api_log (ip_adress,log,function_name,cdate) values (@ipAdress,@log,@function_name,@cdate)", connection))
                {
                    com.Parameters.AddWithValue("@ipAdress", ip);
                    com.Parameters.AddWithValue("@log", log);
                    com.Parameters.AddWithValue("@function_name", function_name);
                    com.Parameters.AddWithValue("@cdate", now);
                   await com.ExecuteNonQueryAsync();
                    com.Dispose();

                }

                connection.Close();

            }


        }

    }
}
