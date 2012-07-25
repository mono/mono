//
// HttpClientHandlerTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Net.Http;
using System.Net;

namespace MonoTests.System.Net.Http
{
	[TestFixture]
	public class HttpClientHandlerTest
	{
		class Proxy : IWebProxy
		{
			public ICredentials Credentials {
				get {
					throw new NotImplementedException ();
				}
				set {
					throw new NotImplementedException ();
				}
			}

			public Uri GetProxy (Uri destination)
			{
				throw new NotImplementedException ();
			}

			public bool IsBypassed (Uri host)
			{
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void Properties_Defaults ()
		{
			var h = new HttpClientHandler ();
			Assert.IsTrue (h.AllowAutoRedirect, "#1");
			Assert.AreEqual (DecompressionMethods.None, h.AutomaticDecompression, "#2");
			Assert.AreEqual (0, h.CookieContainer.Count, "#3");
			Assert.AreEqual (4096, h.CookieContainer.MaxCookieSize, "#3b");
			Assert.AreEqual (null, h.Credentials, "#4");
			Assert.AreEqual (50, h.MaxAutomaticRedirections, "#5");
			Assert.AreEqual (int.MaxValue, h.MaxRequestContentBufferSize, "#6");
			Assert.IsFalse (h.PreAuthenticate, "#7");
			Assert.IsNull (h.Proxy, "#8");
			Assert.IsTrue (h.SupportsAutomaticDecompression, "#9");
			Assert.IsTrue (h.SupportsProxy, "#10");
			Assert.IsTrue (h.SupportsRedirectConfiguration, "#11");
			Assert.IsTrue (h.UseCookies, "#12");
			Assert.IsFalse (h.UseDefaultCredentials, "#13");
			Assert.IsTrue (h.UseProxy, "#14");
			Assert.AreEqual (ClientCertificateOption.Manual, h.ClientCertificateOptions, "#15");
		}

		[Test]
		public void Properties_Invalid ()
		{
			var h = new HttpClientHandler ();
			try {
				h.MaxAutomaticRedirections = 0;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				h.MaxRequestContentBufferSize = -1;
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}

			h.UseProxy = false;
			try {
				h.Proxy = new Proxy ();
				Assert.Fail ("#3");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void Properties_AfterClientCreation ()
		{
			var h = new HttpClientHandler ();
			h.AllowAutoRedirect = true;

			// We may modify properties after creating the HttpClient.
			using (var c = new HttpClient (h, true)) {
				h.AllowAutoRedirect = false;
			}
		}
	}
}
