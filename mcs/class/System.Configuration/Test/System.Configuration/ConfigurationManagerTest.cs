//
// System.Configuration.ConfigurationManagerTest.cs - Unit tests
// for System.Configuration.ConfigurationManager.
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Configuration;
using System.IO;
using NUnit.Framework;
using SysConfig = System.Configuration.Configuration;
using System.Runtime.InteropServices;

namespace MonoTests.System.Configuration {
	[TestFixture]
	public class ConfigurationManagerTest
	{
		private string originalCurrentDir;
		private string tempFolder;

		[SetUp]
		public void SetUp ()
		{
			originalCurrentDir = Directory.GetCurrentDirectory ();
			tempFolder = Path.Combine (Path.GetTempPath (), this.GetType ().FullName);
			if (!Directory.Exists (tempFolder))
				Directory.CreateDirectory (tempFolder);
		}

		[TearDown]
		public void TearDown ()
		{
			Directory.SetCurrentDirectory (originalCurrentDir);
			if (Directory.Exists (tempFolder))
				Directory.Delete (tempFolder, true);
		}

		[Test] // OpenExeConfiguration (ConfigurationUserLevel)
		[Category ("NotWorking")] // bug #323622
		public void OpenExeConfiguration1_Remote ()
		{
			AppDomain domain = null;
			string config_file;
			string config_xml = @"
				<configuration>
					<appSettings>
						<add key='anyKey' value='42' />
					</appSettings>
				</configuration>";

			config_file = Path.Combine (tempFolder, "otherConfig.noconfigext");
			File.WriteAllText (config_file, config_xml);

			try {
				AppDomainSetup setup = new AppDomainSetup();
				setup.ConfigurationFile = config_file;
				setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
				domain = AppDomain.CreateDomain("foo", null, setup);

				RemoteConfig config = RemoteConfig.GetInstance (domain);

				ConfigurationUserLevel userLevel = ConfigurationUserLevel.None;
				Assert.AreEqual (config_file, config.GetFilePath (userLevel));
				Assert.AreEqual ("42", config.GetSettingValue (userLevel, "anyKey"));
				Assert.AreEqual ("42", config.GetSettingValue ("anyKey"));
			} finally {
				if (domain != null)
					AppDomain.Unload (domain);
				File.Delete (config_file);
			}

			config_file = Path.Combine (tempFolder, "otherConfig.exe.config");
			File.WriteAllText (config_file, config_xml);

			try {
				AppDomainSetup setup = new AppDomainSetup();
				setup.ConfigurationFile = config_file;
				setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
				domain = AppDomain.CreateDomain("foo", null, setup);

				RemoteConfig config = RemoteConfig.GetInstance (domain);

				ConfigurationUserLevel userLevel = ConfigurationUserLevel.None;
				Assert.AreEqual (config_file, config.GetFilePath (userLevel));
				Assert.AreEqual ("42", config.GetSettingValue (userLevel, "anyKey"));
				Assert.AreEqual ("42", config.GetSettingValue ("anyKey"));
			} finally {
				if (domain != null)
					AppDomain.Unload (domain);
				File.Delete (config_file);
			}

			try {
				AppDomainSetup setup = new AppDomainSetup();
				setup.ConfigurationFile = config_file;
				setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
				domain = AppDomain.CreateDomain("foo", null, setup);

				RemoteConfig config = RemoteConfig.GetInstance (domain);

				ConfigurationUserLevel userLevel = ConfigurationUserLevel.None;
				Assert.AreEqual (config_file, config.GetFilePath (userLevel));
				Assert.IsNull (config.GetSettingValue (userLevel, "anyKey"));
				Assert.IsNull (config.GetSettingValue ("anyKey"));
			} finally {
				if (domain != null)
					AppDomain.Unload (domain);
				File.Delete (config_file);
			}
		}

