<%@ WebService Language="C#" Class="Sys.Net.WebServiceProxy.WebService" %>
 
using System;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Web.Services.Protocols;
using System.Web.Script.Services;

namespace Sys.Net.WebServiceProxy
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ScriptService]
    public class WebService : System.Web.Services.WebService
    {

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public string GetGreetings(string greeting, 
               string name)
        {
            return greeting + " " + name;
        }

     
        [WebMethod]
        [ScriptMethod(UseHttpGet = false)]
        public string PostGreetings(string greeting,
               string name)
        {
            return greeting + " " + name;
        }
         
        [WebMethod]
        public string GetServerTime()
        {

            string serverTime =
                String.Format("The current time is {0}.", DateTime.Now);

            return serverTime;
           
        }
       
        
    }
    
    
}
