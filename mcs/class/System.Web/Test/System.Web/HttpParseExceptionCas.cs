//
// HttpParseExceptionCas.cs 
//	- CAS unit tests for System.Web.HttpParseException
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

#if NET_2_0

// Note: class exists in 1.x but has no public ctor

using NUnit.Framework;

using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpParseExceptionCas : AspNetHostingMinimal {

		private HttpParseException hpe;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			hpe = new HttpParseException ();
		}


		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor0_Deny_Unrestricted ()
		{
			HttpParseException e = new HttpParseException ();
			Assert.IsNull (e.FileName, "FileName");
			Assert.AreEqual (0, e.Line, "Line");
			Assert.IsNull (e.VirtualPath, "VirtualPath");
			Assert.AreEqual (1, e.ParserErrors.Count, "ParserErrors");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor1_Deny_Unrestricted ()
		{
			HttpParseException e = new HttpParseException ("message");
			Assert.IsNull (e.FileName, "FileName");
			Assert.AreEqual (0, e.Line, "Line");
			Assert.IsNull (e.VirtualPath, "VirtualPath");
			Assert.AreEqual (1, e.ParserErrors.Count, "ParserErrors");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor2_Deny_Unrestricted ()
		{
			HttpParseException e = new HttpParseException ("message", new Exception ());
			Assert.IsNull (e.FileName, "FileName");
			Assert.AreEqual (0, e.Line, "Line");
			Assert.IsNull (e.VirtualPath, "VirtualPath");
			Assert.AreEqual (1, e.ParserErrors.Count, "ParserErrors");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor5_Deny_Unrestricted ()
		{
			HttpParseException e = new HttpParseException ("message", new Exception (), "virtualPath", "sourceCode", 100);
			Assert.IsNull (e.FileName, "FileName");
			Assert.AreEqual (100, e.Line, "Line");
			Assert.AreEqual ("virtualPath", e.VirtualPath, "VirtualPath");
			Assert.AreEqual (1, e.ParserErrors.Count, "ParserErrors");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetObjectData_Deny_SerializationFormatter ()
		{
			hpe.GetObjectData (null, new StreamingContext ());
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, SerializationFormatter = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetObjectData_PermitOnly_SerializationFormatter ()
		{
			hpe.GetObjectData (null, new StreamingContext ());
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (HttpParseException); }
		}
	}
}

#endif
