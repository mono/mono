<%@ WebService Language="C#" Class="Samples.AspNet.WebService" %>
 
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
    public class WebService : System.Web.Services.WebService
    {

        [WebMethod]
        public string Add(int a, int b)
        {

            int addition = a + b;
            string result =
                String.Format("The addition result is {0}.",
                    addition.ToString());

            return result;

        }
       
        
    }
    
    
}
