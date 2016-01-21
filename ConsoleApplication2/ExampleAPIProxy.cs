using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    internal class ExampleAPIProxy
    {
        private static WebService ExampleAPI = new WebService("http://.../example.asmx");    // DEFAULT location of the WebService, containing the WebMethods

        public static void ChangeUrl(string webserviceEndpoint)
        {
            ExampleAPI = new WebService(webserviceEndpoint);
        }

        public static string ExampleWebMethod(string name, int number)
        {
            ExampleAPI.PreInvoke();

            ExampleAPI.AddParameter("name", name);                    // Case Sensitive! To avoid typos, just copy the WebMethod's signature and paste it
            ExampleAPI.AddParameter("number", number.ToString());     // all parameters are passed as strings
            try
            {
                ExampleAPI.Invoke("ExampleWebMethod");                // name of the WebMethod to call (Case Sentitive again!)
            }
            finally { ExampleAPI.PosInvoke(); }

            return ExampleAPI.ResultString;                           // you can either return a string or an XML, your choice
        }
    }
}
