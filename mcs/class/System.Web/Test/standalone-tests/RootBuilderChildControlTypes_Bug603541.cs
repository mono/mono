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

namespace StandAloneTests.Control_GetUniqueIDRelativeTo
{
	[TestCase ("RootBuilderChildControlTypes_Bug603541", "HTML control types mapped by RootBuilder")]
	public sealed class RootBuilderChildControlTypes_Bug603541 : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "RootBuilderChildControlTypes_Bug603541"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx));
			
			return true;
		}
		
		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = @"<pre id=""log"">a: System.Web.UI.HtmlControls.HtmlAnchor
button: System.Web.UI.HtmlControls.HtmlButton
img: System.Web.UI.HtmlControls.HtmlImage
link: System.Web.UI.HtmlControls.HtmlGenericControl
meta: System.Web.UI.HtmlControls.HtmlGenericControl
select: System.Web.UI.HtmlControls.HtmlSelect
table: System.Web.UI.HtmlControls.HtmlTable
td: System.Web.UI.HtmlControls.HtmlTableCell
tr: System.Web.UI.HtmlControls.HtmlTableRow
th: System.Web.UI.HtmlControls.HtmlTableCell
textarea: System.Web.UI.HtmlControls.HtmlTextArea
inputButton: System.Web.UI.HtmlControls.HtmlInputButton
inputSubmit: System.Web.UI.HtmlControls.HtmlInputSubmit
inputReset: System.Web.UI.HtmlControls.HtmlInputReset
inputCheckbox: System.Web.UI.HtmlControls.HtmlInputCheckBox
inputFile: System.Web.UI.HtmlControls.HtmlInputFile
inputHidden: System.Web.UI.HtmlControls.HtmlInputHidden
inputImage: System.Web.UI.HtmlControls.HtmlInputImage
inputRadio: System.Web.UI.HtmlControls.HtmlInputRadioButton
inputText: System.Web.UI.HtmlControls.HtmlInputText
inputPassword: System.Web.UI.HtmlControls.HtmlInputPassword
</pre>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}
}

