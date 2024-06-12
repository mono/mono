//
// RuntimeConfigTest.cs 
//	- unit tests for System.Web.Configuration.RuntimeConfig
//
// Author:
//	Mel Dafert <m@dafert.at>
//
// Copyright (C) 2022 Mel Dafert
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


using NUnit.Framework;

using System;
using System.Configuration;
using _Configuration = System.Configuration.Configuration;
using System.IO;
using System.Web.Configuration;
using System.Web;
using System.Web.Security;
using MonoTests.SystemWeb.Framework;
using System.Web.UI;

namespace MonoTests.System.Web.Configuration {

	[TestFixture]
	public class RuntimeConfigTest  {

		[Test]
		[Category ("NunitWeb")]
		public void GetAppConfig ()
		{
			WebTest t = new WebTest ();
			t.Invoker = PageInvoker.CreateOnLoad (GetAppConfig_load);
			t.Run ();
		}

		public static void GetAppConfig_load (Page p)
		{
			CompilationSection cs = RuntimeConfig.GetAppConfig().Compilation;

			Assert.IsTrue (cs.Debug, "A1");
			Assert.IsFalse (cs.Batch, "A2");

			MembershipSection ms = RuntimeConfig.GetAppConfig().Membership;

			Assert.AreEqual (ms.DefaultProvider, "FakeProvider", "B1");
		}
	}
}
