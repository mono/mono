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
			string originalHtml = "<div>\r\n	<table id=\"GridView1\" cellspacing=\"0\" rules=\"all\" border=\"1\" style=\"border-collapse:collapse;\">\r\n			<tr>\r\n				<th scope=\"col\">ID</th><th scope=\"col\">FULLNAME</th><th scope=\"col\">&nbsp;</th>\r\n			</tr><tr>\r\n				<td>1</td><td>Sheryl Hunter</td><td align=\"left\" style=\"width:60px;\"><input type=\"submit\" name=\"GridView1$ctl02$DeleteBtn\" value=\"Delete\" id=\"GridView1_ctl02_DeleteBtn\" /></td>\r\n			</tr><tr>\r\n				<td>2</td><td>Dean Straight</td><td disabled=\"disabled\" align=\"left\" style=\"width:60px;\"><input type=\"submit\" name=\"GridView1$ctl03$DeleteBtn\" value=\"Delete\" id=\"GridView1_ctl03_DeleteBtn\" disabled=\"disabled\" /></td>\r\n			</tr><tr>\r\n				<td>3</td><td>Marjorie Green</td><td align=\"left\" style=\"width:60px;\"><input type=\"submit\" name=\"GridView1$ctl04$DeleteBtn\" value=\"Delete\" id=\"GridView1_ctl04_DeleteBtn\" /></td>\r\n			</tr>\r\n		</table>\r\n	</div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}
}
