//
// HttpWriterCas.cs - CAS unit tests for System.Web.HttpWriter
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpWriterCas : AspNetHostingMinimal {

		private HttpWriter writer;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			HttpContext context = new HttpContext (null);
			writer = (HttpWriter) context.Response.Output;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties_Deny_Unrestricted ()
		{
			Assert.IsNotNull (writer.Encoding, "Encoding");
			Assert.IsNotNull (writer.OutputStream, "OutputStream");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Methods_Deny_Unrestricted ()
		{
			writer.Write (Char.MinValue);
			writer.Write (this);
			writer.Write (String.Empty);
			writer.Write (new char [1], 0, 1);
			writer.WriteLine ();
			writer.WriteString ("mono", 0, 4);
			writer.WriteBytes (new byte[1], 0, 1);
			writer.Flush ();
			writer.Close ();
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			// there are no public ctor so we're taking a method that we know isn't protected
			// (by a Demand) and call it thru reflection so any linkdemand (on the class) will
			// be promoted to a Demand
			MethodInfo mi = this.Type.GetProperty ("OutputStream").GetGetMethod ();
			return mi.Invoke (writer, null);
		}

		public override Type Type {
			get { return typeof (HttpWriter); }
		}
	}
}
