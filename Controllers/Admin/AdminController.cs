using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PulluBackEnd.Model.Admin;
using PulluBackEnd.Model.Database.Admin;

namespace PulluBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        private readonly IWebHostEnvironment _hostingEnvironment;
        public IConfiguration Configuration;
        public AdminController(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;

            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/Admin
        [HttpGet]
        [Route("user/logIn")]
        [EnableCors("AllowOrigin")]
        public List<AdminStruct> logIn(string username, string pass)
        {
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            List<AdminStruct> user = new List<AdminStruct>();
            user = select.getUser(username, pass);

            return user;
        }
        [HttpPost]
        [Route("user/get/ads")]
        [EnableCors("AllowOrigin")]
        public List<Advertisement> getAds(string username, string pass)
        {
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            List<Advertisement> adsList = new List<Advertisement>();
            adsList = select.getAds(username, pass);

            return adsList;
        }


    }
}
