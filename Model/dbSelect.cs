using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
        public List<User> Log_in(string mail, string password)
        {


            List<User> user = new List<User>();
            MySqlConnection connection = new MySqlConnection(ConnectionString);


            connection.Open();

            MySqlCommand com = new MySqlCommand("select *,(select name from pullu_db.gender where genderId=a.genderId) as gender," +
                "(select balanceValue from users_balance where userId=a.userId) as balance," +
                "(select earningValue from users_balance where userId=a.userId) as earning," +
                "(select name from pullu_db.city where cityId=a.cityId)as city," +
                "(select name from pullu_db.profession where professionId=a.professionId)as profession" +
                " from user a where  email=@login and passwd=SHA2(@pass,512) and isActive=1", connection);


            com.Parameters.AddWithValue("@login", mail);
            com.Parameters.AddWithValue("@pass", password);

            MySqlDataReader reader = com.ExecuteReader();
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
                    usr.birthDate = DateTime.Parse(reader["birthdate"].ToString());
                    usr.gender = reader["gender"].ToString();
                    usr.city = reader["city"].ToString();
                    usr.profession = reader["profession"].ToString();
                    usr.regDate = DateTime.Parse(reader["cdate"].ToString());
                    usr.balance = Convert.ToDecimal(reader["balance"]).ToString("0.00");
                    usr.earning = Convert.ToDecimal(reader["earning"]).ToString("0.00");
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
        public List<Advertisement> Advertisements(string username, string password)

        {
            List<Advertisement> adsList = new List<Advertisement>();
            if (Log_in(username, password).Count > 0)
            {




                MySqlConnection connection = new MySqlConnection(ConnectionString);


                connection.Open();

                MySqlCommand com = new MySqlCommand("select *,(select httpUrl from media where announcementId=a.announcementId limit 1) as photoUrl,(select name from category where categoryId=a.categoryId ) as categoryName," +
                    "(select name from announcement_type where aTypeId=a.aTypeId ) as aTypeName" +
                    " from announcement a where isActive=1 and isPaid=0", connection);




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
                    connection.Close();



                }
                //Сортировка платных реклам по пользователю
                long userID = getUserID(username, password);

                connection.Open();
                com.CommandText = "select * ,(select httpUrl from media where announcementId=a.announcementId limit 1) as photoUrl,(select name from category where categoryId=a.categoryId ) as categoryName, " +
                    "(select name from announcement_type where aTypeId=a.aTypeId ) as aTypeName " +
                    "from announcement a where isPaid=1 and isActive=1 and (announcementID =(select distinct announcementId from announcement_view where announcementID=a.announcementID and userId=2604 and DATE_FORMAT(cdate, '%Y-%m-%d')<DATE_FORMAT(now(), '%Y-%m-%d')) or announcementId not in (select distinct announcementId from announcement_view where userId=@userID))";
                com.Parameters.AddWithValue("@userID", userID);
                reader = com.ExecuteReader();
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
                    connection.Close();
                }
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
                return professionList;
            }


        }
        public long getUserID(string mail, string pass)
        {
            try
            {
                long userID = 0;

                MySqlConnection connection = new MySqlConnection(ConnectionString);

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



                return userID;
            }
            catch
            {
                return 0;

            }
        }
        public bool getAdvertPaidType(int advertID)
        {
            try
            {
                long userID = 0;

                MySqlConnection connection = new MySqlConnection(ConnectionString);

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



                return true;
            }
            catch
            {
                return true;

            }
        }
        public bool viewExist(long userID, int advertID)
        {
            try
            {


                MySqlConnection connection = new MySqlConnection(ConnectionString);

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



                return false;
            }
            catch
            {
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
        public ProfileStruct profile(string mail, string pass)

        {
            ProfileStruct profile = new ProfileStruct();


            long userID = getUserID(mail, pass);


            if (userID > 0)
            {



                MySqlConnection connection = new MySqlConnection(ConnectionString);


                connection.Open();

                MySqlCommand com = new MySqlCommand("select *,(select name from profession where professionID=a.professionid)as profession," +
                    "(select name from city where cityID=a.cityID )as city," +
                    "(select name from gender where genderID=a.genderID )as gender from user a where userID=@userID and isActive=1", connection);


                com.Parameters.AddWithValue("@userId", userID);


                MySqlDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {


                    while (reader.Read())
                    {

                        profile.name = reader["name"].ToString();
                        profile.surname = reader["surname"].ToString();
                        profile.bDate = Convert.ToDateTime(reader["birthdate"]);
                        profile.mail = reader["email"].ToString();
                        profile.phone = reader["mobile"].ToString();
                        profile.profession = reader["profession"].ToString();
                        profile.city = reader["city"].ToString();
                        profile.gender = reader["gender"].ToString();
                        profile.cDate = Convert.ToDateTime(reader["cdate"]);
                    }
                }
                else
                {
                    return profile;
                }

                connection.Close();
                profile.response = 0;
                return profile;
            }
            profile.response = 2;//user not found
            return profile;



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
            else
            {
                return aTypeList;
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
            else
            {
                return aCatList;
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
            else
            {
                return aTariffList;
            }

            connection.Close();

            return aTariffList;
        }


    }
}
