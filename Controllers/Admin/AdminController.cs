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
        [Route("logIn")]
        [EnableCors("AllowOrigin")]
        public List<AdminStruct> logIn(string username, string pass)
        {
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            List<AdminStruct> user = new List<AdminStruct>();
            user = select.logIn(username, pass);

            return user;
        }
        [HttpPost]
        [Route("get/ads")]
        [EnableCors("AllowOrigin")]
        public List<Advertisement> getAds(string username, string pass)
        {
            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            List<Advertisement> adsList = new List<Advertisement>();
            adsList = select.getAds(username, pass);

            return adsList;
        }
        [HttpPost]
        [Route("ads/activate")]
        [EnableCors("AllowOrigin")]
        public List<Status> activateAds(string username, string pass, int aID, int isActive)
        {
            DbInsert insert = new DbInsert(Configuration, _hostingEnvironment);
            List<Status> status = new List<Status>();
            status = insert.activateAds(username, pass, aID, isActive);

            return status;
        }
        [HttpGet]
        [Route("get/logs")]
        [EnableCors("AllowOrigin")]
        public List<LogStruct> getLogs(string username, string pass)
        {


            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            List<LogStruct> logList = new List<LogStruct>();
            logList = select.getLogs(username, pass);

            return logList;


        }
        [HttpGet]
        [Route("get/logs/pretty")]
        [EnableCors("AllowOrigin")]
        public ContentResult getLogsPretty(string username, string pass)
        {


            DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
            List<LogStruct> logList = new List<LogStruct>();
            logList = select.getLogs(username, pass);
            string html = "";
            foreach (var item in logList)
            {
                html += @$"<tr>
    <td>{item.ipAdress}</td>
    <td>{item.log}</td>
<td>{item.functionName}</td>
<td>{item.cdate}</td>
  </tr>";
            }
            return base.Content(@"<html><head><style>
table, th, td {
  border: 1px solid black;
}
th, td {
  padding: 10px;
}
</style><meta charset = 'UTF-8' ></head>" +
               @$"<table>
  <tr>
    <th>IP Adress</th>
    <th>Log</th>
<th>Function name</th>
<th>Created</th>
  </tr>
  {html}
</table>", "text/html");

        }

    }
}
