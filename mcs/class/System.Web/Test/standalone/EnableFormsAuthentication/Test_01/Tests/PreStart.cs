using System;
using System.Web;
using System.Web.Security;

namespace Test_01.Tests
{
	public class PreStart
	{
		public static void FormsAuthenticationSetUp ()
		{
			FormsAuthentication.EnableFormsAuthentication (null);
		}
	}
}