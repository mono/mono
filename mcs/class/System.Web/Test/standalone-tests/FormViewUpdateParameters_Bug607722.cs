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

namespace StandAloneTests.FormViewUpdateParameters_Bug607722
{
	[TestCase ("FormViewUpdateParameters_Bug607722", "FormView update parameters should include keys")]
	public sealed class Test_01 : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "FormViewUpdateParameters_Bug607722"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx));
			
#if BUG_IN_THE_RUNTIME_IS_FIXED
			// With this version of code, the runtime segfaults. Until this is fixed,
			// we'll be using an alternative version of the code
			runItems.Add (new TestRunItem ("Default.aspx", null) {
					PostValues = new SerializableDictionary <string, string> {
						{"__EVENTTARGET", "FormView1$EditButton"},
						{"__EVENTARGUMENT", String.Empty}
					},
					UrlDescription = "Edit phase"
				}
			);
			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx_POST) {
					PostValues = new SerializableDictionary <string, string> {
						{"__EVENTTARGET", "FormView1$UpdateButton"},
						{"__EVENTARGUMENT", String.Empty},
						{"FormView1$M1TextBox", "12"},
						{"FormView1$M2TextBox", "12"}
					},
					UrlDescription = "Update phase"
				}
			);
#else
			runItems.Add (new TestRunItem ("Default.aspx", null) {
					PostValues = new string[] {
						"__EVENTTARGET", "FormView1$EditButton",
						"__EVENTARGUMENT", String.Empty
					},
					UrlDescription = "Edit phase"
				}
			);
			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx_Update) {
					PostValues = new string[] {
						"__EVENTTARGET", "FormView1$UpdateButton",
						"__EVENTARGUMENT", String.Empty,
						"FormView1$M1TextBox", "12",
						"FormView1$M2TextBox", "12"
					},
					UrlDescription = "Update phase"
				}
			);
#endif
			
			return true;
		}
		
		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = @"M1: <span id=""FormView1_M1Label"">0</span><br />M2: <span id=""FormView1_M2Label"">0</span>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
		
		void Default_Aspx_Update (string result, TestRunItem runItem)
		{
			string originalHtml = @"M1: <span id=""FormView1_M1Label"">12</span><br />M2: <span id=""FormView1_M2Label"">12</span>";
			Helpers.ExtractAndCompareCodeFromHtml (result, originalHtml, "#A1");
		}
	}
}

