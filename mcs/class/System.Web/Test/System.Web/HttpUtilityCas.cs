//
// HttpUtilityCas.cs - CAS unit tests for System.Web.HttpUtility
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
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpUtilityCas : AspNetHostingMinimal {

		private StringWriter sw;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			sw = new StringWriter ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StaticMethods_Deny_Unrestricted ()
		{
			try {
				Assert.IsNull (HttpUtility.HtmlAttributeEncode (null), "HtmlAttributeEncode(string)");
			}
			catch (NullReferenceException) {
				// ms 1.x
			}
			try {
				HttpUtility.HtmlAttributeEncode (null, sw);
			}
			catch (NullReferenceException) {
				// ms 1.x
			}

			Assert.AreEqual (String.Empty, HttpUtility.HtmlDecode (String.Empty), "HtmlDecode(string)");
			HttpUtility.HtmlDecode (String.Empty, sw);

			Assert.AreEqual (String.Empty, HttpUtility.HtmlEncode (String.Empty), "HtmlEncode(string)");
			HttpUtility.HtmlEncode (String.Empty, sw);

			Assert.AreEqual (String.Empty, HttpUtility.UrlDecode (String.Empty), "UrlDecode(string)");
			Assert.AreEqual (String.Empty, HttpUtility.UrlDecode (String.Empty, Encoding.ASCII), "UrlDecode(string,Encoding)");
			Assert.AreEqual (String.Empty, HttpUtility.UrlDecode (new byte[0], Encoding.ASCII), "UrlDecode(byte[],Encoding)");
			Assert.AreEqual (String.Empty, HttpUtility.UrlDecode (new byte[0], 0, 0, Encoding.ASCII), "UrlDecode(byte[],int,int,Encoding)");

			Assert.AreEqual (0, HttpUtility.UrlDecodeToBytes (String.Empty).Length, "UrlDecodeToBytes(string)");
			Assert.AreEqual (0, HttpUtility.UrlDecodeToBytes (String.Empty, Encoding.ASCII).Length, "UrlDecodeToBytes(string,Encoding)");
			Assert.AreEqual (0, HttpUtility.UrlDecodeToBytes (new byte[0]).Length, "UrlDecodeToBytes(byte[])");
			Assert.AreEqual (0, HttpUtility.UrlDecodeToBytes (new byte[0], 0, 0).Length, "UrlDecodeToBytes(byte[],int,int)");

			Assert.AreEqual (String.Empty, HttpUtility.UrlEncode (String.Empty), "UrlEncode(string)");
			Assert.AreEqual (String.Empty, HttpUtility.UrlEncode (String.Empty, Encoding.ASCII), "UrlEncode(string,Encoding)");
			Assert.AreEqual (String.Empty, HttpUtility.UrlEncode (new byte[0]), "UrlEncode(byte[],Encoding)");
			Assert.AreEqual (String.Empty, HttpUtility.UrlEncode (new byte[0], 0, 0), "UrlEncode(byte[],int,int,Encoding)");

			Assert.AreEqual (0, HttpUtility.UrlEncodeToBytes (String.Empty).Length, "UrlEncodeToBytes(string)");
			Assert.AreEqual (0, HttpUtility.UrlEncodeToBytes (String.Empty, Encoding.ASCII).Length, "UrlEncodeToBytes(string,Encoding)");
			Assert.AreEqual (0, HttpUtility.UrlEncodeToBytes (new byte[0]).Length, "UrlEncodeToBytes(byte[])");
			Assert.AreEqual (0, HttpUtility.UrlEncodeToBytes (new byte[0], 0, 0).Length, "UrlEncodeToBytes(byte[],int,int)");

			Assert.AreEqual (String.Empty, HttpUtility.UrlEncodeUnicode (String.Empty), "UrlEncodeUnicode(string)");

			Assert.AreEqual (0, HttpUtility.UrlEncodeUnicodeToBytes (String.Empty).Length, "UrlEncodeUnicodeToBytes(string)");

			Assert.AreEqual (String.Empty, HttpUtility.UrlPathEncode (String.Empty), "UrlPathEncode(string)");
#if NET_2_0
			try {
				Assert.IsNotNull (HttpUtility.ParseQueryString (String.Empty), "ParseQueryString(string)");
			}
			catch (NotImplementedException)	{
				// mono
			}
			try {
				Assert.IsNotNull (HttpUtility.ParseQueryString (String.Empty, Encoding.ASCII), "ParseQueryString(string)");
			}
			catch (NotImplementedException)	{
				// mono
			}
#endif
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (HttpUtility); }
		}
	}
}
