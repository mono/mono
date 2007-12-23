//
// Tests for System.Web.UI.WebControls.ListBoxTest.cs
//
// Author:
//  Vladimir Krasnov (vladimirk@mainsoft.com)
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
//

#if NET_2_0

using NUnit.Framework;
using System;
using System.IO;
using System.Drawing;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using System.Web.Compilation;

namespace MonoTests.System.Web.Compilation
{
	public class SettingTestingType
	{
		private string strProp;
		private int intProp;

		public string StrProp
		{
			get { return strProp; }
			set { strProp = value; }
		}

		public int IntProp
		{
			get { return intProp; }
			set { intProp = value; }
		}
	}

	[TestFixture]
	public class AppSettingsExpressionBuilderTest
	{

		[Test]
		[Category ("NunitWeb")]
		public void GetAppSetting1 ()
		{
			PageDelegates pd = new PageDelegates ();
			pd.Load = GetAppSetting_Load1;
			WebTest test = new WebTest (new PageInvoker (pd));
			string html = test.Run ();

		}

		public static void GetAppSetting_Load1 (Page p)
		{
			object o = AppSettingsExpressionBuilder.GetAppSetting ("strvalue", typeof (SettingTestingType), "StrProp");
			Assert.AreEqual (typeof (string), o.GetType (), "GetAppSetting1 #1");
			Assert.AreEqual ("str", o, "GetAppSetting1 #2");

			o = AppSettingsExpressionBuilder.GetAppSetting ("strvalue", typeof (SettingTestingType), "NotExistsProp");
			Assert.AreEqual (typeof (string), o.GetType (), "GetAppSetting1 #3");
			Assert.AreEqual ("str", o, "GetAppSetting1 #4");

			o = AppSettingsExpressionBuilder.GetAppSetting ("intvalue", typeof (SettingTestingType), "IntProp");
			Assert.AreEqual (typeof (int), o.GetType (), "GetAppSetting1 #5");
			Assert.AreEqual (123, o, "GetAppSetting1 #6");

			o = AppSettingsExpressionBuilder.GetAppSetting ("intvalue", typeof (SettingTestingType), "NotExistsProp");
			Assert.AreEqual (typeof (string), o.GetType (), "GetAppSetting1 #7");
			Assert.AreEqual ("123", o, "GetAppSetting1 #8");
		}
	}
}
#endif
