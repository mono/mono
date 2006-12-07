using System;

namespace MainsoftWebApp20
{
	public partial class Global : System.Web.HttpApplication
	{
		void Application_Error (object sender, EventArgs e)
		{
			// Code that runs when an unhandled error occurs
			Exception objErr = Server.GetLastError ().GetBaseException ();
			MonoTests.SystemWeb.Framework.WebTest.RegisterException (objErr);
			Server.ClearError ();
		}
	}
}
