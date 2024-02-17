//
// HttpClientCertificateTest.cs
//	- Unit tests for System.Web.HttpClientCertificate
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

using System;
using System.Threading;
using System.Web;

using NUnit.Framework;

namespace MonoTests.System.Web {

	// copied from Miguel's FakeHttpWorkerRequest in HttpRequestTest
	class MyHttpWorkerRequest : HttpWorkerRequest {

		private bool mirror;
		private bool alternate;
		private bool override_cc;

		public MyHttpWorkerRequest ()
		{
		}
		
		public override string GetUriPath()
		{
			return "/uri.aspx";
		}

		public override string GetQueryString()
		{
			return "GetQueryString";
		}

		public override string GetRawUrl()
		{
			return "/bb.aspx";
		}

		public override string GetHttpVerbName()
		{
			return "GET";
		}

		public override string GetHttpVersion()
		{
			return "HTTP/1.1";
		}

		public override int GetRemotePort()
		{
			return 1010;
		}

		public override string GetLocalAddress()
		{
			return "localhost";
		}

		public override string GetRemoteName ()
		{
			return "RemoteName";
		}

		public override string GetRemoteAddress ()
		{
			return "RemoteAddress";
		}

		public override int GetLocalPort()
		{
			return 2020;
		}

		public override void SendStatus(int s, string x)
		{
		}

		public override void SendKnownResponseHeader(int x, string j)
		{
		}

		public override void SendUnknownResponseHeader(string a, string b)
		{
		}
	
		public override void SendResponseFromMemory(byte[] arr, int x)
		{
		}

		public override void SendResponseFromFile(string a, long b , long c)
		{
		}

		public override void SendResponseFromFile (IntPtr a, long b, long c)
		{
		}

		public override void FlushResponse (bool x)
		{
		}

		public override void EndOfRequest ()
		{
		}

		public bool MirrorVariableName {
			get { return mirror; }
			set { mirror = value; }
		}

		public bool AlternateChoice {
			get { return alternate; }
			set { alternate = value; }
		}

		public override string GetServerVariable (string name)
		{
			if (mirror) {
				switch (name) {
				case "CERT_FLAGS":
					return "11";
				case "CERT_KEYSIZE":
					return (alternate) ? base.GetServerVariable (name) : "12";
				case "CERT_SECRETKEYSIZE":
					return (alternate) ? base.GetServerVariable (name) : "13";
				case "HTTPS_KEYSIZE":
					return "22";
				case "HTTPS_SECRETKEYSIZE":
					return "23";
				default:
					return name;
				}
			}
			return base.GetServerVariable (name);
		}

		public bool Override {
			get { return override_cc; }
			set { override_cc = value; }
		}

		public override byte[] GetClientCertificate ()
		{
			if (override_cc) {
				return new byte[1];
			} else {
				return base.GetClientCertificate ();
			}
		}

		public override byte[] GetClientCertificateBinaryIssuer ()
		{
			if (override_cc) {
				return new byte[2];
			} else {
				return base.GetClientCertificateBinaryIssuer ();
			}
		}

		public override int GetClientCertificateEncoding ()
		{
			if (override_cc) {
				return Int32.MinValue;
			} else {
				return base.GetClientCertificateEncoding ();
			}
		}

		public override byte[] GetClientCertificatePublicKey ()
		{
			if (override_cc) {
				return new byte[3];
			} else {
				return base.GetClientCertificatePublicKey ();
			}
		}

		public override DateTime GetClientCertificateValidFrom ()
		{
			if (override_cc) {
				return DateTime.MaxValue;
			} else {
				return base.GetClientCertificateValidFrom ();
			}
		}

		public override DateTime GetClientCertificateValidUntil ()
		{
			if (override_cc) {
				return DateTime.MinValue;
			} else {
				return base.GetClientCertificateValidUntil ();
			}
		}
	}

	[TestFixture]
	public class HttpClientCertificateTest {

		private MyHttpWorkerRequest hwr;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			hwr = new MyHttpWorkerRequest ();
		}

