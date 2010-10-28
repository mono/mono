using System;
using System.Web;
using System.Web.Security;
using System.Collections.Specialized;

namespace Test_04.Tests
{
	public class PreStart
	{
		public static void FormsAuthenticationSetUp ()
		{
			var nvc = new NameValueCollection ();

			nvc.Add ("loginUrl", String.Empty);
			nvc.Add ("defaultUrl", String.Empty);

			FormsAuthentication.EnableFormsAuthentication (nvc);
		}
	}
}