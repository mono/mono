//
// AppResourcesCompilerTest.cs
//
// Author:
//      Marek Habersack <mhabersack@novell.com>
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
//

#if NET_2_0
using System;
using System.Web;
using System.Web.Compilation;
using System.Threading;

using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.Compilation
{
	[TestFixture]
	public class AppResourcesCompilerTest
	{
		[TestFixtureSetUp]
		public void SetUp ()
		{
			WebTest.CopyResource (this.GetType (), "GlobalResourcesLocalization.aspx", "GlobalResourcesLocalization.aspx");
		}

		[Test (Description="Bug #548768")]
		public void GlobalResourcesLocalization ()
		{
			string pageHtml = new WebTest ("GlobalResourcesLocalization.aspx").Run ();
                        string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
                        string originalHtml = "<input type=\"submit\" name=\"button1\" value=\"Recharger\" id=\"button1\" />";
                        
                        HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}
	}
}
#endif
