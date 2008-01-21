<%@ WebService Language="C#" Class="Sys.Net.ErrorHandlingTutorial.WebService" %>
 
using System;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Web.Services.Protocols;
using System.Web.Script.Services;

namespace Sys.Net.ErrorHandlingTutorial
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ScriptService]
    public class WebService : System.Web.Services.WebService
    {

        [WebMethod]
        public string Div(int a, int b)
        {

			if (b == 0)
				throw new DivideByZeroException ("Attempted to divide by zero.");
            
			int division = a / b;

            string result =
                String.Format("The division result is {0}.",
                    division.ToString());

            return result;


        }
       
        
    }
    
    
}
