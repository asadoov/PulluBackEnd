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
        public TransactionStatusStruct pay([FromBody] TransactionStruct obj)
        {
          
            PaymentInsert insert = new PaymentInsert(Configuration, _hostingEnvironment);
            return insert.updateBalance(obj);
           
        }
        [HttpPost]
        [Route("verify")]
        [EnableCors("AllowOrigin")]
        public VerifyStatusStruct verify([FromBody] VerifyStruct obj)
        {

            PaymentInsert insert = new PaymentInsert(Configuration, _hostingEnvironment);
            return insert.verify(obj);

        }

    }
}
