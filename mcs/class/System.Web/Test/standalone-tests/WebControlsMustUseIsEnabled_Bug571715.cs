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
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;

using StandAloneRunnerSupport;
using StandAloneTests;

using NUnit.Framework;

namespace StandAloneTests.WebControlsMustUseIsEnabled_Bug571715
{
	[TestCase ("WebControlsMustUseIsEnabled_Bug571715", "WebControl descendants must use IsEnabled instead of Enabled (Bug #571715)")]
	public sealed class Test_01 : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "WebControlsMustUseIsEnabled_Bug571715"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("default.aspx", Default_Aspx));
			
			return true;
		}

		void Default_Aspx (string result, TestRunItem runItem)
		{
#if NET_4_0
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"GridView1\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">ID</th><th scope=\"col\">FULLNAME</th><th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td>1</td><td>Sheryl Hunter</td><td align=\"left\" style=\"width:60px;\"><input type=\"submit\" name=\"GridView1$ctl02$DeleteBtn\" value=\"Delete\" id=\"GridView1_DeleteBtn_0\" /></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>2</td><td>Dean Straight</td><td class=\"aspNetDisabled\" align=\"left\" style=\"width:60px;\"><input type=\"submit\" name=\"GridView1$ctl03$DeleteBtn\" value=\"Delete\" disabled=\"disabled\" id=\"GridView1_DeleteBtn_1\" /></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>3</td><td>Marjorie Green</td><td align=\"left\" style=\"width:60px;\"><input type=\"submit\" name=\"GridView1$ctl04$DeleteBtn\" value=\"Delete\" id=\"GridView1_DeleteBtn_2\" /></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#else
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"GridView1\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">ID</th><th scope=\"col\">FULLNAME</th><th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td>1</td><td>Sheryl Hunter</td><td align=\"left\" style=\"width:60px;\"><input type=\"submit\" name=\"GridView1$ctl02$DeleteBtn\" value=\"Delete\" id=\"GridView1_ctl02_DeleteBtn\" /></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>2</td><td>Dean Straight</td><td disabled=\"disabled\" align=\"left\" style=\"width:60px;\"><input type=\"submit\" name=\"GridView1$ctl03$DeleteBtn\" value=\"Delete\" disabled=\"disabled\" id=\"GridView1_ctl03_DeleteBtn\" /></td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>3</td><td>Marjorie Green</td><td align=\"left\" style=\"width:60px;\"><input type=\"submit\" name=\"GridView1$ctl04$DeleteBtn\" value=\"Delete\" id=\"GridView1_ctl04_DeleteBtn\" /></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#endif
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}
}
