//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2010 Novell, Inc http://novell.com/
//

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
#if NET_4_0
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Util;

using StandAloneRunnerSupport;
using StandAloneTests;

using NUnit.Framework;

namespace StandAloneTests.GridViewShowHeaderWhenEmpty
{
	[TestCase ("GridViewShowHeaderWhenEmpty 01", "GridView.ShowHeaderWhenEmpty tests")]
	public sealed class Test_01 : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "GridViewShowHeaderWhenEmpty"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx));
			runItems.Add (new TestRunItem ("NoHeaderAtAll.aspx", NoHeaderAtAll_Aspx));
			runItems.Add (new TestRunItem ("NoHeaderWhenEmpty.aspx", NoHeaderWhenEmpty_Aspx));
			runItems.Add (new TestRunItem ("WithHeaderWhenEmpty.aspx", WithHeaderWhenEmpty_Aspx));
			
			return true;
		}
		
		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<div>\r\n\r\n</div><pre id=\"log\"></pre>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
		
		void NoHeaderAtAll_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<div>\r\n\r\n</div><pre id=\"log\"></pre>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void NoHeaderWhenEmpty_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<div>\r\n\r\n</div><pre id=\"log\"></pre>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void WithHeaderWhenEmpty_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"GridView1\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">ProductID</th><th scope=\"col\">ProductName</th><th scope=\"col\">ProductComment</th>\r\n\t\t</tr>\r\n\t</table>\r\n</div><pre id=\"log\">OnRowCreated called</pre>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}
}
#endif