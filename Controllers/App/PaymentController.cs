using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PulluBackEnd.Model.Payment;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using PulluBackEnd.Model.Database.Payment;
using PulluBackEnd.Model.App.server;
using PulluBackEnd.Model.Database.Payment.Database;

namespace PulluBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {

        private readonly IWebHostEnvironment _hostingEnvironment;
        public IConfiguration Configuration;
        public PaymentController(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;

            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/Default
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Default/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Default
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Default/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        // POST: api/Default
        [HttpPost]
        [Route("pay")]
        [EnableCors("AllowOrigin")]
        public TransactionStatusStruct UpBalance([FromBody] TransactionStruct obj)
        {
          
            PaymentInsert insert = new PaymentInsert(Configuration, _hostingEnvironment);
            return insert.UpBalance(obj);
           
        }
        [HttpPost]
        [Route("verify")]
        [EnableCors("AllowOrigin")]
        public VerifyStatusStruct verify([FromBody] VerifyStruct obj)
        {

            PaymentSelect select = new PaymentSelect(Configuration, _hostingEnvironment);
            return select.Verify(obj);

        }
        [HttpGet]
         [Route("get/withdraw/services")]
         [EnableCors("AllowOrigin")]
         public ActionResult<ResponseStruct<WithdrawService>> GetWithdrawServices()
         {
            PaymentSelect select = new PaymentSelect(Configuration, _hostingEnvironment);
            return select.GetWithdrawServices();
         }
       
         [HttpGet]
         [Route("withdraw/mobile/operators/verify")]
         [EnableCors("AllowOrigin")]
         public ActionResult<Status> OperatorVerify(long mobile,string pass,long payMobile,int serviceID)
         {
            
            if (serviceID == 418)
            {
                PaymentOperations pOperations = new PaymentOperations(Configuration, _hostingEnvironment);
               return pOperations.AzercellVeify(mobile, pass,payMobile);
                
            }

            return new Status();
         }


        [HttpPost]
        [Route("withdraw")]
        [EnableCors("AllowOrigin")]
        public ActionResult<Status> WithdrawFunds(long mobile, string pass, long account,long serviceID,long amount)
        {
            PaymentOperations pOperations = new PaymentOperations(Configuration, _hostingEnvironment);
            return pOperations.WithdrawFunds(mobile, pass, account, serviceID, amount);


        }
    }
}
