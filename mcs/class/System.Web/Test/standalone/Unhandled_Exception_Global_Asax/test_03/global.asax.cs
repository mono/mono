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
			HttpResponse response = Response;

			if (response != null) {
				string begin = (string)AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER");
				string end = (string)AppDomain.CurrentDomain.GetData ("END_CODE_MARKER");
				response.Write (begin + "<strong>Application error handled</strong>" + end);
			}
			Server.ClearError ();
		}
	}
}
