using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using PulluBackEnd.Model.App.server;
using PulluBackEnd.Model.Database.App;
using PulluBackEnd.Model.Payment;
using RSACriptoGen;

namespace PulluBackEnd.Model.Database.Payment.Database
{
    public class PaymentOperations
    {
        Timer aTimer;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public IConfiguration Configuration;
        public PaymentOperations(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;

            _hostingEnvironment = hostingEnvironment;
        }

        public Status AzercellVeify(long mobile,string pass,long payMobile)
        {
            Status status = new Status();
            try
            {
                DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
                if (select.getUserIdByMobile(mobile, pass) > 0 && payMobile.ToString().Length == 9)
                {




                    DateTime now = DateTime.Now;



                    string smsXML = @$"<request point=""3587"">
<advanced function=""check"" service=""418"">
 <attribute name=""id1"" value=""{payMobile}""/></advanced >
  </request>";
                    KeyManager.SetKeyPath($"{_hostingEnvironment.ContentRootPath}/wwwroot/private.pem");
                    string certificate = KeyManager.GenerateSignature(smsXML, Encoding.UTF8);
                    var smsContent = new StringContent(smsXML, Encoding.UTF8, "text/xml");
                    //smsContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");

                    string smsUrl = $"https://test.smartpay.az/external/extended";
                    HttpClient smsClient = new HttpClient();
                    smsClient.DefaultRequestHeaders.Add("paylogic-signature", certificate);

                    var smsRslt = smsClient.PostAsync(smsUrl, smsContent).Result;
                    var smsResp = smsRslt.Content.ReadAsStringAsync().Result;

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(smsResp);
                    XmlNodeList parentNode = xmlDoc.GetElementsByTagName("result");
                   
                    foreach (XmlNode childrenNode in parentNode)
                    {
                        status.response = Convert.ToInt32(childrenNode.Attributes["service"].Value);
                    }
                    if (status.response>1)
                    {
                        status.response = 3; // smartpay error
                    }
                }
                else
                {
                    status.response = 2;//access danied
                }
                
            }
            catch
            {

                status.response = 3; // error
            }
            return status;
        }
        public void checkRequestStatus(object source,ElapsedEventArgs e,long paymentID,long userID,long amount) {
            Debug.WriteLine("GIRDI");
            PaymentInsert pInsert = new PaymentInsert(Configuration, _hostingEnvironment);

            string smsXML = @$"<request point = ""3587""><status id=""{paymentID}""/></request >";
            KeyManager.SetKeyPath($"{_hostingEnvironment.ContentRootPath}/wwwroot/private.pem");
            string certificate = KeyManager.GenerateSignature(smsXML, Encoding.UTF8);
            var smsContent = new StringContent(smsXML, Encoding.UTF8, "text/xml");
            //smsContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");

            string smsUrl = $"https://smartpay.az/external/extended";
            HttpClient smsClient = new HttpClient();
            smsClient.DefaultRequestHeaders.Add("paylogic-signature", certificate);

            var smsRslt = smsClient.PostAsync(smsUrl, smsContent).Result;
            var smsResp = smsRslt.Content.ReadAsStringAsync().Result;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(smsResp);
            XmlNodeList parentNode = xmlDoc.GetElementsByTagName("result");

            foreach (XmlNode childrenNode in parentNode)
            {
                int state = Convert.ToInt32(childrenNode.Attributes["state"].Value);
                int substate = Convert.ToInt32(childrenNode.Attributes["substate"].Value);
                int final = Convert.ToInt32(childrenNode.Attributes["final"].Value);
                if (final == 1)
                {
                    aTimer.Stop();
                    aTimer.Enabled = false;

                    pInsert.UpdateTransaction(paymentID, state, substate, final);
                    switch (state)
                    {
                        case 60:
                            pInsert.WithdrawUserBalance(userID,amount);
                            break;
                        default:

                            break;
                    }
                   
                    //status.response = 1;//ugurlu
                  
                }
               
                
            }

        }
        public async Task StartCheckRequestTimer(long paymentID,long userID,long amount) {
            aTimer = new System.Timers.Timer(2000);
            aTimer.Elapsed += (sender, e) => checkRequestStatus(sender, e, paymentID,userID,amount); ;
           // aTimer.Interval = 3600000;
            aTimer.Enabled = true;
        }
        
