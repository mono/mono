using System;
using System.Web;
using System.Web.Security;
using System.Collections.Specialized;

namespace Test_07.Tests
{
	public class PreStart
	{
		public static void FormsAuthenticationSetUp ()
		{
			var nvc = new NameValueCollection ();

			nvc.Add ("loginUrl", "/myLogin.aspx");
			nvc.Add ("defaultUrl", "/myDefault.aspx");
			nvc.Add ("cookieDomain", "MyCookieDomain.com");

			FormsAuthentication.EnableFormsAuthentication (nvc);
		}
	}
}