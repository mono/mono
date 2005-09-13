//
// HttpExceptionCas.cs - CAS unit tests for System.Web.HttpException
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
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpExceptionCas : AspNetHostingMinimal {

		private HttpException he;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			he = new HttpException ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void CreateFromLastError ()
		{
			Assert.IsNotNull (HttpException.CreateFromLastError ("mono"));
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor0_Deny_Unrestricted ()
		{
			HttpException e = new HttpException ();
			e.GetHtmlErrorMessage (); // null for ms, non-null for mono
			Assert.AreEqual (500, e.GetHttpCode (), "HttpCode");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor1_Deny_Unrestricted ()
		{
			HttpException e = new HttpException ("message");
			e.GetHtmlErrorMessage (); // null for ms, non-null for mono
			Assert.AreEqual (500, e.GetHttpCode (), "HttpCode");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor2a_Deny_Unrestricted ()
		{
			HttpException e = new HttpException (501, "message");
			e.GetHtmlErrorMessage (); // null for ms, non-null for mono
			Assert.AreEqual (501, e.GetHttpCode (), "HttpCode");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor2b_Deny_Unrestricted ()
		{
			HttpException e = new HttpException ("message", new Exception ());
			e.GetHtmlErrorMessage (); // null for ms, non-null for mono
			Assert.AreEqual (500, e.GetHttpCode (), "HttpCode");
		}
#if NET_2_0
		[Test]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetObjectData_Deny_SerializationFormatter ()
		{
			he.GetObjectData (null, new StreamingContext ());
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, SerializationFormatter = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetObjectData_PermitOnly_SerializationFormatter ()
		{
			he.GetObjectData (null, new StreamingContext ());
		}
#endif
		// LinkDemand

		public override Type Type {
			get { return typeof (HttpException); }
		}
	}
}
