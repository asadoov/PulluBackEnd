using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PulluBackEnd.Model.Admin;

namespace PulluBackEnd.Model.Database.Admin
{
    public class DbSelect
    {
        private readonly string ConnectionString;
        public IConfiguration Configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public DbSelect(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            ConnectionString = Configuration.GetSection("ConnectionStrings").GetSection("DefaultConnectionString").Value;
            _hostingEnvironment = hostingEnvironment;

        }


        public List<AdminStruct> logIn(string username, string pass)
        {


            List<AdminStruct> userList = new List<AdminStruct>();
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                using (MySqlCommand com = new MySqlCommand("SELECT * FROM manager where uname=@username and passwd=SHA2(@pass,256)", connection))
                {

                    com.Parameters.AddWithValue("@username", username);
                    com.Parameters.AddWithValue("@pass", pass);

                    MySqlDataReader reader = com.ExecuteReader();
                    if (reader.HasRows)
                    {


                        while (reader.Read())
                        {

                            AdminStruct user = new AdminStruct();

                            user.ID = Convert.ToInt32(reader["managerId"]);
                            user.fullName = reader["fullname"].ToString();
                            user.mobile = reader["mobile"].ToString();
                            user.cDate = Convert.ToDateTime(reader["cdate"]);
                            user.managerTpID = Convert.ToInt32(reader["managerTpID"]);
                            userList.Add(user);



                        }



                    }

                    com.Dispose();

                }



                connection.Close();
            }

            return userList;

        }
        public bool checkAdmin(string username, string pass)
        {
            bool userFound = false;

          
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                using (MySqlCommand com = new MySqlCommand("SELECT * FROM manager where uname=@username and passwd=SHA2(@pass,256)", connection))
                {

                    com.Parameters.AddWithValue("@username", username);
                    com.Parameters.AddWithValue("@pass", pass);

                    MySqlDataReader reader = com.ExecuteReader();
                    if (reader.HasRows)
                    {


                        userFound = true;



                    }
                   

                    com.Dispose();

                }



                connection.Close();
            }
            return userFound;
          

        }
        public List<Advertisement> getAds(string username, string pass)
        {




            List<Advertisement> adsList = new List<Advertisement>();
            if (checkAdmin(username,pass))
            {


                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {


                    connection.Open();

                    using (MySqlCommand com = new MySqlCommand("select *,(select httpUrl from media where announcementId=a.announcementId limit 1) as photoUrl,(select name from category where categoryId=a.categoryId ) as categoryName," +
                             "(select name from announcement_type where aTypeId=a.aTypeId ) as aTypeName" +
                             $" from announcement a order by cdate desc", connection))
                    {




                        com.Parameters.AddWithValue("@username", username);
                        com.Parameters.AddWithValue("@pass", pass);

                        MySqlDataReader reader = com.ExecuteReader();
                        if (reader.HasRows)
                        {


                            while (reader.Read())
                            {

                                Advertisement ads = new Advertisement();
                                ads.id = Convert.ToInt32(reader["announcementId"]);
                                ads.name = reader["name"].ToString();
                                ads.description = reader["description"].ToString();
                                ads.price = reader["price"].ToString();
                                ads.aTypeId = Convert.ToInt32(reader["aTypeId"]);
                                ads.aTypeName = reader["aTypeName"].ToString();
                                ads.isPaid = Convert.ToInt32(reader["isPaid"]);
                                ads.mediaTpId = Convert.ToInt32(reader["mediaTpId"]);
                                ads.catId = Convert.ToInt32(reader["categoryId"]);
                                ads.catName = reader["categoryName"].ToString();
                                ads.cDate = DateTime.Parse(reader["cdate"].ToString());
                                ads.photoUrl = new List<string>();
                                ads.photoUrl.Add(reader["photoUrl"].ToString());



                                adsList.Add(ads);



                            }



                        }
                        com.Dispose();

                    }
                    connection.Dispose();
                    connection.Close();
                }
            }

            return adsList;

        }
        public List<LogStruct> getLogs(string username, string pass)
        {




            List<LogStruct> logList = new List<LogStruct>();
            if (checkAdmin(username, pass))
            {


                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {


                    connection.Open();

                    using (MySqlCommand com = new MySqlCommand("select * from api_log", connection))
                    {




                     

                        MySqlDataReader reader = com.ExecuteReader();
                        if (reader.HasRows)
                        {


                            while (reader.Read())
                            {

                                LogStruct logs = new LogStruct();
                                logs.ID = Convert.ToInt32(reader["id"]);
                                logs.ipAdress = reader["ip_adress"].ToString();
                                logs.log = reader["log"].ToString();
                                logs.functionName = reader["function_name"].ToString();
                               
                                logs.cdate = DateTime.Parse(reader["cdate"].ToString());
                               



                                logList.Add(logs);



                            }



                        }
                        com.Dispose();

                    }
                    connection.Dispose();
                    connection.Close();
                }
            }

            return logList;

        }
       

    }

}
