using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication2
{
    class edekSender : AbstractSender
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(edekSender));
        public string Url = "https://test-bramka.edeklaracje.gov.pl/uslugi/dokumenty/";
        public string methodName = "sendDocument";
        public string RequestUpo = "requestUPO";



        string soapStr = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""https://bramka.e-deklaracje.mf.gov.pl/xsd""><soapenv:Header/><soapenv:Body><xsd:sendDocument><xsd:document>{0}</xsd:document></xsd:sendDocument></soapenv:Body></soapenv:Envelope>";


        public edekSender()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            Console.WriteLine("Wysyłam do: " + Url);
        }
        public override void send(Deklaracja deklaracja)
        {
            try
            {
                XDocument ResponseSOAP = XDocument.Parse("<root/>");
                //log.Info(asd);
                byte[] signedFile = System.IO.File.ReadAllBytes(deklaracja.filePath);
                string res = Convert.ToBase64String(signedFile);

                AssertCanInvoke(methodName);

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
                req.Credentials = CredentialCache.DefaultCredentials;
                req.Headers.Add("SOAPAction", "\"urn:" + methodName + "\"");
                req.ContentType = "text/xml;charset=\"utf-8\"";
                req.Accept = "text/xml";
                req.Method = "POST";

                using (Stream stm = req.GetRequestStream())
                {
                    soapStr = string.Format(soapStr.Replace("\r\n", "").Trim(), res);
                    //log.Info(soapStr);
                    //Console.WriteLine(soapStr);
                    using (StreamWriter stmw = new StreamWriter(stm))
                    {
                        stmw.Write(soapStr);
                    }
                }

                using (StreamReader responseReader = new StreamReader(req.GetResponse().GetResponseStream()))
                {
                    string result = responseReader.ReadToEnd();
                    ResponseSOAP = XDocument.Parse(SoapUtils.UnescapeString(result));
                    //log.Info(ResponseSOAP.ToString());
                }


                XNamespace ns = "https://bramka.e-deklaracje.mf.gov.pl/xsd";

                string refId = ResponseSOAP.Descendants(ns + "refId").First().Value;
                string status = ResponseSOAP.Descendants(ns + "status").First().Value;
                string statusOpis = ResponseSOAP.Descendants(ns + "statusOpis").First().Value;
                deklaracja.refId = refId;
                deklaracja.status = status;
                deklaracja.message = statusOpis;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("BASE");
                Console.WriteLine(e.GetBaseException());
            }
        }

        public override string getUpo(Deklaracja deklaracja)
        {
            XDocument ResponseSOAP = XDocument.Parse("<root/>");

            string requestUpoXmlStr = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""https://bramka.e-deklaracje.mf.gov.pl/xsd""><soapenv:Header/><soapenv:Body><xsd:requestUPO><xsd:refId>{0}</xsd:refId></xsd:requestUPO></soapenv:Body></soapenv:Envelope>";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            req.Credentials = CredentialCache.DefaultCredentials;
            req.Headers.Add("SOAPAction", "\"urn:" + RequestUpo + "\"");
            req.ContentType = "text/xml;charset=\"utf-8\"";
            req.Accept = "text/xml";
            req.Method = "POST";


            using (Stream stm = req.GetRequestStream())
            {
                requestUpoXmlStr = string.Format(requestUpoXmlStr, deklaracja.refId);
                using (StreamWriter stmw = new StreamWriter(stm))
                {
                    stmw.Write(requestUpoXmlStr);
                }
            }

            using (StreamReader responseReader = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                string result = responseReader.ReadToEnd();
                //log.Info(result);
                ResponseSOAP = XDocument.Parse(result);
                //log.Info(ResponseSOAP.ToString());
            }

            XNamespace ns = "https://bramka.e-deklaracje.mf.gov.pl/xsd";

            string status = ResponseSOAP.Descendants(ns + "status").First().Value;
            string statusOpis = ResponseSOAP.Descendants(ns + "statusOpis").First().Value;
            string upo = ResponseSOAP.Descendants(ns + "upo").First().Value;

            deklaracja.status = status;
            deklaracja.message = statusOpis;

            return upo;
        }

        private void AssertCanInvoke(string methodName = "")
        {
            if (Url == String.Empty)
                throw new ArgumentNullException("You tried to invoke a webservice without specifying the WebService's URL.");
            if ((methodName == "") && (methodName == String.Empty))
                throw new ArgumentNullException("You tried to invoke a webservice without specifying the WebMethod.");
        }
    }
}