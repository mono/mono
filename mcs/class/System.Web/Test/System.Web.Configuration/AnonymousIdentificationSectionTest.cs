//
// AnonymousIdentificationSectionTest.cs 
//	- unit tests for System.Web.Configuration.AnonymousIdentificationSection
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using NUnit.Framework;

using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web;
using System.Web.Security;

namespace MonoTests.System.Web.Configuration {

	[TestFixture]
	public class AnonymousIdentificationSectionTest  {

		[Test]
		public void Defaults()
		{
			AnonymousIdentificationSection a = new AnonymousIdentificationSection();

			Assert.AreEqual (HttpCookieMode.UseCookies, a.Cookieless, "A1");
			Assert.AreEqual (".ASPXANONYMOUS", a.CookieName, "A2");
			Assert.AreEqual ("/", a.CookiePath, "A3");
			Assert.AreEqual (CookieProtection.Validation, a.CookieProtection, "A4");
			Assert.AreEqual (false, a.CookieRequireSSL, "A5");
			Assert.AreEqual (true, a.CookieSlidingExpiration, "A6");
			Assert.AreEqual (TimeSpan.Parse ("69.10:40:00"), a.CookieTimeout, "A7");
			Assert.AreEqual (null, a.Domain, "A8");
			Assert.AreEqual (false, a.Enabled, "A9");
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void CookieName_validationFailure ()
		{
			AnonymousIdentificationSection a = new AnonymousIdentificationSection();

			a.CookieName = "";
			Assert.AreEqual ("", a.CookieName, "A1");
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void CookiePath_validationFailure ()
		{
			AnonymousIdentificationSection a = new AnonymousIdentificationSection();

			a.CookiePath = "";
			Assert.AreEqual ("", a.CookiePath, "A1");
		}


		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void CookieTimeout_validationFailure ()
		{
			AnonymousIdentificationSection a = new AnonymousIdentificationSection();

			a.CookieTimeout = TimeSpan.FromSeconds (-30);
			Assert.AreEqual (TimeSpan.FromSeconds (-30), a.CookieTimeout, "A1");
		}
	}

}

