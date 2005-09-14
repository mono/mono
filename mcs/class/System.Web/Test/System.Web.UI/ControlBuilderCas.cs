//
// ControlBuilderCas.cs - CAS unit tests for System.Web.UI.ControlBuilder
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
using System.Web.UI;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class ControlBuilderCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			ControlBuilder cb = new ControlBuilder ();
			Assert.IsNull (cb.ControlType, "ControlType");
			Assert.IsFalse (cb.HasAspCode, "HasAspCode");
			cb.ID = "mono";
			Assert.AreEqual ("mono", cb.ID, "ID");
			Assert.AreEqual (typeof (Control), cb.NamingContainerType, "NamingContainerType");
			Assert.IsNull (cb.TagName, "TagName");
			Assert.IsTrue (cb.AllowWhitespaceLiterals (), "AllowWhitespaceLiterals");
			cb.AppendLiteralString ("mono");
			cb.AppendSubBuilder (cb);
			cb.CloseControl ();
			Assert.IsNull (cb.GetChildControlType (null, null), "GetChildControlType");
			Assert.IsTrue (cb.HasBody (), "HasBody");
			Assert.IsFalse (cb.HtmlDecodeLiterals (), "HtmlDecodeLiterals");
			cb.Init (null, cb, typeof (TemplateBuilder), "span", "mono", null);
			Assert.IsFalse (cb.NeedsTagInnerText (), "NeedsTagInnerText");
			//cb.OnAppendToParentBuilder (null);
			cb.SetTagInnerText ("mono");

			cb = ControlBuilder.CreateBuilderFromType (null, cb, typeof (TemplateBuilder), "span", "mono", null, 0, String.Empty);
			Assert.IsNotNull (cb, "CreateBuilderFromType");
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (ControlBuilder); }
		}
	}
}
