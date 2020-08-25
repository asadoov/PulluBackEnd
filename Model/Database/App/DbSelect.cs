using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using PulluBackEnd.Model.App;
using PulluBackEnd.Model.App.client;
using PulluBackEnd.Model.App.server;
using PulluBackEnd.Model.CommonScripts;

namespace PulluBackEnd.Model.Database.App
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
        public ResponseStruct<SignInStruct> LogIn(long phone, string pass)
        {
            ResponseStruct<SignInStruct> response = new ResponseStruct<SignInStruct>();
            response.data = new List<SignInStruct>();
            try
            {
                
                
                if (phone>0&&!string.IsNullOrEmpty(pass))
                {
                    using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                    {

                        connection.Open();


                        using (MySqlCommand com = new MySqlCommand(@"select *,
                     (select balanceValue from users_balance where userId=a.userId) as balance,
                     (select earningValue from users_balance where userId=a.userId) as earning
                     from user a where mobile=@mobile and passwd=SHA2(@passwd,512) and isActive=1", connection))
                        {
                            com.Parameters.AddWithValue("@mobile", phone);
                            com.Parameters.AddWithValue("@passwd", pass);


                            using (MySqlDataReader reader = com.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    string defaultUserPhoto = "";


                                    while (reader.Read())
                                    {

                                        SignInStruct user = new SignInStruct();

                                        user.ID = Convert.ToInt32(reader["userID"]);
                                        user.name = reader["name"].ToString();
                                        user.surname = reader["surname"].ToString();
                                        user.mail = reader["email"].ToString();
                                        user.phone = Convert.ToInt32(reader["mobile"]);
                                        user.birthDate = reader["birthdate"].ToString();
                                        user.genderID = Convert.ToInt32(reader["genderID"]);
                                        user.cityID = Convert.ToInt32(reader["cityID"]);
                                        switch (user.genderID)
                                        {
                                            case 1:
                                                defaultUserPhoto = "http://master.pullu.az/public/assets/images/users/userboy.png";
                                                break;
                                            case 2:
                                                defaultUserPhoto = "http://master.pullu.az/public/assets/images/users/usergirl.png";
                                                break;
                                            default:
                                                break;
                                        }
                                        if (reader["photo"] != DBNull.Value)
                                        {
                                            if (!string.IsNullOrEmpty(reader["photo"].ToString()))
                                            {
                                                user.photoURL = reader["photo"].ToString();
                                            }
                                            else
                                            {
                                                user.photoURL = defaultUserPhoto;
                                            }

                                        }
                                        else
                                        {
                                            user.photoURL = defaultUserPhoto;
                                        }
                                        //(reader["photo"] == DBNull.Value ? defaultUserPhoto : reader["photo"].ToString());
                                        user.regDate = DateTime.Parse(reader["cdate"].ToString());
                                        user.balance = Convert.ToDecimal(reader["balance"]).ToString("0.000");
                                        user.earning = Convert.ToDecimal(reader["earning"]).ToString("0.000");
                                        response.data.Add(user);


                                    }
                                    //  connection.Close();


                                }
                                Security security = new Security(Configuration,_hostingEnvironment);
                                response.status = 1;
                                response.data[0].userToken = security.userTokenGenerator(response.data[0].ID);
                                response.requestToken = security.requestTokenGenerator(response.data[0].userToken, response.data[0].ID); 
                               
                            }


                            com.Dispose();


                        }
                        connection.Dispose();
                        connection.Close();
                    }

                }
                else
                {
                    response.status = 2;//Юзер несуществует
                                        //return statusCode;
                   // status.responseString = "access danied";

                }
            }
            catch (Exception ex)
            {
                response.status = 3;
                Console.WriteLine(ex.Message);
            }
            
          

            return response;





        }
        public ResponseStruct<FinanceStruct> getFinance(string userToken, string requestToken)
        {
            ResponseStruct<FinanceStruct> response = new ResponseStruct<FinanceStruct>();
            try
            {
                    
           
            Security security = new Security(Configuration, _hostingEnvironment);
            int userID1 = security.selectUserToken(userToken);
            int userID2 = security.selectRequestToken(requestToken);
            DateTime now = DateTime.Now;
            if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
            {

                
                response.data = new List<FinanceStruct>();
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {

                    connection.Open();


                    using (MySqlCommand com = new MySqlCommand(@"select *,
                     (select balanceValue from users_balance where userId=a.userId) as balance,
                     (select earningValue from users_balance where userId=a.userId) as earning
                     from user a where userID=@uid and isActive=1", connection))
                    {
                        com.Parameters.AddWithValue("@uid", userID1);
                        
                        using (MySqlDataReader reader = com.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {


                                while (reader.Read())
                                {

                                    FinanceStruct usr = new FinanceStruct();

                                    usr.ID = Convert.ToInt32(reader["userID"]);

                                    usr.balance = Convert.ToDecimal(reader["balance"]).ToString("0.000");
                                    usr.earning = Convert.ToDecimal(reader["earning"]).ToString("0.000");
                                    response.data.Add(usr);


                                }
                                //  connection.Close();


                            }
                        }


                        com.Dispose();


                    }
                    connection.Dispose();
                    connection.Close();
                }
                response.status = 1;
                response.requestToken = security.requestTokenGenerator(userToken, userID1); ;

            }
            else
            {
                response.status = 2;
            }
            }
            catch (Exception ex)
            {
                response.status = 3;
                Console.WriteLine(ex.Message);
            }
            return response;





        }



        public ResponseStruct<Advertisement> Advertisements(string userToken, string requestToken,int pageNo, int isPaid, int categoryID)

        {

            ResponseStruct<Advertisement> adResponse = new ResponseStruct<Advertisement>();

            try
            {

            
            long userID = 0;


                if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken))
                {
                    ResponseStruct<FinanceStruct> response = new ResponseStruct<FinanceStruct>();
                    Security security = new Security(Configuration, _hostingEnvironment);
                    int userID1 = security.selectUserToken(userToken);
                    int userID2 = security.selectRequestToken(requestToken);

                    if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
                    {
                        userID = userID1;
                        adResponse.status = 1;
                        adResponse.requestToken = security.requestTokenGenerator(userToken, userID1);
                    }
                    else
                    {
                        adResponse.status = 2;//not authorized
                    }
                }
                else
                {
                    adResponse.status = 2;//not authorized
                }

                int recPerPage = 10;

                    // List<Advertisement> adsList = new List<Advertisement>();
                    adResponse.data = new List<Advertisement>();
                    if (isPaid >= 0 && pageNo > 0)
                    {
                        int offset = (pageNo - 1) * recPerPage;




                       


                        string categoryQuery = "";
                        if (categoryID > 0)
                        {
                            categoryQuery = $"and categoryID={categoryID}";

                        }


                        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                        {


                            connection.Open();
                            switch (isPaid)
                            {
                                case 0:

                                    using (MySqlCommand com = new MySqlCommand("select *,(select httpUrl from media where announcementId=a.announcementId limit 1) as photoUrl,(select name from category where categoryId=a.categoryId ) as categoryName," +
                              "(select name from announcement_type where aTypeId=a.aTypeId ) as aTypeName" +
                              $" from announcement a where isActive=1 and isPaid=0 {categoryQuery} order by cdate desc LIMIT {offset}, {recPerPage}", connection))
                                    {




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



                                                adResponse.data.Add(ads);


                                            }




                                        }
                                        //connection.Close();
                                        //Сортировка платных реклам по пользователю

                                        com.Dispose();
                                    }
                                    
                                    break;
                                case 1:
                                    string userQuery = "";
                                    if (userID > 0)
                                    {

                                        userQuery = "and announcementID not in (select announcementId from announcement_view where announcementID=a.announcementID and userId=@userID and cdate>= now() - INTERVAL 1 DAY)";

                                    }
                                    using (MySqlCommand com = new MySqlCommand("select * ,(SELECT count FROM announcement_tariff where trfid=a.trfId)as trfViewCount,(select count(distinct userID) from announcement_view where announcementId=a.announcementId)as views,(select httpUrl from media where announcementId=a.announcementId limit 1) as photoUrl,(select name from category where categoryId=a.categoryId ) as categoryName, " +
                               "(select name from announcement_type where aTypeId=a.aTypeId ) as aTypeName " +
                               $"from announcement a  where isPaid=1 and isActive=1 {categoryQuery} {userQuery} order by cdate desc LIMIT {offset}, {recPerPage}", connection))
                                    // OLD ALGO ->  $"(announcementId not in (select distinct announcementId from announcement_view where userId=@userID) or announcementID in (select distinct announcementId from announcement_view where announcementID=a.announcementID and userId=@userID and DATE_FORMAT(cdate, '%Y-%m-%d')<DATE_FORMAT(now(), '%Y-%m-%d')))  order by cdate desc", connection))
                                    {

                                        com.Parameters.AddWithValue("@userID", userID);
                                        MySqlDataReader reader = com.ExecuteReader();
                                        if (reader.HasRows)
                                        {

                                            while (reader.Read())
                                            {
                                                if (Convert.ToInt32(reader["views"]) <= (reader["trfViewCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["trfViewCount"])))
                                                {


                                                    Advertisement ads = new Advertisement();
                                                    ads.id = Convert.ToInt32(reader["announcementId"]);
                                                    ads.name = reader["name"].ToString();
                                                    ads.userID = Convert.ToInt32(reader["userId"]);
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



                                                    adResponse.data.Add(ads);
                                                }

                                            }

                                        }
                                        com.Dispose();
                                    }
                                    break;
                            }
                           

                            // connection.Open();

                            connection.Close();
                            connection.Dispose();
                        }



                    }
                    else
                    {
                        adResponse.status = 3;//error
                    }       
            
            }
            catch (Exception ex)
            {
                adResponse.status = 3;//error
                Console.WriteLine(ex.Message);

            }


            return adResponse;

        }


        public ResponseStruct<Advertisement> getMyViews(string userToken, string requestToken)

        {
            ResponseStruct<Advertisement> myAds = new ResponseStruct<Advertisement>();
            myAds.data = new List<Advertisement>();
            try
            {
                if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken))
                {


                    Security security = new Security(Configuration, _hostingEnvironment);
                    int userID1 = security.selectUserToken(userToken);
                    int userID2 = security.selectRequestToken(requestToken);
                 
                    if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
                    {
                        myAds.status = 1;//authorized
                        myAds.requestToken = security.requestTokenGenerator(userToken, userID1);


                        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                        {
                            connection.Open();

                            using (MySqlCommand com = new MySqlCommand())
                            {

                                com.Connection = connection;




                                //Сортировка платных реклам по пользователю



                                com.CommandText = @"select distinct announcementId
,(select aTypeId from announcement where announcementId=a.announcementId)as aTypeId,
(select cDate from announcement where announcementId=a.announcementId)as cDate,
(select httpUrl from media where announcementId=a.announcementId limit 1) as photoUrl,
(select name from announcement where announcementId=a.announcementId)as aName,
(select description from announcement where announcementId=a.announcementId)as aDescription,
(select price from announcement where announcementId=a.announcementId)as aPrice,
(select isActive from announcement where announcementId=a.announcementId)as IsActive,
(select isPaid from announcement where announcementId=a.announcementId)as IsPaid
 from announcement_view a where userid =@userID";
                                com.Parameters.AddWithValue("@userID", userID1);
                                MySqlDataReader reader = com.ExecuteReader();
                                if (reader.HasRows)
                                {

                                    while (reader.Read())
                                    {

                                        int isActive = reader["IsActive"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IsActive"]);
                                        if (isActive == 1)
                                        {
                                            Advertisement ads = new Advertisement();


                                            ads.id = Convert.ToInt32(reader["announcementId"]);
                                            ads.name = reader["aName"].ToString();
                                            ads.description = reader["aDescription"].ToString();
                                            ads.price = reader["aPrice"].ToString();
                                            ads.aTypeId = Convert.ToInt32(reader["aTypeId"]);
                                            ads.cDate = DateTime.Parse(reader["cdate"].ToString());
                                            ads.isPaid = Convert.ToInt32(reader["isPaid"]);

                                            //  ads.mediaTpId = Convert.ToInt32(reader["mediaTpId"]);
                                            //   ads.catId = Convert.ToInt32(reader["categoryId"]);
                                            //     ads.catName = reader["categoryName"].ToString();
                                            // ads.cDate = DateTime.Parse(reader["cdate"].ToString());
                                            ads.photoUrl = new List<string>();
                                            ads.photoUrl.Add(reader["photoUrl"].ToString());
                                            myAds.data.Add(ads);

                                        }




                                    }

                                }
                            }

                            connection.Close();


                        }




                    }
                    else
                    {
                        myAds.status = 2;//user not found
                    }
                }
                else
                {
                    myAds.status = 2;//not authorized
                }
             
            }
            catch (Exception ex)
            {
                myAds.status = 3;//error
                Console.WriteLine(ex.Message);

            }

            return myAds;

        }



        public ResponseStruct<Advertisement> getMyAds(string userToken, string requestToken)

        {
            ResponseStruct<Advertisement> myAds = new ResponseStruct<Advertisement>();

            myAds.data = new List<Advertisement>();
            try
            {
                if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken))
                {

                    Security security = new Security(Configuration, _hostingEnvironment);
                    int userID1 = security.selectUserToken(userToken);
                    int userID2 = security.selectRequestToken(requestToken);
                   
                    if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
                    {

                        myAds.status = 1;//authorized
                        myAds.requestToken = security.requestTokenGenerator(userToken, userID1);

                        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                        {

                            connection.Open();

                            using (MySqlCommand com = new MySqlCommand())
                            {
                                com.Connection = connection;




                                //Сортировка платных реклам по пользователю



                                com.CommandText = @"select *,(SELECT count FROM announcement_tariff where trfid=a.trfId)as trfViewCount,(select count(distinct userID) from announcement_view where announcementId=a.announcementId)as views,(select httpUrl from media where announcementId=a.announcementId limit 1) as photoUrl from announcement a where userid = @userID order by cdate desc";
                                com.Parameters.AddWithValue("@userID", userID1);
                                MySqlDataReader reader = com.ExecuteReader();
                                if (reader.HasRows)
                                {

                                    while (reader.Read())
                                    {
                                        int isActive = reader["IsActive"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IsActive"]);

                                        if (isActive != 2)
                                        {
                                            Advertisement ads = new Advertisement();


                                            ads.id = Convert.ToInt32(reader["announcementId"]);
                                            ads.name = reader["name"].ToString();
                                            ads.description = reader["description"].ToString();
                                            ads.price = reader["price"].ToString();
                                            ads.aTypeId = Convert.ToInt32(reader["aTypeId"]);
                                            ads.isActive = Convert.ToInt32(reader["IsActive"]);
                                            ads.views = Convert.ToInt32(reader["views"]);
                                            ads.tariffViewCount = reader["trfViewCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["trfViewCount"]);
                                            ads.cDate = DateTime.Parse(reader["cdate"].ToString());
                                            ads.isPaid = Convert.ToInt32(reader["isPaid"]);

                                            //  ads.mediaTpId = Convert.ToInt32(reader["mediaTpId"]);
                                            //   ads.catId = Convert.ToInt32(reader["categoryId"]);
                                            //     ads.catName = reader["categoryName"].ToString();
                                            // ads.cDate = DateTime.Parse(reader["cdate"].ToString());
                                            ads.photoUrl = new List<string>();
                                            ads.photoUrl.Add(reader["photoUrl"].ToString());
                                            myAds.data.Add(ads);

                                        }




                                    }

                                }
                            }

                            connection.Close();

                        }



                    }
                    else
                    {
                        myAds.status = 2;//user not found
                    }
                }
                else
                {
                    myAds.status = 2;//not authorized

                }
            }
            catch (Exception ex)
            {
                myAds.status = 3;// server error
                Console.WriteLine(ex.Message);
            }
          
            return myAds;

        }
        public ResponseStruct<ViewerStruct> GetMyAdViewers(string userToken, string requestToken,int aID)

        {
            ResponseStruct<ViewerStruct> viewersObject = new ResponseStruct<ViewerStruct>();

            viewersObject.data = new List<ViewerStruct>();
            try
            {
                if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken))
                {
                    Security security = new Security(Configuration, _hostingEnvironment);
                    int userID1 = security.selectUserToken(userToken);
                    int userID2 = security.selectRequestToken(requestToken);
                    
                    if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
                    {
                            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                            {
                                long aPublisherID = 0;
                                connection.Open();
                                using (MySqlCommand com = new MySqlCommand())
                                {
                                    com.Connection = connection;




                                    //Сортировка платных реклам по пользователю



                                    com.CommandText = @"select userid from announcement where announcementid=@aID";
                                    com.Parameters.AddWithValue("@aID", aID);
                                    using (MySqlDataReader reader = com.ExecuteReader())
                                    {
                                        if (reader.HasRows)
                                        {

                                            while (reader.Read())
                                            {
                                                aPublisherID = reader["userid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["userid"]);
                                            }

                                        }
                                    }


                                }
                                if (aPublisherID == userID1)
                                {
                                    using (MySqlCommand com = new MySqlCommand())
                                    {
                                        com.Connection = connection;




                                        //Сортировка платных реклам по пользователю



                                        com.CommandText = @"SELECT  distinct userid ,
(select genderId from user where userid = a.userid)as genderID,
(select photo from user where userid = a.userid)as photo,
(select name from user where userid = a.userid)as name,
(select surname from user where userid = a.userid)as surname
FROM pullu_db.announcement_view a where announcementId = @aID and userid > 0;
";
                                        com.Parameters.AddWithValue("@aID", aID);
                                        using (MySqlDataReader reader = com.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                string defaultUserPhoto = "";
                                                while (reader.Read())
                                                {
                                                    ViewerStruct viewer = new ViewerStruct();
                                                    if (reader["name"] != DBNull.Value && reader["surname"] != DBNull.Value && userID1 != (reader["userid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["userid"])))
                                                    {
                                                        switch (Convert.ToInt32(reader["genderID"]))
                                                        {
                                                            case 1:
                                                                defaultUserPhoto = "http://master.pullu.az/public/assets/images/users/userboy.png";
                                                                break;
                                                            case 2:
                                                                defaultUserPhoto = "http://master.pullu.az/public/assets/images/users/usergirl.png";
                                                                break;
                                                            default:
                                                                break;
                                                        }
                                                        if (reader["photo"] != DBNull.Value)
                                                        {
                                                            if (!string.IsNullOrEmpty(reader["photo"].ToString()))
                                                            {
                                                                viewer.photoURL = reader["photo"].ToString();
                                                            }
                                                            else
                                                            {
                                                                viewer.photoURL = defaultUserPhoto;
                                                            }

                                                        }
                                                        else
                                                        {
                                                            viewer.photoURL = defaultUserPhoto;
                                                        }
                                                        //(reader["photo"] == DBNull.Value  ? defaultUserPhoto : reader["photo"].ToString());
                                                        viewer.name = reader["name"].ToString();
                                                        viewer.surname = reader["surname"].ToString();

                                                        viewersObject.data.Add(viewer);

                                                    }






                                                }


                                            }

                                        }
                                    }
                                viewersObject.status = 1;//
                                viewersObject.requestToken = security.requestTokenGenerator(userToken, userID1);

                            }
                                else
                                {
                                viewersObject.status = 2;//access danied
                            }


                                connection.Close();

                            }



                       
                    }
                    else
                    {
                        viewersObject.status = 2;//access danied
                    }
                }
                else
                {
                    viewersObject.status = 3;//param problem

                }
            }
            catch (Exception ex)
            {
                viewersObject.status = 3;// server error
                Console.WriteLine(ex.Message);
            }

            return viewersObject;

        }


        public bool IsValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public List<Country> getCountries()

        {
            List<Country> countyList = new List<Country>();


            MySqlConnection connection = new MySqlConnection(ConnectionString);


            connection.Open();

            MySqlCommand com = new MySqlCommand("Select * from country", connection);



            MySqlDataReader reader = com.ExecuteReader();
            if (reader.HasRows)
            {


                while (reader.Read())
                {

                    Country ads = new Country();
                    ads.ID = Convert.ToInt32(reader["countryId"]);
                    ads.name = reader["name"].ToString();



                    countyList.Add(ads);


                }
                connection.Close();
                return countyList;

            }
            else
            {
                connection.Close();
                return countyList;
            }


        }
        public List<City> GetCities(long countryId)

        {
            List<City> cityList = new List<City>();


            MySqlConnection connection = new MySqlConnection(ConnectionString);


            connection.Open();

            MySqlCommand com = new MySqlCommand("Select * from city where countryId=@countryId", connection);
            com.Parameters.AddWithValue("@countryId", countryId);



            MySqlDataReader reader = com.ExecuteReader();
            if (reader.HasRows)
            {


                while (reader.Read())
                {

                    City ads = new City();
                    ads.ID = Convert.ToInt32(reader["cityId"]);
                    ads.name = reader["name"].ToString();
                    ads.countryID = Convert.ToInt32(reader["countryId"]);



                    cityList.Add(ads);


                }
                connection.Close();
                return cityList;

            }
            else
            {
                connection.Close();
                return cityList;
            }


        }

        public string getUserMail(long userID)

        {
            string mail = "";
            try
            {

           
          

            using (MySqlConnection connection = new MySqlConnection(ConnectionString)) {
                connection.Open();

                using (MySqlCommand com = new MySqlCommand("Select email from user where userID=uID", connection)) {

                    com.Parameters.AddWithValue("uID",userID);
                    MySqlDataReader reader = com.ExecuteReader();
                    if (reader.HasRows)
                    {


                        while (reader.Read())
                        {
                            mail = reader["email"].ToString();

                        }
                        connection.Close();
                        

                    }
                    else
                    {
                        connection.Close();
                        
                    }
                }





            }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return mail;

        }

        public List<Profession> getProfessions()

        {
            List<Profession> professionList = new List<Profession>();


            MySqlConnection connection = new MySqlConnection(ConnectionString);


            connection.Open();

            MySqlCommand com = new MySqlCommand("Select * from profession", connection);




            MySqlDataReader reader = com.ExecuteReader();
            if (reader.HasRows)
            {


                while (reader.Read())
                {

                    Profession ads = new Profession();
                    ads.ID = Convert.ToInt32(reader["professionId"]);
                    ads.name = reader["name"].ToString();




                    professionList.Add(ads);


                }
                connection.Close();
                return professionList;

            }
            else
            {
                connection.Close();
                return professionList;
            }


        }
        public List<BackgroundImageStruct> getBackgrounds()
        {
            List<BackgroundImageStruct> backgroundList = new List<BackgroundImageStruct>();
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            try
            {





                connection.Open();

                MySqlCommand com = new MySqlCommand("select * from backgrounds", connection);



                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {


                    while (reader.Read())
                    {
                        BackgroundImageStruct background = new BackgroundImageStruct();
                        background.ID = Convert.ToInt32(reader["bgid"]);
                        background.imgUrl = reader["imgUrl"].ToString();
                        backgroundList.Add(background);
                    }

                }


                connection.Close();
                return backgroundList;
            }
            catch
            {

                connection.Close();
                return backgroundList;

            }

        }

        public Status VerifySmsOtp(string code, long phone)
        {
            Status status = new Status();
            try
            {
                string otp = "";
                using (MySqlConnection connection = new MySqlConnection(ConnectionString)) {
                connection.Open();
               




                   

                    MySqlCommand com = new MySqlCommand("select name from user where  mobile = @login and otp=SHA2(@otp,512)", connection);


                    com.Parameters.AddWithValue("@login", phone);
                    
                    com.Parameters.AddWithValue("@otp", code);

                    MySqlDataReader reader = com.ExecuteReader();
                    if (reader.HasRows)
                    {
                        status.response = 1;
                        status.responseString = $"Access allowed";
                    }
                    else
                    {
                        status.response = 2;
                        status.responseString = $"Access denied";
                    }

                   
                    
               
                connection.Close();
            }
                 }
                catch (Exception ex)
            {
               
                status.response = 3;
                status.responseString = $"Exception message {ex.Message}";
                return status;

            }

            return status;

        }
        public Status VerifyMailOtp(string code, string mail)
        {
            Status status = new Status();
            try
            {
                string otp = "";
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();







                    MySqlCommand com = new MySqlCommand("select name from user where email=@login  and otp=SHA2(@otp,512)", connection);


                    com.Parameters.AddWithValue("@login", mail);

                    com.Parameters.AddWithValue("@otp", code);

                    MySqlDataReader reader = com.ExecuteReader();
                    if (reader.HasRows)
                    {
                        status.response = 1;
                        status.responseString = $"Access allowed";
                    }
                    else
                    {
                        status.response = 2;
                        status.responseString = $"Access denied";
                    }




                    connection.Close();
                }
            }
            catch (Exception ex)
            {

                status.response = 3;
                status.responseString = $"Exception message {ex.Message}";
                return status;

            }

            return status;

        }
        public long getUserID(string mail, string pass)
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            try
            {
                long userID = 0;



                connection.Open();

                MySqlCommand com = new MySqlCommand("select userID from user where email=@mail and passwd=SHA2(@pass,512) limit 1", connection);


                com.Parameters.AddWithValue("@mail", mail);
                com.Parameters.AddWithValue("@pass", pass);
                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {


                    while (reader.Read())
                    {
                        userID = Convert.ToInt64(reader["userID"]);
                    }
                }


                connection.Close();
                return userID;
            }
            catch
            {
                connection.Close();
                return 0;

            }
        }
        public long getUserIdByMobile(long mobile, string pass)
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            try
            {
                long userID = 0;



                connection.Open();

                MySqlCommand com = new MySqlCommand("select userID from user where mobile=@mobile and passwd=SHA2(@pass,512) limit 1", connection);


                com.Parameters.AddWithValue("@mobile", mobile);
                com.Parameters.AddWithValue("@pass", pass);
                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {


                    while (reader.Read())
                    {
                        userID = Convert.ToInt64(reader["userID"]);
                    }
                }


                connection.Close();
                return userID;
            }
            catch
            {
                connection.Close();
                return 0;

            }
        }


        public bool getAdvertPaidType(long advertID)
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            try
            {
                long userID = 0;



                connection.Open();

                MySqlCommand com = new MySqlCommand("select isPaid from announcement  where announcementID = @advertID", connection);


                com.Parameters.AddWithValue("@advertID", advertID);

                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {


                    while (reader.Read())
                    {
                        if (Convert.ToInt32(reader["isPaid"]) == 0)
                        {
                            return false;

                        }

                    }
                }


                connection.Close();
                return true;
            }
            catch
            {
                connection.Close();
                return true;

            }
        }
        public bool viewExist(long userID, int advertID)
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            try
            {




                connection.Open();

                MySqlCommand com = new MySqlCommand("select count(*) as count from announcement_view  where userID = @userId and announcementID=@advertID", connection);


                com.Parameters.AddWithValue("@userID", userID);
                com.Parameters.AddWithValue("@advertID", advertID);

                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {


                    while (reader.Read())
                    {
                        if (Convert.ToInt32(reader["count"]) > 0)
                        {

                            return true;

                        }

                    }
                }

                connection.Close();

                return false;
            }
            catch
            {
                connection.Close();
                return false;

            }
        }

        public List<Advertisement> getAdvertById(long advertID)

        {

            


            List<Advertisement> adsList = new List<Advertisement>();
            string viewCountQuery = "";
            if (advertID > 0)
            {



                if (!getAdvertPaidType(advertID))
                {
                    //long userID = getUserID(mail, pass);
                    //if (userID > 0)
                    //{
                    //if (!viewExist(userID, advertID))
                    //{

                    //}
                    DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
                    insert.addView(advertID, 0);

                    
                    viewCountQuery = ",(select count(viewId) from announcement_view where announcementId=@advertID)as views";
                }
                else
                {
                    viewCountQuery = ",(select count(distinct userID) from announcement_view where announcementId=@advertID)as views";
                }

                using (MySqlConnection connection = new MySqlConnection(ConnectionString))

                {
                    connection.Open();

                    using (MySqlCommand com = new MySqlCommand("select *,(select name from user where userID=a.userID)as sellerName,(select surname from user where userID=a.userID)as sellerSurname,(select mobile from user where userID=a.userID)as sellerPhone,(select name from category where categoryId=a.categoryId ) as categoryName," +
                          $"(select name from announcement_type where aTypeId=a.aTypeId ) as aTypeName {viewCountQuery}" +
                          " from announcement a where isActive=1 and announcementId=@advertID", connection)) {

                        com.Parameters.AddWithValue("@advertID", advertID);


                        using (MySqlDataReader reader = com.ExecuteReader()) {
                            if (reader.HasRows)
                            {


                                while (reader.Read())
                                {

                                    Advertisement ads = new Advertisement();
                                    ads.id = Convert.ToInt32(reader["announcementId"]);
                                    ads.userID = Convert.ToInt32(reader["userID"]);
                                    ads.name = reader["name"].ToString();
                                    ads.sellerFullName = $"{reader["sellerName"].ToString()} {reader["sellerSurname"].ToString()}";
                                    ads.sellerPhone = reader["sellerPhone"].ToString();
                                    ads.description = reader["description"].ToString();
                                    ads.price = reader["price"].ToString();
                                    ads.aTypeId = Convert.ToInt32(reader["aTypeId"]);
                                    ads.aTypeName = reader["aTypeName"].ToString();
                                    ads.isPaid = Convert.ToInt32(reader["isPaid"]);
                                    ads.mediaTpId = Convert.ToInt32(reader["mediaTpId"]);
                                    ads.catId = Convert.ToInt32(reader["categoryId"]);
                                    ads.catName = reader["categoryName"].ToString();
                                    ads.cDate = DateTime.Parse(reader["cdate"].ToString());
                                    ads.views = Convert.ToInt32(reader["views"]);
                                    //ads.photoUrl = new List<string>();
                                    //ads.photoUrl.Add(reader["photoUrl"].ToString());



                                    adsList.Add(ads);


                                }
                            }
                            else
                            {
                                return adsList;
                            }
                        }
                           


                    }


                    using (MySqlCommand com = new MySqlCommand("select httpUrl from media where announcementId=@advertID", connection))
                    {
                        com.Parameters.AddWithValue("@advertID", advertID);
                        using (MySqlDataReader reader = com.ExecuteReader()) {
                           
                            if (reader.HasRows)
                            {
                                adsList[0].photoUrl = new List<string>();

                                while (reader.Read())
                                {


                                    adsList[0].photoUrl.Add(reader["httpUrl"].ToString());






                                }
                                connection.Close();




                            }
                            else
                            {
                                connection.Close();

                            }

                        }

                          
                    }
                }

            }
            return adsList;


        }
        public ResponseStruct<Statistics> GetStatistics(string userToken, string requestToken)

        {
            ResponseStruct <Statistics> response = new ResponseStruct<Statistics>();
           response.data = new List<Statistics>();
            if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken))
            {

                if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken))
                {
                    Security security = new Security(Configuration, _hostingEnvironment);
                    int userID1 = security.selectUserToken(userToken);
                    int userID2 = security.selectRequestToken(requestToken);

                    if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
                    {


                        response.status = 1;
                        response.requestToken = security.requestTokenGenerator(userToken, userID1);


                        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                        {

                            connection.Open();

                            using (MySqlCommand com = new MySqlCommand("Select(select distinct count(email) from user)as allUsers /* umumi qeydiyyat sayi */," +
                                 "(select distinct count(email) from user where DATE_FORMAT(cdate, '%Y-%m-%d')=DATE_FORMAT(now(), '%Y-%m-%d'))as allUsersToday, /*Bugune qeydiyyat sayi*/" +
                                 "(select count(*) from announcement)as allAds, /*Butun reklam sayi*/" +
                                 "(select count(*) from announcement_view where userID=@userID and DATE_FORMAT(cdate, '%Y-%m-%d')=DATE_FORMAT(now(), '%Y-%m-%d'))as myTodayViews, /*Bugune baxish*/" +
                                 "(select count(*) from announcement_view where userID=@userID)as allMyViews,  /*umumi baxishiniz*/" +
                                 "(select count(*) from announcement_view a where userID=@userID and announcementId=(select announcementId from announcement where announcementID=a.announcementID and isPaid=1))as myPaidViews, /* Pulluara baxish sayi*/" +
                                 "(select count(*) from announcement_view a where userID=@userID and announcementId=(select announcementId from announcement where announcementID=a.announcementID and isPaid=0))as myNotPaidViews,/* Pulsuzlara baxish sayi*/" +
                                 "(select count(*) from announcement where userId=@userID)as myAds, /*menim reklamlarim*/" +
                                 "(select count(*) from announcement where userId=@userID and isPaid=1)as myNotPaidAds, /*menim pullu reklamlarim*/" +
                                 "(select count(*) from announcement where userId=@userID and isPaid=0)as myPaidAds /*menim pulsuz reklamlarim*/", connection))
                            {



                                com.Parameters.AddWithValue("@userId", userID1);


                                MySqlDataReader reader = com.ExecuteReader();
                                if (reader.HasRows)
                                {


                                    while (reader.Read())
                                    {
                                        Statistics statistics = new Statistics();

                                        statistics.allUsers = Convert.ToInt32(reader["allUsers"]);
                                        statistics.allUsersToday = Convert.ToInt32(reader["allUsersToday"]);
                                        statistics.allAds = Convert.ToInt32(reader["allAds"]);
                                        statistics.myTodayViews = Convert.ToInt32(reader["myTodayViews"]);
                                        statistics.allMyViews = Convert.ToInt32(reader["allMyViews"]);
                                        statistics.myPaidViews = Convert.ToInt32(reader["myPaidViews"]);
                                        statistics.myNotPaidViews = Convert.ToInt32(reader["myNotPaidViews"]);
                                        statistics.myAds = Convert.ToInt32(reader["myAds"]);
                                        statistics.myNotPaidAds = Convert.ToInt32(reader["myNotPaidAds"]);
                                        statistics.myPaidAds = Convert.ToInt32(reader["myPaidAds"]);
                                        response.data.Add(statistics);




                                    }
                                }
                            
                               

                            }


                            connection.Close();
                        }



                    }
                    else
                    {
                        response.status = 2;
                      
                    }

                    
                }
            }
            else
            {
                response.status = 2;
                
            }

            return response;

        }
        public ResponseStruct<ProfileStruct> Profile(string userToken, string requestToken)

        {
            ResponseStruct<ProfileStruct> response = new ResponseStruct<ProfileStruct>();
            response.data = new List<ProfileStruct>();
            try
            {




                if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken))
                {

                    Security security = new Security(Configuration, _hostingEnvironment);
                    int userID1 = security.selectUserToken(userToken);
                    int userID2 = security.selectRequestToken(requestToken);

                    if (userID1 == userID2 && userID1 > 0 && userID2 > 0)
                    {
                        response.status = 1;
                        response.requestToken = security.requestTokenGenerator(userToken, userID1);
                        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                        {



                            connection.Open();

                            MySqlCommand com = new MySqlCommand(@"select *,(select name from profession where professionID=a.professionid)as profession,
 (select name from country where countryID=a.countryID )as country,
                    (select name from city where cityID=a.cityID )as city,

                    (select name from gender where genderID=a.genderID )as gender from user a where userID=@userId and isActive=1", connection);


                            com.Parameters.AddWithValue("@userId", userID1);


                            MySqlDataReader reader = com.ExecuteReader();
                            if (reader.HasRows)
                            {


                                while (reader.Read())
                                {
                                    ProfileStruct profile = new ProfileStruct();
                                    profile.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                                    profile.surname = reader["surname"] == DBNull.Value ? "" : reader["surname"].ToString();
                                    profile.bDate = reader["birthdate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["birthdate"]); 
                                    profile.mail = reader["email"] == DBNull.Value ? "" : reader["email"].ToString(); 
                                    profile.phone = reader["mobile"] == DBNull.Value ? "" : reader["mobile"].ToString(); 
                                    profile.profession = reader["profession"] == DBNull.Value ? "" : reader["profession"].ToString();
                                    profile.professionID = reader["professionID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["professionID"]); 
                                    profile.country = reader["country"] == DBNull.Value ? "" : reader["country"].ToString(); 
                                    profile.countryID = reader["countryID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["countryID"]);
                                    profile.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString(); 
                                    profile.cityID = reader["cityID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cityID"]); 
                                    profile.gender = reader["gender"] == DBNull.Value ? "" : reader["gender"].ToString(); 
                                    profile.cDate = reader["cdate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["cdate"]);
                                    int genderID = reader["genderID"] == DBNull.Value ? 1 : Convert.ToInt32(reader["genderID"]);
                                    string defaultUserPhoto = "";
                                    switch (genderID)
                                    {
                                        case 1:
                                            defaultUserPhoto = "http://master.pullu.az/public/assets/images/users/userboy.png";
                                            break;
                                        case 2:
                                            defaultUserPhoto = "http://master.pullu.az/public/assets/images/users/usergirl.png";
                                            break;
                                        default:
                                            break;
                                    }
                                    if (reader["photo"] != DBNull.Value)
                                    {
                                        if (!string.IsNullOrEmpty(reader["photo"].ToString()))
                                        {
                                            profile.photoURL = reader["photo"].ToString();
                                        }
                                        else
                                        {
                                            profile.photoURL = defaultUserPhoto;
                                        }

                                    }
                                    else
                                    {
                                        profile.photoURL = defaultUserPhoto;
                                    }

                                    response.data.Add(profile);
                                }

                            }



                            connection.Close();
                        }


                    }
                    else
                    {
                        response.status = 2;
                    }
                }
                else
                {
                    response.status = 2;
                }
            }

            catch (Exception ex)
            {
                response.status = 3;
                Console.WriteLine(ex.Message);

            }
            return response;



        }
        public List<TypeStruct> AType()

        {




            List<TypeStruct> aTypeList = new List<TypeStruct>();

            try
            {

           

            using (MySqlConnection connection = new MySqlConnection(ConnectionString)) {

                connection.Open();

                MySqlCommand com = new MySqlCommand("select  * from announcement_type", connection);
                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {


                    while (reader.Read())
                    {
                        TypeStruct aType = new TypeStruct();
                        aType.ID = Convert.ToInt32(reader["aTypeId"]);
                        aType.name = reader["name"].ToString();
                        aTypeList.Add(aType);
                    }
                }


                connection.Close();
            }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            return aTypeList;
        }



        public List<CategoryStruct> ACategory()
        {

            List<CategoryStruct> aCatList = new List<CategoryStruct>();
            try
            {

                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    MySqlCommand com = new MySqlCommand("select  * from category", connection);
                    MySqlDataReader reader = com.ExecuteReader();
                    if (reader.HasRows)
                    {


                        while (reader.Read())
                        {
                            CategoryStruct aCat = new CategoryStruct();
                            aCat.ID = Convert.ToInt32(reader["categoryId"]);
                            aCat.name = reader["name"].ToString();
                            aCat.catImage = reader["categoryImgUrl"].ToString();
                            aCatList.Add(aCat);
                        }
                    }
                    connection.Close();
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return aCatList;
        }

        public List<TariffStruct> aTariff()

        {




            List<TariffStruct> aTariffList = new List<TariffStruct>();


            try
            {
                MySqlConnection connection = new MySqlConnection(ConnectionString);


                connection.Open();

                MySqlCommand com = new MySqlCommand("select * from announcement_tariff", connection);
                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {


                    while (reader.Read())
                    {
                        TariffStruct aTariff = new TariffStruct();
                        aTariff.ID = Convert.ToInt32(reader["trfId"]);
                        aTariff.measure = reader["measure"].ToString();
                        aTariff.price = Convert.ToDouble(reader["price"]);
                        aTariff.viewCount = Convert.ToInt32(reader["count"]);
                        aTariffList.Add(aTariff);
                    }
                }


                connection.Close();
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);

            }
            return aTariffList;
        }
        public List<AgeRangeStruct> ageRange()

        {




            List<AgeRangeStruct> ageRangeList = new List<AgeRangeStruct>();



            MySqlConnection connection = new MySqlConnection(ConnectionString);


            connection.Open();

            MySqlCommand com = new MySqlCommand("SELECT * FROM age_range", connection);
            MySqlDataReader reader = com.ExecuteReader();
            if (reader.HasRows)
            {


                while (reader.Read())
                {
                    AgeRangeStruct ageRange = new AgeRangeStruct();
                    ageRange.ID = Convert.ToInt32(reader["rangeId"]);
                    ageRange.range = reader["rangeValue"].ToString();
                    ageRange.values = reader["vals"].ToString();
                    ageRangeList.Add(ageRange);
                }
            }


            connection.Close();

            return ageRangeList;
        }
        public Status verifyOtp(long phone, int code)
        {
            Status status = new Status();
            if (code > 0 && phone>0)
            {


                try
                {


                    using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                    {




                        connection.Open();

                        using (MySqlCommand com = new MySqlCommand("select * from user where otp=SHA2(@otp,512) and mobile = @mobile", connection))
                        {

                            com.Parameters.AddWithValue("@otp", code);
                            com.Parameters.AddWithValue("@mobile", phone);
                            MySqlDataReader reader = com.ExecuteReader();

                            // int exist = com.ExecuteNonQuery();
                            if (reader.HasRows)
                            {
                                status.response = 1;
                                status.responseString = "otp is ok";
                            }
                            else
                            {
                                status.response = 2;
                                status.responseString = "access danied";

                            }

                        }
                        connection.Close();


                    }
                }
                catch (Exception ex)
                {
                    status.response = 3;
                    status.responseString = $"Server error. Exception Message: {ex.Message}";

                }
            }
            else
            {
                status.response = 3;
                status.responseString = "code is empty";
            }
            return status;
        }
        public List<Interest> getInterests()

        {
            List<Interest> interestList = new List<Interest>();


          using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                using (MySqlCommand com = new MySqlCommand("Select * from interests", connection)) {

                    MySqlDataReader reader = com.ExecuteReader();
                    if (reader.HasRows)
                    {


                        while (reader.Read())
                        {
                            
                            Interest interest = new Interest();
                            interest.ID = Convert.ToInt32(reader["interestId"]);
                            interest.name = reader["interestName"].ToString();
                         


                            interestList.Add(interest);


                        }
                       
                    

                    }
                    
                    connection.Close();
                }





            }


            return interestList;


        }

    }
}
