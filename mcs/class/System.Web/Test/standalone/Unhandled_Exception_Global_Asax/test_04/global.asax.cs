using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;

namespace TestWebApp
{
	public partial class Global : System.Web.HttpApplication
	{
		protected virtual void Application_Error (Object sender, EventArgs e)
		{
			Response.Redirect ("http://example.com/");
		}
	}
}
