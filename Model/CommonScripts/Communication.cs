using System;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
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

        public async void sendSMS(string text, int tel)
        {
            try
            {
               


                string smsXML = @"<?xml version='1.0' encoding='utf-8'?>
  <soap12:Envelope xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd = 'http://www.w3.org/2001/XMLSchema' xmlns:soap12 = 'http://www.w3.org/2003/05/soap-envelope' >
         <soap12:Body>
          <SendSms xmlns = 'https://www.e-gov.az'
           <Authentication>
           <RequestName> pullu </RequestName>
           <RequestPassword>4l7E0yuLiquNrLp40bpr</RequestPassword>
              <RequestSmsKey>!pulluRequestSms</RequestSmsKey>
              </Authentication>
              <Information>
              <PhoneNumber> 552136623 </PhoneNumber>
              <Messages>#evde#qal#smssiz#qalma</Messages>
<SenderDate> 27.04.2020 </SenderDate>
   <SenderTime> 12:00:00 </SenderTime>
      </Information>
      </SendSms>
      </soap12:Body>
       </soap12:Envelope>";

                var smsContent = new StringContent(smsXML, Encoding.UTF8, "text/xml");
                string smsUrl = $"https://globalsms.rabita.az/ws/SmsWebServices.asmx";
                HttpClient notifyClient = new HttpClient();
                var smsRslt = notifyClient.PostAsync(smsUrl, smsContent);
                var smsResp = smsRslt.Result.RequestMessage;


            }
            catch
            {


            }

        }

        public async void log(string log, string function_name, string ip)
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
                    com.ExecuteNonQuery();
                    com.Dispose();

                }

                connection.Close();

            }


        }

    }
}
