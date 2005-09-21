//
// System.Configuration.ApplicationsSettingsBaseTest.cs - Unit tests
// for System.Configuration.ApplicationSettingsBase.
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
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

using System;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.System.Configuration {

	/* a basic settings class.  just two settings, one application
	 * scoped, one user scoped */
	class TestSettings1 : ApplicationSettingsBase
	{
		public TestSettings1() 
			: base ("TestSettings1")
		{
		}

		[ApplicationScopedSetting]
		[DefaultSettingValue ("root")]
		public string Username {
			get {
				return (string)this["Username"];
			}
			set {
				this["Username"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue ("8 Cambridge Center")]
		public string Address {
			get {
				return (string)this["Address"];
			}
			set {
				this["Address"] = value;
			}
		}
	}

	/* an error.  both ApplicationScoped and UserScoped attributes on the same property */
	class TestSettings2 : ApplicationSettingsBase
	{
		public TestSettings2() 
			: base ("TestSettings2")
		{
		}

		[ApplicationScopedSetting]
		[UserScopedSetting]
		public string Username {
			get {
				return (string)this["Username"];
			}
			set {
				this["Username"] = value;
			}
		}
	}


	[TestFixture]
	public class ApplicationSettingsBaseTest
	{
		[Test]
		public void TestSettings1_Properties ()
		{
			TestSettings1 settings = new TestSettings1 ();

			IEnumerator props = settings.Properties.GetEnumerator();
			Assert.IsNotNull (props, "A1");
			
			Assert.IsTrue (props.MoveNext(), "A2");
			Assert.AreEqual ("Address", ((SettingsProperty)props.Current).Name, "A3");

			Assert.IsTrue (props.MoveNext(), "A4");
			Assert.AreEqual ("Username", ((SettingsProperty)props.Current).Name, "A5");

			Assert.AreEqual ("root", settings.Username, "A6");
			Assert.AreEqual ("8 Cambridge Center", settings.Address, "A7");
		}

		[Test]
		public void TestSettings1_Provider ()
		{
			TestSettings1 settings = new TestSettings1 ();

			/* since we didn't specify a provider for any
			 * of them, they should all use the
			 * LocalFileSettingsProvider */
			foreach (SettingsProperty prop in settings.Properties) {
				Assert.AreEqual (typeof (LocalFileSettingsProvider), prop.Provider.GetType(), "A1");
				Console.WriteLine ("'{0}'", prop.Provider.ApplicationName);
			}
		}


		[Test]
		public void TestSettings2_Properties ()
		{
			TestSettings2 settings = new TestSettings2 ();

			/* should throw ConfigurationException */
			IEnumerator props = settings.Properties.GetEnumerator();
		}

		public static int Main (string[] args)
		{
			ApplicationSettingsBaseTest test = new ApplicationSettingsBaseTest();
			test.TestSettings1_Properties();
			return 0;
		}
	}

}

#endif
