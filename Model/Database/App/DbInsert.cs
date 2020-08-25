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

                MySqlCommand com = new MySqlCommand("select * from user where email=@mail and isActive = 1", connection);
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
        public Status IsValidPhone(long phone)
        {

            Status status = new Status();
            if (phone>0&&phone.ToString().Length==9)
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
                                while (reader.Read())
                                {
                                    switch (Convert.ToInt32(reader["isActive"]))
                                    {
                                        case 0:
                                            status.response = 1;
                                            status.responseString = "deactive user";
                                            break;
                                        case 1:
                                            status.response = 2;
                                            status.responseString = "active user";
                                            break;
                                    }
                                }
                               
                               
                            }
                            else
                            {
                                status.response = 3;
                                status.responseString = "user not found";

                            }
                            com.Dispose();

                        }
                        connection.Close();
                    }

                }
                catch (Exception ex)
                {
                    status.response = 4;
                    status.responseString = $"Server error. Exception message: {ex.Message}";

                }
            }
            else
            {
                status.response = 5;
                status.responseString = "incorrect phone format";
            }
            return status;
        }
        public  Status sendResetMail(string mail = "")
        {
            Status statusCode = new Status();
           
                if (IsValid(mail))
                {


                    try
                    {

                        string randomCode = createCode(4);


                    using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                    {


                        connection.Open();

                        MySqlCommand com = new MySqlCommand("update user set otp=SHA2(@newPass,512) where email=@mail", connection);


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
                            statusCode.response = 1; // Все ок
                        }
                        else
                        {
                            statusCode.response = 2;// ошибка сервера
                        }
                        connection.Close();

                    }

                    }
                    catch
                    {
                        statusCode.response = 2;// ошибка сервера

                    }


                }
                else
                {
                    statusCode.response = 3;// нет такого мейла
                }
               
            
               
            return statusCode;




        }
        public Status sendSms(long phone)
        {
            Status status = new Status();

            string randomCode;

                status = IsValidPhone(phone);
                switch (status.response)
                {
                   
                    case 3:

                        try
                        {

                           randomCode = createCode(4);


                            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                            {



                                connection.Open();

                                using (MySqlCommand com = new MySqlCommand("insert into user (otp,mobile,isActive,cdate) values (SHA2(@token,512),@mobile,0,@now) ", connection))
                                {
                                    com.Parameters.AddWithValue("@token", randomCode);
                                    com.Parameters.AddWithValue("@mobile", phone);

                                    com.Parameters.AddWithValue("@now", DateTime.Now);
                                    int affectedRows = com.ExecuteNonQuery();

                                    if (affectedRows > 0)
                                    {

                                        communication.sendSMS($"OTP: {randomCode}", phone);

                                        //string json = "{ \"email\" : \"" + mail + "\", \"password\" : \"" + randomPass + "\", \" returnSecureToken\" : true }";

                                        //var content = new StringContent(json, Encoding.UTF8, "application/json");
                                        //string url = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=AIzaSyCwEuju_UmuNNPrYtxEhsuddOfCzqZQ8nI";
                                        //HttpClient client = new HttpClient();
                                        //var rslt = client.PostAsync(url, content);
                                        //var resp = rslt.Result.RequestMessage;
                                        status.response = 3; // New User Registered
                                        status.responseString = "verification code sent via sms"; // Все ок
                                    }

                                    com.Dispose();
                                }

                                connection.Close();

                            }



                        }
                        catch (Exception ex)
                        {
                            status.response = 4;// ошибка сервера
                            status.responseString = ex.Message;

                        }

                        break;
                case 1:
                     randomCode = createCode(4);


                    using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                    {



                        connection.Open();

                        using (MySqlCommand com = new MySqlCommand("update user set otp = SHA2(@token,512) where mobile=@mobile ", connection))
                        {
                            com.Parameters.AddWithValue("@token", randomCode);
                            com.Parameters.AddWithValue("@mobile", phone);

                           
                            int affectedRows = com.ExecuteNonQuery();

                            if (affectedRows > 0)
                            {

                                communication.sendSMS($"OTP: {randomCode}", phone);

                                //string json = "{ \"email\" : \"" + mail + "\", \"password\" : \"" + randomPass + "\", \" returnSecureToken\" : true }";

                                //var content = new StringContent(json, Encoding.UTF8, "application/json");
                                //string url = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=AIzaSyCwEuju_UmuNNPrYtxEhsuddOfCzqZQ8nI";
                                //HttpClient client = new HttpClient();
                                //var rslt = client.PostAsync(url, content);
                                //var resp = rslt.Result.RequestMessage;
                                status.response = 1; // Все ок
                                status.responseString = "verification code sent via sms"; // повторная отправка кода юзеру для проодолжения регистрации
                            }

                            com.Dispose();
                        }

                        connection.Close();

                    }
                    break;
                    default:
                        break;
                }
                


            return status;




        }
        public Status SendResetSMS(long phone)
        {
            Status status = new Status();

            try
            {
                status = IsValidPhone(phone);
                if (status.response == 2)
                {


                 

                        string randomCode = createCode(4);


                        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                        {



                            connection.Open();

                            using (MySqlCommand com = new MySqlCommand("update user set otp=SHA2(@token,512) where mobile=@phone", connection))
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
                                    status.response = 1; // Все ок
                                    status.responseString = "verification code sent via sms"; // Все ок
                                }
                                
                                com.Dispose();
                            }

                            connection.Close();

                        }



                  


                }

                  }
                    catch (Exception ex)
            {
                status.response = 4;// ошибка сервера
                status.responseString = ex.Message;

            }

            return status;




        }
     
        

        public Status UpdateUserPhone(string userToken, string requestToken, long newPhone)
        {

            Status status = new Status();
           
            if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken) && newPhone>0)
            {
                Security security = new Security(Configuration, _hostingEnvironment);
                int userID1 = security.selectUserToken(userToken);
                int userID2 = security.selectRequestToken(requestToken);
                if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
                {

                    try
                    {
                       // Status status = new Status();

                        status = IsValidPhone(newPhone);


                        if (status.response == 2)

                        {
                            string randomCode = createCode(4);
                            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                            {



                                connection.Open();

                                using (MySqlCommand com = new MySqlCommand("update user set otp=SHA2(@token,512) where userId=@uID ", connection))
                                {
                                    com.Parameters.AddWithValue("@token", randomCode);
                                    com.Parameters.AddWithValue("@uID", userID1);
                                   

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
                                        status.requestToken = security.requestTokenGenerator(userToken, userID1);
                                        status.response = 1; // Все ок
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
                        status.response = 3;// ошибка сервера
                        //status.responseString = ex.Message;

                    }

                }
                else
                {
                    status.response = 3;
                    status.responseString = "access danied";
                }


            }
            else
            {
                status.response = 3;
               status.responseString = "access danied";
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

                        using (MySqlCommand com = new MySqlCommand("update user set isActive=1,otp=null where otp=SHA2(@otp,512)", connection))
                        {

                            com.Parameters.AddWithValue("@otp", code);
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
       
        public Status ResetPasswordByMail(string newPassword, string mail, string code)
        {
            Status status = new Status();

            try
            {


                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    MySqlCommand com = new MySqlCommand("update user set passwd=SHA2(@newPassword,512),otp=null where otp=SHA2(@otp,512) and email=@mail", connection);
                    com.Parameters.AddWithValue("@otp", code);
                    com.Parameters.AddWithValue("@newPassword", newPassword);
                    com.Parameters.AddWithValue("@mail", mail);

                    int exist = com.ExecuteNonQuery();
                    if (exist > 0)
                    {
                        status.response = 1;

                    }
                    else
                    {
                        status.response = 2;//password not changed
                    }
                    connection.Close();
                    return status;
                }
            }
            catch (Exception ex)
            {
             
                status.response = 2;//server error
                return status;
            }

        }
        public Status ResetPasswordBySms(string newPassword, long phone, string code)
        {
            Status status = new Status();

            try
            {


                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    MySqlCommand com = new MySqlCommand("update user set passwd=SHA2(@newPassword,512),otp=null where otp=SHA2(@otp,512) and mobile = @phone)", connection);
                    com.Parameters.AddWithValue("@otp", code);
                    com.Parameters.AddWithValue("@newPassword", newPassword);
                    com.Parameters.AddWithValue("@phone", phone);

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
            }
            catch (Exception ex)
            {

                status.response = 2;//server error
                return status;
            }

        }




        public Status SignUp(NewUserStruct newUser)

        {
            Status statusCode = new Status();
            if (!string.IsNullOrEmpty(newUser.mail) &&
               !string.IsNullOrEmpty(newUser.name) &&
               !string.IsNullOrEmpty(newUser.surname) &&
               !string.IsNullOrEmpty(newUser.pass) &&
              newUser.phone > 0 &&
              newUser.otp > 0 &&
               !string.IsNullOrEmpty(newUser.bDate) &&
              newUser.gender > 0 &&
              newUser.country > 0 &&
               newUser.city > 0 &&
               newUser.interestIds.Count>0)
            {
                if (IsValidPhone(newUser.phone).response==1) // Проверка существования юзера
                {


                    try
                    {
                        long userID = 0;
                        long userAffectedRows = 0;
                        long lastInsertedBalanceId = 0;

                        DateTime now = DateTime.Now;

                        using (MySqlConnection connection = new MySqlConnection(ConnectionString)) {


                            connection.Open();

                            using (MySqlCommand com = new MySqlCommand("select userid from user where mobile=@mobile and otp=SHA2(@otp,512)", connection))
                            {

                                com.Parameters.AddWithValue("@mobile", newUser.phone);

                                com.Parameters.AddWithValue("@otp", newUser.otp);
                                using (MySqlDataReader reader = com.ExecuteReader())
                                {

                                    if (reader.HasRows)

                                    {

                                        while (reader.Read())
                                        {
                                            userID = Convert.ToInt64(reader["userId"]);

                                        }
                                    }

                                }





                            }


                            if (userID>0)
                            {
                                using (MySqlCommand com = new MySqlCommand("update user set name=@name,surname=@surname,otp=null,email=@mail,passwd=SHA2(@pass,512),birthdate=@birthDate,genderId=@genderId,isactive=1,isblocked=0,resetrequested=0,cdate=@dateTimeNow, countryId=@countyId,cityId=@cityId" +
                                                                " WHERE userId=@userId", connection))
                                {
                                    com.Parameters.AddWithValue("@name", newUser.name);
                                    com.Parameters.AddWithValue("@surname", newUser.surname);

                                    com.Parameters.AddWithValue("@mail", newUser.mail);
                                    com.Parameters.AddWithValue("@mobile", newUser.phone);
                                    com.Parameters.AddWithValue("@pass", newUser.pass);
                                    com.Parameters.AddWithValue("@birthDate", DateTime.Parse(newUser.bDate));
                                    com.Parameters.AddWithValue("@genderId", newUser.gender);
                                    com.Parameters.AddWithValue("@dateTimeNow", now);
                                    com.Parameters.AddWithValue("@countyId", newUser.country);
                                    com.Parameters.AddWithValue("@cityID", newUser.city);
                                    // com.Parameters.AddWithValue("@professionId", inserestIds);
                                    com.Parameters.AddWithValue("@userID", userID);


                                    userAffectedRows = com.ExecuteNonQuery();


                                }

                                using (MySqlCommand com = new MySqlCommand("insert into users_balance (userId,cdate) values (@userId,@cDate)", connection))
                                {

                                    com.Parameters.AddWithValue("@userId", userID);
                                    com.Parameters.AddWithValue("@cDate", now);


                                    com.ExecuteNonQuery();
                                    lastInsertedBalanceId = com.LastInsertedId;
                                }
                                if (lastInsertedBalanceId > 0 && userAffectedRows > 0)
                                {



                                    //communication.sendMail($"Qeydiyyatı tamamlamaq üçün, zəhmət olmasa <a href=\'https://pullu.az/api/androidmobileapp/verify?code={otp}'>linkə</a> daxil olun", mail);
                                    //communication.sendMail($"Qeydiyyati tamamlamaq ucun shifre: {otp}", mail);
                                    //communication.sendSmsAsync($"Qeydiyyati tamamlamaq ucun shifre: {otp}", phone);


                                    //MailMessage mailMsg = new MailMessage();
                                    //SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                                    //mailMsg.IsBodyHtml = true;
                                    //mailMsg.From = new MailAddress("asadzade99@gmail.com");
                                    //mailMsg.To.Add($"{mail}");
                                    //mailMsg.Subject = "Pullu (Dəstək)";
                                    //mailMsg.Body = $"Qeydiyyatı tamamlamaq üçün, zəhmət olmasa <a href=\'https://pullu.az/api/androidmobileapp/verify?code={otp}'>linkə</a> daxil olun";

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
                                    
                                    statusCode.response = 1; // User created
                                    statusCode.responseString = "OK"; // User created
                                }
                                else
                                {
                                    statusCode.response = 2; //db error
                                    statusCode.responseString = "error";
                                }

                            }
                            else
                            {
                                statusCode.response = 2; //wrong otp or phone
                                statusCode.responseString = "user not found";//wrong otp or phone
                            }


                            connection.Close();
                           
                        }
                      
                       


                    }
                    catch
                    {
                        statusCode.response = 2;//Ошибка сервера
                        statusCode.responseString = "error";
                    }

                    //http://127.0.0.1:44301/api/androidmobileapp/user/signUp?mail=asadzade99@gmail.com&username=asa&name=asa&surname=asa&pass=1&phone=1&bdate=1987-08-23&gender=Ki%C5%9Fi&country=Az%C9%99rbaycan&city=Bak%C4%B1&profession=Texnologiya%20sektoru

                }
                else
                {

                    statusCode.response = 2;//user is active
                    statusCode.responseString = "user is active or not found";
                }
            }
            else
            {
                statusCode.response = 2;//Ошибка параметров

            }
            return statusCode;

        }



        public Status uProfile(UpdateProfileStruct uProfile)

        {
            Status statusCode = new Status();
            if (!string.IsNullOrEmpty(uProfile.userToken) && !string.IsNullOrEmpty(uProfile.requestToken))
            {
                    try
                    {
                    DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
                    List<User> userList = new List<User>();
                    string uQuery = "";

                    Security security = new Security(Configuration, _hostingEnvironment);
                        int userID1 = security.selectUserToken(uProfile.userToken);
                        int userID2 = security.selectRequestToken(uProfile.requestToken);
                    
                        if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
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


                                         com.Parameters.AddWithValue("@uID", userID1);
                                        if (!string.IsNullOrEmpty(uProfile.bDate)) com.Parameters.AddWithValue("@birthDate", DateTime.Parse(uProfile.bDate));
                                        if (uProfile.genderID > 0) com.Parameters.AddWithValue("@genderID", uProfile.genderID);
                                        if (uProfile.countryID > 0) com.Parameters.AddWithValue("@countryID", uProfile.countryID);
                                        if (uProfile.cityID > 0) com.Parameters.AddWithValue("@cityID", uProfile.cityID);
                                        if (!string.IsNullOrEmpty(uProfile.newPass)) com.Parameters.AddWithValue("@newPass", uProfile.newPass);
                                        if (uProfile.professionID > 0) com.Parameters.AddWithValue("@professionID", uProfile.professionID);
                                        com.Parameters.AddWithValue("@dateTimeNow", now);

                                        com.ExecuteNonQuery();

                                        long lastId = com.LastInsertedId;

                                    


                                        //if (userList[0].phone > 0) communication.sendSmsAsync("Profiliniz redaktə olundu əgər bunu siz etmisinizsə bu bildirişə önəm verməyə bilərsiniz, əks hallda bizimlə pullu@pesekar.az maili vasitəsi ilə əlaqə saxlayın", userList[0].phone);
                                        //if (!string.IsNullOrEmpty(uProfile.mail)) communication.sendMailAsync($"Profiliniz redaktə olundu əgər bunu siz etmisinizsə bu bildirişə önəm verməyə bilərsiniz, əks hallda bizimlə pullu@pesekar.az maili vasitəsi ilə əlaqə saxlayın", uProfile.mail);
                                        // if (!string.IsNullOrEmpty(uProfile.mail))  communication.sendMail($"Profiliniz redaktə olundu əgər bunu siz etmisinizsə bu bildirişə önəm verməyə bilərsiniz, əks hallda bizimlə pullu@pesekar.az maili vasitəsi ilə əlaqə saxlayın", uProfile.mail);


                                         communication.sendNotificationAsync("Profiliniz redaktə olundu ", "Əgər bunu siz etmisinizsə bu bildirişə önəm verməyə bilərsiniz, əks hallda bizimlə pullu@pesekar.az maili vasitəsi ilə əlaqə saxlayın", userID1);
                                    // string json = "{ \"email\" : \"" + user.mail + "\", \"password\" : \"" + user.pass + "\", \" returnSecureToken\" : true }";

                                    // var content = new StringContent(json, Encoding.UTF8, "application/json");
                                    //string url = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=AIzaSyCwEuju_UmuNNPrYtxEhsuddOfCzqZQ8nI";
                                    //HttpClient client = new HttpClient();
                                    //var rslt = client.PostAsync(url, content);
                                    //var resp = rslt.Result.RequestMessage;

                                    statusCode.requestToken = security.requestTokenGenerator(uProfile.userToken, userID1);
                                    statusCode.response = 1; // Все ок

                                        com.Dispose();

                                    }


                                    connection.Close();
                                }

                            }
                        else
                        {
                            statusCode.response = 2; //empty
                        }





                        }
                        else
                        {
                            statusCode.response = 3; //access danied
                        statusCode.responseString = "access danied";
                    }
                    }
                    catch (Exception ex)
                    {
                        statusCode.response = 4;//Ошибка сервера
                        statusCode.responseString = ex.Message;
                        return statusCode;
                    }

                    //http://127.0.0.1:44301/api/androidmobileapp/user/signUp?mail=asadzade99@gmail.com&username=asa&name=asa&surname=asa&pass=1&phone=1&bdate=1987-08-23&gender=Ki%C5%9Fi&country=Az%C9%99rbaycan&city=Bak%C4%B1&profession=Texnologiya%20sektoru

            
            }
            else
            {
                statusCode.response = 3;
                statusCode.responseString = "access danied";
            }
            return statusCode;
        }

        public Status VerifyUserNewPhone(string userToken, string requestToken, int newPhone, int otp)
        {
            Status status = new Status();

            try
            {

                if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken) &&newPhone>0 && otp > 0)
                {
                    Security security = new Security(Configuration, _hostingEnvironment);
                    int userID1 = security.selectUserToken(userToken);
                    int userID2 = security.selectRequestToken(requestToken);
                    DateTime now = DateTime.Now;
                    if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
                    {
                        status = IsValidPhone(newPhone);
                        if (status.response == 2)
                        {



                            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                            {

                                connection.Open();

                                using (MySqlCommand com = new MySqlCommand("update user set mobile=@newPhone,otp=null where otp=SHA2(@otp,512) and userID=@uID", connection))
                                {
                                    com.Parameters.AddWithValue("@uID", userID1);
                                    
                                    com.Parameters.AddWithValue("@otp", otp);
                                    com.Parameters.AddWithValue("@newPhone", newPhone);

                                    int exist = com.ExecuteNonQuery();
                                    if (exist > 0)
                                    {
                                        status.requestToken = security.requestTokenGenerator(userToken, userID1);
                                        status.response = 1;
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
                    }
                    else
                    {
                        status.response = 3;
                        status.responseString = "access danied";
                    }

                }
                else
                {
                    status.response = 3;
                    status.responseString = "access danied";
                }



            }
            catch (Exception ex)
            {


                status.response = 4;//server error
                status.responseString = ex.Message;//server error

            }
            return status;
        }

        public Status uAd(string userToken, string requestToken, int aID, string aName,string aDescription, int aPrice)
        {
            Status status = new Status();

            try
            {

                if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken)&&aID>0 && aPrice>0 && !string.IsNullOrEmpty(aName) && !string.IsNullOrEmpty(aDescription))
                {
                    DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
                    Security security = new Security(Configuration, _hostingEnvironment);
                    int userID1 = security.selectUserToken(userToken);
                    int userID2 = security.selectRequestToken(requestToken);
                   
                    if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
                    {
                        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                        {

                            connection.Open();

                            using (MySqlCommand com = new MySqlCommand("update announcement set name=@aName,description=@aDescription,price=@aPrice,isActive=0 where userID = @uID and announcementID=@aID", connection))
                            {
                                com.Parameters.AddWithValue("@uID", userID1);
                                com.Parameters.AddWithValue("@aID", aID);
                                com.Parameters.AddWithValue("@aName", aName);
                                com.Parameters.AddWithValue("@aDescription", aDescription);
                                com.Parameters.AddWithValue("@aPrice", aPrice);
                                int exist = com.ExecuteNonQuery();
                                if (exist > 0)
                                {
                                    status.requestToken = security.requestTokenGenerator(userToken, userID1);
                                    status.response = 1;
                                    status.responseString = "data changed";

                                }
                                else
                                {
                                    status.response = 2;//phone not changed
                                    status.responseString = "data not changed";
                                }
                                com.Dispose();
                            }

                            connection.Close();
                        }
                    }
                    else
                    {
                        status.response = 3;
                        status.responseString = "access danied";
                    }
                }
                else
                {
                    status.response = 3;
                    status.responseString = "access danied";
                }



            }
            catch (Exception ex)
            {


                status.response = 2;//server error
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
        public string SaveFile(IFormFile fileStr, string fileName)
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
                using (Stream fileStream = File.Create($"{path}{fileName}{Path.GetExtension(fileStr.FileName)}"))
                {
                    fileStr.CopyTo(fileStream);
                }


                return $"{fileName}{Path.GetExtension(fileStr.FileName)}";


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
            Status status = new Status();
            try
            {

            
            Security security = new Security(Configuration, _hostingEnvironment);
            int userID1 = security.selectUserToken(obj.userToken);
            int userID2 = security.selectRequestToken(obj.requestToken);
           
            if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
            {

                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
                string mediaInsertQuery = null;
                string mediaPathUrl = "https://pullu.az/media/";

                long lastId;
                bool writePermission = true;
                if (!string.IsNullOrEmpty(obj.aDescription) && !string.IsNullOrEmpty(obj.aTitle) && !string.IsNullOrEmpty(obj.aPrice) && (!string.IsNullOrEmpty(obj.aBackgroundUrl) || obj.files.Count > 0))
                {
               
                      
                            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                            {
                                List<string> mediaList = new List<string>();
                                DateTime now = DateTime.Now;
                                connection.Open();

                                if (obj.isPaid > 0 && obj.aTrfID > 0)
                                {
                                    Double uBalance = 0, aTrfPrice = 0;
                                    long balanceID = 0;

                                    using (MySqlCommand com = new MySqlCommand("select * from users_balance where userID=@userID", connection))
                                    {
                                        com.Parameters.AddWithValue("@userID", userID1);

                                        using (MySqlDataReader reader = com.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    uBalance = Convert.ToDouble(reader["balanceValue"]);
                                                    balanceID = Convert.ToInt64(reader["balanceId"]);
                                                }
                                            }
                                        }


                                        com.Dispose();
                                    }
                                    using (MySqlCommand com = new MySqlCommand("select * from announcement_tariff where trfID=@trfID", connection))
                                    {
                                        com.Parameters.AddWithValue("@trfID", obj.aTrfID);

                                        using (MySqlDataReader reader = com.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    aTrfPrice = Convert.ToDouble(reader["price"]);

                                                }
                                            }
                                        }


                                        com.Dispose();
                                    }
                                    if (uBalance > aTrfPrice && uBalance > 0 && aTrfPrice > 0)
                                    {


                                 
                                            int affectedRows = 0;


                                            using (MySqlCommand com = new MySqlCommand("update users_balance set balanceValue = @balance where balanceID=@balanceID", connection))
                                            {
                                                com.Parameters.AddWithValue("@balance", uBalance - aTrfPrice);
                                                com.Parameters.AddWithValue("@balanceID", balanceID);
                                                affectedRows = com.ExecuteNonQuery();

                                                com.Dispose();
                                            }





                                  

                                }
                                    else
                                    {
                                        writePermission = false;
                                        status.response = 3;
                                        status.responseString = "Not enough money";
                                    }
                                }
                            if (writePermission)
                            {
                                using (MySqlCommand com = new MySqlCommand("INSERT INTO announcement (userID,name,description,price,atypeid,ispaid,isactive,mediatpid,trfid,categoryId,countryid,cityid,genderid,rangeid,professionId,cdate)" +
                             " Values (@userID,@name,@description,@price,@aTypeId,@isPaid,0,@mediaTpId,@trfId,@categoryId" +
                             ",@countryId,@cityId,@genderId,@rangeId,@professionID,@cDate)", connection))
                                {


                                    com.Parameters.AddWithValue("@userID", userID1);
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
                                                    string photoName = SaveFile(item, sha256(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")));
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
                                        case 3:
                                            if (obj.files != null)
                                            {
                                                int lastrow = 1;
                                                foreach (var item in obj.files)
                                                {
                                                    string videoName = SaveFile(item, sha256(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")));
                                                    if (videoName != "")
                                                    {

                                                        // mediaList.Add(@$"https://pullu.az/media/{photoName}");
                                                        if (lastrow == obj.files.Count())
                                                        {
                                                            mediaInsertQuery += $"({obj.aMediaTypeID},'{mediaPathUrl}{videoName}',{lastId},@cDate)";
                                                        }
                                                        else
                                                        {
                                                            mediaInsertQuery += $"({obj.aMediaTypeID},'{mediaPathUrl}{videoName}',{lastId},@cDate),";
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

                                    string email = select.getUserMail(userID1);

                                    communication.sendMailAsync($"Reklamınız moderatora yoxlama üçün göndərildi", email);
                                    communication.sendNotificationAsync("Reklamınız moderatora yoxlama üçün göndərildi", "Yaxın zamanda təsdiq olunacaq", userID1);
                                    status.requestToken = security.requestTokenGenerator(obj.userToken, userID1);
                                    status.response = 1; // Все ок

                                }
                                else
                                {
                                    status.response = 2;

                                }
                            }
                           

                            connection.Close();

                            }



                       
                        //http://127.0.0.1:44301/api/androidmobileapp/user/signUp?mail=asadzade99@gmail.com&username=asa&name=asa&surname=asa&pass=1&phone=1&bdate=1987-08-23&gender=Ki%C5%9Fi&country=Az%C9%99rbaycan&city=Bak%C4%B1&profession=Texnologiya%20sektoru

                   
                }
                else
                {

                    status.response = 4;
                        //status.responseString = "params problem";
                        //  return statusCode;
                    }
            }
            else
            {
                status.response = 5;
                status.responseString = "access danied";
            }
            }
            catch (Exception ex)
            {
                status.response = 2;//Ошибка сервера
                                    // return statusCode;
                status.responseString = ex.Message.ToString();
            }
            return status;
        }

        public Status deleteAd(string userToken,string requestToken,long aID)
        {
            Status status = new Status();
            try
            {
                Security security = new Security(Configuration, _hostingEnvironment);
            int userID1 = security.selectUserToken(userToken);
            int userID2 = security.selectRequestToken(requestToken);

              if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken) && aID > 0)
                {
                    if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
                    {

                        DbSelect select = new DbSelect(Configuration, _hostingEnvironment);



                        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                        {
                            List<string> mediaList = new List<string>();
                            DateTime now = DateTime.Now;
                            connection.Open();
                            using (MySqlCommand com = new MySqlCommand("update announcement set isActive=2 where announcementID=@aID and userID=@uID", connection))
                            {
                                com.Parameters.AddWithValue("@uID", userID1);
                                com.Parameters.AddWithValue("@aID", aID);
                                com.ExecuteNonQuery();
                                status.requestToken = security.requestTokenGenerator(userToken, userID1);
                                status.response = 1;
                                com.Dispose();
                            }



                            connection.Close();

                        }


                    }

                    //http://127.0.0.1:44301/api/androidmobileapp/user/signUp?mail=asadzade99@gmail.com&username=asa&name=asa&surname=asa&pass=1&phone=1&bdate=1987-08-23&gender=Ki%C5%9Fi&country=Az%C9%99rbaycan&city=Bak%C4%B1&profession=Texnologiya%20sektoru

                    else
                    {
                        status.response = 2;
                        status.responseString = "access danied";
                    }
                    }
                else
                {

                    status.response = 2;
                    status.responseString = "parameter problem";
                    //  return statusCode;
                }
            }
           
            
            catch (Exception ex)
            {


                status.response = 3;//Ошибка сервера
                                        // return statusCode;
                status.responseString = ex.Message.ToString();
            }
            return status;
        }
        public Status uPass(string userToken, string requestToken, string newPass)
        {
            Status status = new Status();

                    try
                    {

                if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken) && !string.IsNullOrEmpty(newPass))
                {

                    DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
                    Security security = new Security(Configuration, _hostingEnvironment);
                    int userID1 = security.selectUserToken(userToken);
                    int userID2 = security.selectRequestToken(requestToken);
                   
                    if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
                    {
                        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                        {
                           
                            DateTime now = DateTime.Now;
                            connection.Open();
                            using (MySqlCommand com = new MySqlCommand("update user set passwd=SHA2(@newPass,512) where userID=@uID", connection))
                            {
                                com.Parameters.AddWithValue("@uID", userID1);
                                com.Parameters.AddWithValue("@newPass", newPass);
                                com.ExecuteNonQuery();
                                status.requestToken = security.requestTokenGenerator(userToken, userID1);
                                status.response = 1;
                                status.responseString = "Pass Changed";
                                com.Dispose();
                            }



                            connection.Close();

                        }
                    }
                else
                    {

                        status.response = 2;//Юзер несуществует
                                                //return statusCode;
                        status.responseString = "access danied";
                    }
                }
                else
                {

                    status.response = 3;
                    status.responseString = "parameter problem";
                    //  return statusCode;
                }


            }
                    catch (Exception ex)
                    {


                        status.response = 3;//Ошибка сервера
                                                // return statusCode;
                        status.responseString = ex.Message.ToString();
                    }
                    //http://127.0.0.1:44301/api/androidmobileapp/user/signUp?mail=asadzade99@gmail.com&username=asa&name=asa&surname=asa&pass=1&phone=1&bdate=1987-08-23&gender=Ki%C5%9Fi&country=Az%C9%99rbaycan&city=Bak%C4%B1&profession=Texnologiya%20sektoru

               

            return status;
        }
        public void addView(long advertID, long userID)
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



        public Status EarnMoney(int adverID, string userToken, string requestToken)

        {


            Status status = new Status();

            try
            {
                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
                Security security = new Security(Configuration, _hostingEnvironment);
                int userID1 = security.selectUserToken(userToken);
                int userID2 = security.selectRequestToken(requestToken);

                if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
                {
                    status = AddDailyView(adverID, userID1);



                    using (MySqlConnection connection = new MySqlConnection(ConnectionString)) {


                        if (status.response == 0)
                        {

                            DateTime now = DateTime.Now;
                            connection.Open();

                            using (MySqlCommand com = new MySqlCommand("update users_balance set earningValue = earningValue + (select  price from earnings_tariff where earningstpid = (select atypeID from announcement where announcementId=@advertID) ), udate=now() where userID=@userID", connection)) {

                                com.Parameters.AddWithValue("@advertID", adverID);
                                com.Parameters.AddWithValue("@userID", userID1);
                                com.Parameters.AddWithValue("@dateTimeNow", now);
                                //com.ExecuteNonQuery();

                                int updated = com.ExecuteNonQuery();
                                //update users_balance set earningValue = earningValue + (select  price from earnings_tariff where earningstpid = (select atypeID from announcement where announcementId=@advertID) ), udate=now() where userId=@userID and udate<DATE_FORMAT(now(), '%Y-%m-%d')

                                connection.Close();
                                status.requestToken = security.requestTokenGenerator(userToken, userID1);
                                status.response = 1;
                                status.responseString = "View added and balance increased";


                            }



                        }
                        else
                        {
                            status.response = 2;
                            status.responseString = "limit";
                        }
                        connection.Close();

                    }
                }
                else
                {
                    status.response = 3;//Юзер несуществует
                                        //return statusCode;
                    status.responseString = "access danied";
                }
                
            }
            catch (Exception ex)
            {


                status.response = 4;
                status.responseString = $"Exception reason {ex.Message}";

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
