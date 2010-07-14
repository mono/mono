//
// Tests for System.Web.UI.ObjectStateFormatter
//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
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
using System.Reflection;
using System.Web;
using System.Web.UI;

using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using NUnit.Framework;

namespace MonoTests.System.Web.UI
{
	[TestFixture]
	public class ObjectStateFormatterTest
	{
		[TestFixtureSetUp]
		public void SetUp ()
		{
			Type t = GetType ();

			WebTest.CopyResource (t, "StateFormatter_CorrectConverter.aspx", "StateFormatter_CorrectConverter.aspx");
			WebTest.CopyResource (t, "StateFormatter_CollectionConverter.aspx", "StateFormatter_CollectionConverter.aspx");
			WebTest.CopyResource (t, "StateFormatter_CollectionConverter.aspx.cs", "StateFormatter_CollectionConverter.aspx.cs");
		}

		public static Assembly ResolveAssemblyHandler (object sender, ResolveEventArgs e)
		{
			if (e.Name != "System.Web_test")
				return null;

			return Assembly.GetExecutingAssembly ();
		}
		
		[Test (Description="Bug #545979")]
		public void StateFormatter_CorrectConverter ()
		{
			// We test only if it doesn't throw exception on postback
			try {
				WebTest.Host.AppDomain.AssemblyResolve += new ResolveEventHandler (ResolveAssemblyHandler);
				WebTest t = new WebTest ("StateFormatter_CorrectConverter.aspx");
				t.Run ();

				var fr = new FormRequest (t.Response, "Form1");
				fr.Controls.Add ("Button1");
				fr.Controls ["Button1"].Value = "Change";
				t.Request = fr;
				t.Run ();

				fr = new FormRequest (t.Response, "Form1");
				fr.Controls.Add ("Button2");
				fr.Controls ["Button2"].Value = "Refresh";
			} finally {
				WebTest.Host.AppDomain.AssemblyResolve -= new ResolveEventHandler (ResolveAssemblyHandler);
			}
		}

		[Test (Description="Bug #565547")]
		public void StateFormatter_CollectionFormatter ()
		{
			WebTest t = new WebTest ("StateFormatter_CollectionConverter.aspx");
			t.Run ();

			var fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("btnSearch");
			fr.Controls.Add ("ddlDate");
			fr.Controls.Add ("txtSearchValue");

			fr.Controls ["btnSearch"].Value = "Search";
			fr.Controls ["ddlDate"].Value = "2009";

			t.Request = fr;
			string pageHtml = t.Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			string originalHtml = @"<div>

		<table id=""gvECCN"" cellspacing=""0"" rules=""all"" border=""1"" style=""border-collapse:collapse;"">
				<tr>
					<th align=""left"" scope=""col"">&nbsp;</th><th align=""left"" scope=""col"">Schedule B</th><th align=""left"" scope=""col"">Count</th><th align=""left"" scope=""col"">Total</th><th align=""left"" scope=""col"">Percent</th>
				</tr><tr>
					<td style=""height:18px;width:30px;"">1</td><td style=""width:140px;"">test</td><td style=""width:90px;"">1</td><td style=""width:100px;"">100</td><td style=""width:90px;"">250.00 %</td>

				</tr>
			</table>
		</div>";

			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "#A1");
		}
	}
}
#endif