//
// FormsAuthenticationTest.cs - NUnit Test Cases for FormsAuthentication
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;

using NUnit.Framework;
using MonoTests.SystemWeb.Framework;

namespace MonoTests.System.Web.Security
{
	[TestFixture]
	public class FormsAuthenticationTest
	{
		[Test]
		[Category ("NotDotNet")] // Dot.net url must include Namespace name
		[Category("NunitWeb")]
		public void DefaultValues ()
		{
			new WebTest(new HandlerInvoker (new HandlerDelegate(DefaultValues_delegate))).Run ();
		}

		static public void DefaultValues_delegate ()
		{
			// MS use ".ASPXAUTH" while Mono use ".MONOAUTH"
			string str = FormsAuthentication.FormsCookieName;
			Assert.IsTrue ((str.Length == 9 && str [0] == '.' && str.EndsWith ("AUTH")), "FormsCookieName");
			Assert.AreEqual ("/", FormsAuthentication.FormsCookiePath, "FormsCookiePath");
			Assert.IsFalse (FormsAuthentication.RequireSSL, "RequireSSL");
			Assert.IsTrue (FormsAuthentication.SlidingExpiration, "SlidingExpiration");
#if NET_2_0
			// MSDN: The default is an empty string ("") but null.
			Assert.AreEqual ("", FormsAuthentication.CookieDomain, "CookieDomain");
			Assert.AreEqual (HttpCookieMode.UseDeviceProfile, FormsAuthentication.CookieMode, "CookieMode");
			Assert.IsTrue (FormsAuthentication.CookiesSupported, "CookiesSupported");
			Assert.AreEqual ("/NunitWeb/default.aspx", FormsAuthentication.DefaultUrl);
			Assert.IsFalse (FormsAuthentication.EnableCrossAppRedirects, "EnableCrossAppRedirects");
			Assert.AreEqual ("/NunitWeb/login.aspx", FormsAuthentication.LoginUrl, "LoginUrl");
#endif
		}

		[Test]
		[Category ("NotDotNet")] // Dot.net url must include Namespace name
		[Category("NunitWeb")]
		public void Initialize ()
		{
			new WebTest(new HandlerInvoker (new HandlerDelegate(Initialize_delegate))).Run ();
		}

		static public void Initialize_delegate ()
		{
			// calling Initialize without an HttpContext
			FormsAuthentication.Initialize ();
			// and that doesn't change the default values
			DefaultValues_delegate ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void HashPasswordForStoringInConfigFile_NullPassword ()
		{
			FormsAuthentication.HashPasswordForStoringInConfigFile (null, "MD5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void HashPasswordForStoringInConfigFile_NullPasswordFormat ()
		{
			FormsAuthentication.HashPasswordForStoringInConfigFile ("Mono", null);
		}

		[Test]
		public void HashPasswordForStoringInConfigFile_MD5 ()
		{
			// § (C2-A7)
			string s = Encoding.UTF8.GetString (new byte [2] { 0xC2, 0xA7 });
			Assert.AreEqual ("BD9A4C255DEEC8944D99E01A64C1E322", FormsAuthentication.HashPasswordForStoringInConfigFile (s, "MD5"));

			// ä (C3-A4)
			s = Encoding.UTF8.GetString (new byte [2] { 0xC3, 0xA4 });
			Assert.AreEqual ("8419B71C87A225A2C70B50486FBEE545", FormsAuthentication.HashPasswordForStoringInConfigFile (s, "md5"));
		}

		[Test]
		public void HashPasswordForStoringInConfigFile_SHA1 ()
		{
			// § (C2-A7)
			string s = Encoding.UTF8.GetString (new byte [2] { 0xC2, 0xA7 });
			Assert.AreEqual ("EB2CB244889599F736B6CDD633C5E324F521D1BB", FormsAuthentication.HashPasswordForStoringInConfigFile (s, "SHA1"));

			// ä (C3-A4)
			s = Encoding.UTF8.GetString (new byte [2] { 0xC3, 0xA4 });
			Assert.AreEqual ("961FA22F61A56E19F3F5F8867901AC8CF5E6D11F", FormsAuthentication.HashPasswordForStoringInConfigFile (s, "sha1"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HashPasswordForStoringInConfigFile_SHA256 ()
		{
			FormsAuthentication.HashPasswordForStoringInConfigFile ("mono", "SHA256");
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void RedirectToLoginPage ()
		{
			FormsAuthentication.RedirectToLoginPage ();
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void RedirectToLoginPage_XtraQuery_Null ()
		{
			FormsAuthentication.RedirectToLoginPage (null);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void RedirectToLoginPage_XtraQuery_Empty ()
		{
			FormsAuthentication.RedirectToLoginPage (String.Empty);
		}

		[Test]
		[Category ("NotWorking")] // works on .net
		public void Authenticate ()
		{
			Assert.IsFalse (FormsAuthentication.Authenticate (null, "password"), "null,string");
			Assert.IsFalse (FormsAuthentication.Authenticate ("user", null), "string,null");
			// not throwing
			Assert.IsFalse (FormsAuthentication.Authenticate ("user", "password"), "string,string");
		}
#endif

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			WebTest.Unload();
		}
	}
}
