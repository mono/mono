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

namespace StandAloneTests.GridViewSortingStyles
{
	[TestCase ("GridViewSortingStyles 01", "GridView supports separate styles for headers/cells when sorting.")]
	public sealed class Test_01 : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "GridViewSortingStyles"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx));
			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx_Ascending_ProductName) {
					PostValues = new string[] {
						"__EVENTTARGET", "GridView1",
						"__EVENTARGUMENT", "Sort$ProductName"
					},
					UrlDescription = "Ascending sorting on ProductName"
				}
			);
			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx_Descending_ProductName) {
					PostValues = new string[] {
						"__EVENTTARGET", "GridView1",
						"__EVENTARGUMENT", "Sort$ProductName"
					},
					UrlDescription = "Descending sorting on ProductName"
				}
			);
			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx_Ascending_ProductID) {
					PostValues = new string[] {
						"__EVENTTARGET", "GridView1",
						"__EVENTARGUMENT", "Sort$ProductID"
					},
					UrlDescription = "Ascending sorting on ProductID"
				}
			);
			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx_Descending_ProductID) {
					PostValues = new string[] {
						"__EVENTTARGET", "GridView1",
						"__EVENTARGUMENT", "Sort$ProductID"
					},
					UrlDescription = "Descending sorting on ProductID"
				}
			);
			return true;
		}
		
		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"GridView1\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\"><a href=\"javascript:__doPostBack(&#39;GridView1&#39;,&#39;Sort$ProductName&#39;)\">Name</a></th><th scope=\"col\"><a href=\"javascript:__doPostBack(&#39;GridView1&#39;,&#39;Sort$ProductID&#39;)\">ID</a></th>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Pear</td><td>1</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Apple</td><td>2</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Orange</td><td>3</td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Default_Aspx_Ascending_ProductName (string result, TestRunItem runItem)
		{
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"GridView1\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\" style=\"background-color:Yellow;\"><a href=\"javascript:__doPostBack(&#39;GridView1&#39;,&#39;Sort$ProductName&#39;)\">Name</a></th><th scope=\"col\"><a href=\"javascript:__doPostBack(&#39;GridView1&#39;,&#39;Sort$ProductID&#39;)\">ID</a></th>\r\n\t\t</tr><tr>\r\n\t\t\t<td style=\"background-color:LightYellow;\">Apple</td><td>2</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td style=\"background-color:LightYellow;\">Orange</td><td>3</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td style=\"background-color:LightYellow;\">Pear</td><td>1</td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Default_Aspx_Descending_ProductName (string result, TestRunItem runItem)
		{
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"GridView1\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\" style=\"background-color:LightBlue;\"><a href=\"javascript:__doPostBack(&#39;GridView1&#39;,&#39;Sort$ProductName&#39;)\">Name</a></th><th scope=\"col\"><a href=\"javascript:__doPostBack(&#39;GridView1&#39;,&#39;Sort$ProductID&#39;)\">ID</a></th>\r\n\t\t</tr><tr>\r\n\t\t\t<td style=\"background-color:AliceBlue;\">Pear</td><td>1</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td style=\"background-color:AliceBlue;\">Orange</td><td>3</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td style=\"background-color:AliceBlue;\">Apple</td><td>2</td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Default_Aspx_Ascending_ProductID (string result, TestRunItem runItem)
		{
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"GridView1\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\"><a href=\"javascript:__doPostBack(&#39;GridView1&#39;,&#39;Sort$ProductName&#39;)\">Name</a></th><th scope=\"col\" style=\"background-color:Yellow;\"><a href=\"javascript:__doPostBack(&#39;GridView1&#39;,&#39;Sort$ProductID&#39;)\">ID</a></th>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Pear</td><td style=\"background-color:LightYellow;\">1</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Apple</td><td style=\"background-color:LightYellow;\">2</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Orange</td><td style=\"background-color:LightYellow;\">3</td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Default_Aspx_Descending_ProductID (string result, TestRunItem runItem)
		{
			string originalHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"GridView1\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\"><a href=\"javascript:__doPostBack(&#39;GridView1&#39;,&#39;Sort$ProductName&#39;)\">Name</a></th><th scope=\"col\" style=\"background-color:LightBlue;\"><a href=\"javascript:__doPostBack(&#39;GridView1&#39;,&#39;Sort$ProductID&#39;)\">ID</a></th>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Orange</td><td style=\"background-color:AliceBlue;\">3</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Apple</td><td style=\"background-color:AliceBlue;\">2</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Pear</td><td style=\"background-color:AliceBlue;\">1</td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}
}
#endif