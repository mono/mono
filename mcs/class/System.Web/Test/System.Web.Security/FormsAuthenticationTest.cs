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
using System.Web.Security;

using NUnit.Framework;

namespace MonoTests.System.Web.Security {

	[TestFixture]
	public class FormsAuthenticationTest {

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
			Assert.AreEqual ("8419B71C87A225A2C70B50486FBEE545", FormsAuthentication.HashPasswordForStoringInConfigFile (s, "MD5"));
		}

		[Test]
		public void HashPasswordForStoringInConfigFile_SHA1 ()
		{
			// § (C2-A7)
			string s = Encoding.UTF8.GetString (new byte [2] { 0xC2, 0xA7 });
			Assert.AreEqual ("EB2CB244889599F736B6CDD633C5E324F521D1BB", FormsAuthentication.HashPasswordForStoringInConfigFile (s, "SHA1"));

			// ä (C3-A4)
			s = Encoding.UTF8.GetString (new byte [2] { 0xC3, 0xA4 });
			Assert.AreEqual ("961FA22F61A56E19F3F5F8867901AC8CF5E6D11F", FormsAuthentication.HashPasswordForStoringInConfigFile (s, "SHA1"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HashPasswordForStoringInConfigFile_SHA256 ()
		{
			FormsAuthentication.HashPasswordForStoringInConfigFile ("mono", "SHA256");
		}
	}
}
