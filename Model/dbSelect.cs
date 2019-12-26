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
        public List<User> log_in(string username, string password)
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
    }
}
