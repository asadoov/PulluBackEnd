using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using PulluBackEnd.Model.Payment;
using PulluBackEnd.Model.CommonScripts;
using PulluBackEnd.Model.Database.Payment;

namespace PulluBackEnd.Model.Payment
{
    public class PaymentInsert
    {
        Communication communication;
        private readonly string ConnectionString;
        public IConfiguration Configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public PaymentInsert(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            ConnectionString = Configuration.GetSection("ConnectionStrings").GetSection("DefaultConnectionString").Value;
            _hostingEnvironment = hostingEnvironment;
            communication = new Communication(Configuration, _hostingEnvironment);

        }
      

        public TransactionStatusStruct UpBalance(TransactionStruct uBalance)

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
                        communication.sendNotificationAsync("Mədaxil", "Online odəmə sistemi ilə balansiniz artırldı",uBalance.userID);
                        communication.sendPushNotificationAsync("Mədaxil", "Online odəmə sistemi ilə balansiniz artırldı", uBalance.userID);

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
        public long InsertTransaction(long userID,long serviceID,long amount,long account) {
            long paymentID = 0;

            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                
                DateTime now = DateTime.Now;
                connection.Open();
                using (MySqlCommand com = new MySqlCommand(@"insert into withdraw
(amount,serviceID,userID,account,cdate)
values(@amount,@serviceID,@userID,@account,@cdate)", connection))
                {
                    com.Parameters.AddWithValue("@amount", amount);
                    com.Parameters.AddWithValue("@serviceID", serviceID);
                    com.Parameters.AddWithValue("@userID", userID);
                    com.Parameters.AddWithValue("@account", account);
                    com.Parameters.AddWithValue("@cdate", now);
                    
  
                    com.ExecuteNonQuery();
                   paymentID = com.LastInsertedId;
                    com.Dispose();
                }



                connection.Close();

            }
            return paymentID;
        }
        public void UpdateTransaction(long paymentID, long state, long substate,int final)
        {
           

            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {

                DateTime now = DateTime.Now;
                connection.Open();
                using (MySqlCommand com = new MySqlCommand(@"update withdraw set state=@state,substate=@substate,final=@final,udate=@udate where withdrawID = @paymentID", connection))
                {
                    com.Parameters.AddWithValue("@state", state);
                    com.Parameters.AddWithValue("@substate", substate);
                    com.Parameters.AddWithValue("@final", final);
                    com.Parameters.AddWithValue("@udate", now);
                    com.Parameters.AddWithValue("@paymentID", paymentID);


                    com.ExecuteNonQuery();
                   
                    com.Dispose();
                }



                connection.Close();

            }
       
        }
        public void WithdrawUserBalance(long userID,long amount) {

            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {

                DateTime now = DateTime.Now;
                connection.Open();
                using (MySqlCommand com = new MySqlCommand(@"update users_balance set earningValue = earningValue - @amount, udate=@now where userID=@userID", connection))
                {
                    com.Parameters.AddWithValue("@amount",Convert.ToDouble(amount)/100);
                    com.Parameters.AddWithValue("@userID", userID);
                    com.Parameters.AddWithValue("@now", now);
  
                    com.ExecuteNonQuery();

                    com.Dispose();
                }



                connection.Close();

            }

        }
    }
}
