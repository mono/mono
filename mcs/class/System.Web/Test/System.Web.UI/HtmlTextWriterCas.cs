//
// HtmlTextWriterCas.cs - CAS unit tests for System.Web.UI.HtmlTextWriter
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
	public class HtmlTextWriterCas : AspNetHostingMinimal {

		private StringWriter sw;

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();
			sw = new StringWriter ();
		}

		private void Deny_Unrestricted (HtmlTextWriter htw)
		{
			Assert.IsTrue (htw.Indent >= 0, "Indent");
			Assert.AreSame (sw, htw.InnerWriter, "InnerWriter");
			htw.NewLine = Environment.NewLine;
			Assert.IsNotNull (htw.NewLine, "NewLine");

			htw.AddAttribute (HtmlTextWriterAttribute.Bgcolor, "blue");
			htw.AddAttribute (HtmlTextWriterAttribute.Bgcolor, "blue", false);
			htw.AddAttribute ("align", "left");
			htw.AddAttribute ("align", "left", false);

			htw.AddStyleAttribute (HtmlTextWriterStyle.BackgroundColor, "blue");
			htw.AddStyleAttribute ("left", "1");

			htw.RenderBeginTag (HtmlTextWriterTag.Table);
			htw.RenderBeginTag ("<tr>");
			htw.RenderEndTag ();

			htw.WriteAttribute ("align", "left");
			htw.WriteAttribute ("align", "left", false);
			htw.WriteBeginTag ("table");
			htw.WriteEndTag ("table");
			htw.WriteFullBeginTag ("div");

			htw.WriteStyleAttribute ("left", "2");
			htw.WriteStyleAttribute ("left", "3", false);

			htw.Write (new char[1], 0, 1);
			htw.Write ((double)1.0);
			htw.Write (Char.MinValue);
			htw.Write (new char[1]);
			htw.Write ((int)1);
			htw.Write ("{0}", 1);
			htw.Write ("{0}{1}", 1, 2);
			htw.Write ("{0}{1}{2}", 1, 2, 3);
			htw.Write (String.Empty);
			htw.Write ((long)1);
			htw.Write (this);
			htw.Write ((float)1.0);
			htw.Write (false);

			htw.WriteLine (new char[1], 0, 1);
			htw.WriteLine ((double)1.0);
			htw.WriteLine (Char.MinValue);
			htw.WriteLine (new char[1]);
			htw.WriteLine ((int)1);
			htw.WriteLine ("{0}", 1);
			htw.WriteLine ("{0}{1}", 1, 2);
			htw.WriteLine ("{0}{1}{2}", 1, 2, 3);
			htw.WriteLine (String.Empty);
			htw.WriteLine ((long)1);
			htw.WriteLine (this);
			htw.WriteLine ((float)1.0);
			htw.WriteLine (false);
			htw.WriteLine ((uint)0);
			htw.WriteLine ();
			htw.WriteLineNoTabs (String.Empty);

			htw.Flush ();
			htw.Close ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Ctor1_Deny_Unrestricted ()
		{
			HtmlTextWriter htw = new HtmlTextWriter (sw);
			Deny_Unrestricted (htw);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Ctor2_Deny_Unrestricted ()
		{
			HtmlTextWriter htw = new HtmlTextWriter (sw, String.Empty);
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
			get { return typeof (HtmlTextWriter); }
		}
	}
}
