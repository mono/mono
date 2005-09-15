//
// Html32TextWriterCas.cs - CAS unit tests for System.Web.UI.Html32TextWriter
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
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class Html32TextWriterCas : AspNetHostingMinimal {

		private StringWriter sw;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			sw = new StringWriter ();
		}

		private void Deny_Unrestricted (Html32TextWriter htw)
		{
#if NET_2_0
			htw.ShouldPerformDivTableSubstitution = true;
			Assert.IsTrue (htw.ShouldPerformDivTableSubstitution, "ShouldPerformDivTableSubstitution");
			htw.SupportsBold = true;
			Assert.IsTrue (htw.SupportsBold, "SupportsBold");
			htw.SupportsItalic = true;
			Assert.IsTrue (htw.SupportsItalic, "SupportsItalic");
#endif
			htw.RenderBeginTag (HtmlTextWriterTag.Table);
			htw.RenderBeginTag ("<tr>");
			htw.RenderEndTag ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Ctor1_Deny_Unrestricted ()
		{
			Html32TextWriter htw = new Html32TextWriter (sw);
			Deny_Unrestricted (htw);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Ctor2_Deny_Unrestricted ()
		{
			Html32TextWriter htw = new Html32TextWriter (sw, String.Empty);
			Deny_Unrestricted (htw);
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[1] { typeof (TextWriter) });
			Assert.IsNotNull (ci, ".ctor(TextWriter)");
			return ci.Invoke (new object[1] { sw });
		}

		public override Type Type {
			get { return typeof (Html32TextWriter); }
		}
	}
}
