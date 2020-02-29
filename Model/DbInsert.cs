using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PulluBackEnd.Model
{
    public class DbInsert
    {
        private readonly string ConnectionString;
        public IConfiguration Configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public DbInsert(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            ConnectionString = Configuration.GetSection("ConnectionStrings").GetSection("DefaultConnectionString").Value;
            _hostingEnvironment = hostingEnvironment;

        }
        public bool IsValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);



                MySqlConnection connection = new MySqlConnection(ConnectionString);


                connection.Open();

                MySqlCommand com = new MySqlCommand("select * from user where email=@mail", connection);
                com.Parameters.AddWithValue("@mail", emailaddress);
                MySqlDataReader reader = com.ExecuteReader();

                bool except = reader.HasRows;
                connection.Close();
                if (except)
                {

                    return false;
                }

                return true;

            }
            catch (FormatException)
            {
                return false;
            }
        }
        public bool verify(int userID)
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);


            connection.Open();

            MySqlCommand com = new MySqlCommand("update user set isActive=1 where userID=@userID", connection);
            com.Parameters.AddWithValue("@userID", userID);
            int exist = com.ExecuteNonQuery();
            if (exist > 0)
            {
                return true;
            }

            return false;
        }
        public Status SignUp(string mail, string name, string surname, string pass, string phone, string bDate, string gender, string country, string city, string profession)

        {
            Status statusCode = new Status();

            if (IsValid(mail)) // Проверка существования юзера
            {


                try
                {


                    DateTime now = DateTime.Now;

                MySqlConnection connection = new MySqlConnection(ConnectionString);


                connection.Open();

                MySqlCommand com = new MySqlCommand("INSERT INTO user (name,surname,email,passwd,birthdate,genderId,isactive,isblocked,resetrequested,cdate, countryId,cityId,professionId)" +
                    " Values (@name,@surname,@mail,SHA2(@pass,512),@birthDate,(SELECT genderid FROM gender WHERE name = @gendername)" +
                    ",0,0,0,@dateTimeNow,(SELECT countryId FROM country WHERE name = @countryName)," +
                    "(SELECT cityId FROM city WHERE name = @cityName),(SELECT professionId FROM profession WHERE name = @professionName))", connection);

                
                com.Parameters.AddWithValue("@name", name);
                com.Parameters.AddWithValue("@surname", surname);

                com.Parameters.AddWithValue("@mail", mail);
                com.Parameters.AddWithValue("@pass", pass);
                com.Parameters.AddWithValue("@birthDate", DateTime.Parse(bDate));
                com.Parameters.AddWithValue("@genderName", gender);
                com.Parameters.AddWithValue("@dateTimeNow", now);
                com.Parameters.AddWithValue("@countryName", country);
                com.Parameters.AddWithValue("@cityName", city);
                com.Parameters.AddWithValue("@professionName", profession);
                com.ExecuteNonQuery();

                long lastId = com.LastInsertedId;
                connection.Close();

                connection.Open();
                com.CommandText = "insert into users_balance (userId,cdate) values (@userId,@cDate)";
                com.Parameters.AddWithValue("@userId", lastId);
                com.Parameters.AddWithValue("@cDate", now);


                com.ExecuteNonQuery();
                connection.Close();



               
                    MailMessage mailMsg = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                    mailMsg.IsBodyHtml = true;
                    mailMsg.From = new MailAddress("asadzade99@gmail.com");
                    mailMsg.To.Add($"{mail}");
                    mailMsg.Subject = "Pullu (Dəstək)";
                    mailMsg.Body = $"Qeydiyyatı tamamlamaq üçün, zəhmət olmasa <a href=\'https://pullu.az/api/androidmobileapp/verify?code={lastId}'>linkə</a> daxil olun";

                    SmtpServer.Port = 587;
                    SmtpServer.Credentials = new System.Net.NetworkCredential("asadzade99@gmail.com", "odqnmjiogipltmwi");
                    SmtpServer.EnableSsl = true;

                    SmtpServer.Send(mailMsg);
                    


                    string json = "{ \"email\" : \""+mail+ "\", \"password\" : \""+pass+"\", \" returnSecureToken\" : true }";

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    string url = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=AIzaSyCwEuju_UmuNNPrYtxEhsuddOfCzqZQ8nI";
                    HttpClient client = new HttpClient();
                    var rslt = client.PostAsync(url, content);
                    var resp = rslt.Result.RequestMessage;

                    statusCode.response = 0; // Все ок
                    return statusCode;
              

                }
                catch
                {
                    statusCode.response = 1;//Ошибка сервера
                    return statusCode;
                }

                //http://127.0.0.1:44301/api/androidmobileapp/user/signUp?mail=asadzade99@gmail.com&username=asa&name=asa&surname=asa&pass=1&phone=1&bdate=1987-08-23&gender=Ki%C5%9Fi&country=Az%C9%99rbaycan&city=Bak%C4%B1&profession=Texnologiya%20sektoru

            }
            else {

                statusCode.response = 2;//Юзер существует
                return statusCode;
            }
           

        }

        public void Deactivator()
        {



        }

        public void addView(int advertID, long userID)
        {
            DateTime now = DateTime.Now;

            MySqlConnection connection = new MySqlConnection(ConnectionString);
            connection.Open();
            MySqlCommand com = new MySqlCommand("Insert into announcement_view (announcementId,userId,cdate) values (@advertID,@userID,@dateTimeNow)", connection);
            com.Parameters.AddWithValue("@advertID", advertID);
            com.Parameters.AddWithValue("@userID", userID);

            com.Parameters.AddWithValue("@dateTimeNow", now);
            com.ExecuteNonQuery();
            connection.Close();
        }
        public bool AddDailyView(int advertID, long userID)
        {
            //+1 просмотр к платной рекламе при каждом вызове метода


            try
            {

                DateTime now = DateTime.Now;
                DateTime viewDate = now;
                MySqlConnection connection = new MySqlConnection(ConnectionString);

                connection.Open();

                MySqlCommand com = new MySqlCommand("select announcementId from announcement where announcementId=@advertID and isPaid=1", connection);
                com.Parameters.AddWithValue("@advertID", advertID);
                MySqlDataReader reader = com.ExecuteReader();

                if (reader.HasRows)
                {
                    connection.Close();

                    connection.Open();
                    com.CommandText = "select cdate from announcement_view where userId=@userID and announcementID=@advertID order by cdate desc limit 1";
                    com.Parameters.AddWithValue("@userID", userID);

                    reader = com.ExecuteReader();
                    if (reader.HasRows)
                    {


                        while (reader.Read())
                        {
                            viewDate = DateTime.Parse(reader["cdate"].ToString());




                        }
                    }


                    if ((now - viewDate).TotalDays > 1 || reader.HasRows == false)
                    {
                        connection.Close();
                        addView(advertID, userID);
                        return true;
                    }


                    return false;
                }
                else
                {
                    return false;
                }

            }
            catch
            {

                return false;
            }

        }



        public EarnMoney EarnMoney(int adverID, string mail, string pass)

        {
            dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
            long userID = select.getUserID(mail, pass);

            EarnMoney status = new EarnMoney();


            try
            {
                if (AddDailyView(adverID, userID))
                {


                    DateTime now = DateTime.Now;

                    MySqlConnection connection = new MySqlConnection(ConnectionString);





                    connection.Open();

                    MySqlCommand com = new MySqlCommand("update users_balance set earningValue = earningValue + (select  price from earnings_tariff where earningstpid = (select atypeID from announcement where announcementId=@advertID) ), udate=now() where userID=@userID", connection);
                    com.Parameters.AddWithValue("@advertID", adverID);
                    com.Parameters.AddWithValue("@userID", userID);
                    com.Parameters.AddWithValue("@dateTimeNow", now);
                    //com.ExecuteNonQuery();

                    int updated = com.ExecuteNonQuery();
                    //update users_balance set earningValue = earningValue + (select  price from earnings_tariff where earningstpid = (select atypeID from announcement where announcementId=@advertID) ), udate=now() where userId=@userID and udate<DATE_FORMAT(now(), '%Y-%m-%d')


                    status.statusCode = 1;
                    return status;


                }
                else
                {
                    status.statusCode = 2;
                    return status;
                }
            }
            catch
            {
                status.statusCode = 3;
                return status;
            }




        }


        public bool sendNotification(string mail,string pass)
        {
            try
            {
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
                
                
                string notifyJson = "{ \"title\" : \"Salam\", \"userID\" : \"1213\", \"seen\" : false }";
               
                var notifyContent = new StringContent(notifyJson, Encoding.UTF8, "application/json");
                string notifyUrl = $"https://pullu-2e3bb.firebaseio.com/users/{deserializedUser.localId}/notifications.json?auth={deserializedUser.idToken}";
                HttpClient notifyClient = new HttpClient();
                var notifyRslt = notifyClient.PostAsync(notifyUrl, notifyContent);
                var notifyResp = notifyRslt.Result.RequestMessage;

                return true;
            }
            catch
            {

                return false;
            }

        }




        //public NewAdvertisementStatus newAdvertisement(NewAdvertisementStruct obj)

        //{
        //    dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
        //    long userID = select.getUserID(obj.mail, obj.pass);

        //    EarnMoney status = new EarnMoney();


        //    try
        //    {
        //        if (userID>0)
        //        {


        //            DateTime now = DateTime.Now;

        //            MySqlConnection connection = new MySqlConnection(ConnectionString);





        //            connection.Open();

        //            MySqlCommand com = new MySqlCommand("update users_balance set earningValue = earningValue + (select  price from earnings_tariff where earningstpid = (select atypeID from announcement where announcementId=@advertID) ), udate=now() where userID=@userID", connection);
        //            com.Parameters.AddWithValue("@advertID", adverID);
        //            com.Parameters.AddWithValue("@userID", userID);
        //            com.Parameters.AddWithValue("@dateTimeNow", now);
        //            //com.ExecuteNonQuery();

        //            int updated = com.ExecuteNonQuery();
        //            //update users_balance set earningValue = earningValue + (select  price from earnings_tariff where earningstpid = (select atypeID from announcement where announcementId=@advertID) ), udate=now() where userId=@userID and udate<DATE_FORMAT(now(), '%Y-%m-%d')


        //            status.statusCode = 1;
        //            return status;


        //        }
        //        else
        //        {
        //            status.statusCode = 2;
        //            return status;
        //        }
        //    }
        //    catch
        //    {
        //        status.statusCode = 3;
        //        return status;
        //    }




        //}


    }
}