        public Status WithdrawFunds(long mobile, string pass, long account,long serviceID,long amount)
        {
            Status status = new Status();
            try
            {
                //(amount / 100) % 1 == 0
                if (amount>100)
                {


                    PaymentSelect pSelect = new PaymentSelect(Configuration, _hostingEnvironment);
                    long smartpayServiceID = pSelect.GetSmartpayServiceID(serviceID);
                    if (smartpayServiceID > 0)
                    {
                        DbSelect select = new DbSelect(Configuration, _hostingEnvironment);
                        long userID = select.getUserIdByMobile(mobile, pass);


                        if (userID > 0)
                        {
                            decimal earningValue = pSelect.GetUserEarningValue(userID);
                            if (earningValue > amount/100)
                            {
                                if (account.ToString().Length == 9)
                                {

                                    PaymentInsert pInsert = new PaymentInsert(Configuration, _hostingEnvironment);
                                    long paymentID = pInsert.InsertTransaction(userID, serviceID, amount, account);
                                    if (paymentID > 0)
                                    {
                                        DateTime now = DateTime.Now;



                                        string smsXML = @$"<request point=""3587"">
<payment account = ""{account}"" check = ""24776"" date = ""{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszz00")}""
id = ""{paymentID}"" service = ""{smartpayServiceID}"" source = ""CASH"" sum = ""{amount}""> <attribute name = ""id2"" value = ""pack""/>
            
                </payment>
            </request>";
                                        KeyManager.SetKeyPath($"{_hostingEnvironment.ContentRootPath}/wwwroot/private.pem");
                                        string certificate = KeyManager.GenerateSignature(smsXML, Encoding.UTF8);
                                        var smsContent = new StringContent(smsXML, Encoding.UTF8, "text/xml");
                                        //smsContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");

                                        string smsUrl = $"https://smartpay.az/external/extended";
                                        HttpClient smsClient = new HttpClient();
                                        smsClient.DefaultRequestHeaders.Add("paylogic-signature", certificate);

                                        var smsRslt = smsClient.PostAsync(smsUrl, smsContent).Result;
                                        var smsResp = smsRslt.Content.ReadAsStringAsync().Result;

                                        XmlDocument xmlDoc = new XmlDocument();
                                        xmlDoc.LoadXml(smsResp);
                                        XmlNodeList parentNode = xmlDoc.GetElementsByTagName("result");
                                        if (parentNode.Count>0)
                                        {
                                            foreach (XmlNode childrenNode in parentNode)
                                            {
                                                int state = Convert.ToInt32(childrenNode.Attributes["state"].Value);
                                                int substate = Convert.ToInt32(childrenNode.Attributes["substate"].Value);
                                                int final = Convert.ToInt32(childrenNode.Attributes["final"].Value);
                                                if (state == 60 && substate == 0 && final == 1)
                                                {
                                                    pInsert.UpdateTransaction(paymentID, state, substate, final);
                                                    pInsert.WithdrawUserBalance(userID, amount);
                                                    status.response = 1;//ugurlu
                                                                        //Debug.WriteLine("");

                                                }
                                                else if (state == 0 || state == 30 || state == 40 || state == 70)
                                                {
                                                    pInsert.UpdateTransaction(paymentID, state, substate, final);
                                                    StartCheckRequestTimer(paymentID, userID, amount);
                                                    status.response = 2;//gozlemededi

                                                }
                                                else
                                                {
                                                    pInsert.UpdateTransaction(paymentID, state, substate, final);
                                                    status.response = 3;//odenish xetasi 
                                                }
                                            }
                                        }
                                        else
                                        {
                                            status.response = 4; // signature and etc
                                            status.responseString = "Error";
                                        }
                                       
                                    }
                                    else
                                    {
                                        status.response = 4; // error
                                        status.responseString = "Error";
                                    }

                                }
                                else
                                {
                                    status.response = 5;//wrong payNumber
                                    status.responseString = "check account";
                                }
                            }

                            else
                            {
                                status.response = 6;//access danied
                                status.responseString = "not enough earning balance";
                            }



                        }
                        else
                        {
                            status.response = 7;//access danied
                            status.responseString = "access danied";
                        }
                    }
                    else
                    {
                        status.response = 8; // service not found
                        status.responseString = "Service not found";
                    }
                }
                else
                {
                    status.response = 9; // service not found
                    status.responseString = "wrong amount";
                }


            }
            catch (Exception ex)
            {

                status.response = 4; // error
                status.responseString = $"Internal error: {ex.Message}";
            }
            return status;
        }

    }
}
