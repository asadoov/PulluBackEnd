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
using PulluBackEnd.Model.Database.App;
using Microsoft.AspNetCore.Http;
using PulluBackEnd.Model.Admin;
using PulluBackEnd.Model.CommonScripts;

namespace PulluBackEnd.Model.Database.Admin

{
    public class DbInsert
    {
        Communication communication; 

        private readonly string ConnectionString;
        public IConfiguration Configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public DbInsert(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            ConnectionString = Configuration.GetSection("ConnectionStrings").GetSection("DefaultConnectionString").Value;
            _hostingEnvironment = hostingEnvironment;
            communication =new Communication(Configuration,_hostingEnvironment);

        }
        static string sha256(string randomString)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        public string createCode(int length)
        {
            // const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            const string valid = "1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }



        public bool IsValid(string emailaddress)
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            try
            {
                MailAddress m = new MailAddress(emailaddress);






                connection.Open();

                MySqlCommand com = new MySqlCommand("select * from user where email=@mail", connection);
                com.Parameters.AddWithValue("@mail", emailaddress);
                MySqlDataReader reader = com.ExecuteReader();

                bool except = reader.HasRows;
                connection.Close();
                if (except)
                {

                    return true;
                }

                return false;

            }
            catch (FormatException)
            {
                connection.Close();
                return false;
            }
        }
        public Status sendResetMail(string mail)
        {
            Status statusCode = new Status();
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            if (IsValid(mail))
            {


                try
                {

                    string randomCode = createCode(4);




                    connection.Open();

                    MySqlCommand com = new MySqlCommand("update user set userToken=SHA2(@newPass,512) where email=@mail", connection);


                    com.Parameters.AddWithValue("@newPass", randomCode);
                    com.Parameters.AddWithValue("@mail", mail);


                    int affectedRows = com.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {

                        communication.sendMail($"Bizə şifrənizin bərpası barədə müraciət daxil olub, əgər bu doğrunu əks etdirirsə aşağıdakı 4 rəqəmli şifrəni programa daxil edin<br><h2>ŞİFRƏ: {randomCode}</h2>", mail);
                        //string json = "{ \"email\" : \"" + mail + "\", \"password\" : \"" + randomPass + "\", \" returnSecureToken\" : true }";

                        //var content = new StringContent(json, Encoding.UTF8, "application/json");
                        //string url = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=AIzaSyCwEuju_UmuNNPrYtxEhsuddOfCzqZQ8nI";
                        //HttpClient client = new HttpClient();
                        //var rslt = client.PostAsync(url, content);
                        //var resp = rslt.Result.RequestMessage;
                        statusCode.response = 0; // Все ок
                    }
                    else
                    {
                        statusCode.response = 1;// ошибка сервера
                    }



                }
                catch
                {
                    statusCode.response = 1;// ошибка сервера

                }


            }
            else
            {
                statusCode.response = 2;// нет такого мейла
            }
            connection.Close();
            return statusCode;




        }

