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
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using RSACriptoGen;
using System.Reflection;

namespace PulluBackEnd.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class androidmobileappController : Controller
    {

        Communication communication;
        Security security;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public IConfiguration Configuration;
        public androidmobileappController(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;

            _hostingEnvironment = hostingEnvironment;
            communication = new Communication(Configuration, _hostingEnvironment);
            security = new Security(Configuration, _hostingEnvironment);
        }
        [HttpGet]
        [Route("testIP")]
        [EnableCors("AllowOrigin")]
        public ActionResult<string> testIP()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            return ipAddress.ToString();
        }

        [HttpPost]
        [Route("user/login")]
        [EnableCors("AllowOrigin")]
        public ActionResult<ResponseStruct<SignInStruct>> Login(long phone, string pass,int platformID = 2)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"phone = {phone}\n pass ->{pass} \n platformID ->{platformID}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());




            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return select.LogIn(phone, pass,platformID);



        }
        [HttpPost]
        [Route("user/get/finance")]
        [EnableCors("AllowOrigin")]
        public ActionResult<ResponseStruct<FinanceStruct>> getFinance(string userToken = "", string requestToken = "")
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"userToken = {userToken}\requestToken ->{requestToken}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());



            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return select.getFinance(userToken, requestToken);



        }

        [HttpPost]
        [Route("user/get/ads")]
        [EnableCors("AllowOrigin")]
        public ActionResult<ResponseStruct<Advertisement>> getAds(string userToken = null, string requestToken = null, int pageNo = 1, int isPaid = 0, int catID = 0)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            string tokens = "";
            if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken))
            {
                tokens = $"userToken = { userToken}\requestToken = {requestToken}";
            }

            communication.log($"{tokens}\ncatID = {catID}\npageNo = {pageNo}\nisPaid = {isPaid}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return Ok(select.GetAdvertisements(userToken, requestToken, pageNo, isPaid, catID));


        }
        //[HttpGet]
        //[Route("check")]
        //[EnableCors("AllowOrigin")]
        //public ActionResult<string> check()
        //{

        //    DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
        //    return Ok(select.GetThumnailImage("a","bbbbb"));


        //}

        [HttpPost]
        [Route("user/search/ads")]
        [EnableCors("AllowOrigin")]
        public ActionResult<ResponseStruct<Advertisement>> SearchForAd(string userToken,string requestToken, string searchQuery,long pageNo=1,long catID=0, int isPaid = 3)
        {

            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            string userData = "";
            if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(requestToken))
            {
                userData = $"userToken = { userToken}\requestToken = {requestToken}";
            }

            communication.log($"{userData}\npageNo = {pageNo}\nisPaid = {isPaid}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return Ok(select.SearchForAds(userToken, requestToken, pageNo, isPaid,searchQuery, catID));


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

            communication.log($"", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
            List<Country> countries = new List<Country>();
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            countries = select.getCountries();

            return countries;
        }
        [HttpGet]
        [Route("get/cities")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<City>> getCities(long countryId)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"countryID -> {countryId}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
            List<City> cities = new List<City>();
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            cities = select.GetCities(countryId);

            return cities;
        }
        [HttpGet]
        [Route("get/interests")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<Interest>> getInterests()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());

            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return select.getInterests();


        }
        [Route("get/professions")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<Profession>> getProfessions()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
            List<Profession> professions = new List<Profession>();
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            professions = select.getProfessions();

            return professions;
        }

        [HttpPost]
        [Route("user/signUp")]
        [EnableCors("AllowOrigin")]
        public ActionResult<Status> signUp(NewUserStruct newUser)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log(@$"mail = {newUser.mail}
name = {newUser.name}
surname = {newUser.surname}
pass = {security.sha256(newUser.pass)}
phone = {newUser.phone}
bDate = {newUser.bDate}
gender = {newUser.gender}
country = {newUser.country}
city = {newUser.city}\n
interestIds = {newUser.interestIds}
otp = {newUser.otp}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());




            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            return insert.SignUp(newUser);






        }

        [HttpPost]
        [Route("user/update/profile")]
        [EnableCors("AllowOrigin")]
        public ActionResult uProfile(UpdateProfileStruct uProfile)
        {

            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log(Newtonsoft.Json.JsonConvert.SerializeObject(uProfile).ToString(), MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);


            return Ok(insert.uProfile(uProfile));



        }



        [HttpGet]
        [Route("user/about")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<Advertisement>> about(long advertID)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"adverID -> {advertID}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());


            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return select.getAdvertById(advertID);






        }
        [HttpPost]
        [Route("user/earnMoney")]
        [EnableCors("AllowOrigin")]
        public ActionResult<Status> earnMoney(int advertID, string userToken, string requestToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"userToken -> {userToken}\requestToken -> {requestToken}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
            Status status;



            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            status = insert.EarnMoney(advertID, userToken, requestToken);
            return Ok(status);





        }
        [HttpGet]
        [Route("verify")]
        [EnableCors("AllowOrigin")]
        public ContentResult verify(int code)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"code -> {security.sha256(code.ToString())}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
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
        public Status ActivateUser(int code)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"code -> {security.sha256(code.ToString())}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());


            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            return insert.verify(code);



        }
        [HttpPost]
        [Route("user/get/statistics")]
        [EnableCors("AllowOrigin")]
        public ActionResult<ResponseStruct<Statistics>> GetStatistics(string userToken, string requestToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"userToken = {userToken}\requestToken ->{requestToken}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
          
           
                    DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
                    return select.GetStatistics(userToken, requestToken);
               


        }


        [HttpPost]
        [Route("user/get/profile")]
        [EnableCors("AllowOrigin")]
        public ActionResult<ResponseStruct<ProfileStruct>> Profile(string userToken, string requestToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"userToken -> {userToken}\n requestToken -> {requestToken}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
           


           
                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
               
            

            return select.Profile(userToken, requestToken);





        }

        [HttpGet]
        [Route("get/age/range")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<AgeRangeStruct>> AgeRange()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
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
        public ActionResult<List<TypeStruct>> AType()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
          
           


                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
              
                return select.AType();






        }

        [HttpGet]
        [Route("get/aCategory")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<CategoryStruct>> ACategory()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
         

                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return select.ACategory();
               



          


        }

        [HttpGet]
        [Route("get/aTariff")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<TariffStruct>> ATariff()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
          

                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return select.aTariff();
               





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
        public Status NewAdvertisement([FromForm] NewAdvertisementStruct obj)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log(Newtonsoft.Json.JsonConvert.SerializeObject(obj).ToString(), MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            // return 
          
            return insert.addNewAdvert(obj);

            

        }
        
        //Yeni girish sistemi uсun yaradilmish servis
        [HttpPost]
        [Route("accounts/send/sms")]
        [EnableCors("AllowOrigin")]
        public Status SendSMS(long phone = 0)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log(@$"phone -> {phone}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());


            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);



            return insert.sendSms(phone);

        }
       

        //Qeydiyyat otp yoxlamasi
        //OLD accounts/verify/otp
        [HttpPost]
        [Route("registration/verify/otp")]
        [EnableCors("AllowOrigin")]
        public Status VerifyOtp(long phone = 0, int otp = 0)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"phone = {phone}\nOTP = {otp}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());


            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);



            return select.verifyOtp(phone, otp);

        }

        //OLD accounts/verify/mobile
        [HttpPost]
        [Route("accounts/update/phone")]
        [EnableCors("AllowOrigin")]
        public Status UpdateUserPhone(string userToken = "", string requestToken = "", long newPhone = 0)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"userToken = {userToken}\nrequestToken = {requestToken}\nnewPhone -> {newPhone}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());


            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);



            return insert.UpdateUserPhone(userToken, requestToken, newPhone);

        }
        [HttpPost]
        [Route("accounts/verify/newPhone")]
        [EnableCors("AllowOrigin")]
        public ActionResult<Status> VerifyUserNewPhone(string userToken = "", string requestToken = "", int phone = 0, int otp = 0)
        {
            //boshluq var -> Kimse kodu bilse api ni calishdirib istediyi nomre yaza biler movcud olamayan bele
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"userToken -> {userToken}\nrequestToken = {requestToken}\nphone = {phone}\ncode = {otp.ToString()}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);


            return insert.VerifyUserNewPhone(userToken, requestToken, phone, otp);



        }
        [HttpPost]
        [Route("password/reset/send/mail")]
        [EnableCors("AllowOrigin")]
        public Status SendMail(string mail)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"mail -> {mail}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());


            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            return insert.sendResetMail(mail);
            // return 

            // status = insert.newAdvertisement(obj);





        }
        //Shifreni unutdum sms ile
        [HttpPost]
        [Route("password/reset/send/sms")]
        [EnableCors("AllowOrigin")]
        public Status SendResetSMS(long phone = 0)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"phone = {phone}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());


            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);



            return insert.SendResetSMS(phone);

        }

        //OLD accounts/password/reset/confirm

        [HttpPost]
        [Route("password/reset/verify/sms/otp")]
        [EnableCors("AllowOrigin")]
        public Status ConfirmCode(string otp, long phone)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"code -> {otp}\n phone-> {phone}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
            Status status;



            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            status = select.VerifySmsOtp(otp, phone);

            return status;




        }
        [HttpPost]
        [Route("password/reset/verify/mail/otp")]
        [EnableCors("AllowOrigin")]
        public Status ConfirmCode(string otp, string mail)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"code -> {otp}\n mail-> {mail}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
            Status status;



            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            status = select.VerifyMailOtp(otp, mail);

            return status;




        }
        //OLD accounts/password/reset/newpass
        [HttpPost]
        [Route("password/reset/bySMS/set")]
        [EnableCors("AllowOrigin")]
        public Status ChangePassBySms(string newPass, long phone, string otp)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"newPass = {security.sha256(newPass)}\nphone -> {phone}\ncode -> {otp.ToString()} ", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());

            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            return insert.ResetPasswordBySms(newPass, phone, otp);

            




        }

        //OLD accounts/password/reset/newpass
        [HttpPost]
        [Route("password/reset/byMail/set")]
        [EnableCors("AllowOrigin")]
        public Status ChangePassByMail(string newPass, string mail, string otp)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"newPass = {security.sha256(newPass)}\nmail -> {mail}\ncode -> {otp.ToString()} ", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());

            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            return insert.ResetPasswordByMail(newPass, mail, otp);






        }

        [HttpGet]
        [Route("get/backgrounds")]
        [EnableCors("AllowOrigin")]
        public List<BackgroundImageStruct> GetBackgrounds()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
           


            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return select.getBackgrounds();

           




        }


        [HttpPost]
        [Route("user/get/views")]
        [EnableCors("AllowOrigin")]
        public ResponseStruct<Advertisement> GetViews(string userToken, string requestToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"userToken -> {userToken}\n requestToken -> {requestToken} ", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());

            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return select.getMyViews(userToken, requestToken);

        }
        [HttpPost]
        [Route("user/get/my/ads")]
        [EnableCors("AllowOrigin")]
        public ResponseStruct<Advertisement> GetMyAds(string userToken, string requestToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"userToken -> {userToken}\n requestToken -> {requestToken} ", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
         



            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return select.getMyAds(userToken, requestToken);






        }
        [HttpPost]
        [Route("user/get/my/ads/viewers")]
        [EnableCors("AllowOrigin")]
        public ResponseStruct<ViewerStruct> GetMyAdViewers(string userToken, string requestToken, int aID)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"userToken = {userToken}\n requestToken = {requestToken} ", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
           



            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            return select.GetMyAdViewers(userToken, requestToken, aID);






        }



        [HttpGet]
        [Route("user/Firebase")]
        [EnableCors("AllowOrigin")]
        public bool firebase(string title, string body, long userID)
        {

            try
            {

                var ipAddress = HttpContext.Connection.RemoteIpAddress;

                communication.log($"title -> {title} \n body -> {body} \n userID -> {userID} ", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());
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
        public ActionResult<Status> uAd(string userToken = "", string requestToken = "", int aID = 0, string aName = "", string aDescription = "", int aPrice = 0)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"userToken -> {userToken} \n requestToken -> {requestToken} \n aID -> {aID} \n aName -> {aName} \n aDescription -> {aDescription} \n aPrice -> {aPrice}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());



            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            return insert.uAd(userToken, requestToken, aID, aName, aDescription, aPrice);



        }
        [HttpPost]
        [Route("user/delete/ad")]
        [EnableCors("AllowOrigin")]
        public ActionResult<Status> deleteAd(string userToken = "", string requestToken = "", int aID = 0)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"userToken -> {userToken} \n requestToken -> {requestToken} \n aID -> {aID} ", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());



            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            return insert.deleteAd(userToken, requestToken, aID);



        }
        [HttpPost]
        [Route("accounts/update/pass")]
        [EnableCors("AllowOrigin")]
        public ActionResult<Status> uPass(string userToken = "", string requestToken = "", string newPass = "")
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            communication.log($"userToken = {userToken} \n requestToken = {requestToken} \n newPass = {newPass}", MethodBase.GetCurrentMethod().Name, ipAddress.ToString());



            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            return insert.uPass(userToken, requestToken, newPass);



        }
        
        [HttpGet]
        [Route("user/pay")]
        [EnableCors("AllowOrigin")]
        public ActionResult<string> signCert(string request)
        {
            /*
            string clearText = @"<request point = ""469""> <menu/> </request>";
            string privateKey = @"-----BEGIN RSA PRIVATE KEY-----
  MIICXQIBAAKBgQCteFbuy0K9Xo/dM3o+ODAioylGIqeWxlZ/2Q6HqRnHRCpEbyjA
  xyb/uh1sHiw10iG0B3ff/gSX5LF1K5Z+9hf3AiP1NBeMHXqpal0ZRVUF+USpiPk5
  LSj3Oi5dlkwh8xpq5Mo5U/u0PHTLMt8rhe6IgoPZp1gfs6l9Ji9I7JpcuQIDAQAB
  AoGAdjwInLgj5CjYy78zeccYX/NvxWMHcUf8WyWZtrN2Y5A9cumFEGhtV24GcdPa
  9FAmMqvIc/6SKOlyXtd3u0+HIuwYD0vfb7N7hrpD2qac60oHfKhjRz2C78xmPK7i
  YzjWLiDUPy8aot/vecTux0XZozssK+UqzhCnvUO2ZLEqQJUCQQD7zLdeFiIX6e1C
  FWveFBiEGwGxxsQ65kfHuiv8TPzdFGYQU4K3n8bKhQt20xGcSc178qCJJE2RByJa
  CJ7LqOCTAkEAsF0gBgEqF6gvamJeCAvXapcY8YgXmyRSC9y9LjCIzg51/UW38ygi
  3dgmdJhfgcHiSM3rkzQSWVgHe5Gb+EU5AwJBAJx1PdMWiaS2VBhl2xqo/frIFStz
  yGaYxC1UfxRMeiqdDDZEzcpvW0RnmxIAYMbuDOJhhmLwzcm51xx+kr0VeEUCQGoZ
  rEFQhAU1bkkfIpjOnusGOcBc8m6oPB/czYczNapZcsxLHC5R0CAMgJ4WaSbEAKFy
  GK43Xm6XkfDaGa4T3wcCQQDT6FU+8+YbbZwP2BXC3cgD1i2KXbZCVtpKqaQtzpd1
  vzLcYIsZmp7jIH1l+1uReyC5rSHMrz0xr6+2RUYoakXi
  -----END RSA PRIVATE KEY-----";

            var bytesToEncrypt = Encoding.UTF8.GetBytes(clearText);

            var encryptEngine = new Pkcs1Encoding(new RsaEngine());

            using (var txtreader = new StringReader(privateKey))
            {
                var keyPair = (AsymmetricCipherKeyPair)new PemReader(txtreader).ReadObject();

                encryptEngine.Init(true, keyPair.Private);
            }

            var encrypted = Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
            return encrypted;
            */


            KeyManager.SetKeyPath($"{_hostingEnvironment.ContentRootPath}/wwwroot/private.pem");
            return KeyManager.GenerateSignature(request, Encoding.UTF8);


        }
    }

}