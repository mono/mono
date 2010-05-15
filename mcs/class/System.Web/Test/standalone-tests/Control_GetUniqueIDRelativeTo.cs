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


namespace StandAloneTests.Control_GetUniqueIDRelativeTo
{
	[TestCase ("Control_GetUniqueIDRelativeTo", "Control.GetUniqueIDRelativeTo tests")]
	public sealed class Control_GetUniqueIDRelativeTo : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "Control_GetUniqueIDRelativeTo"); }
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
			string originalHtml = @"<pre id=""log"">Page; Relative to: null; Result: exception System.ArgumentNullException (expected)
A control; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
TextBox; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
Item: 0; Relative to: repeater1$ctl00; Result: &#39;ctl00$label1&#39;
Item: 0; Relative to: repeater1; Result: &#39;repeater1$ctl00$label1&#39;
Item: 0; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
	Item: 0; Relative to: repeater1$ctl00$innerRepeater1$ctl00; Result: &#39;ctl00$innerLabel1&#39;
	Item: 0; Relative to: repeater1; Result: &#39;repeater1$ctl00$innerRepeater1$ctl00$innerLabel1&#39;
	Item: 0; Relative to: repeater1$ctl00$innerRepeater1; Result: &#39;innerRepeater1$ctl00$innerLabel1&#39;
	Item: 0; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
	Item: 1; Relative to: repeater1$ctl00$innerRepeater1$ctl01; Result: &#39;ctl01$innerLabel1&#39;
	Item: 1; Relative to: repeater1; Result: &#39;repeater1$ctl00$innerRepeater1$ctl01$innerLabel1&#39;
	Item: 1; Relative to: repeater1$ctl00$innerRepeater1; Result: &#39;innerRepeater1$ctl01$innerLabel1&#39;
	Item: 1; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
	Item: 2; Relative to: repeater1$ctl00$innerRepeater1$ctl02; Result: &#39;ctl02$innerLabel1&#39;
	Item: 2; Relative to: repeater1; Result: &#39;repeater1$ctl00$innerRepeater1$ctl02$innerLabel1&#39;
	Item: 2; Relative to: repeater1$ctl00$innerRepeater1; Result: &#39;innerRepeater1$ctl02$innerLabel1&#39;
	Item: 2; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
Item: 1; Relative to: repeater1$ctl02; Result: &#39;ctl02$label1&#39;
Item: 1; Relative to: repeater1; Result: &#39;repeater1$ctl02$label1&#39;
Item: 1; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
	Item: 0; Relative to: repeater1$ctl02$innerRepeater1$ctl00; Result: &#39;ctl00$innerLabel1&#39;
	Item: 0; Relative to: repeater1; Result: &#39;repeater1$ctl02$innerRepeater1$ctl00$innerLabel1&#39;
	Item: 0; Relative to: repeater1$ctl02$innerRepeater1; Result: &#39;innerRepeater1$ctl00$innerLabel1&#39;
	Item: 0; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
	Item: 1; Relative to: repeater1$ctl02$innerRepeater1$ctl01; Result: &#39;ctl01$innerLabel1&#39;
	Item: 1; Relative to: repeater1; Result: &#39;repeater1$ctl02$innerRepeater1$ctl01$innerLabel1&#39;
	Item: 1; Relative to: repeater1$ctl02$innerRepeater1; Result: &#39;innerRepeater1$ctl01$innerLabel1&#39;
	Item: 1; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
	Item: 2; Relative to: repeater1$ctl02$innerRepeater1$ctl02; Result: &#39;ctl02$innerLabel1&#39;
	Item: 2; Relative to: repeater1; Result: &#39;repeater1$ctl02$innerRepeater1$ctl02$innerLabel1&#39;
	Item: 2; Relative to: repeater1$ctl02$innerRepeater1; Result: &#39;innerRepeater1$ctl02$innerLabel1&#39;
	Item: 2; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
Item: 2; Relative to: repeater1$ctl04; Result: &#39;ctl04$label1&#39;
Item: 2; Relative to: repeater1; Result: &#39;repeater1$ctl04$label1&#39;
Item: 2; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
	Item: 0; Relative to: repeater1$ctl04$innerRepeater1$ctl00; Result: &#39;ctl00$innerLabel1&#39;
	Item: 0; Relative to: repeater1; Result: &#39;repeater1$ctl04$innerRepeater1$ctl00$innerLabel1&#39;
	Item: 0; Relative to: repeater1$ctl04$innerRepeater1; Result: &#39;innerRepeater1$ctl00$innerLabel1&#39;
	Item: 0; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
	Item: 1; Relative to: repeater1$ctl04$innerRepeater1$ctl01; Result: &#39;ctl01$innerLabel1&#39;
	Item: 1; Relative to: repeater1; Result: &#39;repeater1$ctl04$innerRepeater1$ctl01$innerLabel1&#39;
	Item: 1; Relative to: repeater1$ctl04$innerRepeater1; Result: &#39;innerRepeater1$ctl01$innerLabel1&#39;
	Item: 1; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
	Item: 2; Relative to: repeater1$ctl04$innerRepeater1$ctl02; Result: &#39;ctl02$innerLabel1&#39;
	Item: 2; Relative to: repeater1; Result: &#39;repeater1$ctl04$innerRepeater1$ctl02$innerLabel1&#39;
	Item: 2; Relative to: repeater1$ctl04$innerRepeater1; Result: &#39;innerRepeater1$ctl02$innerLabel1&#39;
	Item: 2; Relative to: __Page; Result: exception System.InvalidOperationException (expected)
</pre>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}
}

#endif
