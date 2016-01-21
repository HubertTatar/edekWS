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
    class Sender
    {
        public XDocument ResponseSOAP = XDocument.Parse("<root/>");
        public string Url = "https://test-bramka.edeklaracje.gov.pl/uslugi/dokumenty/";
        public string Method = "sendDocument";


        private void Invoke(string methodName, bool encode)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };


            String path = @"D:\testlok\wynik\IFT3\70651_IFT1.xml.xades";
            //String path = @"D:\CGI\BGZ\Prace\2015-11-06_ifty\Testy\wynik\IFT1\200166_IFT1.xml.xades";
            byte[] signedFile = System.IO.File.ReadAllBytes(path);
            string res = Convert.ToBase64String(signedFile);
            //Convert.FromBase64String(string)

            AssertCanInvoke(methodName);
            string soapStr =
                @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""https://bramka.e-deklaracje.mf.gov.pl/xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <xsd:sendDocument>
         <xsd:document>{0}</xsd:document>
      </xsd:sendDocument>
   </soapenv:Body>
</soapenv:Envelope>";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            req.Credentials = CredentialCache.DefaultCredentials;
            req.Headers.Add("SOAPAction", "\"urn:" + methodName + "\"");
            req.ContentType = "text/xml;charset=\"utf-8\"";
            req.Accept = "text/xml";
            req.Method = "POST";

            using (Stream stm = req.GetRequestStream())
            {
                string postValues = "";
                //foreach (var param in Params)
                //{
                //    if (encode) postValues += string.Format("<{0}>{1}</{0}>", HttpUtility.HtmlEncode(param.Key), HttpUtility.HtmlEncode(param.Value));
                //    else postValues += string.Format("<{0}>{1}</{0}>", param.Key, param.Value);
                //}

                soapStr = string.Format(soapStr, res);
                Console.WriteLine(soapStr);
                //System.IO.File.WriteAllText(@"D:\testlok\wynik\IFT3\70651_IFT1.xml.xades_debug_app", soapStr);
                using (StreamWriter stmw = new StreamWriter(stm))
                {
                    stmw.Write(soapStr);
                }
            }
            String reqStre = req.GetRequestStream().ToString();
            System.IO.File.WriteAllText(path + "dmp", soapStr);
            using (StreamReader responseReader = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                string result = responseReader.ReadToEnd();
                ResponseSOAP = XDocument.Parse(Utils.UnescapeString(result));
                Console.WriteLine(ResponseSOAP.ToString());
                //ExtractResult(methodName);
            }
        }

        private void AssertCanInvoke(string methodName = "")
        {
            if (Url == String.Empty)
                throw new ArgumentNullException("You tried to invoke a webservice without specifying the WebService's URL.");
            if ((methodName == "") && (Method == String.Empty))
                throw new ArgumentNullException("You tried to invoke a webservice without specifying the WebMethod.");
        }
    }
}
