using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PulluBackEnd.Model;

namespace PulluBackEnd.Controllers
{
   

    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class androidmobileappController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        public IConfiguration Configuration;
        public androidmobileappController(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;

            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        [Route("user/login")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<User>> log_in(string mail, string pass)
        {
            List<User> user = new List<User>();
            if (string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(pass))
            {

                return user;
            }
            else
            {



                dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
                user = select.Log_in(mail, pass);

                return user;
            }
        }

        [HttpGet]
        [Route("user/getAds")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<Advertisement>> getAds(string username, string pass)
        {
            List<Advertisement> ads = new List<Advertisement>();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pass))
            {

                return ads;
            }
            else
            {



                dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
                ads = select.Advertisements(username, pass);

                return ads;
            }
        }


        [HttpGet]
        [Route("test")]
        [EnableCors("AllowOrigin")]
        public ActionResult test()
        {

            return Ok(new
            {
                id = "sa"

            });
        }

        [HttpGet]
        [Route("changePassword")]
        [EnableCors("AllowOrigin")]
        public ActionResult changePassword(string mail, string pass)
        {

            return Ok();
        }
        [HttpGet]
        [Route("getCountries")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<Country>> getCountries()
        {
            List<Country> countries = new List<Country>();
            dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
            countries = select.getCountries();

            return countries;
        }
        [HttpGet]
        [Route("getCities")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<City>> getCities(int countryId)
        {
            List<City> cities = new List<City>();
            dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
            cities = select.getCities(countryId);

            return cities;
        }
        [Route("getProfessions")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<Profession>> getProfessions()
        {
            List<Profession> professions = new List<Profession>();
            dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
            professions = select.getProfessions();

            return professions;
        }

        [HttpGet]
        [Route("user/signUp")]
        [EnableCors("AllowOrigin")]
        public ActionResult<Status> signUp(string mail, string name, string surname, string pass, string phone, string bDate, string gender, string country, string city, string profession)
        {
            Status statusCode = new Status();
            if (!string.IsNullOrEmpty(mail) &&
                !string.IsNullOrEmpty(name) &&
                !string.IsNullOrEmpty(surname) &&
                !string.IsNullOrEmpty(pass) &&
                !string.IsNullOrEmpty(phone) &&
                !string.IsNullOrEmpty(bDate) &&
                !string.IsNullOrEmpty(gender) &&
                !string.IsNullOrEmpty(country) &&
                !string.IsNullOrEmpty(city) &&
                !string.IsNullOrEmpty(profession))
            {


                DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
                statusCode = insert.SignUp(mail, name, surname, pass, phone, bDate, gender, country, city, profession);

                return statusCode;
            }
            statusCode.response = 3;//Ошибка параметров
            return statusCode;

        }
        [HttpGet]
        [Route("user/about")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<Advertisement>> about(int advertID, string mail, string pass)
        {
            List<Advertisement> advert = new List<Advertisement>();
            if (advertID > 0 && !string.IsNullOrEmpty(mail) && !string.IsNullOrEmpty(pass))
            {

                dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
                if (advertID > 0) advert = select.getAdvertById(advertID, mail, pass);

                return advert;

            }
            return advert;




        }
        [HttpGet]
        [Route("earnMoney")]
        [EnableCors("AllowOrigin")]
        public ActionResult<EarnMoney> earnMoney(int advertID, string mail, string pass)
        {
            EarnMoney status;
            try
            {


                DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
                status = insert.EarnMoney(advertID, mail, pass);
                return status;


            }
            catch
            {
                status = new EarnMoney();
                status.statusCode = 4;
                return status;

            }


        }
        [HttpGet]
        [Route("verify")]
        [EnableCors("AllowOrigin")]
        public ContentResult verify(int code)
        {

            try
            {


                DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
                if (insert.verify(code))
                {
                    return base.Content("<html><head><meta charset = 'UTF-8' ></head>" +
               "<center><h2>Bildiriş<br>Təşəkkürlər, siz e-poçtunuzu təstiq etdiniz!</h2></center></html>", "text/html");
                }

                return base.Content("<html><head><meta charset = 'UTF-8'></head><center><h2>Bildiriş<br>Təəssüfki e-poçtunuz təstiq olunmadı</h2></center></html>", "text/html");


            }
            catch (Exception ex)
            {
                return base.Content($"<html><head><meta charset = 'UTF-8'><center><h2>Bildiriş<br>Server xətası:{ex.Message}</h2></center></html>", "text/html");

            }


        }
        [HttpGet]
        [Route("user/getStatistics")]
        [EnableCors("AllowOrigin")]
        public ActionResult<Statistics> getStatistics(string mail, string pass)
        {
            Statistics statistics = new Statistics();
            try
            {

                if (!string.IsNullOrEmpty(mail) && !string.IsNullOrEmpty(pass))
                {
                    dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
                    statistics = select.getStatistics(mail, pass);
                    return statistics;
                }

                return statistics;


            }
            catch
            {
                return statistics;

            }


        }


        [HttpGet]
        [Route("user/profile")]
        [EnableCors("AllowOrigin")]
        public ActionResult<ProfileStruct> profile(string mail, string pass)
        {
            ProfileStruct profile = new ProfileStruct();
            try
            {

                if (!string.IsNullOrEmpty(mail) && !string.IsNullOrEmpty(pass))
                {
                    dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
                    profile = select.profile(mail, pass);
                    return profile;
                }

                return profile;


            }
            catch
            {
                profile.response = 1;//server error

                return profile;

            }


        }


        [HttpGet]
        [Route("aType")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<TypeStruct>> aType()
        {
            List<TypeStruct> aTypeList = new List<TypeStruct>();
            try
            {


                dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
                aTypeList = select.aType();
                return aTypeList;



            }
            catch
            {


                return aTypeList;

            }


        }

        [HttpGet]
        [Route("aCategory")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<CategoryStruct>> aCategory()
        {
            List<CategoryStruct> aCatList = new List<CategoryStruct>();
            try
            {


                dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
                aCatList = select.aCategory();
                return aCatList;



            }
            catch
            {


                return aCatList;

            }


        }

        [HttpGet]
        [Route("aTariff")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<TariffStruct>> aTariff()
        {
            List<TariffStruct> aTariffList = new List<TariffStruct>();
            try
            {


                dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
                aTariffList = select.aTariff();
                return aTariffList;



            }
            catch
            {


                return aTariffList;

            }


        }
        [HttpPost]
        [Route("newAdvertisement")]
        [EnableCors("AllowOrigin")]
        public NewAdvertisementStatus newAdvertisement([FromBody] NewAdvertisementStruct obj)
        {

            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            // return 
            NewAdvertisementStatus status=new NewAdvertisementStatus();
            // status = insert.newAdvertisement(obj);
            status.response = 0;
            return status;

        }



        [HttpGet]
        [Route("user/Firebase")]
        [EnableCors("AllowOrigin")]
        public bool firebase(string mail, string pass)
        {

            try
            {
                DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
                if (insert.sendNotification(mail, pass))
                {
                    return true;
                }
                return false;


            }
            catch
            {
                return false;

            }


        }
    }

}