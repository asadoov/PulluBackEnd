using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using PulluBackEnd.Model.Payment;
using PulluBackEnd.Model.CommonScripts;
using PulluBackEnd.Model.App.server;
using PulluBackEnd.Model.Database.Payment;
using System.Diagnostics;

namespace PulluBackEnd.Model.Payment
{
    public class PaymentSelect
    {
        Communication communication;
        private readonly string ConnectionString;
        public IConfiguration Configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public PaymentSelect(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            ConnectionString = Configuration.GetSection("ConnectionStrings").GetSection("DefaultConnectionString").Value;
            _hostingEnvironment = hostingEnvironment;
            communication = new Communication(Configuration, _hostingEnvironment);

        }
        public VerifyStatusStruct Verify(VerifyStruct transaction)

        {



            VerifyStatusStruct status = new VerifyStatusStruct();


            try
            {
                //40bd001563085fc35165329ea1ff5c5ecbdbbeef -> smart Pay sha1 1 time

                string encriptedBundle = null;

                DateTime now = DateTime.Now;

                MySqlConnection connection = new MySqlConnection(ConnectionString);
                connection.Open();
                MySqlCommand com = new MySqlCommand("select * from api_access where bundleID=sha1(@bundleID)", connection);
                com.Parameters.AddWithValue("bundleID", transaction.bundleID);

                MySqlDataReader reader = com.ExecuteReader();

                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        encriptedBundle = reader["bundleID"].ToString();


                    }

                }
                connection.Close();

                if (!string.IsNullOrEmpty(encriptedBundle))
                {
                    connection.Open();
                    com.CommandText = "Select(select name from user where userID=@userID ) as name," +
                        "(select surname from user where userID=@userID) as surname," +
                        "(select balanceValue from users_balance where userID=@userID) as balance";
                    com.Parameters.AddWithValue("@userID", transaction.userID);
                    reader = com.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            status.userNameSurname = $"{reader["name"].ToString()} {reader["surname"].ToString()}";
                            status.balance = Convert.ToDouble(reader["balance"]);
                        }
                        connection.Close();

                        status.response = "0";


                        return status;
                    }
                    status.response = "1";


                    return status;
                }
                status.response = "3";

                return status;


            }
            catch (Exception ex)
            {
                status.response = $"Internal error: {ex.Message}";
                return status;
            }




        }
        public long GetSmartpayServiceID(long serviceID) {
            long smartpayServiceID = 0;
            try
            {
                

                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand com = new MySqlCommand("SELECT smartpayServiceID from withdraw_services where serviceid = @serviceID;", connection))
                    {
                        com.Parameters.AddWithValue("@serviceID",serviceID);
                        MySqlDataReader reader = com.ExecuteReader();
                        
                        if (reader.HasRows)
                        {
                           
                            while (reader.Read())
                            {
                                smartpayServiceID = Convert.ToInt64(reader["smartpayServiceID"]);

                            }

                        }
                       
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {

                smartpayServiceID = 0;
            }
            return smartpayServiceID;
        }
        public ResponseStruct<WithdrawService> GetWithdrawServices()

        {



            ResponseStruct<WithdrawService> response = new ResponseStruct<WithdrawService>();
            response.data = new List<WithdrawService>();


            try
            {
            
                DateTime now = DateTime.Now;

                using (MySqlConnection connection = new MySqlConnection(ConnectionString)) {
                    connection.Open();
                    using (MySqlCommand com = new MySqlCommand("SELECT *,(select name from service_cat where catId=a.catId)as catName FROM withdraw_services a;", connection)) {
                        MySqlDataReader reader = com.ExecuteReader();

                        if (reader.HasRows)
                        {
                            response.status = 1;
                            while (reader.Read())
                            {
                                WithdrawService wService =new WithdrawService();
                                wService.serviceID = Convert.ToInt64(reader["serviceID"]);
                                wService.smartpayServiceID = Convert.ToInt64(reader["smartpayServiceID"]);
                                wService.serviceName = reader["name"].ToString();
                                wService.serviceCatID = Convert.ToInt64(reader["catId"]);
                                wService.serviceCatName = reader["catName"].ToString();
                                wService.serviceImgURL = reader["service_icon"].ToString();
                                response.data.Add(wService);


                            }

                        }
                        else
                        {
                            response.status = 2;
                        }
                        connection.Close();
                    }


                    

                   
                }
                   

               


            }
            catch (Exception ex)
            {
                response.status = 3;
              
            }

            return response;


        }

        public decimal GetUserEarningValue(long userID) {
            decimal earningValue = 0;
            try
            {


                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                  
                    connection.Open();
                    using (MySqlCommand com = new MySqlCommand("SELECT earningValue from users_balance where userID = @userID;", connection))
                    {
                        com.Parameters.AddWithValue("@userID", userID);
                        MySqlDataReader reader = com.ExecuteReader();

                        if (reader.HasRows)
                        {

                            while (reader.Read())
                            {
                                earningValue = Convert.ToDecimal(reader["earningValue"]);

                            }

                        }

                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Error: {ex.Message}");
            }
            return earningValue;
        }
    }
}
