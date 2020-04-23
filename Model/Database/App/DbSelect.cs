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

namespace PulluBackEnd.Model.Database.App
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
        public List<User> LogIn(string mail, string password)
        {


            List<User> user = new List<User>();
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {

                connection.Open();


                using (MySqlCommand com = new MySqlCommand(@"select *,
                     (select balanceValue from users_balance where userId=a.userId) as balance,
                     (select earningValue from users_balance where userId=a.userId) as earning
                     from user a where  email=@login and passwd=SHA2(@pass,512) and isActive=1", connection))
                {
                    com.Parameters.AddWithValue("@login", mail);
                    com.Parameters.AddWithValue("@pass", password);

                    using (MySqlDataReader reader = com.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {


                            while (reader.Read())
                            {

                                User usr = new User();

                                usr.ID = Convert.ToInt32(reader["userID"]);
                                usr.name = reader["name"].ToString();
                                usr.surname = reader["surname"].ToString();
                                usr.mail = reader["email"].ToString();
                                usr.phone = reader["mobile"].ToString();
                                usr.birthDate = reader["birthdate"].ToString();
                                usr.genderID = Convert.ToInt32(reader["genderID"]);
                                usr.cityID = Convert.ToInt32(reader["cityID"]);
                                usr.professionID = Convert.ToInt32(reader["professionID"]);
                                usr.regDate = DateTime.Parse(reader["cdate"].ToString());
                                usr.balance = Convert.ToDecimal(reader["balance"]).ToString("0.00");
                                usr.earning = Convert.ToDecimal(reader["earning"]).ToString("0.00");
                                user.Add(usr);


                            }
                            //  connection.Close();


                        }
                    }


                    com.Dispose();


                }
                connection.Dispose();
                connection.Close();
            }


            return user;





        }
        public List<Advertisement> Advertisements(string mail, string password, int categoryID)

        {
            List<Advertisement> adsList = new List<Advertisement>();
            long userID = 0;
            if (LogIn(mail, password).Count > 0)
            {

                string categoryQuery = "";
                if (categoryID > 0)
                {
                    categoryQuery = $"and categoryID={categoryID}";
                }

                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {


                    connection.Open();

                    using (MySqlCommand com = new MySqlCommand("select *,(select httpUrl from media where announcementId=a.announcementId limit 1) as photoUrl,(select name from category where categoryId=a.categoryId ) as categoryName," +
                         "(select name from announcement_type where aTypeId=a.aTypeId ) as aTypeName" +
                         $" from announcement a where isActive=1 and isPaid=0 {categoryQuery} order by cdate desc", connection))
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



                                adsList.Add(ads);


                            }




                        }
                        //connection.Close();
                        //Сортировка платных реклам по пользователю
                        userID = getUserID(mail, password);
                        com.Dispose();
                    }
                    // connection.Open();
                    using (MySqlCommand com = new MySqlCommand("select * ,(select httpUrl from media where announcementId=a.announcementId limit 1) as photoUrl,(select name from category where categoryId=a.categoryId ) as categoryName, " +
                    "(select name from announcement_type where aTypeId=a.aTypeId ) as aTypeName " +
                    $"from announcement a where isPaid=1 and isActive=1 {categoryQuery} and (announcementID =(select distinct announcementId from announcement_view where announcementID=a.announcementID and userId=@userID and DATE_FORMAT(cdate, '%Y-%m-%d')<DATE_FORMAT(now(), '%Y-%m-%d')) or announcementId not in (select distinct announcementId from announcement_view where userId=@userID))order by cdate desc", connection))
                    {

                        com.Parameters.AddWithValue("@userID", userID);
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
                    connection.Close();
                    connection.Dispose();
                }
                return adsList;
            }

            return adsList;

        }


        public List<Advertisement> getViews(string mail, string password)

        {
            List<Advertisement> adsList = new List<Advertisement>();
            long userID = getUserID(mail, password);
            if (userID > 0)
            {




                MySqlConnection connection = new MySqlConnection(ConnectionString);


                connection.Open();

                MySqlCommand com = new MySqlCommand();
                com.Connection = connection;




                //Сортировка платных реклам по пользователю



                com.CommandText = @"select distinct announcementId
,(select aTypeId from announcement where announcementId=a.announcementId)as aTypeId,
(select httpUrl from media where announcementId=a.announcementId limit 1) as photoUrl,
(select name from announcement where announcementId=a.announcementId)as aName,
(select description from announcement where announcementId=a.announcementId)as aDescription,
(select price from announcement where announcementId=a.announcementId)as aPrice,
(select isActive from announcement where announcementId=a.announcementId)as IsActive,
(select isPaid from announcement where announcementId=a.announcementId)as IsPaid
 from announcement_view a where userid =@userID";
                com.Parameters.AddWithValue("@userID", userID);
                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {

                    while (reader.Read())
                    {


                        if (Convert.ToInt32(reader["IsActive"]) == 1)
                        {
                            Advertisement ads = new Advertisement();


                            ads.id = Convert.ToInt32(reader["announcementId"]);
                            ads.name = reader["aName"].ToString();
                            ads.description = reader["aDescription"].ToString();
                            ads.price = reader["aPrice"].ToString();
                            ads.aTypeId = Convert.ToInt32(reader["aTypeId"]);

                            ads.isPaid = Convert.ToInt32(reader["isPaid"]);

                            //  ads.mediaTpId = Convert.ToInt32(reader["mediaTpId"]);
                            //   ads.catId = Convert.ToInt32(reader["categoryId"]);
                            //     ads.catName = reader["categoryName"].ToString();
                            // ads.cDate = DateTime.Parse(reader["cdate"].ToString());
                            ads.photoUrl = new List<string>();
                            ads.photoUrl.Add(reader["photoUrl"].ToString());
                            adsList.Add(ads);

                        }




                    }

                }
                connection.Close();
                return adsList;
            }
            return adsList;

        }



        public List<Advertisement> getMyAds(string mail, string password)

        {
            List<Advertisement> adsList = new List<Advertisement>();
            long userID = getUserID(mail, password);
            if (userID > 0)
            {




                MySqlConnection connection = new MySqlConnection(ConnectionString);


                connection.Open();

                MySqlCommand com = new MySqlCommand();
                com.Connection = connection;




                //Сортировка платных реклам по пользователю



                com.CommandText = @"select *,(select httpUrl from media where announcementId=a.announcementId limit 1) as photoUrl from announcement a where userid = @userID";
                com.Parameters.AddWithValue("@userID", userID);
                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {

                    while (reader.Read())
                    {


                        if (Convert.ToInt32(reader["IsActive"]) == 1)
                        {
                            Advertisement ads = new Advertisement();


                            ads.id = Convert.ToInt32(reader["announcementId"]);
                            ads.name = reader["name"].ToString();
                            ads.description = reader["description"].ToString();
                            ads.price = reader["price"].ToString();
                            ads.aTypeId = Convert.ToInt32(reader["aTypeId"]);

                            ads.isPaid = Convert.ToInt32(reader["isPaid"]);

                            //  ads.mediaTpId = Convert.ToInt32(reader["mediaTpId"]);
                            //   ads.catId = Convert.ToInt32(reader["categoryId"]);
                            //     ads.catName = reader["categoryName"].ToString();
                            // ads.cDate = DateTime.Parse(reader["cdate"].ToString());
                            ads.photoUrl = new List<string>();
                            ads.photoUrl.Add(reader["photoUrl"].ToString());
                            adsList.Add(ads);

                        }




                    }

                }
                connection.Close();
                return adsList;
            }
            return adsList;

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
        public List<City> getCities(int countryId)

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

        public Status checkUserToken(string code, string mail)
        {
            Status status = new Status();
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            try
            {
                string userToken = "";




                connection.Open();

                MySqlCommand com = new MySqlCommand("select name from user where email=@mail and userToken=SHA2(@userToken,512)", connection);


                com.Parameters.AddWithValue("@mail", mail);
                com.Parameters.AddWithValue("@usertoken", code);

                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {
                    status.response = 0;
                }
                else
                {
                    status.response = 1;
                }

                connection.Close();
                return status;
            }
            catch
            {
                connection.Close();
                status.response = 2;
                return status;

            }

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



        public bool getAdvertPaidType(int advertID)
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

        public List<Advertisement> getAdvertById(int advertID, string mail, string pass)

        {

            if (!getAdvertPaidType(advertID))
            {
                long userID = getUserID(mail, pass);
                if (userID > 0)
                {
                    if (!viewExist(userID, advertID))
                    {
                        DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
                        insert.addView(advertID, userID);
                    }
                }

            }


            List<Advertisement> adsList = new List<Advertisement>();



            MySqlConnection connection = new MySqlConnection(ConnectionString);


            connection.Open();

            MySqlCommand com = new MySqlCommand("select *,(select name from user where userID=a.userID)as sellerName,(select surname from user where userID=a.userID)as sellerSurname,(select mobile from user where userID=a.userID)as sellerPhone,(select name from category where categoryId=a.categoryId ) as categoryName," +
                "(select name from announcement_type where aTypeId=a.aTypeId ) as aTypeName, (select count(distinct userID) from announcement_view where announcementId=@advertID)as views" +
                " from announcement a where isActive=1 and announcementId=@advertID", connection);


            com.Parameters.AddWithValue("@advertID", advertID);


            MySqlDataReader reader = com.ExecuteReader();
            if (reader.HasRows)
            {


                while (reader.Read())
                {

                    Advertisement ads = new Advertisement();
                    ads.id = Convert.ToInt32(reader["announcementId"]);
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

            connection.Close();

            connection.Open();

            com.CommandText = "select httpUrl from media where announcementId=@advertID";

            reader = com.ExecuteReader();
            if (reader.HasRows)
            {
                adsList[0].photoUrl = new List<string>();

                while (reader.Read())
                {


                    adsList[0].photoUrl.Add(reader["httpUrl"].ToString());






                }
                connection.Close();


                return adsList;

            }
            else
            {
                connection.Close();
                return adsList;
            }






        }
        public Statistics getStatistics(string mail, string pass)

        {
            Statistics statistics = new Statistics();
            if (!string.IsNullOrEmpty(mail) && !string.IsNullOrEmpty(pass))
            {

                long userID = getUserID(mail, pass);






                MySqlConnection connection = new MySqlConnection(ConnectionString);


                connection.Open();

                MySqlCommand com = new MySqlCommand("Select(select distinct count(email) from user)as allUsers /* umumi qeydiyyat sayi */," +
                    "(select distinct count(email) from user where DATE_FORMAT(cdate, '%Y-%m-%d')=DATE_FORMAT(now(), '%Y-%m-%d'))as allUsersToday, /*Bugune qeydiyyat sayi*/" +
                    "(select count(*) from announcement)as allAds, /*Butun reklam sayi*/" +
                    "(select count(*) from announcement where userID=@userID)as myAds, /* istifadecinin reklamlari*/" +
                    "(select count(*) from announcement_view where userID=@userID and DATE_FORMAT(cdate, '%Y-%m-%d')=DATE_FORMAT(now(), '%Y-%m-%d'))as myTodayViews, /*Bugune baxish*/" +
                    "(select count(*) from announcement_view where userID=@userID)as allMyViews,  /*umumi baxishiniz*/" +
                    "(select count(*) from announcement_view a where userID=@userID and announcementId=(select announcementId from announcement where announcementID=a.announcementID and isPaid=1))as myPaidViews, /* Pulluara baxish sayi*/" +
                    "(select count(*) from announcement_view a where userID=@userID and announcementId=(select announcementId from announcement where announcementID=a.announcementID and isPaid=0))as myNotPaidViews,/* Pulsuzlara baxish sayi*/" +
                    "(select count(*) from announcement where userId=@userID)as myAds, /*menim reklamlarim*/" +
                    "(select count(*) from announcement where userId=@userID and isPaid=1)as myNotPaidAds, /*menim pullu reklamlarim*/" +
                    "(select count(*) from announcement where userId=@userID and isPaid=0)as myPaidAds /*menim pulsuz reklamlarim*/", connection);


                com.Parameters.AddWithValue("@userId", userID);


                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {


                    while (reader.Read())
                    {


                        statistics.allUsers = Convert.ToInt32(reader["allUsers"]);
                        statistics.allUsersToday = Convert.ToInt32(reader["allUsersToday"]);
                        statistics.allAds = Convert.ToInt32(reader["allAds"]);
                        statistics.myAds = Convert.ToInt32(reader["myAds"]);
                        statistics.myTodayViews = Convert.ToInt32(reader["myTodayViews"]);
                        statistics.allMyViews = Convert.ToInt32(reader["allMyViews"]);
                        statistics.myPaidViews = Convert.ToInt32(reader["myPaidViews"]);
                        statistics.myNotPaidViews = Convert.ToInt32(reader["myNotPaidViews"]);
                        statistics.myAds = Convert.ToInt32(reader["myAds"]);
                        statistics.myNotPaidAds = Convert.ToInt32(reader["myNotPaidAds"]);
                        statistics.myPaidAds = Convert.ToInt32(reader["myPaidAds"]);




                    }
                }
                else
                {
                    return statistics;
                }

                connection.Close();

                return statistics;
            }

            return statistics;

        }
        public List<ProfileStruct> profile(string mail, string pass)

        {
            List<ProfileStruct> profileList = new List<ProfileStruct>();


            long userID = getUserID(mail, pass);
            MySqlConnection connection = new MySqlConnection(ConnectionString);

            if (userID > 0)
            {

                connection.Open();

                MySqlCommand com = new MySqlCommand(@"select *,(select name from profession where professionID=a.professionid)as profession,
 (select name from country where countryID=a.countryID )as country,
                    (select name from city where cityID=a.cityID )as city,

                    (select name from gender where genderID=a.genderID )as gender from user a where userID=@userID and isActive=1", connection);


                com.Parameters.AddWithValue("@userId", userID);


                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {


                    while (reader.Read())
                    {
                        ProfileStruct profile = new ProfileStruct();
                        profile.name = reader["name"].ToString();
                        profile.surname = reader["surname"].ToString();
                        profile.bDate = Convert.ToDateTime(reader["birthdate"]);
                        profile.mail = reader["email"].ToString();
                        profile.phone = reader["mobile"].ToString();
                        profile.profession = reader["profession"].ToString();
                        profile.professionID = Convert.ToInt32(reader["professionID"]);
                        profile.country = reader["country"].ToString();
                        profile.countryID = Convert.ToInt32(reader["countryID"]);
                        profile.city = reader["city"].ToString();
                        profile.cityID = Convert.ToInt32(reader["cityID"]);
                        profile.gender = reader["gender"].ToString();
                        profile.cDate = Convert.ToDateTime(reader["cdate"]);

                        profileList.Add(profile);
                    }

                }

            }

            connection.Close();

            return profileList;



        }
        public List<TypeStruct> aType()

        {




            List<TypeStruct> aTypeList = new List<TypeStruct>();



            MySqlConnection connection = new MySqlConnection(ConnectionString);


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

            return aTypeList;
        }



        public List<CategoryStruct> aCategory()

        {




            List<CategoryStruct> aCatList = new List<CategoryStruct>();



            MySqlConnection connection = new MySqlConnection(ConnectionString);


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

            return aCatList;
        }

        public List<TariffStruct> aTariff()

        {




            List<TariffStruct> aTariffList = new List<TariffStruct>();



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


    }
}