		[Test] // OpenExeConfiguration (ConfigurationUserLevel)
		public void OpenExeConfiguration1_UserLevel_None ()
		{
			SysConfig config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			Console.WriteLine("application config path: {0}", config.FilePath);
			FileInfo fi = new FileInfo (config.FilePath);
#if TARGET_JVM
			Assert.AreEqual ("nunit-console.jar.config", fi.Name);
#elif NET_4_0
			Assert.AreEqual ("System.Configuration_test_net_4_0.dll.config", fi.Name);
#else
			Assert.AreEqual ("System.Configuration_test_net_2_0.dll.config", fi.Name);
#endif
		}

		[Test]
		public void OpenExeConfiguration1_UserLevel_PerUserRoaming ()
		{
			SysConfig config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);
			Console.WriteLine("roaming user config path: {0}", config.FilePath);

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("user.config", fi.Name);
		}

		[Test]
		public void OpenExeConfiguration1_UserLevel_PerUserRoamingAndLocal ()
		{
			SysConfig config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
			Console.WriteLine("local user config path: {0}", config.FilePath);

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("user.config", fi.Name);
		}

		[Test] // OpenExeConfiguration (String)
		public void OpenExeConfiguration2 ()
		{
			String exePath;
			SysConfig config;

			exePath = Path.Combine (tempFolder, "DoesNotExist.whatever");
			File.Create (exePath).Close ();

			config = ConfigurationManager.OpenExeConfiguration (exePath);
			Assert.AreEqual (exePath + ".config", config.FilePath, "#1");

			exePath = Path.Combine (tempFolder, "SomeExecutable.exe");
			File.Create (exePath).Close ();

			config = ConfigurationManager.OpenExeConfiguration (exePath);
			Assert.AreEqual (exePath + ".config", config.FilePath, "#2");

			exePath = Path.Combine (tempFolder, "Foo.exe.config");
			File.Create (exePath).Close ();

			config = ConfigurationManager.OpenExeConfiguration (exePath);
			Assert.AreEqual (exePath + ".config", config.FilePath, "#3");

			Directory.SetCurrentDirectory (tempFolder);

			exePath = "relative.exe";
			File.Create (Path.Combine (tempFolder, exePath)).Close ();

			config = ConfigurationManager.OpenExeConfiguration (exePath);
			Assert.AreEqual (Path.Combine (tempFolder, exePath + ".config"), config.FilePath, "#4");
		}

		[Test] // OpenExeConfiguration (String)
		public void OpenExeConfiguration2_ExePath_Empty ()
		{
			AppDomain domain = AppDomain.CurrentDomain;

			SysConfig config = ConfigurationManager.OpenExeConfiguration (string.Empty);
			Assert.AreEqual (domain.SetupInformation.ConfigurationFile, config.FilePath);
		}

		[Test] // OpenExeConfiguration (String)
		public void OpenExeConfiguration2_ExePath_Null ()
		{
			AppDomain domain = AppDomain.CurrentDomain;

			SysConfig config = ConfigurationManager.OpenExeConfiguration (string.Empty);
			Assert.AreEqual (domain.SetupInformation.ConfigurationFile, config.FilePath);
		}

		[Test] // OpenExeConfiguration (String)
		public void OpenExeConfiguration2_ExePath_DoesNotExist ()
		{
			String exePath = Path.Combine (tempFolder, "DoesNotExist.exe");

			try {
				ConfigurationManager.OpenExeConfiguration (exePath);
				Assert.Fail ("#1");
			} catch (ConfigurationErrorsException ex) {
				// An error occurred loading a configuration file:
				// The parameter 'exePath' is invalid
				Assert.AreEqual (typeof (ConfigurationErrorsException), ex.GetType (), "#2");
				Assert.IsNull (ex.Filename, "#3");
				Assert.IsNotNull (ex.InnerException, "#4");
				Assert.AreEqual (0, ex.Line, "#5");
				Assert.IsNotNull (ex.Message, "#6");

				// The parameter 'exePath' is invalid
				ArgumentException inner = ex.InnerException as ArgumentException;
				Assert.IsNotNull (inner, "#7");
				Assert.AreEqual (typeof (ArgumentException), inner.GetType (), "#8");
				Assert.IsNull (inner.InnerException, "#9");
				Assert.IsNotNull (inner.Message, "#10");
				Assert.AreEqual ("exePath", inner.ParamName, "#11");
			}
		}

