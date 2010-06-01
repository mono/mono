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
using System.Web;
using System.Web.Hosting;

using StandAloneRunnerSupport;
using StandAloneTests;

using NUnit.Framework;

namespace StandAloneTests.PageMetaAttributes
{
	[TestCase ("PageMetaAttributes", "Tests for Page.Meta{Description,Keywords} properties/attributes")]
	public sealed class PageMetaAttributes : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "PageMetaAttributes"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("/Default.aspx", Default_Aspx));
			
			return true;
		}

		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<head><title>\r\n	Test\r\n</title><meta name=\"description\" content=\"This is meta description\" /><meta name=\"keywords\" content=\"meta,keywords,here\" /></head>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}
}
#endif