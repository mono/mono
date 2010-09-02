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

namespace StandAloneTests.Menu_40_List
{
	[TestCase ("Menu_40_List", "Menu control List rendering mode for 4.0")]
	public sealed class Test_01 : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "Menu_4.0_List"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("test_01.aspx", Test_01_Aspx));
			return true;
		}
		
		void Test_01_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<a href=\"#MyMenu1_SkipLink\"><img alt=\"Skip Navigation Links\" src=\"/WebResource.axd\" width=\"0\" height=\"0\" style=\"border-width:0px;\" /></a><div id=\"MyMenu1\">\r\n\t<ul class=\"level1\">\r\n\t\t<li><a title=\"Root description\" class=\"popout level1\" href=\"./\">Root</a><ul class=\"level2\">\r\n\t\t\t<li><a title=\"Mono Project Home Page\" class=\"popout level2\" href=\"http://mono-project.com/\">Mono</a><ul class=\"level3\">\r\n\t\t\t\t<li><a title=\"How to report Mono bugs\" class=\"popout level3\" href=\"http://mono-project.com/Bugs\">Mono Bugs</a><ul class=\"level4\">\r\n\t\t\t\t\t<li><a title=\"Includes Mono bugs interface\" class=\"level4\" href=\"http://bugzilla.novell.com\">Novell Bugzilla</a></li>\r\n\t\t\t\t</ul></li>\r\n\t\t\t</ul></li><li><a title=\"Main page of the Google search engine\" class=\"popout level2\" href=\"http://google.com\">Google</a><ul class=\"level3\">\r\n\t\t\t\t<li><a title=\"Google language translation interface\" class=\"level3\" href=\"http://translate.google.com\">Translator</a></li><li><a title=\"Google code search engine\" class=\"level3\" href=\"http://google.com/codesearch\">Code Search</a></li>\r\n\t\t\t</ul></li>\r\n\t\t</ul></li>\r\n\t</ul>\r\n</div><a id=\"MyMenu1_SkipLink\"></a>";
			Helpers.ExtractAndCompareCodeFromHtml (Helpers.StripWebResourceAxdQuery (result), originalHtml, "#A1");
		}
	}
}
#endif
