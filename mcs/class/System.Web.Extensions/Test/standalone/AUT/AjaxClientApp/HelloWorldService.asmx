<%@ WebService Language="C#" Class="Samples.AspNet.HelloWorldService" %>
using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;
 
namespace Samples.AspNet {
 
 [WebService(Namespace = "http://tempuri.org/")]
 [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
 [ScriptService]
 public class HelloWorldService  :  System.Web.Services.WebService {
 
   [WebMethod]
   public string HelloWorld(String query)
   {
     string inputString = Server.HtmlEncode(query);
     if(!String.IsNullOrEmpty(inputString))
     {
       return String.Format("Hello, you queried for {0}. The "
         + "current time is {1}", inputString, DateTime.Now);
     }
     else
     {
       return "The query string was null or empty";
     }
   }
 }
}
