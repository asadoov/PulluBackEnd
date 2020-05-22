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
using PulluBackEnd.Model.App.client;
using PulluBackEnd.Model.App.server;
using Microsoft.AspNetCore.Http;
using PulluBackEnd.Model.CommonScripts;

namespace PulluBackEnd.Model.App
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
            communication = new Communication(Configuration, _hostingEnvironment);

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
        public Status IsValidPhone(string phone)
        {

            Status status = new Status();
            if (!string.IsNullOrEmpty(phone))
            {


                try
                {
                    using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                    {


                        connection.Open();

                        using (MySqlCommand com = new MySqlCommand("select * from user where mobile=@phone", connection))
                        {


                            com.Parameters.AddWithValue("@phone", phone);

                            MySqlDataReader reader = com.ExecuteReader();

                            if (reader.HasRows)
                            {
                                status.response = 0;
                                status.responseString = "user found";
                            }
                            else
                            {
                                status.response = 2;
                                status.responseString = "user not found";

                            }
                            com.Dispose();

                        }
                        connection.Close();
                    }

                }
                catch (Exception ex)
                {
                    status.response = 1;
                    status.responseString = $"Server error. Exception message: {ex.Message}";

                }
            }
            return status;
        }
        public  Status sendResetMail(string mail)
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
        public Status sendResetSMS(string phone)
        {
            Status status = new Status();
            if (!string.IsNullOrEmpty(phone))
            {



                status = IsValidPhone(phone);
                if (status.response == 0)
                {


                    try
                    {

                        string randomCode = createCode(4);


                        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                        {



                            connection.Open();

                            using (MySqlCommand com = new MySqlCommand("update user set userToken=SHA2(@token,512) where mobile=@phone", connection))
                            {
                                com.Parameters.AddWithValue("@token", randomCode);
                                com.Parameters.AddWithValue("@phone", phone);


                                int affectedRows = com.ExecuteNonQuery();

                                if (affectedRows > 0)
                                {

                                    communication.sendSMS($"Sizin shifreniz: {randomCode}", phone);

                                    //string json = "{ \"email\" : \"" + mail + "\", \"password\" : \"" + randomPass + "\", \" returnSecureToken\" : true }";

                                    //var content = new StringContent(json, Encoding.UTF8, "application/json");
                                    //string url = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=AIzaSyCwEuju_UmuNNPrYtxEhsuddOfCzqZQ8nI";
                                    //HttpClient client = new HttpClient();
                                    //var rslt = client.PostAsync(url, content);
                                    //var resp = rslt.Result.RequestMessage;
                                    status.response = 0; // Все ок
                                    status.responseString = "verification code sent via sms"; // Все ок
                                }
                                
                                com.Dispose();
                            }

                            connection.Close();

                        }



                    }
                    catch (Exception ex)
                    {
                        status.response = 1;// ошибка сервера
                        status.responseString = ex.Message;

                    }


                }


            }
            else
            {
                status.response = 3;
                status.responseString = "phone is incorrect";
            }
            return status;




        }
     
        

        public Status verifyMobile(string mail, string pass, string newPhone)
        {

            Status status = new Status();

            if (!string.IsNullOrEmpty(mail) && !string.IsNullOrEmpty(pass) && !string.IsNullOrEmpty(newPhone))
            {
              


                try
                {
                    status = IsValidPhone(newPhone);


                    if (status.response == 2)

                    {
                        string randomCode = createCode(4);
                        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                        {



                            connection.Open();

                            using (MySqlCommand com = new MySqlCommand("update user set userToken=SHA2(@token,512) where email=@mail and passwd=SHA2(@pass,512) ", connection))
                            {
                                com.Parameters.AddWithValue("@token", randomCode);
                                com.Parameters.AddWithValue("@mail", mail);
                                com.Parameters.AddWithValue("@pass", pass);

                                int affectedRows = com.ExecuteNonQuery();

                                if (affectedRows > 0)
                                {

                                    communication.sendSMS($"Sizin shifreniz: {randomCode}", newPhone);

                                    //string json = "{ \"email\" : \"" + mail + "\", \"password\" : \"" + randomPass + "\", \" returnSecureToken\" : true }";

                                    //var content = new StringContent(json, Encoding.UTF8, "application/json");
                                    //string url = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=AIzaSyCwEuju_UmuNNPrYtxEhsuddOfCzqZQ8nI";
                                    //HttpClient client = new HttpClient();
                                    //var rslt = client.PostAsync(url, content);
                                    //var resp = rslt.Result.RequestMessage;
                                    status.response = 0; // Все ок
                                    status.responseString = "verification code sent via sms"; // Все ок
                                }
                                else
                                {
                                    status.response = 2;// not affected
                                    status.responseString = "user not found";

                                }
                                com.Dispose();
                            }

                            connection.Close();

                        }

                    }
                    

                    



                }
                catch (Exception ex)
                {
                    status.response = 1;// ошибка сервера
                    status.responseString = ex.Message;

                }




            }
            else
            {
                status.response = 3;
                status.responseString = "params is incorrect";
            }
            return status;




        }

        public Status verify(int code)
        {
            Status status = new Status();
            if (code > 0)
            {


                try
                {


                    using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                    {




                        connection.Open();

                        using (MySqlCommand com = new MySqlCommand("update user set isActive=1,userToken=null where userToken=SHA2(@userToken,512)", connection))
                        {

                            com.Parameters.AddWithValue("@userToken", code);
                            int exist = com.ExecuteNonQuery();
                            if (exist > 0)
                            {
                                status.response = 0;
                                status.responseString = "user activated";
                            }
                            else
                            {
                                status.response = 2;
                                status.responseString = "code is wrong";

                            }

                        }
                        connection.Close();


                    }
                }
                catch (Exception ex)
                {
                    status.response = 1;
                    status.responseString = $"Server error. Exception Message: {ex.Message}";

                }
            }
            else
            {
                status.response = 4;
                status.responseString = "code is empty";
            }
            return status;
        }
        public Status resetPassword(string newPassword, string login, string code)
        {
            Status status = new Status();
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            try
            {





                connection.Open();

                MySqlCommand com = new MySqlCommand("update user set passwd=SHA2(@newPassword,512),userToken=null where userToken=SHA2(@userToken,512) and (email=@login or mobile = @login)", connection);
                com.Parameters.AddWithValue("@userToken", code);
                com.Parameters.AddWithValue("@newPassword", newPassword);
                com.Parameters.AddWithValue("@login", login);
                
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
        public Status SignUp(string mail, string name, string surname, string pass, string phone, string bDate, string gender, string country, string city, string profession)

        {
            Status statusCode = new Status();

            if (!IsValid(mail)) // Проверка существования юзера
            {


                try
                {
                    string userToken = createCode(4);


                    DateTime now = DateTime.Now;

                    MySqlConnection connection = new MySqlConnection(ConnectionString);


                    connection.Open();

                    MySqlCommand com = new MySqlCommand("INSERT INTO user (name,surname,email,mobile,passwd,birthdate,genderId,isactive,isblocked,resetrequested,cdate, countryId,cityId,professionId,userToken)" +
                        " Values (@name,@surname,@mail,@mobile,SHA2(@pass,512),@birthDate,(SELECT genderid FROM gender WHERE name = @gendername)" +
                        ",0,0,0,@dateTimeNow,(SELECT countryId FROM country WHERE name = @countryName)," +
                        "(SELECT cityId FROM city WHERE name = @cityName),(SELECT professionId FROM profession WHERE name = @professionName),SHA2(@userToken,512))", connection);


                    com.Parameters.AddWithValue("@name", name);
                    com.Parameters.AddWithValue("@surname", surname);

                    com.Parameters.AddWithValue("@mail", mail);
                    com.Parameters.AddWithValue("@mobile", phone);
                    com.Parameters.AddWithValue("@pass", pass);
                    com.Parameters.AddWithValue("@birthDate", DateTime.Parse(bDate));
                    com.Parameters.AddWithValue("@genderName", gender);
                    com.Parameters.AddWithValue("@dateTimeNow", now);
                    com.Parameters.AddWithValue("@countryName", country);
                    com.Parameters.AddWithValue("@cityName", city);
                    com.Parameters.AddWithValue("@professionName", profession);
                    com.Parameters.AddWithValue("@userToken", userToken);
                    com.ExecuteNonQuery();

                    long lastId = com.LastInsertedId;
                    connection.Close();

                    connection.Open();
                    com.CommandText = "insert into users_balance (userId,cdate) values (@userId,@cDate)";
                    com.Parameters.AddWithValue("@userId", lastId);
                    com.Parameters.AddWithValue("@cDate", now);


                    com.ExecuteNonQuery();
                    connection.Close();


                    //communication.sendMail($"Qeydiyyatı tamamlamaq üçün, zəhmət olmasa <a href=\'https://pullu.az/api/androidmobileapp/verify?code={userToken}'>linkə</a> daxil olun", mail);
                    communication.sendMail($"Qeydiyyati tamamlamaq ucun shifre: {userToken}", mail);
                    communication.sendSmsAsync($"Qeydiyyati tamamlamaq ucun shifre: {userToken}", phone);


                    //MailMessage mailMsg = new MailMessage();
                    //SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                    //mailMsg.IsBodyHtml = true;
                    //mailMsg.From = new MailAddress("asadzade99@gmail.com");
                    //mailMsg.To.Add($"{mail}");
                    //mailMsg.Subject = "Pullu (Dəstək)";
                    //mailMsg.Body = $"Qeydiyyatı tamamlamaq üçün, zəhmət olmasa <a href=\'https://pullu.az/api/androidmobileapp/verify?code={userToken}'>linkə</a> daxil olun";

                    //SmtpServer.Port = 587;
                    //SmtpServer.Credentials = new System.Net.NetworkCredential("asadzade99@gmail.com", "odqnmjiogipltmwi");
                    //SmtpServer.EnableSsl = true;

                    //SmtpServer.Send(mailMsg);



                    //string json = "{ \"email\" : \"" + mail + "\", \"password\" : \"" + pass + "\", \" returnSecureToken\" : true }";

                    //var content = new StringContent(json, Encoding.UTF8, "application/json");
                    //string url = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=AIzaSyCwEuju_UmuNNPrYtxEhsuddOfCzqZQ8nI";
                    //HttpClient client = new HttpClient();
                    //var rslt = client.PostAsync(url, content);
                    //var resp = rslt.Result.RequestMessage;

                    statusCode.response = 0; // Все ок
                    statusCode.responseString = "user created"; // Все ок
                    return statusCode;


                }
                catch
                {
                    statusCode.response = 1;//Ошибка сервера
                    return statusCode;
                }

                //http://127.0.0.1:44301/api/androidmobileapp/user/signUp?mail=asadzade99@gmail.com&username=asa&name=asa&surname=asa&pass=1&phone=1&bdate=1987-08-23&gender=Ki%C5%9Fi&country=Az%C9%99rbaycan&city=Bak%C4%B1&profession=Texnologiya%20sektoru

            }
            else
            {

                statusCode.response = 2;//Юзер существует
                return statusCode;
            }


        }



        public Status uProfile(UpdateProfileStruct uProfile)

        {
            Status statusCode = new Status();
            if (!string.IsNullOrEmpty(uProfile.mail) && !string.IsNullOrEmpty(uProfile.pass) && uProfile.uID > 0)
            {



                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
                List<User> userList = new List<User>();
                string uQuery = "";
                userList = select.LogIn(uProfile.mail, uProfile.pass);
                if (userList.Count > 0) // Проверка существования юзера
                {


                    try
                    {


                        if (!string.IsNullOrEmpty(uProfile.name))
                        {
                            if (string.IsNullOrEmpty(uQuery))
                            {
                                uQuery += "name=@name";
                            }
                            else
                            {
                                uQuery += " ,name=@name";
                            }

                        }
                        if (!string.IsNullOrEmpty(uProfile.surname))
                        {
                            if (string.IsNullOrEmpty(uQuery))
                            {
                                uQuery += "surname = @surname";
                            }
                            else
                            {
                                uQuery += " ,surname = @surname";
                            }

                        }
                        //if (!string.IsNullOrEmpty(uProfile.newMail))
                        //{
                        //    if (string.IsNullOrEmpty(uQuery))
                        //    {
                        //        uQuery += "email = @newMail";
                        //    }
                        //    else
                        //    {
                        //        uQuery += " ,email = @newMail";
                        //    }

                        //}
                        //if (uProfile.phone > 0)
                        //{
                        //    if (string.IsNullOrEmpty(uQuery))
                        //    {
                        //        uQuery += "mobile = @mobile";
                        //    }
                        //    else
                        //    {
                        //        uQuery += " ,mobile = @mobile";
                        //    }

                        //}
                        if (!string.IsNullOrEmpty(uProfile.newPass))
                        {
                            if (string.IsNullOrEmpty(uQuery))
                            {
                                uQuery += "passwd = SHA2(@newPass,512)";
                            }
                            else
                            {
                                uQuery += " ,passwd = SHA2(@newPass,512)";
                            }

                        }

                        if (!string.IsNullOrEmpty(uProfile.bDate))
                        {
                            if (string.IsNullOrEmpty(uQuery))
                            {
                                uQuery += "birthDate = @birthDate";
                            }
                            else
                            {
                                uQuery += " ,birthDate = @birthDate";
                            }

                        }
                        if (uProfile.genderID > 0)
                        {
                            if (string.IsNullOrEmpty(uQuery))
                            {
                                uQuery += "genderId=@genderID";
                            }
                            else
                            {
                                uQuery += " ,genderId=@genderID";
                            }

                        }
                        if (uProfile.countryID > 0)
                        {
                            if (string.IsNullOrEmpty(uQuery))
                            {
                                uQuery += "countryId=@countryID";
                            }
                            else
                            {
                                uQuery += " ,countryId=@countryID";
                            }


                        }
                        if (uProfile.cityID > 0)
                        {
                            if (string.IsNullOrEmpty(uQuery))
                            {
                                uQuery += "cityId=@cityID";
                            }
                            else
                            {
                                uQuery += " ,cityId=@cityID";
                            }


                        }
                        if (uProfile.professionID > 0)
                        {
                            if (string.IsNullOrEmpty(uQuery))
                            {
                                uQuery += "professionId=@professionID";
                            }
                            else
                            {
                                uQuery += " ,professionId=@professionID";
                            }

                        }

                        if (!string.IsNullOrEmpty(uQuery))
                        {
                            DateTime now = DateTime.Now;

                            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                            {


                                connection.Open();

                                using (MySqlCommand com = new MySqlCommand(@$"update user set {uQuery} ,udate=@dateTimeNow
     WHERE userID=@uID", connection))
                                {
                                    if (!string.IsNullOrEmpty(uProfile.name)) com.Parameters.AddWithValue("@name", uProfile.name);
                                    if (!string.IsNullOrEmpty(uProfile.surname)) com.Parameters.AddWithValue("@surname", uProfile.surname);


                                    if (uProfile.uID > 0) com.Parameters.AddWithValue("@uID", uProfile.uID);
                                    if (!string.IsNullOrEmpty(uProfile.bDate)) com.Parameters.AddWithValue("@birthDate", DateTime.Parse(uProfile.bDate));
                                    if (uProfile.genderID > 0) com.Parameters.AddWithValue("@genderID", uProfile.genderID);
                                    if (uProfile.countryID > 0) com.Parameters.AddWithValue("@countryID", uProfile.countryID);
                                    if (uProfile.cityID > 0) com.Parameters.AddWithValue("@cityID", uProfile.cityID);
                                    if (!string.IsNullOrEmpty(uProfile.newPass)) com.Parameters.AddWithValue("@newPass", uProfile.newPass);
                                    if (uProfile.professionID > 0) com.Parameters.AddWithValue("@professionID", uProfile.professionID);
                                    com.Parameters.AddWithValue("@dateTimeNow", now);

                                    com.ExecuteNonQuery();

                                    long lastId = com.LastInsertedId;




                                    if (!string.IsNullOrEmpty(userList[0].phone)) communication.sendSmsAsync("Profiliniz redaktə olundu əgər bunu siz etmisinizsə bu bildirişə önəm verməyə bilərsiniz, əks hallda bizimlə pullu@pesekar.az maili vasitəsi ilə əlaqə saxlayın", userList[0].phone);
                                    //if (!string.IsNullOrEmpty(uProfile.mail)) communication.sendMailAsync($"Profiliniz redaktə olundu əgər bunu siz etmisinizsə bu bildirişə önəm verməyə bilərsiniz, əks hallda bizimlə pullu@pesekar.az maili vasitəsi ilə əlaqə saxlayın", uProfile.mail);
                                    if (!string.IsNullOrEmpty(uProfile.mail))  communication.sendMail($"Profiliniz redaktə olundu əgər bunu siz etmisinizsə bu bildirişə önəm verməyə bilərsiniz, əks hallda bizimlə pullu@pesekar.az maili vasitəsi ilə əlaqə saxlayın", uProfile.mail);


                                    if (uProfile.uID > 0) communication.sendNotificationAsync("Profiliniz redaktə olundu ", "Əgər bunu siz etmisinizsə bu bildirişə önəm verməyə bilərsiniz, əks hallda bizimlə pullu@pesekar.az maili vasitəsi ilə əlaqə saxlayın", uProfile.uID);
                                    // string json = "{ \"email\" : \"" + user.mail + "\", \"password\" : \"" + user.pass + "\", \" returnSecureToken\" : true }";

                                    // var content = new StringContent(json, Encoding.UTF8, "application/json");
                                    //string url = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=AIzaSyCwEuju_UmuNNPrYtxEhsuddOfCzqZQ8nI";
                                    //HttpClient client = new HttpClient();
                                    //var rslt = client.PostAsync(url, content);
                                    //var resp = rslt.Result.RequestMessage;

                                    statusCode.response = 0; // Все ок

                                    com.Dispose();

                                }


                                connection.Close();
                            }

                        }






                    }
                    catch (Exception ex)
                    {
                        statusCode.response = 1;//Ошибка сервера
                        statusCode.responseString = ex.Message;
                        return statusCode;
                    }

                    //http://127.0.0.1:44301/api/androidmobileapp/user/signUp?mail=asadzade99@gmail.com&username=asa&name=asa&surname=asa&pass=1&phone=1&bdate=1987-08-23&gender=Ki%C5%9Fi&country=Az%C9%99rbaycan&city=Bak%C4%B1&profession=Texnologiya%20sektoru

                }
                else
                {

                    statusCode.response = 2;//Юзер существует
                    return statusCode;
                }
            }
            else
            {
                statusCode.response = 3;
                statusCode.responseString = "Authentication fail";
            }
            return statusCode;
        }

        public Status uPhone(string mail, string pass, string newPhone, int code)
        {
            Status status = new Status();

            try
            {

                if (!string.IsNullOrEmpty(mail) && !string.IsNullOrEmpty(pass) &&!string.IsNullOrEmpty(newPhone) && code > 0)
                {

                    if (IsValidPhone(newPhone).response == 2)
                    {



                        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                        {

                            connection.Open();

                            using (MySqlCommand com = new MySqlCommand("update user set mobile=@newPhone,userToken=null where userToken=SHA2(@userToken,512) and email=@email and passwd=SHA2(@pass,512)", connection))
                            {
                                com.Parameters.AddWithValue("@email", mail);
                                com.Parameters.AddWithValue("@pass", pass);
                                com.Parameters.AddWithValue("@userToken", code);
                                com.Parameters.AddWithValue("@newPhone", newPhone);

                                int exist = com.ExecuteNonQuery();
                                if (exist > 0)
                                {
                                    status.response = 0;
                                    status.responseString = "phone is changed";

                                }
                                else
                                {
                                    status.response = 2;//phone not changed
                                    status.responseString = "phone not changed";
                                }
                                com.Dispose();
                            }

                            connection.Close();
                        }
                    }
                    else
                    {
                        status.response = 4;//phone not changed
                        status.responseString = "Phone is already refistered";
                    }

                }
                else
                {
                    status.response = 3;//phone not changed
                    status.responseString = "params incorrect";
                }



            }
            catch (Exception ex)
            {


                status.response = 1;//server error
                status.responseString = ex.Message;//server error

            }
            return status;
        }

        public Status uAd(string mail, string pass, int aID, string aName,string aDescription)
        {
            Status status = new Status();

            try
            {

                if (!string.IsNullOrEmpty(mail) && !string.IsNullOrEmpty(pass)&&aID>0 && !string.IsNullOrEmpty(aName) && !string.IsNullOrEmpty(aDescription))
                {
                    using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                    {

                        connection.Open();

                        using (MySqlCommand com = new MySqlCommand("update announcement set mobile=@newPhone,userToken=null where userToken=SHA2(@userToken,512) and email=@email and passwd=SHA2(@pass,512)", connection))
                        {
                            com.Parameters.AddWithValue("@email", mail);
                            com.Parameters.AddWithValue("@pass", pass);
                            com.Parameters.AddWithValue("@aID", aID);
                            com.Parameters.AddWithValue("@aName", aName);
                            com.Parameters.AddWithValue("@aDescription", aName);

                            int exist = com.ExecuteNonQuery();
                            if (exist > 0)
                            {
                                status.response = 0;
                                status.responseString = "phone is changed";

                            }
                            else
                            {
                                status.response = 2;//phone not changed
                                status.responseString = "phone not changed";
                            }
                            com.Dispose();
                        }

                        connection.Close();
                    }
                }
                else
                {
                    status.response = 3;//phone not changed
                    status.responseString = "params incorrect";
                }



            }
            catch (Exception ex)
            {


                status.response = 1;//server error
                status.responseString = ex.Message;//server error

            }
            return status;
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
        //public string SaveImage(string ImgStr, string ImgName)
        //{
        //    try
        //    {
        //        string type = GetFileExtension(ImgStr);
        //        if (type == "png" || type == "jpg" || type == "mp4")
        //        {
        //            String path = $"{_hostingEnvironment.ContentRootPath}/wwwroot/media/";


        //            //Check if directory exist
        //            if (!System.IO.Directory.Exists(path))
        //            {
        //                System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
        //            }

        //            string imageName = $"{ImgName}.{type}";

        //            //set the image path
        //            string imgPath = Path.Combine(path, imageName);

        //            byte[] imageBytes = Convert.FromBase64String(ImgStr);

        //            File.WriteAllBytes(imgPath, imageBytes);

        //            return imageName;
        //        }


        //        return "";


        //    }
        //    catch
        //    {
        //        return "";
        //    }

        //}
        public Status addNewAdvert(NewAdvertisementStruct obj)
        {
            Status statusCode = new Status();
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            string mediaInsertQuery = null;
            string mediaPathUrl = "https://pullu.az/media/";
            long userID = select.getUserID(obj.mail, obj.pass);
            long lastId;
            if (!string.IsNullOrEmpty(obj.aDescription) && !string.IsNullOrEmpty(obj.aTitle) && !string.IsNullOrEmpty(obj.aPrice) && (!string.IsNullOrEmpty(obj.aBackgroundUrl) || obj.files.Count > 0))
            {
                if (userID > 0) // Проверка существования юзера
                {
                    try
                    {
                        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                        {
                            List<String> mediaList = new List<string>();
                            DateTime now = DateTime.Now;
                            connection.Open();

                            using (MySqlCommand com = new MySqlCommand("INSERT INTO announcement (userID,name,description,price,atypeid,ispaid,isactive,mediatpid,trfid,categoryId,countryid,cityid,genderid,rangeid,professionId,cdate)" +
                                 " Values (@userID,@name,@description,@price,@aTypeId,@isPaid,0,@mediaTpId,@trfId,@categoryId" +
                                 ",@countryId,@cityId,@genderId,@rangeId,@professionID,@cDate)", connection))
                            {


                                com.Parameters.AddWithValue("@userID", userID);
                                com.Parameters.AddWithValue("@name", obj.aTitle);
                                com.Parameters.AddWithValue("@description", obj.aDescription);
                                com.Parameters.AddWithValue("@isPaid", obj.isPaid);
                                com.Parameters.AddWithValue("@price", obj.aPrice);
                                com.Parameters.AddWithValue("@aTypeId", obj.aTypeID);
                                com.Parameters.AddWithValue("@mediaTpId", obj.aMediaTypeID);
                                com.Parameters.AddWithValue("@trfId", obj.aTrfID);
                                com.Parameters.AddWithValue("@categoryId", obj.aCategoryID);
                                com.Parameters.AddWithValue("@countryID", obj.aCountryId);
                                com.Parameters.AddWithValue("@cityId", obj.aCityId);
                                com.Parameters.AddWithValue("@genderId", obj.aGenderID);
                                com.Parameters.AddWithValue("@rangeId", obj.aAgeRangeID);
                                com.Parameters.AddWithValue("@professionID", obj.aProfessionID);
                                com.Parameters.AddWithValue("@cDate", now);

                                com.ExecuteNonQuery();

                                lastId = com.LastInsertedId;
                                com.Dispose();
                            }
                            // string photoName = SaveImage(obj.files[0], sha256(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")));

                            if (lastId > 0)
                            {

                                switch (obj.aMediaTypeID)
                                {
                                    case 1:
                                        // mediaList.Add(obj.mediaBase64[0]);

                                        mediaInsertQuery = $"({obj.aMediaTypeID},'{obj.aBackgroundUrl}',{lastId},@cDate)";



                                        break;
                                    case 2:
                                        if (obj.files != null)
                                        {
                                            int lastrow = 1;
                                            foreach (var item in obj.files)
                                            {
                                                string photoName = SaveImage(item, sha256(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")));
                                                if (photoName != "")
                                                {

                                                    // mediaList.Add(@$"https://pullu.az/media/{photoName}");
                                                    if (lastrow == obj.files.Count())
                                                    {
                                                        mediaInsertQuery += $"({obj.aMediaTypeID},'{mediaPathUrl}{photoName}',{lastId},@cDate)";
                                                    }
                                                    else
                                                    {
                                                        mediaInsertQuery += $"({obj.aMediaTypeID},'{mediaPathUrl}{photoName}',{lastId},@cDate),";
                                                    }
                                                }
                                                lastrow++;

                                            }
                                        }
                                        break;


                                }



                                //if (obj.mediaBase64 != null)
                                //{
                                //    switch (obj.aMediaTypeID)
                                //    {
                                //        case 1:
                                //            // mediaList.Add(obj.mediaBase64[0]);
                                //            mediaInsertQuery = $"({obj.aMediaTypeID},'{obj.mediaBase64[0]}',{lastId},@cDate)";

                                //            break;
                                //        case 2:
                                //            int lastrow = 1;
                                //            foreach (var item in obj.mediaBase64)
                                //            {
                                //                string photoName = SaveImage(item, sha256(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")));
                                //                if (photoName != "")
                                //                {

                                //                    // mediaList.Add(@$"https://pullu.az/media/{photoName}");
                                //                    if (lastrow == obj.mediaBase64.Count())
                                //                    {
                                //                        mediaInsertQuery += $"({obj.aMediaTypeID},'{mediaPathUrl}{photoName}',{lastId},@cDate)";
                                //                    }
                                //                    else
                                //                    {
                                //                        mediaInsertQuery += $"({obj.aMediaTypeID},'{mediaPathUrl}{photoName}',{lastId},@cDate),";
                                //                    }
                                //                }
                                //                lastrow++;

                                //            }
                                //            break;


                                //    }

                                //}
                                //if (mediaList.Count > 0)
                                //{

                                //    //int lastrow = 1;
                                //    foreach (var media in mediaList)
                                //    {

                                //        if (lastrow == mediaList.Count())
                                //        {
                                //            mediaInsertQuery += $"({obj.aMediaTypeID},'{media}',{lastId},@cDate)";
                                //        }
                                //        else
                                //        {
                                //            mediaInsertQuery += $"({obj.aMediaTypeID},'{media}',{lastId},@cDate),";
                                //        }

                                //        lastrow++;

                                //    }
                                //}


                                using (MySqlCommand com = new MySqlCommand($"insert into media (mediaTpId,httpUrl,announcementId,cdate) values {mediaInsertQuery}", connection))
                                {
                                    com.Parameters.AddWithValue("@cDate", now);
                                    com.ExecuteNonQuery();
                                    com.Dispose();

                                }
                                //ishlemelidi indi indi 2 ci inserti elemiyecey. onunniye oldugunu arashdirmalisan. yoxsa acib buraxmisanki error verir axi. ele shey olmur error verirse arashdir aradan qald;r
                                // vermir eerror
                                //vermirse ishleda))
                                //inesrteli elemir
                                //bax gor niye elemir. serverde error verir lokalda yox?
                                //serverde insert elemir
                                //localda eliir
                                //serverde umumiyyetce mysqle insert elemir ya konkret dbye?
                                //pnu bilmirem amma bu db ya
                                //ona gore ola biler ki serverin qiraga cixishi baglidi. administratora demek lazimdi
                                //acixdi qiraga cixishi
                                //yoxlamisan?
                                //remote qoshula bilirsen servere?
                                //elbette
                                //qoshul
                                //bax

                                communication.sendMailAsync($"Reklamınız moderatora yoxlama üçün göndərildi", obj.mail);
                                communication.sendNotificationAsync("Reklamınız moderatora yoxlama üçün göndərildi","Yaxın zamanda təsdiq olunacaq", userID);

                                statusCode.response = 0; // Все ок
                            }
                            else
                            {
                                statusCode.response = 1;

                            }

                            //connection.Close();
                            //return statusCode;


                            connection.Close();

                        }



                    }
                    catch (Exception ex)
                    {


                        statusCode.response = 2;//Ошибка сервера
                                                // return statusCode;
                        statusCode.responseString = ex.Message.ToString();
                    }
                    //http://127.0.0.1:44301/api/androidmobileapp/user/signUp?mail=asadzade99@gmail.com&username=asa&name=asa&surname=asa&pass=1&phone=1&bdate=1987-08-23&gender=Ki%C5%9Fi&country=Az%C9%99rbaycan&city=Bak%C4%B1&profession=Texnologiya%20sektoru

                }
                else
                {

                    statusCode.response = 3;//Юзер несуществует
                                            //return statusCode;
                }
            }
            else
            {

                statusCode.response = 1;
                //  return statusCode;
            }

            return statusCode;
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
        public Status AddDailyView(int advertID, long userID)
        {
            Status status = new Status();
            bool viewExcept = false;
            //+1 просмотр к платной рекламе при каждом вызове метода
            try
            {

                using (MySqlConnection connection = new MySqlConnection(ConnectionString)) {


                DateTime now = DateTime.Now;
                DateTime viewDate = now;


                connection.Open();

                    using (MySqlCommand com = new MySqlCommand("select announcementId from announcement where announcementId=@advertID and isPaid=1", connection)) {

                        com.Parameters.AddWithValue("@advertID", advertID);
                        MySqlDataReader reader = com.ExecuteReader();

                        if (reader.HasRows)
                        {
                            connection.Close();
                            connection.Open();    
                            com.CommandText = "select cdate from announcement_view where userId=@userID and announcementID=@advertID order by cdate desc limit 1";
                            com.Parameters.AddWithValue("@userID", userID);

                            reader = com.ExecuteReader();
                            viewExcept = reader.HasRows;
                            if (viewExcept)
                            {


                                while (reader.Read())
                                {
                                    viewDate = DateTime.Parse(reader["cdate"].ToString());




                                }
                               
                            }
                            if ((now - viewDate).TotalDays > 1 || viewExcept == false)
                            {

                                addView(advertID, userID);
                                status.response = 0;
                                status.responseString = "View Added";
                            }
                            else
                            {
                                status.response = 4;
                                status.responseString = "View not added, beacuse of daily limit";
                            }






                        }
                        else
                        {

                            status.response = 2;
                            status.responseString = "Annnouncement not found";

                        }
                    }
                 
                connection.Close();
            }
               

                }
                catch (Exception ex)
                {

                status.response = 1;
                status.responseString = $"Exception reason: {ex.Message}";
                }
            return status;

        }



        public Status EarnMoney(int adverID, string mail, string pass)

        {
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            long userID = select.getUserID(mail, pass);

            Status status = AddDailyView(adverID, userID);

            using (MySqlConnection connection = new MySqlConnection(ConnectionString)) {

                try
                {
                    if (status.response == 0)
                    {

                        DateTime now = DateTime.Now;
                        connection.Open();

                        using (MySqlCommand com = new MySqlCommand("update users_balance set earningValue = earningValue + (select  price from earnings_tariff where earningstpid = (select atypeID from announcement where announcementId=@advertID) ), udate=now() where userID=@userID", connection)) {

                            com.Parameters.AddWithValue("@advertID", adverID);
                            com.Parameters.AddWithValue("@userID", userID);
                            com.Parameters.AddWithValue("@dateTimeNow", now);
                            //com.ExecuteNonQuery();

                            int updated = com.ExecuteNonQuery();
                            //update users_balance set earningValue = earningValue + (select  price from earnings_tariff where earningstpid = (select atypeID from announcement where announcementId=@advertID) ), udate=now() where userId=@userID and udate<DATE_FORMAT(now(), '%Y-%m-%d')

                            connection.Close();

                            status.response = 0;
                            status.responseString = "View added and balance increased";


                        }

                     

                    }

                }
                catch (Exception ex)
                {
                    

                    status.response = 1;
                    status.responseString = $"Exception reason {ex.Message}";
                    
                }
                connection.Close();
            }


            return status;

        }  
        




        //public NewAdvertisementStatus newAdvertisement(NewAdvertisementStruct obj)

        //{
        //    DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
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
