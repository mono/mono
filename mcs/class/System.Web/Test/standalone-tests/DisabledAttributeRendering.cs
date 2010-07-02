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

namespace StandAloneTests.DisabledAttributeRendering
{
	[TestCase ("DisabledAttributeRendering 01", "4.0 does not render the 'disabled' attribute in default mode. Using default CSS class name.")]
	public sealed class Test_01 : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "DisabledAttributeRendering", "DefaultClassName"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx));
			runItems.Add (new TestRunItem ("OldRendering/Default.aspx", OldRendering_Default_Aspx));
			return true;
		}
		
		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<div class=\"aspNetDisabled MyClass\">\r\n\tI am disabled\r\n</div><a class=\"aspNetDisabled\">Disabled link</a>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
		
		void OldRendering_Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<div disabled=\"disabled\" class=\"MyClass\">\r\n\tI am disabled\r\n</div><a disabled=\"disabled\">Disabled link</a>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}

	[TestCase ("DisabledAttributeRendering 02", "4.0 does not render the 'disabled' attribute in default mode. Using custom CSS class name.")]
	public sealed class Test_02 : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "DisabledAttributeRendering", "CustomClassName"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx));
			runItems.Add (new TestRunItem ("OldRendering/Default.aspx", OldRendering_Default_Aspx));
			return true;
		}
		
		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<div class=\"MyDisabledControlClass MyClass\">\r\n\tI am disabled\r\n</div><a class=\"MyDisabledControlClass\">Disabled link</a>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
		
		void OldRendering_Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<div disabled=\"disabled\" class=\"MyClass\">\r\n\tI am disabled\r\n</div><a disabled=\"disabled\">Disabled link</a>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}
}
#endif