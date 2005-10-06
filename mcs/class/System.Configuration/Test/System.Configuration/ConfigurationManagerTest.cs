//
// System.Configuration.ConfigurationManagerTest.cs - Unit tests
// for System.Configuration.ConfigurationManager.
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
using System.Configuration;
using System.IO;
using NUnit.Framework;
using SysConfig = System.Configuration.Configuration;
using System.Runtime.InteropServices;

namespace MonoTests.System.Configuration {
	[TestFixture]
	public class ConfigurationManagerTest
	{
		[Test]
		public void UserLevelNone ()
		{
			SysConfig config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			Console.WriteLine("application config path: {0}", config.FilePath);
			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("System.Configuration_test_net_2_0.dll.config", fi.Name);
		}

		[Test]
		public void UserLevelPerRoaming ()
		{
			SysConfig config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);
			Console.WriteLine("roaming user config path: {0}", config.FilePath);

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("user.config", fi.Name);
		}

		[Test]
		public void UserLevelPerRoamingAndLocal ()
		{
			SysConfig config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
			Console.WriteLine("local user config path: {0}", config.FilePath);

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("user.config", fi.Name);
		}

		[Test]
		public void exePath_UserLevelNone_absolute ()
		{
#if false
			string path = String.Format ("{0}hi{1}there.exe", Path.DirectorySeparatorChar, Path.DirectorySeparatorChar);
			SysConfig config = ConfigurationManager.OpenExeConfiguration(path);
			Assert.AreEqual ("", config.FilePath);
#endif
		}

		[Test]
		public void exePath_UserLevelNone ()
		{
#if false
			SysConfig config = ConfigurationManager.OpenExeConfiguration("System.Configuration_test_net_2_0.dll.mdb");
			Assert.AreEqual ("", config.FilePath);
#endif
		}

		[Test]
		public void exePath_UserLevelPerRoaming ()
		{
#if false
			SysConfig config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming, "System.Configuration_test_net_2_0.dll.mdb");
			Assert.AreEqual ("", config.FilePath);
#endif
		}

		[Test]
		public void exePath_UserLevelPerRoamingAndLocal ()
		{
#if false
			SysConfig config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal, "System.Configuration_test_net_2_0.dll.mdb");
			Assert.AreEqual ("", config.FilePath);
#endif
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
		public void mapped_UserLevelPerRoaming_no_execonfig ()
		{
			ExeConfigurationFileMap map = new ExeConfigurationFileMap ();
			map.RoamingUserConfigFilename = "roaminguser";

			SysConfig config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.PerUserRoaming);
			Console.WriteLine("mapped roaming user config path: {0}", config.FilePath);	

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("roaminguser", fi.Name);
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
		public void mapped_UserLevelPerRoamingAndLocal_no_execonfig ()
		{
			ExeConfigurationFileMap map = new ExeConfigurationFileMap ();
			map.RoamingUserConfigFilename = "roaminguser";
			map.LocalUserConfigFilename = "localuser";

			SysConfig config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.PerUserRoamingAndLocal);
			Console.WriteLine("mapped local user config path: {0}", config.FilePath);	

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("localuser", fi.Name);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void mapped_UserLevelPerRoamingAndLocal_no_roaminguser ()
		{
			ExeConfigurationFileMap map = new ExeConfigurationFileMap ();
			map.ExeConfigFilename = "execonfig";
			map.LocalUserConfigFilename = "localuser";

			SysConfig config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.PerUserRoamingAndLocal);
			Console.WriteLine("mapped local user config path: {0}", config.FilePath);	

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("localuser", fi.Name);
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
#if false
			SysConfig config = ConfigurationManager.OpenExeConfiguration(null);
			Console.WriteLine("null exe application config path: {0}", config.FilePath);	

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("System.Configuration_test_net_2_0.dll.config", fi.Name);
#endif
		}

		[Test]
		public void mapped_ExeConfiguration_null ()
		{
			SysConfig config = ConfigurationManager.OpenMappedExeConfiguration(null, ConfigurationUserLevel.None);
			Console.WriteLine("null mapped application config path: {0}", config.FilePath);	

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("System.Configuration_test_net_2_0.dll.config", fi.Name);
		}

		[Test]
		public void mapped_MachineConfig_null ()
		{
			SysConfig config = ConfigurationManager.OpenMappedMachineConfiguration (null);
			Console.WriteLine("null mapped machine config path: {0}", config.FilePath);

			FileInfo fi = new FileInfo (config.FilePath);
			Assert.AreEqual ("machine.config", fi.Name);
		}

	}
}

#endif
