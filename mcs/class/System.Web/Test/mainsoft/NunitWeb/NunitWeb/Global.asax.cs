using System;

namespace MainsoftWebApp
{
	public partial class Global : System.Web.HttpApplication
	{
#if NET_2_0
		protected void Application_Error (object sender, EventArgs e)
		{
			// Code that runs when an unhandled error occurs
			Exception objErr = Server.GetLastError ().GetBaseException ();
			MonoTests.SystemWeb.Framework.WebTest.RegisterException (objErr);
			Server.ClearError ();
		}

		protected void Application_OnEndRequest (object sender, EventArgs e) {
			// Ensure the headers are sent
			MonoTests.SystemWeb.Framework.WebTest.CurrentTest.SendHeaders ();
		}
#endif
	}
}
