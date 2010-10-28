using System;
using System.Web;
using System.Web.Security;
using System.Collections.Specialized;

namespace Test_02.Tests
{
	public class PreStart
	{
		public static void FormsAuthenticationSetUp ()
		{
			FormsAuthentication.EnableFormsAuthentication (new NameValueCollection ());
		}
	}
}