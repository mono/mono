<%@ WebService Language="C#" Class="MyAuthenticationService" %>

using System.Web.Services;
using System.Web.Script.Services;

[ScriptService]
public class MyAuthenticationService  : System.Web.Services.WebService 
{
    [WebMethod]
    public bool Login(string userName, 
        string password, bool createPersistentCookie)
    {
        //Place code here.
        return true;
    }

    [WebMethod]
    public void Logout()
    {
        //Place code here.
    }  
}
