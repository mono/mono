using System;
using System.Web;
using System.Web.Security;
using System.Collections.Specialized;

namespace Test_09.Tests
{
	public class PreStart
	{
		public static void FormsAuthenticationSetUp ()
		{
			var nvc = new NameValueCollection ();

			nvc.Add ("loginUrl", "/myLogin.aspx");
			nvc.Add ("defaultUrl", "/myDefault.aspx");
			FormsAuthentication.EnableFormsAuthentication (nvc);
		}
	}
}