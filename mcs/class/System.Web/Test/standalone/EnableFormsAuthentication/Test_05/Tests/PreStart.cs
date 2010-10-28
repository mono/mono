using System;
using System.Web;
using System.Web.Security;
using System.Collections.Specialized;

namespace Test_05.Tests
{
	public class PreStart
	{
		public static void FormsAuthenticationSetUp ()
		{
			var nvc = new NameValueCollection ();

			nvc.Add ("loginUrl", null);
			nvc.Add ("defaultUrl", null);

			FormsAuthentication.EnableFormsAuthentication (nvc);
		}
	}
}