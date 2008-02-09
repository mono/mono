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
		private DateTime dateTimeProp;
		private Type typeProp;

		public string StrProp {
			get { return strProp; }
			set { strProp = value; }
		}

		public int IntProp {
			get { return intProp; }
			set { intProp = value; }
		}

		public DateTime DateTimeProp {
			get { return dateTimeProp; }
			set { dateTimeProp = value; }
		}

		public Type TypeProp {
			get { return typeProp; }
			set { typeProp = value; }
		}
	}

	[TestFixture]
	public class AppSettingsExpressionBuilderTest
	{
		[Test] // GetAppSetting (String)
		[Category ("NunitWeb")]
		public void GetAppSetting1 ()
		{
			PageDelegates pd = new PageDelegates ();
			pd.Load = GetAppSetting1_Load;
			WebTest test = new WebTest (new PageInvoker (pd));
			test.Run ();
		}

		[Test] // GetAppSetting (String)
		public void GetAppSetting1_Key_DoesNotExist ()
		{
			try {
				AppSettingsExpressionBuilder.GetAppSetting ("DoesNotExist");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// The application setting 'DoesNotExist' was
				// not found in the applications configuration
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'DoesNotExist'") != -1, "#5");
			}
		}

		[Test] // GetAppSetting (String)
		public void GetAppSetting1_Key_Null ()
		{
			try {
				AppSettingsExpressionBuilder.GetAppSetting ((string) null);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// The application setting '' was not found in
				// the applications configuration
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#5");
			}
		}

		[Test] // GetAppSetting (String, Type, String)
		[Category ("NunitWeb")]
		public void GetAppSetting2 ()
		{
			PageDelegates pd = new PageDelegates ();
			pd.Load = GetAppSetting2_Load;
			WebTest test = new WebTest (new PageInvoker (pd));
			test.Run ();
		}

		[Test] // GetAppSetting (String, Type, String)
		public void GetAppSetting2_Key_Null ()
		{
			try {
				AppSettingsExpressionBuilder.GetAppSetting (
					(string) null, 
					typeof (SettingTestingType),
					"StrProp");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// The application setting '' was not found in
				// the applications configuration
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#5");
			}
		}

		[Test]
		public void SupportsEvaluate ()
		{
			AppSettingsExpressionBuilder aseb = new AppSettingsExpressionBuilder ();
			Assert.IsTrue (aseb.SupportsEvaluate);
		}

		public static void GetAppSetting1_Load (Page p)
		{
			object o = AppSettingsExpressionBuilder.GetAppSetting ("strvalue");
			Assert.AreEqual (typeof (string), o.GetType (), "#A1");
			Assert.AreEqual ("str", o, "#A2");

			o = AppSettingsExpressionBuilder.GetAppSetting ("intvalue");
			Assert.AreEqual (typeof (string), o.GetType (), "#B1");
			Assert.AreEqual ("123", o, "#B2");
		}

		public static void GetAppSetting2_Load (Page p)
		{
			object o = AppSettingsExpressionBuilder.GetAppSetting ("strvalue", typeof (SettingTestingType), "StrProp");
			Assert.AreEqual (typeof (string), o.GetType (), "#A1");
			Assert.AreEqual ("str", o, "#A2");

			// property does not exist
			o = AppSettingsExpressionBuilder.GetAppSetting ("strvalue", typeof (SettingTestingType), "NotExistsProp");
			Assert.AreEqual (typeof (string), o.GetType (), "#B1");
			Assert.AreEqual ("str", o, "#B2");

			o = AppSettingsExpressionBuilder.GetAppSetting ("intvalue", typeof (SettingTestingType), "IntProp");
			Assert.AreEqual (typeof (int), o.GetType (), "#C1");
			Assert.AreEqual (123, o, "#C2");

			// conversion
			o = AppSettingsExpressionBuilder.GetAppSetting ("intvalue", typeof (SettingTestingType), "StrProp");
			Assert.AreEqual (typeof (string), o.GetType (), "#D1");
			Assert.AreEqual ("123", o, "#D2");

			// property does not exist
			o = AppSettingsExpressionBuilder.GetAppSetting ("intvalue", typeof (SettingTestingType), "NotExistsProp");
			Assert.AreEqual (typeof (string), o.GetType (), "#E1");
			Assert.AreEqual ("123", o, "#E2");

			// targetType null
			o = AppSettingsExpressionBuilder.GetAppSetting ("intvalue", (Type) null, "NotExistsProp");
			Assert.AreEqual (typeof (string), o.GetType (), "#F1");
			Assert.AreEqual ("123", o, "#F2");

			// conversion failed
			try {
				AppSettingsExpressionBuilder.GetAppSetting ("intvalue",
					typeof (SettingTestingType), "DateTimeProp");
				Assert.Fail ("#G1");
			} catch (FormatException ex) {
				// String was not recognized as a valid DateTime
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#G2");
				Assert.IsNotNull (ex.Message, "#G3");
			}

			// conversion not supported
			try {
				AppSettingsExpressionBuilder.GetAppSetting ("intvalue",
					typeof (SettingTestingType), "TypeProp");
				Assert.Fail ("#H1");
			} catch (InvalidOperationException ex) {
				// Could not convert the AppSetting '123' to the
				// type 'Type' on property 'TypeProp'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#H2");
				Assert.IsNull (ex.InnerException, "#H3");
				Assert.IsNotNull (ex.Message, "#H4");
				Assert.IsTrue (ex.Message.IndexOf ("'123'") != -1, "#H5");
				Assert.IsTrue (ex.Message.IndexOf ("'Type'") != -1, "#H6");
				Assert.IsTrue (ex.Message.IndexOf ("'TypeProp'") != -1, "#H7");
			}

			// propertyName null
			try {
				AppSettingsExpressionBuilder.GetAppSetting ("intvalue",
					typeof (SettingTestingType), (string) null);
				Assert.Fail ("#I1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#I2");
				Assert.IsNull (ex.InnerException, "#I3");
				Assert.IsNotNull (ex.Message, "#I4");
				//Assert.AreEqual ("key", ex.ParamName, "#I5");
			}
		}
	}
}
#endif
