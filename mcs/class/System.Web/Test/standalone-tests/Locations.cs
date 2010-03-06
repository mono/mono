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

namespace StandAloneTests.Locations
{
	[TestCase ("Locations", "Configuration <location> tests.")]
	public sealed class Locations : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "Locations"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx));
			runItems.Add (new TestRunItem ("Stuff.aspx", Stuff_Aspx));
			runItems.Add (new TestRunItem ("sub/Default.aspx", Sub_Default_Aspx));
			runItems.Add (new TestRunItem ("sub/Stuff.aspx", Sub_Stuff_Aspx));
			runItems.Add (new TestRunItem ("sub/sub/Default.aspx", Sub_Sub_Default_Aspx));
			runItems.Add (new TestRunItem ("sub/sub/Stuff.aspx", Sub_Sub_Stuff_Aspx));
			
			return true;
		}

		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "/Hello";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Stuff_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = @"<pre id=""settings"">        /Web.config [1]: '[toplevel]'
        /Web.config [2]: 'Stuff.aspx'
</pre>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Sub_Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "/sub/Hello";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Sub_Stuff_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = @"<pre id=""settings"">        /Web.config [1]: '[toplevel]'
        /Web.config [3]: 'sub'
    /sub/Web.config [1]: '[toplevel]'
        /Web.config [4]: 'sub/Stuff.aspx'
    /sub/Web.config [2]: 'Stuff.aspx'
</pre>";

			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Sub_Sub_Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "/sub/sub/Hello";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}

		void Sub_Sub_Stuff_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = @"<pre id=""settings"">        /Web.config [1]: '[toplevel]'
        /Web.config [3]: 'sub'
    /sub/Web.config [1]: '[toplevel]'
        /Web.config [6]: 'sub/sub'
    /sub/Web.config [4]: 'sub'
/sub/sub/Web.config [1]: '[toplevel]'
        /Web.config [5]: 'sub/sub/Stuff.aspx'
    /sub/Web.config [3]: 'sub/Stuff.aspx'
/sub/sub/Web.config [2]: 'Stuff.aspx'
</pre>";

			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}
}
