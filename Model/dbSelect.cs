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

            MySqlCommand com = new MySqlCommand("select * from user where username=@login and passwd=@pass", connection);
       

            com.Parameters.AddWithValue("@login", username);
            com.Parameters.AddWithValue("@pass", password);

            MySqlDataReader reader = com.ExecuteReader();
            if (reader.HasRows)
            {


                while (reader.Read())
                {

                    User usr = new User();
                    usr.username = reader["username"].ToString();
                    usr.name = reader["name"].ToString();
                    usr.surname = reader["surname"].ToString();
                   


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
