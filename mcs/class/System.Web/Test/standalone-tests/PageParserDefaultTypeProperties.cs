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
using System.Configuration;
using System.Configuration.Provider;
using System.IO;
using System.Web;
using System.Web.Hosting;

using StandAloneRunnerSupport;
using StandAloneTests;

using NUnit.Framework;

namespace StandAloneTests.PageParserDefaultTypeProperties
{
	[TestCase ("PageParserDefaultTypeProperties 01", "Tests for PageParser.Default*Type properties")]
	public sealed class PageParserDefaultTypeProperties_01 : ITestCase
	{
		static string[] expectedMessages_1 = {
			"1: DefaultApplicationBaseType: set",
			"1: DefaultPageBaseType: set",
			"1: DefaultPageParserFilterType: set",
			"1: DefaultUserControlBaseType: set"
		};

		static string[] expectedMessages_2 = {
			"2: DefaultApplicationBaseType: exception 'System.ArgumentException' thrown.",
			"2: DefaultPageBaseType: exception 'System.ArgumentException' thrown.",
			"2: DefaultPageParserFilterType: exception 'System.ArgumentException' thrown.",
			"2: DefaultUserControlBaseType: exception 'System.ArgumentException' thrown."
		};

		static string[] expectedMessages_3 = {
			"3: DefaultApplicationBaseType: set",
			"3: DefaultPageBaseType: set",
			"3: DefaultPageParserFilterType: set",
			"3: DefaultUserControlBaseType: set"
		};
		
		public string PhysicalPath {
			get {
				return Path.Combine (Consts.BasePhysicalDir, "PageParserDefaultTypeProperties");
			}
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (
				new TestRunItem ("/default.aspx", "Set correct values", Default_Aspx_1) {
					AppDomainData = new object[] { "TestNumber", 1 }
				}
			);
			runItems.Add (
				new TestRunItem ("/default.aspx", "Set invalid values", Default_Aspx_2) {
					AppDomainData = new object[] { "TestNumber", 2 }
				}
			);
			runItems.Add (
				new TestRunItem ("/default.aspx", "Set null values", Default_Aspx_3) {
					AppDomainData = new object[] { "TestNumber", 3 }
				}
			);
			
			return true;
		}

		void Default_Aspx_1 (string result, TestRunItem runItem)
		{
			CheckResults (runItem.TestRunData as List <string>, expectedMessages_1);
		}

		void Default_Aspx_2 (string result, TestRunItem runItem)
		{
			CheckResults (runItem.TestRunData as List <string>, expectedMessages_2);
		}

		void Default_Aspx_3 (string result, TestRunItem runItem)
		{
			CheckResults (runItem.TestRunData as List <string>, expectedMessages_3);
		}
		
		void CheckResults (List <string> messages, string[] expectedMessages)
		{
			Assert.IsNotNull (messages, "#A1");

			int len = messages.Count;
			if (expectedMessages.Length != len)
				Assert.Fail ("Expected {0} messages, found {1}", expectedMessages.Length, len);
			
			for (int i = 0; i < len; i++)
				Assert.AreEqual (expectedMessages [i], messages [i], "#A2-" + i.ToString ());
		}
	}
}
#endif