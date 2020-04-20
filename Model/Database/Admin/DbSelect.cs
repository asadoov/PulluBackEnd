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


        public List<AdminStruct> getUser(string username, string pass)
        {


            List<AdminStruct> userList = new List<AdminStruct>();
            MySqlConnection connection = new MySqlConnection(ConnectionString);


            connection.Open();

            MySqlCommand com = new MySqlCommand("SELECT * FROM manager where uname=@username and passwd=SHA2(@pass,256)", connection);


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
                connection.Close();
                return userList;

            }
            else
            {
                connection.Close();
                return userList;
            }
        }
        public List<Advertisement> getAds(string username, string pass)
        {




                List<Advertisement> adsList = new List<Advertisement>();
            if (getUser(username,pass).Count>0)
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

    }
}
