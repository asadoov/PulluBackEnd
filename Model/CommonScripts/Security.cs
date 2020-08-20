using System;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace PulluBackEnd.Model.CommonScripts
{
   
        public class Security
        {
            private readonly string ConnectionString;
            public IConfiguration Configuration;
            private readonly IWebHostEnvironment _hostingEnvironment;
            public Security(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
            {
                Configuration = configuration;
                ConnectionString = Configuration.GetSection("ConnectionStrings").GetSection("DefaultConnectionString").Value;
                _hostingEnvironment = hostingEnvironment;

            }
            public string sha256(string randomString = "")
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


            public String userTokenGenerator(long userID)
            {
                var SecretKeyword1 = "myNewSecretKeywordForServizzApplication1";
                var userToken = "";
                var now = (DateTime.Now).ToString("yyyy-MM-dd hh:mm:ss");



                if (userID > 0)
                {
                    userToken = sha256(SecretKeyword1 + now + userID.ToString());

                    string query = "update user set userToken =  @token where userid = @userID";

                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand com = new MySqlCommand(query, con))
                        {
                            com.Parameters.AddWithValue("userID", userID);

                            com.Parameters.AddWithValue("token", userToken);
                            com.ExecuteNonQuery();
                        }

                    }


                }

                return userToken;
            }
            public string requestTokenGenerator(string userToken, long userID)
            {

                var requestToken = "";
                var now = (DateTime.Now).ToString("yyyy-MM-dd hh:mm:ss");





                if (userID > 0)
                {

                    requestToken = sha256(userToken + now + userID.ToString());



                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {

                        con.Open();
                        string query = "update requesttoken set isactive=0 where userid=@userID;";
                        using (MySqlCommand cmd = new MySqlCommand(@query, con))
                        {

                            cmd.Parameters.AddWithValue("userId", userID);
                            cmd.ExecuteNonQuery();
                        }

                        query = "insert into requestToken (userID,token,isactive,cdate) values (@userID,@token,1,@now)";
                        using (MySqlCommand com = new MySqlCommand(query, con))
                        {
                            com.Parameters.AddWithValue("userID", userID);

                            com.Parameters.AddWithValue("now", now);
                            com.Parameters.AddWithValue("token", requestToken);
                            com.ExecuteNonQuery();
                        }

                    }


                }

                return requestToken;
            }
            public int selectUserToken(string userToken)
            {


                int userID = 0;

                string query = "select userid from user where userToken = @token order by userid desc limit 1";

                using (MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(@query, con))
                    {

                        cmd.Parameters.AddWithValue("token", userToken);
                        MySqlDataReader dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {

                            userID = Convert.ToInt32(dr[0]);
                        }
                    }

                    con.Close();


                }


                return userID;
            }
            public int selectRequestToken(string requestToken)
            {


                int userID = 0;

                string query = "select userID from requestToken where token = @token and isActive = 1 order by userID desc limit 1";

                using (MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(@query, con))
                    {

                        cmd.Parameters.AddWithValue("token", requestToken);
                        MySqlDataReader dr = cmd.ExecuteReader();

                        while (dr.Read())
                        {

                            userID = Convert.ToInt32(dr[0]);

                        }
                    }

                    con.Close();


                }


                return userID;
            }
        }

    
}
