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

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Text;
using System.Configuration;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

using MonoTests.Helpers;

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
		TempDirectory _tempDir;
		string tempDir;

		[TestFixtureSetUp]
		public void FixtureSetup ()
		{
			// Use random temp directory to store settings files of tests.
			_tempDir = new TempDirectory ();
			tempDir = _tempDir.Path;
			var localAppData = Path.Combine (tempDir, "LocalAppData");
			Directory.CreateDirectory (localAppData);
			var appData = Path.Combine (tempDir, "AppData");
			Directory.CreateDirectory (appData);

			Environment.SetEnvironmentVariable ("XDG_DATA_HOME", localAppData);
			Environment.SetEnvironmentVariable ("XDG_CONFIG_HOME", appData);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			Environment.SetEnvironmentVariable ("XDG_DATA_HOME", null);
			Environment.SetEnvironmentVariable ("XDG_CONFIG_HOME", null);
			_tempDir.Dispose ();
		}

		[Test]
		public void TestSettings1_Properties ()
		{
			TestSettings1 settings = new TestSettings1 ();

			IEnumerator props = settings.Properties.GetEnumerator();
			Assert.IsNotNull (props, "A1");

			Assert.IsTrue (props.MoveNext(), "A4");
			Assert.AreEqual ("Address", ((SettingsProperty)props.Current).Name, "A5");
			
			Assert.IsTrue (props.MoveNext(), "A2");
			Assert.AreEqual ("Username", ((SettingsProperty)props.Current).Name, "A3");

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
#if XAMMAC_4_5
			string expected = "MonoTests.System.Configuration.ProviderPoker, xammac_net_4_5_System_test, Version=0.0.0.0";
#else
			string expected = "MonoTests.System.Configuration.ProviderPoker, net_4_x_System_test, Version=0.0.0.0";
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
                public void DefaultSettingValueAsWithReload() 
		{
                        Bug532180 settings = new Bug532180();
                        Assert.AreEqual (10, settings.IntSetting, "A1");
                        settings.IntSetting = 1;
                        Assert.AreEqual (1, settings.IntSetting, "A2");
                        settings.Reload ();
                        Assert.AreEqual (10, settings.IntSetting, "A3");
                }                        
		
		class Bug8592ConfHolder : ApplicationSettingsBase {
			[UserScopedSetting]
			public string TestKey1OnHolder { 
				get { return (string) this ["TestKey1OnHolder"] ?? ""; }
				set { this ["TestKey1OnHolder"] = value; }
			}
		}

		[Test]
		[Category ("NotOnWindows")] // https://github.com/mono/mono/issues/7343
		public void TestBug8592BasicOperations ()
		{
			var holder = new Bug8592ConfHolder ();
			holder.Reset ();
			holder.Save ();
			Assert.AreEqual ("", holder.TestKey1OnHolder, "#1");
			holder.TestKey1OnHolder = "candy";
			Assert.AreEqual ("candy", holder.TestKey1OnHolder, "#2");
			holder.Reload ();
			Assert.AreEqual ("", holder.TestKey1OnHolder, "#3");
			holder.TestKey1OnHolder = "candy";
			Assert.AreEqual ("candy", holder.TestKey1OnHolder, "#4");
			holder.Save ();
			Assert.AreEqual ("candy", holder.TestKey1OnHolder, "#5");
			holder.Reload ();
			Assert.AreEqual ("candy", holder.TestKey1OnHolder, "#6");
			holder.Reset ();
			Assert.AreEqual ("", holder.TestKey1OnHolder, "#7");
		}

		class Bug8533ConfHolder1 : ApplicationSettingsBase {
			[UserScopedSetting]
			public string TestKey1OnHolder1 {
				get { return (string) this ["TestKey1OnHolder1"] ?? ""; }
				set { this ["TestKey1OnHolder1"] = value; }
			}

			[UserScopedSetting]
			public string TestKey1OnHolder2 {
				get { return (string) this ["TestKey1OnHolder2"] ?? ""; }
				set { this ["TestKey1OnHolder2"] = value; }
			}
			
			[UserScopedSetting]
			public string TestKey {
				get { return (string) this ["TestKey"] ?? ""; }
				set { this ["TestKey"] = value; }
			}
		}

		class Bug8533ConfHolder2 : ApplicationSettingsBase {
			[UserScopedSetting]
			public string TestKey1OnHolder2 {
				get { return (string) this ["TestKey1OnHolder2"] ?? ""; }
				set { this ["TestKey1OnHolder2"] = value; }
			}

			[UserScopedSetting]
			public string TestKey {
				get { return (string) this ["TestKey"] ?? ""; }
				set { this ["TestKey"] = value; }
			}
		}

		[Test]
		public void TestBug8533ConfHandlerWronglyMixedUp ()
		{
			var holder1 = new Bug8533ConfHolder1 ();
			holder1.TestKey1OnHolder1 = "candy";
			holder1.TestKey = "eclair";
			Assert.AreEqual ("", holder1.TestKey1OnHolder2, "#-1");
			holder1.Save ();
			Assert.AreEqual ("", holder1.TestKey1OnHolder2, "#0");
			holder1.Reload ();
			
			var holder2 = new Bug8533ConfHolder2 ();
			holder2.TestKey1OnHolder2 = "donut";
			Assert.AreEqual ("", holder1.TestKey1OnHolder2, "#1");
			holder2.Save ();
			holder2.Reload();
			Assert.AreEqual ("candy", holder1.TestKey1OnHolder1, "#2");
			Assert.AreEqual ("donut", holder2.TestKey1OnHolder2, "#3");
			Assert.AreEqual ("eclair", holder1.TestKey, "#4");
			Assert.AreEqual ("", holder2.TestKey, "#5");
		}

		class Settings : ApplicationSettingsBase
		{
			[UserScopedSetting]
			public WindowPositionList WindowPositions {
				get {
					return ((WindowPositionList)(this ["WindowPositions"]));
				}
				set {
					this ["WindowPositions"] = value;
				}
			}
		}

		[Serializable]
		public class WindowPositionList : IXmlSerializable
		{
			public XmlSchema GetSchema ()
			{
				return null;
			}

			public void ReadXml (XmlReader reader)
			{
				reader.ReadStartElement ("sampleNode");
				reader.ReadEndElement ();
			}

			public void WriteXml (XmlWriter writer)
			{
				writer.WriteStartElement ("sampleNode");
				writer.WriteEndElement ();
			}
		}

		[Test] //Covers 36388
		public void XmlHeader ()
		{
			try {
				var settings = new Settings ();
				settings.Reset ();
				settings.Save ();

				settings.WindowPositions = new WindowPositionList ();

				settings.Save ();
				// If Reloads fails then saved data is corrupted
				settings.Reload ();
			} catch (ConfigurationErrorsException e) {
				// Delete corrupted config file so other test won't fail.
				File.Delete (e.Filename);
				Assert.Fail ("Invalid data was saved to config file.");
			}
		}
		#region Bug #2315
		class Bug2315Settings : ApplicationSettingsBase
		{
			public Bug2315Settings () : base ("Bug2315Settings")
			{
			}

			[UserScopedSetting]
			[DefaultSettingValue ("some text")]
			public string Text {
				get { return (string)this ["Text"]; }
				set { this ["Text"] = value; }
			}
		}

		[Test]
		public void SettingSavingEventFired_Bug2315 ()
		{
			bool settingsSavingCalled = false;
			var settings = new Bug2315Settings ();
			settings.SettingsSaving += (object sender, CancelEventArgs e) => {
				settingsSavingCalled = true;
			};

			settings.Text = DateTime.Now.ToString ();
			settings.Save ();

			Assert.IsTrue (settingsSavingCalled);
		}
		#endregion

		#region Bug #15818
		class Bug15818SettingsProvider: SettingsProvider, IApplicationSettingsProvider
		{
			public Bug15818SettingsProvider ()
			{
			}

			public static void ResetUpgradeCalled ()
			{
				UpgradeCalled = false;
			}

			public static bool UpgradeCalled { get; private set; }

			public override void Initialize (string name, NameValueCollection config)
			{
				if (name != null && config != null) {
					base.Initialize (name, config);
				}
			}

			public override string Name
			{
				get { return "Bug15818SettingsProvider"; }
			}

			public override string Description
			{
				get { return "Bug15818SettingsProvider"; }
			}

			public override string ApplicationName
			{
				get { return "Bug15818"; }
				set { }
			}

			public override SettingsPropertyValueCollection GetPropertyValues (SettingsContext context, SettingsPropertyCollection collection)
			{
				return null;
			}

			public override void SetPropertyValues (SettingsContext context, SettingsPropertyValueCollection collection)
			{
			}

			#region IApplicationSettingsProvider implementation

			public SettingsPropertyValue GetPreviousVersion (SettingsContext context, SettingsProperty property)
			{
				return null;
			}

			public void Reset (SettingsContext context)
			{
			}

			public void Upgrade (SettingsContext context, SettingsPropertyCollection properties)
			{
				UpgradeCalled = true;
			}

			#endregion
		}

		class Bug15818Settings : ApplicationSettingsBase
		{
			public Bug15818Settings () : base ("Bug15818Settings")
			{
			}

			[UserScopedSetting]
			[SettingsProvider (typeof (Bug15818SettingsProvider))]
			[DefaultSettingValue ("some text")]
			public string Text {
				get { return (string)this ["Text"]; }
				set { this ["Text"] = value; }
			}
		}

		public class Bug15818Class
		{
			public string Name { get; set; }
			public int Value { get; set; }
		}

		class Bug15818Settings2 : ApplicationSettingsBase
		{
			public Bug15818Settings2 () : base ("Bug15818Settings2")
			{
			}

			[UserScopedSetting]
			[DefaultSettingValue ("default text")]
			public string Text {
				get { return (string)this ["Text"]; }
				set { this ["Text"] = value; }
			}

			[UserScopedSetting]
			public Bug15818Class MyObject {
				get { return (Bug15818Class)this ["MyObject"]; }
				set { this ["MyObject"] = value; }
			}
		}

		[Test]
		public void UpgradeGetsCalled_Bug15818 ()
		{
			Bug15818SettingsProvider.ResetUpgradeCalled ();

			var settings = new Bug15818Settings ();
			settings.Upgrade ();
			Assert.IsTrue (Bug15818SettingsProvider.UpgradeCalled);
		}

		[Test]
		public void CustomClass_Roundtrip ()
		{
			var settings = new Bug15818Settings2
			{
				Text = "foo",
				MyObject = new Bug15818Class { Name = "Some Name", Value = 15818 }
			};
			settings.Save ();

			var settings2 = new Bug15818Settings2 ();
			Assert.AreEqual ("foo", settings2.Text);
			Assert.IsNotNull (settings2.MyObject);
			Assert.AreEqual ("Some Name", settings2.MyObject.Name);
			Assert.AreEqual (15818, settings2.MyObject.Value);
		}

		[Test]
		public void ModifiedObjectsAreSerialized_Bug15818 ()
		{
			var settings = new Bug15818Settings2
			{
				Text = "foo",
				MyObject = new Bug15818Class { Name = "Some Name", Value = 15818 }
			};
			settings.Save ();

			// Modify the value of the object - bug #15818
			settings.Text = "bla";
			settings.MyObject.Name = "xyz";
			settings.MyObject.Value = -1;
			settings.Save ();

			// Verify that the new values got saved
			var settings2 = new Bug15818Settings2 ();
			Assert.AreEqual ("bla", settings2.Text);
			Assert.IsNotNull (settings2.MyObject);
			Assert.AreEqual ("xyz", settings2.MyObject.Name);
			Assert.AreEqual (-1, settings2.MyObject.Value);
		}

		[Test]
		public void Reset_FiresPropChangedOnly_Bug15818 ()
		{
			bool propChangedCalled = false;
			bool settingsLoadedCalled = false;
			bool settingsSavingCalled = false;
			var settings = new Bug15818Settings2 ();
			settings.PropertyChanged += (sender, e) => { propChangedCalled = true; };
			settings.SettingsLoaded += (sender, e) => { settingsLoadedCalled = true; };
			settings.SettingsSaving += (sender, e) => { settingsSavingCalled = true; };

			settings.Reset ();

			Assert.IsTrue (propChangedCalled, "#1");
			Assert.IsFalse (settingsLoadedCalled, "#2");
			Assert.IsFalse (settingsSavingCalled, "#3");
		}
		#endregion
	}
}

