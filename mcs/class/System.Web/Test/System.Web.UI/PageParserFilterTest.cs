//
// Tests for System.Web.UI.WebControls.TemplateControlTest.cs
//
// Author:
//	Marek Habersack <mhabersack@novell.com>
//
//
// Copyright (C) 2009 Novell, Inc (http://novell.com)
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

#if NET_2_0
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using MyWebControl = System.Web.UI.WebControls;
using System.Collections;
using MonoTests.SystemWeb.Framework;
using NUnit.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Threading;

namespace MonoTests.System.Web.UI.WebControls
{
	class MyPageParserFilter : PageParserFilter
	{
		public int GetLine()
		{
			return Line;
		}

		public string GetVirtualPath()
		{
			return VirtualPath;
		}
	}
	
	[TestFixture]
	public class PageParserFilterTests
	{
		[Test]
		public void Defaults ()
		{
			var ppf = new MyPageParserFilter ();

			Assert.AreEqual (false, ppf.AllowCode, "#A1");
			Assert.AreEqual (0, ppf.NumberOfControlsAllowed, "#A2");
			Assert.AreEqual (0, ppf.NumberOfDirectDependenciesAllowed, "#A3");
			Assert.AreEqual (0, ppf.TotalNumberOfDependenciesAllowed, "#A4");
			Assert.AreEqual (false, ppf.AllowBaseType (typeof (Page)), "#A5");
			Assert.AreEqual (false, ppf.AllowControl (typeof (Page), null), "#A6");
			Assert.AreEqual (false, ppf.AllowServerSideInclude (String.Empty), "#A7");
			Assert.AreEqual (false, ppf.AllowVirtualReference (String.Empty, VirtualReferenceType.Master), "#A8");
			Assert.AreEqual (CompilationMode.Auto, ppf.GetCompilationMode (CompilationMode.Auto), "#A9");
			Assert.AreEqual (null, ppf.GetNoCompileUserControlType (), "#A10");
			Assert.AreEqual (false, ppf.ProcessCodeConstruct (CodeConstructType.ExpressionSnippet, String.Empty), "#A11");
			Assert.AreEqual (false, ppf.ProcessDataBindingAttribute (String.Empty, String.Empty, String.Empty), "#A12");
			Assert.AreEqual (false, ppf.ProcessEventHookup (String.Empty, String.Empty, String.Empty), "#A13");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void DefaultsNREX_1 ()
		{
			var ppf = new MyPageParserFilter ();
			ppf.GetLine ();
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void DefaultsNREX_2 ()
		{
			var ppf = new MyPageParserFilter ();
			ppf.GetVirtualPath ();
		}
	}
}
#endif
