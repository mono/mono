
<%@ WebService Language="C#" Class="Samples.AspNet.ServerTime" %>

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
    public class ServerTime : System.Web.Services.WebService
    {

        [WebMethod]
        public string GetServerTime()
        {
           return String.Format("The server time is {0}.", 
               new DateTime(2007,1,1)); 
        }
        
    }
    
}
