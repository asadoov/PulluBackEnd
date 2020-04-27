using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PulluBackEnd.Model.App;
using PulluBackEnd.Model.CommonScripts;
using PulluBackEnd.Model.Database.App;

namespace PulluBackEnd.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class androidmobileappController : Controller
    {

        Communication communication;
        Security security = new Security();
        private readonly IWebHostEnvironment _hostingEnvironment;
        public IConfiguration Configuration;
        public androidmobileappController(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;

            _hostingEnvironment = hostingEnvironment;
            communication =new Communication(Configuration, _hostingEnvironment);
        }
        [HttpGet]
        [Route("testIP")]
        [EnableCors("AllowOrigin")]
        public ActionResult<string> testIP()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            return ipAddress.ToString();
        }

        [HttpGet]
        [Route("user/login")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<User>> log_in(string mail, string pass)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"mail -> {mail}\npass ->{security.sha256(pass)}", "log_in(string mail, string pass)", ipAddress.ToString());

            List<User> user = new List<User>();
            if (string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(pass))
            {

                return user;
            }
            else
            {



                dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
                user = select.LogIn(mail, pass);

                return user;
            }
        }

        [HttpGet]
        [Route("user/get/ads")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<Advertisement>> getAds(string mail, string pass, int catID)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"mail -> {mail}\npass ->{security.sha256(pass)}\ncatID -> {catID}", "getAds(string mail, string pass, int catID)", ipAddress.ToString());
            List<Advertisement> ads = new List<Advertisement>();
            if (string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(pass))
            {

                return ads;
            }
            else
            {



                dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
                ads = select.Advertisements(mail, pass, catID);

                return ads;
            }
        }


        [HttpPost]
        [Route("test")]
        [EnableCors("AllowOrigin")]
        public ActionResult<string> test(IFormFile file)
        {

            //DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            //string a = insert.SaveImage(data.name, data.ID.ToString());
            return file.ContentType;


        }

        [HttpGet]
        [Route("changePassword")]
        [EnableCors("AllowOrigin")]
        public ActionResult changePassword(string mail, string pass)
        {

            return Ok();
        }
        [HttpGet]
        [Route("get/countries")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<Country>> getCountries()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"", "getCountries()", ipAddress.ToString());
            List<Country> countries = new List<Country>();
            dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
            countries = select.getCountries();

            return countries;
        }
        [HttpGet]
        [Route("get/cities")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<City>> getCities(int countryId)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"countryID -> {countryId}", "getCities(int countryId)", ipAddress.ToString());
            List<City> cities = new List<City>();
            dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
            cities = select.getCities(countryId);

            return cities;
        }
        [Route("get/professions")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<Profession>> getProfessions()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"", "getProfessions()", ipAddress.ToString());
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
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log(@$"mail ->{mail}\n
                                name -> {name}\n
                                surname ->{surname}\n
                                pass ->{security.sha256(pass)}\n
                                phone ->{phone}\n
                                bDate ->{bDate}\n
                                gender ->{gender}\n
                                country ->{country}\n
                                city ->{city}\n
                                profession ->{profession}\n
", "signUp(string mail, string name, string surname, string pass, string phone, string bDate, string gender, string country, string city, string profession)", ipAddress.ToString());
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

        [HttpPost]
        [Route("user/update/profile")]
        [EnableCors("AllowOrigin")]
        public ActionResult<Status> uProfile(User user)
        {

            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log(Newtonsoft.Json.JsonConvert.SerializeObject(user).ToString(), "uProfile(User user)", ipAddress.ToString());
            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);


            return insert.uProfile(user);



        }


        [HttpGet]
        [Route("user/about")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<Advertisement>> about(int advertID, string mail, string pass)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"adverID -> {advertID}\nmail -> {mail}\npass ->{security.sha256(pass)}", "about(int advertID, string mail, string pass)", ipAddress.ToString());
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
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"adverID -> {advertID}\nmail -> {mail}\npass ->{security.sha256(pass)}", "earnMoney(int advertID, string mail, string pass)", ipAddress.ToString());
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
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"code -> {security.sha256(code.ToString())}", "verify(int code)", ipAddress.ToString());

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
        [Route("user/get/statistics")]
        [EnableCors("AllowOrigin")]
        public ActionResult<Statistics> getStatistics(string mail, string pass)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"mail:{mail}\npass:{security.sha256(pass)}", "getStatistics(string mail, string pass)", ipAddress.ToString());
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
        [Route("user/get/profile")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<ProfileStruct>> profile(string mail, string pass)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"mail -> {mail}\n pass -> {security.sha256(pass)}", "profile(string mail, string pass)", ipAddress.ToString());
            List<ProfileStruct> profile = new List<ProfileStruct>();


            if (!string.IsNullOrEmpty(mail) && !string.IsNullOrEmpty(pass))
            {
                dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
                profile = select.profile(mail, pass);
                return profile;
            }

            return profile;





        }

        [HttpGet]
        [Route("get/age/range")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<AgeRangeStruct>> ageRange()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"", "ageRange()", ipAddress.ToString());
            List<AgeRangeStruct> ageRangeList = new List<AgeRangeStruct>();
            try
            {


                dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
                ageRangeList = select.ageRange();
                return ageRangeList;



            }
            catch
            {


                return ageRangeList;

            }


        }

        [HttpGet]
        [Route("get/aType")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<TypeStruct>> aType()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"", "aType()", ipAddress.ToString());
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
        [Route("get/aCategory")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<CategoryStruct>> aCategory()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"", "aCategory()", ipAddress.ToString());
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
        [Route("get/aTariff")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<TariffStruct>> aTariff()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"", "aTariff()", ipAddress.ToString());
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
        // MultipartBodyLengthLimit  was needed for zip files with form data.
        // [DisableRequestSizeLimit] works for the KESTREL server, but not IIS server 
        // for IIS: webconfig... <requestLimits maxAllowedContentLength="102428800" />
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        [Consumes("multipart/form-data")] // for Zip files with form data
        [HttpPost]
        [Route("user/advertisements/add")]
        [EnableCors("AllowOrigin")]
        public Status newAdvertisement([FromForm] NewAdvertisementStruct obj)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
           
            communication.log(Newtonsoft.Json.JsonConvert.SerializeObject(obj).ToString(), "newAdvertisement([FromForm] NewAdvertisementStruct obj)", ipAddress.ToString());
            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            // return 
            Status status = new Status();
            status = insert.addNewAdvert(obj);

            return status;

        }
        [HttpGet]
        [Route("accounts/password/reset/send/mail")]
        [EnableCors("AllowOrigin")]
        public Status sendMail(string mail)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"account reset -> {mail}", "sendMail(string mail)", ipAddress.ToString());
            Status status = new Status();
            if (!string.IsNullOrEmpty(mail))
            {


                DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
                status = insert.sendResetMail(mail);
                // return 

                // status = insert.newAdvertisement(obj);

                return status;
            }

            status.response = 3; // пусто
            return status;

        }
        [HttpGet]
        [Route("accounts/password/reset/confirm")]
        [EnableCors("AllowOrigin")]
        public Status confirmCode(string code, string mail)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"code -> {code}\nmail -> {mail}, ", "confirmCode(string code, string mail)", ipAddress.ToString());
            Status status;



            dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
            status = select.checkUserToken(code, mail);

            return status;




        }


        [HttpGet]
        [Route("accounts/password/reset/newpass")]
        [EnableCors("AllowOrigin")]
        public Status changePass(string newPass, string mail, string code)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"newPass -> {security.sha256(newPass)}\n mail -> {mail}\n code -> {code} ", "changePass(string newPass, string mail, string code)", ipAddress.ToString());

            Status status;



            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            status = insert.resetPassword(newPass, mail, code);

            return status;




        }

        [HttpGet]
        [Route("get/backgrounds")]
        [EnableCors("AllowOrigin")]
        public List<BackgroundImageStruct> getBackgrounds(string code, string mail)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"mail -> {mail}\n code -> {code} ", "getBackgrounds(string code, string mai)", ipAddress.ToString());
            List<BackgroundImageStruct> backgroundList;



            dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
            backgroundList = select.getBackgrounds();

            return backgroundList;




        }


        [HttpGet]
        [Route("user/get/views")]
        [EnableCors("AllowOrigin")]
        public List<Advertisement> getViews(string mail, string pass)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"mail -> {mail}\n pass -> {security.sha256(pass)} ", "getViews(string mail, string pass)", ipAddress.ToString());
            List<Advertisement> advertisement;
            dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
            advertisement = select.getViews(mail, pass);
            return advertisement;
        }
        [HttpGet]
        [Route("user/get/my/ads")]
        [EnableCors("AllowOrigin")]
        public List<Advertisement> getMyAds(string mail, string pass)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"mail -> {mail}\n pass -> {security.sha256(pass)} ", "getMyAds(string mail, string pass)", ipAddress.ToString());
            List<Advertisement> advertisement;



            dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
            advertisement = select.getMyAds(mail, pass);

            return advertisement;




        }

        

        [HttpGet]
        [Route("user/Firebase")]
        [EnableCors("AllowOrigin")]
        public bool firebase(string text, long userID)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"text -> {text}\n userID -> {userID} ", "firebase(string text, long userID)", ipAddress.ToString());

            try
            {

                communication.sendNotification(text, userID);
                return true;



            }
            catch
            {
                return false;

            }


        }
    }

}