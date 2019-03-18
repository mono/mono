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
using System.Web.Util;

using StandAloneRunnerSupport;
using StandAloneTests;

using NUnit.Framework;

namespace StandAloneTests.DataPager_NumericPagerField_Bug615315
{
	[StandAloneRunnerSupport.TestCase ("DataPager_NumericPagerField_Bug615315, Test 01", "Page numbers must be sequential (Query Mode)")]
	public sealed class Test_01 : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "DataPager_NumericPagerField_Bug615315"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("Test_QueryMode.aspx", TestQueryMode_Aspx_Start));
			runItems.Add (new TestRunItem ("Test_QueryMode.aspx?pageNumber=2", TestQueryMode_Aspx_Page2));
			return true;
		}
		
		void TestQueryMode_Aspx_Start (string result, TestRunItem runItem)
		{
			string originalHtml = @"<span id=""ctl00_ContentPlaceHolder1_ListView1_DataPager1""><a class=""aspNetDisabled"">First</a>&nbsp;<span>1</span>&nbsp;<a href=""/Test_QueryMode.aspx?pageNumber=2"">2</a>&nbsp;<a href=""/Test_QueryMode.aspx?pageNumber=3"">3</a>&nbsp;<a href=""/Test_QueryMode.aspx?pageNumber=4"">4</a>&nbsp;<a href=""/Test_QueryMode.aspx?pageNumber=5"">5</a>&nbsp;&nbsp;<a href=""/Test_QueryMode.aspx?pageNumber=6"">...</a>&nbsp;<a href=""/Test_QueryMode.aspx?pageNumber=10"">Last</a>&nbsp;</span>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void TestQueryMode_Aspx_Page2 (string result, TestRunItem runItem)
		{
			string originalHtml = @"<span id=""ctl00_ContentPlaceHolder1_ListView1_DataPager1""><a href=""/Test_QueryMode.aspx?pageNumber=1"">First</a>&nbsp;<a href=""/Test_QueryMode.aspx?pageNumber=1"">1</a>&nbsp;<span>2</span>&nbsp;<a href=""/Test_QueryMode.aspx?pageNumber=3"">3</a>&nbsp;<a href=""/Test_QueryMode.aspx?pageNumber=4"">4</a>&nbsp;<a href=""/Test_QueryMode.aspx?pageNumber=5"">5</a>&nbsp;&nbsp;<a href=""/Test_QueryMode.aspx?pageNumber=6"">...</a>&nbsp;<a href=""/Test_QueryMode.aspx?pageNumber=10"">Last</a>&nbsp;</span>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}

	[StandAloneRunnerSupport.TestCase ("DataPager_NumericPagerField_Bug615315, Test 02", "Page numbers must be sequential (PostBack Mode)")]
	public sealed class Test_02 : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "DataPager_NumericPagerField_Bug615315"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("Test_PostBackMode.aspx", TestPostBackMode_Aspx_Start));
#if BUG_IN_THE_RUNTIME_IS_FIXED
                        // With this version of code, the runtime segfaults. Until this is fixed,
                        // we'll be using an alternative version of the code
                        runItems.Add (new TestRunItem ("Default.aspx", TestPostBackMode_Aspx_Page2) {
                                        PostValues = new SerializableDictionary <string, string> {
                                                {"__EVENTTARGET", "ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl01"},
                                                {"__EVENTARGUMENT", String.Empty}
                                        },
                                        UrlDescription = "Page 2"
                                }
                        );
#else
                        runItems.Add (new TestRunItem ("Test_PostBackMode.aspx", TestPostBackMode_Aspx_Page2) {
                                        PostValues = new string[] {
                                                "__EVENTTARGET", "ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl01",
                                                "__EVENTARGUMENT", String.Empty
                                        },
                                        UrlDescription = "Page 2"
                                }
                        );
			
                        runItems.Add (new TestRunItem ("Test_PostBackMode.aspx", TestPostBackMode_Aspx_PageNext) {
                                        PostValues = new string[] {
                                                "__EVENTTARGET", "ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl05",
                                                "__EVENTARGUMENT", String.Empty
                                        },
                                        UrlDescription = "Page 6 (next)"
                                }
                        );

			runItems.Add (new TestRunItem ("Test_PostBackMode.aspx", TestPostBackMode_Aspx_PagePrev) {
                                        PostValues = new string[] {
                                                "__EVENTTARGET", "ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl00",
                                                "__EVENTARGUMENT", String.Empty
                                        },
                                        UrlDescription = "Page 5 (prev)"
                                }
                        );
#endif

			return true;
		}
		
		void TestPostBackMode_Aspx_Start (string result, TestRunItem runItem)
		{
			string originalHtml = @"<span id=""ctl00_ContentPlaceHolder1_ListView1_DataPager1""><input type=""submit"" name=""ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl00$ctl00"" value=""First"" disabled=""disabled"" />&nbsp;<span>1</span>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl01&#39;,&#39;&#39;)"">2</a>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl02&#39;,&#39;&#39;)"">3</a>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl03&#39;,&#39;&#39;)"">4</a>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl04&#39;,&#39;&#39;)"">5</a>&nbsp;&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl05&#39;,&#39;&#39;)"">...</a>&nbsp;<input type=""submit"" name=""ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl02$ctl00"" value=""Last"" />&nbsp;</span>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void TestPostBackMode_Aspx_Page2 (string result, TestRunItem runItem)
		{
			string originalHtml = @"<span id=""ctl00_ContentPlaceHolder1_ListView1_DataPager1""><input type=""submit"" name=""ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl00$ctl00"" value=""First"" />&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl00&#39;,&#39;&#39;)"">1</a>&nbsp;<span>2</span>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl02&#39;,&#39;&#39;)"">3</a>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl03&#39;,&#39;&#39;)"">4</a>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl04&#39;,&#39;&#39;)"">5</a>&nbsp;&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl05&#39;,&#39;&#39;)"">...</a>&nbsp;<input type=""submit"" name=""ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl02$ctl00"" value=""Last"" />&nbsp;</span>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void TestPostBackMode_Aspx_PageNext (string result, TestRunItem runItem)
		{
			string originalHtml = @"<span id=""ctl00_ContentPlaceHolder1_ListView1_DataPager1""><input type=""submit"" name=""ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl00$ctl00"" value=""First"" />&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl00&#39;,&#39;&#39;)"">...</a>&nbsp;<span>6</span>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl02&#39;,&#39;&#39;)"">7</a>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl03&#39;,&#39;&#39;)"">8</a>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl04&#39;,&#39;&#39;)"">9</a>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl05&#39;,&#39;&#39;)"">10</a>&nbsp;<input type=""submit"" name=""ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl02$ctl00"" value=""Last"" />&nbsp;</span>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void TestPostBackMode_Aspx_PagePrev (string result, TestRunItem runItem)
		{
			string originalHtml = @"<span id=""ctl00_ContentPlaceHolder1_ListView1_DataPager1""><input type=""submit"" name=""ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl00$ctl00"" value=""First"" />&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl00&#39;,&#39;&#39;)"">1</a>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl01&#39;,&#39;&#39;)"">2</a>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl02&#39;,&#39;&#39;)"">3</a>&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl03&#39;,&#39;&#39;)"">4</a>&nbsp;<span>5</span>&nbsp;&nbsp;<a href=""javascript:__doPostBack(&#39;ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl01$ctl05&#39;,&#39;&#39;)"">...</a>&nbsp;<input type=""submit"" name=""ctl00$ContentPlaceHolder1$ListView1$DataPager1$ctl02$ctl00"" value=""Last"" />&nbsp;</span>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}
}

