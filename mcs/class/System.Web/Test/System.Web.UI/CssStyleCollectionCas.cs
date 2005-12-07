//
// CssStyleCollectionCas.cs 
//	- CAS unit tests for System.Web.UI.CssStyleCollectionCas
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
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class CssStyleCollectionCas : AspNetHostingMinimal {

		private CssStyleCollection css;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			css = new Table ().Style;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			Assert.AreEqual (0, css.Count, "Count");
			css ["mono"] = "monkey";
			Assert.AreEqual ("monkey", css["mono"], "this[string]");
			Assert.IsNotNull (css.Keys, "Keys");
			css.Add ("monkey", "mono");
			css.Remove ("monkey");
			css.Clear ();
#if NET_2_0
			css[HtmlTextWriterStyle.Top] = "1";
			Assert.AreEqual ("1", css[HtmlTextWriterStyle.Top], "this[HtmlTextWriterStyle]");
			Assert.IsNotNull (css.Value, "Value");
			css.Value = String.Empty;
			css.Add (HtmlTextWriterStyle.Left, "1");
			css.Remove (HtmlTextWriterStyle.Left);
#endif
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			// no public ctor is available but we know the Count property isn't protected
			MethodInfo mi = this.Type.GetProperty ("Count").GetGetMethod ();
			Assert.IsNotNull (mi, "Count");
			return mi.Invoke (css, null);
		}

		public override Type Type {
			get { return typeof (CssStyleCollection); }
		}
	}
}
