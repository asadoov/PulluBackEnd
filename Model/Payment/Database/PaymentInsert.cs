using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using PulluBackEnd.Model.Payment;

namespace PulluBackEnd.Model.Payment
{
    public class PaymentInsert
    {
        private readonly string ConnectionString;
        public IConfiguration Configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public PaymentInsert(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            ConnectionString = Configuration.GetSection("ConnectionStrings").GetSection("DefaultConnectionString").Value;
            _hostingEnvironment = hostingEnvironment;

        }
        public VerifyStatusStruct verify(VerifyStruct transaction)

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

        public TransactionStatusStruct updateBalance(TransactionStruct uBalance)

        {



            TransactionStatusStruct status = new TransactionStatusStruct();



            try
            {


                DateTime now = DateTime.Now;

                MySqlConnection connection = new MySqlConnection(ConnectionString);



             
                connection.Open();

                MySqlCommand com = new MySqlCommand("select * from api_access where bundleID=sha1(@bundleID)", connection);
                //com.parameters.addwithvalue("@transactionid", ubalance.transactionid);
                com.Parameters.AddWithValue("@bundleID", uBalance.bundleID);

                //com.ExecuteNonQuery();
                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {

                    connection.Close();
                    connection.Open();
                    com.CommandText = "select * from income_transaction where transactionID = @transactionID";
                    com.Parameters.AddWithValue("@transactionID", uBalance.transactionID);
                    reader = com.ExecuteReader();
                    if (reader.HasRows == false)
                    {
                        connection.Close();

                        connection.Open();
                        com.CommandText = "insert into income_transaction (transactionID,userID,bundleID,amount,cdate,smartPayDate) values (@transactionID,@userID,sha1(@bundleID),@amount,@dateTimeNow,@smartPayDate)";
                        com.Parameters.AddWithValue("@userID", uBalance.userID);
                        com.Parameters.AddWithValue("@amount", uBalance.amount);
                        com.Parameters.AddWithValue("@dateTimeNow", now);
                        com.Parameters.AddWithValue("@smartPayDate", uBalance.transactionDate);

                        //com.ExecuteNonQuery();

                        com.ExecuteNonQuery();
                        //update users_balance set earningValue = earningValue + (select  price from earnings_tariff where earningstpid = (select atypeID from announcement where announcementId=@advertID) ), udate=now() where userId=@userID and udate<DATE_FORMAT(now(), '%Y-%m-%d')
                        connection.Close();


                        connection.Open();
                        com.CommandText = "update users_balance set balanceValue = balanceValue + @amount, cdate=@dateTimeNow where userID=@userID ";
                 
                       
                     
                        com.ExecuteNonQuery();
                        connection.Close();
                        //update users_balance set earningValue = earningValue + (select  price from earnings_tariff where earningstpid = (select atypeID from announcement where announcementId=@advertID) ), udate=now() where userId=@userID and udate<DATE_FORMAT(now(), '%Y-%m-%d')


                        status.response = "0";
                        status.transactionID = uBalance.transactionID;
                        return status;
                    }
                    status.transactionID = uBalance.transactionID;
                    status.response = "2";
                    return status;
                }
                status.transactionID = uBalance.transactionID;
                status.response = "3";
                return status;


            }
            catch (Exception ex)
            {
                status.response = $"Internal error: {ex.Message}";
                return status;
            }




        }
    }
}
