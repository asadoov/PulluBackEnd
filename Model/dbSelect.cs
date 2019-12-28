using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;


namespace PulluBackEnd.Model
{
    public class dbSelect
    {
        private readonly string ConnectionString;
        public IConfiguration Configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public dbSelect(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            ConnectionString = Configuration.GetSection("ConnectionStrings").GetSection("DefaultConnectionString").Value;
            _hostingEnvironment = hostingEnvironment;

        }
        public List<User> Log_in(string username, string password)
        {


            List<User> user = new List<User>();
            MySqlConnection connection = new MySqlConnection(ConnectionString);


            connection.Open();

            MySqlCommand com = new MySqlCommand("select *,(select name from pulludb.gender where genderId=a.genderId) as gender," +
                "(select name from pulludb.city where cityId=a.cityId)as city," +
                "(select name from pulludb.profession where professionId=a.professionId)as profession" +
                " from user a where (username=@login or email=@login) and passwd=@pass", connection);


            com.Parameters.AddWithValue("@login", username);
            com.Parameters.AddWithValue("@pass", password);

            MySqlDataReader reader = com.ExecuteReader();
            if (reader.HasRows)
            {


                while (reader.Read())
                {

                    User usr = new User();
                    usr.name = reader["name"].ToString();
                    usr.surname = reader["surname"].ToString();
                    usr.username = reader["username"].ToString();
                    usr.mail = reader["email"].ToString();
                    usr.phone = reader["mobile"].ToString();
                    usr.birthDate = DateTime.Parse(reader["birthdate"].ToString());
                    usr.gender = reader["gender"].ToString();
                    usr.city = reader["city"].ToString();
                    usr.profession = reader["profession"].ToString();
                    usr.regDate = DateTime.Parse(reader["cdate"].ToString());




                    user.Add(usr);


                }
                connection.Close();
                return user;

            }
            else
            {
                return user;
            }

        }
        public List<Ads> Advertisements(string username, string password)

        {
            List<Ads> adsList = new List<Ads>();
            if (Log_in(username, password).Count > 0)
            {




                MySqlConnection connection = new MySqlConnection(ConnectionString);


                connection.Open();

                MySqlCommand com = new MySqlCommand("select *,(select name from category where categoryId=a.categoryId ) as categoryName," +
                    "(select name from announcement_type where aTypeId=a.aTypeId ) as aTypeName" +
                    " from announcement a where isActive=1", connection);


                com.Parameters.AddWithValue("@login", username);
                com.Parameters.AddWithValue("@pass", password);

                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {


                    while (reader.Read())
                    {

                        Ads ads = new Ads();
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




                        adsList.Add(ads);


                    }
                    connection.Close();
                    return adsList;

                }
                else
                {
                    return adsList;
                }
            }
            return adsList;

        }
    }
}
