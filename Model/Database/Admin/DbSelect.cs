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

    }
}
