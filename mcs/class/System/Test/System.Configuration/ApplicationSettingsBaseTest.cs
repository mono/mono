//
// System.Configuration.ApplicationsSettingsBaseTest.cs - Unit tests
// for System.Configuration.ApplicationSettingsBase.
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
//
// Copyright (C) 2005, 2006 Novell, Inc (http://www.novell.com)
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

//#define SPEW

#if NET_2_0

using System;
using System.Text;
using System.Configuration;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Configuration {
	class ProviderPoker : LocalFileSettingsProvider {
		public override void Initialize (string name,
						 NameValueCollection values)
		{
#if SPEW
			Console.WriteLine ("Initialize '{0}'", name);
			Console.WriteLine (Environment.StackTrace);
#endif
			if (name == null)
				name = "ProviderPoker";

			base.Initialize (name, values);
		}

		public override SettingsPropertyValueCollection GetPropertyValues (SettingsContext context,
										   SettingsPropertyCollection properties)
		{
#if SPEW
			Console.WriteLine (Environment.StackTrace);
#endif
			return base.GetPropertyValues (context, properties);
		}

		public override void SetPropertyValues (SettingsContext context,
							SettingsPropertyValueCollection values)
		{
#if SPEW
			Console.WriteLine (Environment.StackTrace);
#endif
			base.SetPropertyValues (context, values);
		}

		public override string ApplicationName {
			get {
#if SPEW
				Console.WriteLine (Environment.StackTrace);
#endif
				return base.ApplicationName;
			}
			set {
#if SPEW
				Console.WriteLine ("ApplicationName = {0}", value);
				Console.WriteLine (Environment.StackTrace);
#endif
				base.ApplicationName = value;
			}
		}
	}

	/* a basic settings class.  just two settings, one application
	 * scoped, one user scoped */
	class TestSettings1 : ApplicationSettingsBase
	{
		public TestSettings1() : base ("TestSettings1")
		{
		}

		[ApplicationScopedSetting]
		[DefaultSettingValue ("root")]
		public string Username {
			get { return (string)this["Username"]; }
			set { this["Username"] = value; }
		}

		[UserScopedSetting]
		[DefaultSettingValue ("8 Cambridge Center")]
		public string Address {
			get { return (string)this["Address"]; }
			set { this["Address"] = value; }
		}
	}

	/* an error according to msdn2 docs.  both ApplicationScoped
	 * and UserScoped attributes on the same property */
	class TestSettings2 : ApplicationSettingsBase
	{
		public TestSettings2() : base ("TestSettings2")
		{
		}

		[ApplicationScopedSetting]
		[UserScopedSetting]
		[SettingsProvider (typeof (ProviderPoker))]
		public string Username {
			get { return (string)this["Username"]; }
			set { this["Username"] = value; }
		}
	}

	/* a custom provider for our setting */
	class TestSettings3 : ApplicationSettingsBase
	{
		public TestSettings3() : base ("TestSettings3")
		{
		}

		[ApplicationScopedSetting]
		[SettingsProvider (typeof (ProviderPoker))]
		public string Username {
			get { return (string)this["Username"]; }
			set { this["Username"] = value; }
		}
	}

	class TestSettings4 : ApplicationSettingsBase {

		public TestSettings4 ()
			: base ("TestSettings4")
		{
		}

		[ApplicationScopedSetting]
		[DefaultSettingValue ("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <string>go</string>\r\n  <string>mono</string>\r\n  </ArrayOfString>")]
		public StringCollection Values {
			get { return (StringCollection) this ["Values"]; }
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
				Console.WriteLine ("'{0}': '{1}'", prop.Provider.Name, prop.Provider.ApplicationName);
			}
		}

		[Test]
		public void TestSettings1_SetProperty ()
		{
			TestSettings1 settings = new TestSettings1 ();
			bool setting_changing = false;
			bool setting_changed = false;

			settings.SettingChanging += delegate (object sender, SettingChangingEventArgs e) {
				setting_changing = true;
				Assert.AreEqual ("Username", e.SettingName, "A1");
				Assert.AreEqual ("toshok", e.NewValue, "A2");
				Assert.AreEqual ("TestSettings1", e.SettingKey, "A3");
				Assert.AreEqual (settings.GetType().FullName, e.SettingClass, "A4");
			};

			settings.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e) {
				Assert.IsTrue (setting_changing, "A5");
				setting_changed = true;
				Assert.AreEqual ("Username", e.PropertyName, "A6");
			};

			settings.Username = "toshok";

			Assert.IsTrue (setting_changing && setting_changed, "A7");
			Assert.AreEqual ("toshok", settings.Username, "A8");
		}

		[Test]
		public void TestSettings1_SetPropertyCancel ()
		{
			TestSettings1 settings = new TestSettings1 ();
			bool setting_changing = false;
			bool setting_changed = false;

			settings.SettingChanging += delegate (object sender, SettingChangingEventArgs e) {
				setting_changing = true;
				Assert.AreEqual ("Username", e.SettingName, "A1");
				Assert.AreEqual ("toshok", e.NewValue, "A2");
				Assert.AreEqual ("TestSettings1", e.SettingKey, "A3");
				Assert.AreEqual (settings.GetType().FullName, e.SettingClass, "A4");
				e.Cancel = true;
			};

			settings.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e) {
				setting_changed = true;
				Assert.Fail ("shouldn't reach here.", "A5");
			};

			settings.Username = "toshok";

			Assert.IsTrue (setting_changing, "A6");
			Assert.IsFalse (setting_changed, "A7");

			Assert.AreEqual ("root", settings.Username, "A8");
		}

		[Test]
		public void TestSettings1_SettingsLoaded ()
		{
			TestSettings1 settings = new TestSettings1 ();
			bool settings_loaded = false;
			SettingsProvider loaded_provider = null;

			settings.SettingsLoaded += delegate (object sender, SettingsLoadedEventArgs e) {
				settings_loaded = true;
				loaded_provider = e.Provider;
			};

			Assert.AreEqual ("root", settings.Username, "A1");
			Assert.IsTrue (settings_loaded, "A2");
			Assert.AreEqual (loaded_provider, settings.Properties ["Username"].Provider, "A3");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSettings1_SetPropertyReset ()
		{
			TestSettings1 settings = new TestSettings1 ();

			settings.Username = "toshok";

			Assert.AreEqual ("toshok", settings.Username, "A1");

			settings.Reset ();

			Assert.AreEqual ("root", settings.Username, "A2");
		}

		[Test]
		public void TestSettings2_Properties ()
		{
			// This test will fail when there are newer versions
			// of the test assemblies - so conditionalize it in
			// such cases.
#if TARGET_JVM
			string expected = "MonoTests.System.Configuration.ProviderPoker, System.Test, Version=0.0.0.0";
#else
#if NET_4_0
			string expected = "MonoTests.System.Configuration.ProviderPoker, System_test_net_4_0, Version=0.0.0.0";
#else
			string expected = "MonoTests.System.Configuration.ProviderPoker, System_test_net_2_0, Version=0.0.0.0";
#endif
#endif
			Assert.AreEqual (expected, new SettingsProviderAttribute (typeof (ProviderPoker)).ProviderTypeName.Substring (0, expected.Length), "#1");
			TestSettings2 settings = new TestSettings2 ();

			/* should throw ConfigurationException */
			IEnumerator props = settings.Properties.GetEnumerator();
		}

		[Test]
		[Ignore ("On MS.NET it returns null ...")]
		public void TestSettings3_Properties ()
		{
			TestSettings3 settings = new TestSettings3 ();

			Assert.AreEqual ("root", settings.Username, "A1");
		}

		public static void Main (string[] args)
		{
			ApplicationSettingsBaseTest test = new ApplicationSettingsBaseTest();
			test.TestSettings1_Properties();
		}

		[Test]
		public void Synchronized ()
		{
			Bug78430 s = new Bug78430 ();
			s.Initialize (null, new SettingsPropertyCollection (),
				new SettingsProviderCollection ());
			SettingsBase sb = SettingsBase.Synchronized (s);
			Assert.IsTrue (sb.IsSynchronized, "#1");
			Assert.IsTrue (sb is Bug78430, "#2");
			// these checks are so cosmetic, actually not
			// worthy of testing.
			Assert.IsTrue (Object.ReferenceEquals (s, sb), "#3");
			Assert.IsFalse (sb.Properties.IsSynchronized, "#4");
		}

		class Bug78430 : SettingsBase
		{
		}

		[Test] // bug #78654
		public void DefaultSettingValueAs ()
		{
			Assert.AreEqual (1, new Bug78654 ().IntSetting);
		}

		class Bug78654 : ApplicationSettingsBase
		{
			[UserScopedSettingAttribute ()]
			[DefaultSettingValueAttribute ("1")]
			public int IntSetting {
				get { return ((int)(this ["IntSetting"])); }
			}
		}

		[Test]
		public void TestSettings4_StringCollection_DefaultSettingValue ()
		{
			TestSettings4 settings = new TestSettings4 ();
			Assert.AreEqual (2, settings.Values.Count, "Count");
			Assert.AreEqual ("go", settings.Values[0], "0");
			Assert.AreEqual ("mono", settings.Values[1], "1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Providers ()
		{
			Assert.AreEqual (0, new TestSettings1 ().Providers.Count);
		}

                class Bug532180 : ApplicationSettingsBase {
                        [UserScopedSetting]
                        [DefaultSettingValue("10")]
                        public int IntSetting {
                                get { return (int)this["IntSetting"]; }
                                set { this["IntSetting"] = value; }                               
                        }
                }

                [Test] // bug #532180
                public void DefaultSettingValueAsWithReload() {
                        Bug532180 settings = new Bug532180();
                        Assert.AreEqual(10, settings.IntSetting, "A1");
                        settings.IntSetting = 1;
                        Assert.AreEqual(1, settings.IntSetting, "A2");
                        settings.Reload();
                        Assert.AreEqual(10, settings.IntSetting, "A3");
                }                        
	}
}

#endif
