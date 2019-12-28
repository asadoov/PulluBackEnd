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
        public ActionResult<List<User>> log_in(string username, string pass)
        {
            List<User> user = new List<User>();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pass))
            {

                return user;
            }
            else
            {



                dbSelect select = new dbSelect(Configuration, _hostingEnvironment);
                user = select.Log_in(username, pass);

                return user;
            }
        }

        [HttpGet]
        [Route("user/getAds")]
        [EnableCors("AllowOrigin")]
        public ActionResult<List<Ads>> getAds(string username, string pass)
        {
            List<Ads> ads = new List<Ads>();
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
                id="sa"
               
            }); 
        }

    }
}