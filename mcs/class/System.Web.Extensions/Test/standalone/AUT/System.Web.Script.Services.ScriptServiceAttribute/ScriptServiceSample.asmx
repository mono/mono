<%@ WebService Language="C#" Class="SimpleWebService" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[ScriptService]
public class SimpleWebService : System.Web.Services.WebService 
{

    [WebMethod]
    public string GetServerTime() 
    {
        string serverTime =
            String.Format("The current time is {0}.", DateTime.Now);

        return serverTime;
    }
    
}
