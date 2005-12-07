//
// HttpCacheVaryByParamsCas.cs 
//	- CAS unit tests for System.Web.HttpCacheVaryByParams
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using System.Reflection;
using System.Security.Permissions;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
#if ONLY_1_1
	[Category ("NotDotNet")] // we don't want to duplicate this
#endif
	public class HttpClientCertificateCas : AspNetHostingMinimal {

		private HttpClientCertificate hcc;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			HttpWorkerRequest hwr = new MonoTests.System.Web.MyHttpWorkerRequest ();
			hcc = new HttpContext (hwr).Request.ClientCertificate;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			// from HttpClientCertificateTest
			Assert.AreEqual (0, hcc.BinaryIssuer.Length, "BinaryIssuer");
			Assert.AreEqual (0, hcc.CertEncoding, "CertEncoding");
			Assert.AreEqual (0, hcc.Certificate.Length, "Certificate");
			Assert.AreEqual (String.Empty, hcc.Cookie, "Cookie");
			Assert.AreEqual (0, hcc.Flags, "Flags");
			Assert.IsFalse (hcc.IsPresent, "IsPresent");
			Assert.AreEqual (String.Empty, hcc.Issuer, "Issuer");
			Assert.IsTrue (hcc.IsValid, "IsValid");
			Assert.AreEqual (0, hcc.KeySize, "KeySize");
			Assert.AreEqual (0, hcc.PublicKey.Length, "PublicKey");
			Assert.AreEqual (0, hcc.SecretKeySize, "SecretKeySize");
			Assert.AreEqual (String.Empty, hcc.SerialNumber, "SerialNumber");
			Assert.AreEqual (String.Empty, hcc.ServerIssuer, "ServerIssuer");
			Assert.AreEqual (String.Empty, hcc.ServerSubject, "ServerSubject");
			Assert.AreEqual (String.Empty, hcc.Subject, "Subject");
			DateTime start = DateTime.Now.AddMinutes (1);
			DateTime end = start.AddMinutes (-2);
			// creation time - doesn't update (at least after first call)
			Assert.IsTrue (hcc.ValidFrom < start, "ValidFrom <");
			Assert.IsTrue (hcc.ValidFrom > end, "ValidFrom >");
			Assert.IsTrue (hcc.ValidUntil < start, "ValidUntil <");
			Assert.IsTrue (hcc.ValidUntil > end, "ValidUntil >");

			// NameValueCollection stuff
			Assert.AreEqual (0, hcc.Count, "Count");
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			// no public ctor is available but we know that it's properties don't have any restrictions
			MethodInfo mi = this.Type.GetProperty ("IsPresent").GetGetMethod ();
			Assert.IsNotNull (mi, "get_IsPresent");
			return mi.Invoke (hcc, null);
		}

		public override Type Type {
			get { return typeof (HttpClientCertificate); }
		}
	}
}
