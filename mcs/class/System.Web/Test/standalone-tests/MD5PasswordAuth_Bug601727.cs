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

namespace StandAloneTests.MD5PasswordAuth_Bug601727
{
	[TestCase ("MD5PasswordAuth_Bug601727", "Hashed passwords should be compared case-insensitively")]
	public sealed class Test_01 : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "MD5PasswordAuth_Bug601727"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("login.aspx", null));
			runItems.Add (new TestRunItem ("login.aspx", LoggedIn_Aspx) {
                                        PostValues = new string[] {
                                                "__EVENTTARGET", String.Empty,
                                                "__EVENTARGUMENT", String.Empty,
						"loginControl$LoginButton", "Log In",
						"loginControl$Password", "test",
						"loginControl$UserName", "admin"
                                        },
                                        UrlDescription = "Login postback"
                                }
                        );
			return true;
		}
		
		void LoggedIn_Aspx (string result, TestRunItem runItem)
		{
			Assert.IsTrue (runItem.Redirected, "#A1");
			Assert.AreEqual ("/default.aspx", runItem.RedirectLocation, "#A2");
		}
	}
}

