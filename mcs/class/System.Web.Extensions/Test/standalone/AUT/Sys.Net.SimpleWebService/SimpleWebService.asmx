<%@ WebService Language="C#" Class="Samples.AspNet.SimpleWebService" %>

using System;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Web.Services.Protocols;
using System.Web.Script.Services;
 
namespace Samples.AspNet
{

    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ScriptService]
    public class SimpleWebService : System.Web.Services.WebService
    {

        [WebMethod]
        public string EchoInput(String input)
        {
            string inputString = Server.HtmlEncode(input);
            if (!String.IsNullOrEmpty(inputString))
            {
                return String.Format("You entered {0}. The "
                  + "current time is {1}.", inputString, DateTime.Now);
            }
            else
            {
                return "The input string was null or empty.";
            }
        }
        
    }
    
}
