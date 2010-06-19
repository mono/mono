using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

public partial class MyLoginPage : Page
{
	protected void loginControl_Authenticate(object sender, AuthenticateEventArgs e)
	{
		e.Authenticated = FormsAuthentication.Authenticate(loginControl.UserName, loginControl.Password);
        }
}
