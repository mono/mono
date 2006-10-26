//
// Tests for MappingUrlTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

// ************IMPORTANT**********
// Note: This test completed ,but cannot be run by Framework limitations!

#if NET_2_0

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NUnit.Framework;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class MappingUrlTest
	{
		[TestFixtureSetUp]
		public void CopyTestResources ()
		{
#if DOT_NET
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.Mapping.aspx", "Mapping.aspx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.Mapping1.aspx", "Mapping1.aspx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.WebMapping.config", "Web.config");
#else
			WebTest.CopyResource (GetType (), "Mapping.aspx", "Mapping.aspx");
			WebTest.CopyResource (GetType (), "Mapping1.aspx", "Mapping1.aspx");
			WebTest.CopyResource (GetType (), "WebMapping.config", "Web.config");
#endif
		}

		[Test]
		[Category ("NunitWeb")]
        [Category ("NotDotNet")]
		[Category ("NotWorking")]
		public void MappingUrl()
		{
			WebTest t = new WebTest ("Mapping.aspx");
			string result = t.Run ();
			if (result.IndexOf ("Default from mapping") < 0)
				Assert.Fail ("Mapping URL Fail");
		}

		[TestFixtureTearDown]
		public void Unload ()
		{
			WebTest.Unload ();
		}
	}
}
#endif
