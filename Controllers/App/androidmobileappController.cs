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
using PulluBackEnd.Model.App.client;
using PulluBackEnd.Model.App.server;
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
            communication = new Communication(Configuration, _hostingEnvironment);
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



                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
                user = select.LogIn(mail, pass);

                return user;
            }
        }
        [HttpPost]
        [Route("user/get/finance")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<FinanceStruct>> getFinance(string mail = "", string pass = "")
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"mail -> {mail} \n pass ->{security.sha256(pass)}", "getFinance(string mail, string pass)", ipAddress.ToString());



            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return select.getFinance(mail, pass);



        }

        [HttpGet]
        [Route("user/get/ads")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<Advertisement>> getAds(string mail=null, string pass=null,int pageNo=1, int isPaid=0, int catID=0)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"mail -> {mail}\npass ->{security.sha256(pass)}\ncatID -> {catID}", "getAds(string mail, string pass, int catID)", ipAddress.ToString());
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return Ok(select.Advertisements(mail, pass,pageNo, isPaid, catID));


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
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
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
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
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
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
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
                                profession ->{profession}
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
        public ActionResult uProfile(UpdateProfileStruct uProfile)
        {

            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log(Newtonsoft.Json.JsonConvert.SerializeObject(uProfile).ToString(), "uProfile(UpdateProfileStruct uProfile)", ipAddress.ToString());
            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);


            return Ok(insert.uProfile(uProfile));



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

                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
                if (advertID > 0) advert = select.getAdvertById(advertID, mail, pass);

                return advert;

            }
            return advert;




        }
        [HttpPost]
        [Route("earnMoney")]
        [EnableCors("AllowOrigin")]
        public ActionResult<Status> earnMoney(int advertID, string mail, string pass)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"adverID -> {advertID}\nmail -> {mail}\npass ->{security.sha256(pass)}", "earnMoney(int advertID, string mail, string pass)", ipAddress.ToString());
            Status status;



            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            status = insert.EarnMoney(advertID, mail, pass);
            return Ok(status);





        }
        [HttpGet]
        [Route("verify")]
        [EnableCors("AllowOrigin")]
        public ContentResult verify(int code)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"code -> {security.sha256(code.ToString())}", "verify(int code)", ipAddress.ToString());
            string html;
            try
            {


                DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
                Status status = insert.verify(code);

                if (status.response == 0)
                {
                    html = @"<html><head><meta charset = 'UTF-8' ></head>" +
               "<center><h2>Bildiriş<br>Təşəkkürlər, siz e-poçtunuzu təstiq etdiniz!</h2></center></html>";
                    return base.Content("", "text/html");
                }
                else
                {
                    html = @"<html><head><meta charset = 'UTF-8'></head><center><h2>Bildiriş<br>Təəssüfki e-poçtunuz təstiq olunmadı</h2></center></html>";
                }




            }
            catch (Exception ex)
            {
                html = @$"<html><head><meta charset = 'UTF-8'><center><h2>Bildiriş<br>Server xətası:{ex.Message}</h2></center></html>";


            }
            return base.Content($"{html}", "text/html");



        }
        [HttpGet]
        [Route("accounts/activate/user")]
        [EnableCors("AllowOrigin")]
        public Status activateUser(int code)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"code -> {security.sha256(code.ToString())}", "activateUser(int code)", ipAddress.ToString());


            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            return insert.verify(code);



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
                    DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
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
                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
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


                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
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


                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
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


                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
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


                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
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

            communication.log($"mail -> {mail}", "sendMail(string mail)", ipAddress.ToString());
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
        [HttpPost]
        [Route("accounts/send/sms/code")]
        [EnableCors("AllowOrigin")]
        public Status sendResetSMS(string phone = "")
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log(@$"phone -> {phone}", "sendResetSMS(int phone)", ipAddress.ToString());


            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);



            return insert.sendResetSMS(phone);

        }





        [HttpPost]
        [Route("accounts/verify/mobile")]
        [EnableCors("AllowOrigin")]
        public Status verifyMobile(string mail = "", string pass = "", string newPhone = "")
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log(@$"mail -> {mail}\npass -> {security.sha256(pass)}\nnewPhone -> {newPhone}", "verifyMobile(string mail,string pass,int newPhone)", ipAddress.ToString());


            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);



            return insert.verifyMobile(mail, pass, newPhone);

        }
        [HttpPost]
        [Route("accounts/update/phone")]
        [EnableCors("AllowOrigin")]
        public ActionResult<Status> uPhone(string mail = "", string pass = "", string phone = "", int code = 0)
        {
            //boshluq var -> Kimse kodu bilse api ni calishdirib istediyi nomre yaza biler movcud olamayan bele
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"phone -> {phone}\ncode -> {security.sha256(code.ToString())}", "uPhone(int phone,int code)", ipAddress.ToString());
            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);


            return insert.uPhone(mail, pass, phone, code);



        }





        [HttpGet]
        [Route("accounts/password/reset/confirm")]
        [EnableCors("AllowOrigin")]
        public Status confirmCode(string code, string login)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"code -> {code}\n login-> {login}", "confirmCode(string code, string login)", ipAddress.ToString());
            Status status;



            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            status = select.checkUserToken(code, login);

            return status;




        }


        [HttpGet]
        [Route("accounts/password/reset/newpass")]
        [EnableCors("AllowOrigin")]
        public Status changePass(string newPass, string login, string code)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"newPass -> {security.sha256(newPass)}\n login -> {login}\n code -> {security.sha256(code.ToString())} ", "changePass(string newPass, string mail, string code)", ipAddress.ToString());

            Status status;



            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            status = insert.resetPassword(newPass, login, code);

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



            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
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
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
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



            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            advertisement = select.getMyAds(mail, pass);

            return advertisement;




        }



        [HttpGet]
        [Route("user/Firebase")]
        [EnableCors("AllowOrigin")]
        public bool firebase(string title, string body, long userID)
        {

            try
            {

                var ipAddress = HttpContext.Connection.RemoteIpAddress;

                communication.log($"title -> {title} \n body -> {body} \n userID -> {userID} ", "firebase(string text, long userID)", ipAddress.ToString());
                communication.sendPushNotificationAsync(title, body, userID);
                communication.sendNotificationAsync(title, body, userID);

                return true;



                return true;



            }
            catch
            {
                return false;

            }


        }
        [HttpPost]
        [Route("user/update/ad")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<FinanceStruct>> uAd(string mail = "", string pass = "",int aID=0,string aName="",string aDescription="")
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"mail -> {mail} \n pass ->{security.sha256(pass)}", "uAd(string mail = '', string pass = '',int aID=0,string aName='',string aDescription='')", ipAddress.ToString());



            DbInsert select = new DbInsert(Configuration, _hostingEnvironment);
            return select.getFinance(mail, pass);



        }

    }

}