		[SetUp]
		public void SetUp ()
		{
			hwr.Override = false;
			hwr.MirrorVariableName = false;
			hwr.AlternateChoice = false;

		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void HttpRequestPublicCtor ()
		{
			HttpRequest hr = new HttpRequest ("file", "http://www.mono-project.com/", "");
			// always throw a NullReferenceException if the public ctor is used
			hr.ClientCertificate.ToString ();
		}

		private HttpClientCertificate GetHttpClientCertificate ()
		{
			return new HttpContext (hwr).Request.ClientCertificate;
		}

		[Test]
		public void DefaultValues ()
		{
			HttpClientCertificate hcc = GetHttpClientCertificate ();
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

		[Test]
		public void MirrorValues ()
		{
			hwr.MirrorVariableName = true;
			HttpClientCertificate hcc = GetHttpClientCertificate ();

			// not default (because we now have some data)
			Assert.IsFalse (hcc.IsValid, "IsValid");
			Assert.IsTrue (hcc.IsPresent, "IsPresent");

			Assert.AreEqual ("CERT_COOKIE", hcc.Cookie, "Cookie");
			Assert.AreEqual (11, hcc.Flags, "Flags");
			Assert.AreEqual ("CERT_ISSUER", hcc.Issuer, "Issuer");
			Assert.AreEqual (12, hcc.KeySize, "KeySize");
			Assert.AreEqual (13, hcc.SecretKeySize, "SecretKeySize");
			Assert.AreEqual ("CERT_SERIALNUMBER", hcc.SerialNumber, "SerialNumber");
			Assert.AreEqual ("CERT_SERVER_ISSUER", hcc.ServerIssuer, "ServerIssuer");
			Assert.AreEqual ("CERT_SERVER_SUBJECT", hcc.ServerSubject, "ServerSubject");
			Assert.AreEqual ("CERT_SUBJECT", hcc.Subject, "Subject");
		}

		[Test]
		public void MirrorValues_Alternate ()
		{
			hwr.MirrorVariableName = true;
			hwr.AlternateChoice = true;
			HttpClientCertificate hcc = GetHttpClientCertificate ();
			// if CERT_KEYSIZE is missing then HTTPS_KEYSIZE isn't checked
			Assert.AreEqual (0, hcc.KeySize, "Alternate-KeySize");
			// if CERT_SECRETKEYSIZE is missing then HTTPS_SECRETKEYSIZE isn't looked
			Assert.AreEqual (0, hcc.SecretKeySize, "Alternate-SecretKeySize");
		}

		[Test]
		public void HttpWorkerRequest ()
		{
			// required to "activate" later call as IsPresent will return true
			hwr.MirrorVariableName = true; 
			hwr.Override = true;
			HttpClientCertificate hcc = GetHttpClientCertificate ();

			// not affected by server variables (but by HttpWorkerRequest)
			Assert.AreEqual (2, hcc.BinaryIssuer.Length, "BinaryIssuer");
			Assert.AreEqual (Int32.MinValue, hcc.CertEncoding, "CertEncoding");
			Assert.AreEqual (1, hcc.Certificate.Length, "Certificate");
			Assert.AreEqual (3, hcc.PublicKey.Length, "PublicKey");
			Assert.AreEqual (DateTime.MaxValue, hcc.ValidFrom, "ValidFrom");
			Assert.AreEqual (DateTime.MinValue, hcc.ValidUntil, "ValidUntil");
		}

		[Test]
		public void Valid ()
		{
			HttpClientCertificate hcc = GetHttpClientCertificate ();
			// just to see if it always returns DateTime.Now or if it cache the value
			long from1 = hcc.ValidFrom.Ticks;
			Thread.Sleep (100); // don't go too fast
			long until1 = hcc.ValidUntil.Ticks;
			Thread.Sleep (100); // don't go too fast
			long from2 = hcc.ValidFrom.Ticks;
			Thread.Sleep (100); // don't go too fast
			long until2 = hcc.ValidUntil.Ticks;
			Assert.AreEqual (from1, from2, "from-from");
			Assert.AreEqual (until1, until2, "until-until");
			//Assert.AreEqual (from1, until2, "from-until");  // TODO: fails on .net occasionally as well, investigate
		}

		[Test]
		public void Add ()
		{
			HttpClientCertificate hcc = GetHttpClientCertificate ();
			Assert.AreEqual (0, hcc.Count, "0");
			hcc.Add ("a", "b");
			Assert.AreEqual (1, hcc.Count, "1");
			// it's not read-only (at least not in this case)
		}

		[Test]
		public void Get ()
		{
			HttpClientCertificate hcc = GetHttpClientCertificate ();
			Assert.AreEqual (String.Empty, hcc.Get (null), "null");
			hcc.Add ("a", "b");
			Assert.AreEqual (String.Empty, hcc.Get ("a"), "Get(string)");
			Assert.AreEqual ("b", hcc.Get (0), "Get(int)");
		}
	}
}