		[Test]
		public void exePath_UserLevelNone ()
		{
			string basedir = AppDomain.CurrentDomain.BaseDirectory;
			SysConfig config = ConfigurationManager.OpenExeConfiguration("System.Configuration_test_net_2_0.dll.mdb");
			Assert.AreEqual (Path.Combine (basedir, "System.Configuration_test_net_2_0.dll.mdb.config"), config.FilePath);
		}

		[Test]
		public void exePath_UserLevelPerRoaming ()
		{
			SysConfig config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);
			string filePath = config.FilePath;
			string applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			Assert.IsTrue (filePath.StartsWith (applicationData), "#1:" + filePath);
			Assert.AreEqual ("user.config", Path.GetFileName (filePath), "#2:" + filePath);
		}

		[Test]
		public void exePath_UserLevelPerRoamingAndLocal ()
		{
			SysConfig config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
			string filePath = config.FilePath;
			string applicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			Assert.IsTrue (filePath.StartsWith (applicationData), "#1:" + filePath);
			Assert.AreEqual ("user.config", Path.GetFileName (filePath), "#2:" + filePath);
		}

		[Test]
		public void mapped_UserLevelNone ()
		{
			ExeConfigurationFileMap map = new ExeConfigurationFileMap ();
			map.ExeConfigFilename = "execonfig";

			SysConfig config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
			Console.WriteLine("mapped application config path: {0}", config.FilePath);	

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("execonfig", fi.Name);

		}

		[Test]
		public void mapped_UserLevelPerRoaming ()
		{
			ExeConfigurationFileMap map = new ExeConfigurationFileMap ();
			map.ExeConfigFilename = "execonfig";
			map.RoamingUserConfigFilename = "roaminguser";

			SysConfig config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.PerUserRoaming);
			Console.WriteLine("mapped roaming user config path: {0}", config.FilePath);	

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("roaminguser", fi.Name);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotWorking")]
		public void mapped_UserLevelPerRoaming_no_execonfig ()
		{
			ExeConfigurationFileMap map = new ExeConfigurationFileMap ();
			map.RoamingUserConfigFilename = "roaminguser";

			ConfigurationManager.OpenMappedExeConfiguration (map, ConfigurationUserLevel.PerUserRoaming);
		}

		[Test]
		public void mapped_UserLevelPerRoamingAndLocal ()
		{
			ExeConfigurationFileMap map = new ExeConfigurationFileMap ();
			map.ExeConfigFilename = "execonfig";
			map.RoamingUserConfigFilename = "roaminguser";
			map.LocalUserConfigFilename = "localuser";

			SysConfig config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.PerUserRoamingAndLocal);
			Console.WriteLine("mapped local user config path: {0}", config.FilePath);	

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("localuser", fi.Name);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotWorking")]
		public void mapped_UserLevelPerRoamingAndLocal_no_execonfig ()
		{
			ExeConfigurationFileMap map = new ExeConfigurationFileMap ();
			map.RoamingUserConfigFilename = "roaminguser";
			map.LocalUserConfigFilename = "localuser";

			ConfigurationManager.OpenMappedExeConfiguration (map, ConfigurationUserLevel.PerUserRoamingAndLocal);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotWorking")]
		public void mapped_UserLevelPerRoamingAndLocal_no_roaminguser ()
		{
			ExeConfigurationFileMap map = new ExeConfigurationFileMap ();
			map.ExeConfigFilename = "execonfig";
			map.LocalUserConfigFilename = "localuser";

			ConfigurationManager.OpenMappedExeConfiguration (map, ConfigurationUserLevel.PerUserRoamingAndLocal);
		}

		[Test]
		public void MachineConfig ()
		{
			SysConfig config = ConfigurationManager.OpenMachineConfiguration ();
			Console.WriteLine("machine config path: {0}", config.FilePath);

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("machine.config", fi.Name);
		}

		[Test]
		public void mapped_MachineConfig ()
		{
			ConfigurationFileMap map = new ConfigurationFileMap ();
			map.MachineConfigFilename = "machineconfig";

			SysConfig config = ConfigurationManager.OpenMappedMachineConfiguration (map);
			Console.WriteLine("mapped machine config path: {0}", config.FilePath);

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("machineconfig", fi.Name);
		}

		[Test]
		public void exePath_UserLevelNone_null ()
		{
			SysConfig config = ConfigurationManager.OpenExeConfiguration (null);
#if false
			Console.WriteLine("null exe application config path: {0}", config.FilePath);	

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("System.Configuration_test_net_2_0.dll.config", fi.Name);
#endif
		}

		[Test]
		[Category ("NotWorking")]
		public void mapped_ExeConfiguration_null ()
		{
			SysConfig config = ConfigurationManager.OpenMappedExeConfiguration(null, ConfigurationUserLevel.None);
			Console.WriteLine("null mapped application config path: {0}", config.FilePath);	

			FileInfo fi = new FileInfo (config.FilePath);
#if TARGET_JVM
			Assert.AreEqual("System.Configuration.Test20.jar.config", fi.Name);
#else
			Assert.AreEqual ("System.Configuration_test_net_2_0.dll.config", fi.Name);
#endif
		}

		[Test]
		[Category ("NotWorking")]
		public void mapped_MachineConfig_null ()
		{
			SysConfig config = ConfigurationManager.OpenMappedMachineConfiguration (null);
			Console.WriteLine("null mapped machine config path: {0}", config.FilePath);

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("machine.config", fi.Name);
		}

		[Test]
		public void GetSectionReturnsNativeObject ()
		{
			Assert.IsTrue (ConfigurationManager.GetSection ("appSettings") is NameValueCollection);
		}

		[Test] // test for bug #78372.
		public void OpenMachineConfiguration ()
		{
			SysConfig cfg = ConfigurationManager.OpenMachineConfiguration ();
			Assert.IsTrue (cfg.Sections.Count > 0, "#1");
#if !TARGET_JVM
			ConfigurationSection s = cfg.SectionGroups ["system.net"].Sections ["connectionManagement"];
			Assert.IsNotNull (s, "#2");
			Assert.IsTrue (s is ConnectionManagementSection, "#3");
#endif
		}

		[Test]
		public void SectionCollectionEnumerator ()
		{
			SysConfig c = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			ConfigurationSectionCollection col =
				c.GetSectionGroup ("system.web").Sections;
			IEnumerator e = col.GetEnumerator ();
			e.MoveNext ();
			Assert.IsTrue (e.Current is ConfigurationSection);
		}

		class RemoteConfig : MarshalByRefObject
		{
			public static RemoteConfig GetInstance (AppDomain domain)
			{
				RemoteConfig config = (RemoteConfig) domain.CreateInstanceAndUnwrap (
					typeof (RemoteConfig).Assembly.FullName,
					typeof (RemoteConfig).FullName, new object [0]);
				return config;
			}

			public string GetFilePath (string exePath)
			{
				global::System.Configuration.Configuration config =
					ConfigurationManager.OpenExeConfiguration (exePath);
				return config.FilePath;
			}

			public string GetFilePath (ConfigurationUserLevel userLevel)
			{
				global::System.Configuration.Configuration config =
					ConfigurationManager.OpenExeConfiguration (userLevel);
				return config.FilePath;
			}

			public string GetSettingValue (string exePath, string key)
			{
				global::System.Configuration.Configuration config =
					ConfigurationManager.OpenExeConfiguration (exePath);
				return config.AppSettings.Settings [key].Value;
			}

			public string GetSettingValue (ConfigurationUserLevel userLevel, string key)
			{
				global::System.Configuration.Configuration config =
					ConfigurationManager.OpenExeConfiguration (userLevel);
				KeyValueConfigurationElement value = config.AppSettings.Settings [key];
				return value != null ? value.Value : null;
			}

			public string GetSettingValue (string key)
			{
				return ConfigurationManager.AppSettings [key];
			}
		}
	}
}
