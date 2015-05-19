//
// LosFormatterCas.cs - CAS unit tests for System.Web.UI.LosFormatter
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
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class LosFormatterCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Ctor0_Deny_Unrestricted ()
		{
			LosFormatter lf = new LosFormatter ();

			MemoryStream ms = new MemoryStream ();
			lf.Serialize (ms, "mono");
			ms.Position = 0;
			Assert.IsNotNull (lf.Deserialize (ms), "Deserialize(Stream)");

			StringWriter sw = new StringWriter ();
			lf.Serialize (sw, "mono");
			string s = sw.ToString ();
			StringReader sr = new StringReader (s);
			Assert.IsNotNull (lf.Deserialize (sr), "Deserialize(TextReader)");

			Assert.IsNotNull (lf.Deserialize (s), "Deserialize(string)");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void CtorBoolString_Deny_Unrestricted ()
		{
			LosFormatter lf = new LosFormatter (true, String.Empty);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void CtorBoolByteArray_Deny_Unrestricted ()
		{
			LosFormatter lf = new LosFormatter (true, (byte[])null);
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (LosFormatter); }
		}
	}
}