        public List<Status> activateAds(string uname, string pass, int aID, int isActive)
        {
            List<Status> statusList = new List<Status>();
            Status statusCode = new Status();

            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            string userMail = "", aInfo = "";
            long userID = 0;

            if (select.checkAdmin(uname, pass))
            {
                int affectedRows = 0;

                try
                {
                    using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();

                        using (MySqlCommand com = new MySqlCommand("update announcement set isActive=@isActive where announcementID=@aID", connection))
                        {
                            com.Parameters.AddWithValue("@isActive", isActive);
                            com.Parameters.AddWithValue("@aID", aID);


                            affectedRows = com.ExecuteNonQuery();

                            com.Dispose();
                        }
                        if (affectedRows > 0)
                        {
                            using (MySqlCommand com = new MySqlCommand("select name,userID,(select email from user where userID=a.userID)as userMail from announcement a where announcementID=@aID and isActive=@isActive", connection))
                            {
                                com.Parameters.AddWithValue("@isActive", isActive);
                                com.Parameters.AddWithValue("@aID", aID);

                                using (MySqlDataReader reader = com.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {

                                            userMail = reader["userMail"].ToString();
                                            aInfo = reader["name"].ToString();
                                            userID = Convert.ToInt32(reader["userID"]);
                                        }

                                    }

                                }


                                com.Dispose();
                                if (isActive==1)
                                {
                                    communication.sendMail($"{aInfo} başlıqlı reklamınız aktiv edildi", userMail);
                                    communication.sendNotification($"{aInfo} başlıqlı aktiv edildi", userID);
                                }
                                else
                                {
                                    communication.sendMail($"{aInfo} başlıqlı reklamınız deaktiv edildi", userMail);
                                    communication.sendNotification($"{aInfo} başlıqlı deaktiv edildi", userID);

                                }
                                
                                statusCode.response = 0; // Все ок
                            }







                            //string json = "{ \"email\" : \"" + mail + "\", \"password\" : \"" + randomPass + "\", \" returnSecureToken\" : true }";

                            //var content = new StringContent(json, Encoding.UTF8, "application/json");
                            //string url = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=AIzaSyCwEuju_UmuNNPrYtxEhsuddOfCzqZQ8nI";
                            //HttpClient client = new HttpClient();
                            //var rslt = client.PostAsync(url, content);
                            //var resp = rslt.Result.RequestMessage;

                        }
                        else
                        {
                            statusCode.response = 3;// ничего не обновилось
                        }




                        connection.Close();
                    }

                }
                catch
                {
                    statusCode.response = 1;// ошибка сервера

                }


            }
            else
            {
                statusCode.response = 2;// нет такого пользователя
            }
            statusList.Add(statusCode);
            return statusList;




        }

        public bool verify(int code)
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);


            connection.Open();

            MySqlCommand com = new MySqlCommand("update user set isActive=1 where userToken=SHA2(@userToken,512)", connection);
            com.Parameters.AddWithValue("@userToken", code);
            int exist = com.ExecuteNonQuery();
            if (exist > 0)
            {
                return true;
            }

            return false;
        }
        public Status resetPassword(string newPassword, string mail, string code)
        {
            Status status = new Status();
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            try
            {





                connection.Open();

                MySqlCommand com = new MySqlCommand("update user set passwd=SHA2(@newPassword,512),userToken=null where userToken=SHA2(@userToken,512) and email=@email", connection);
                com.Parameters.AddWithValue("@userToken", code);
                com.Parameters.AddWithValue("@newPassword", newPassword);
                com.Parameters.AddWithValue("@email", mail);
                int exist = com.ExecuteNonQuery();
                if (exist > 0)
                {
                    status.response = 0;

                }
                else
                {
                    status.response = 1;//password not changed
                }
                connection.Close();
                return status;
            }
            catch
            {
                connection.Close();

                status.response = 2;//server error
                return status;
            }
        }






        public void AdsDeactivator()
        {



        }
        public string GetFileExtension(string base64String)
        {
            var data = base64String.Substring(0, 5);

            switch (data.ToUpper())
            {
                case "IVBOR":
                    return "png";
                case "/9J/4":
                    return "jpg";
                case "AAAAF":
                    return "mp4";
                case "JVBER":
                    return "pdf";
                case "AAABA":
                    return "ico";
                case "UMFYI":
                    return "rar";
                case "E1XYD":
                    return "rtf";
                case "U1PKC":
                    return "txt";
                case "MQOWM":
                case "77U/M":
                    return "srt";
                default:
                    return string.Empty;
            }
        }
        public string SaveImage(IFormFile ImgStr, string ImgName)
        {
            try
            {

                String path = $"{_hostingEnvironment.ContentRootPath}/wwwroot/media/";


                //Check if directory exist
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
                }

                //string imageName = $"{ImgName}.{type}";

                //set the image path
                //string imgPath = Path.Combine(path, );
                using (Stream fileStream = File.Create($"{path}{ImgName}{Path.GetExtension(ImgStr.FileName)}"))
                {
                    ImgStr.CopyTo(fileStream);
                }


                return $"{ImgName}{Path.GetExtension(ImgStr.FileName)}";


            }
            catch
            {
                return "";
            }

        }














    }
}
