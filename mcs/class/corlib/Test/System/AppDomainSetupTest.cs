//
// AppDomainSetupTest.cs - NUnit Test Cases for the System.AppDomainSetup class
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
// 

using NUnit.Framework;
using System;
using System.IO;

namespace MonoTests.System
{
	[TestFixture]
	public class AppDomainSetupTest {

		static readonly string tmpPath = Path.GetTempPath ();
		static readonly string curDir = Directory.GetCurrentDirectory ();

		private bool RunningOnWindows {
			get {
				int os = (int)Environment.OSVersion.Platform;
				return (os != 4);
			}
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void ConfigurationFile_Relative_ApplicationBase ()
		{
			string fileName = "blar.config";
			AppDomainSetup setup = new AppDomainSetup();
			string dir = "app_base";
			setup.ApplicationBase = dir;
			setup.ConfigurationFile = fileName;
			string baseDir = Path.GetFullPath(dir);
			string configFile = Path.Combine(baseDir, fileName);
			Assert.AreEqual(configFile, setup.ConfigurationFile, "Check relative to ApplicationBase");
		}

		[Test]
		public void ConfigurationFile_Null ()
		{
			AppDomainSetup setup = new AppDomainSetup();
			Assert.IsNull(setup.ConfigurationFile);
		}

		[Test]
		[ExpectedException (typeof (MemberAccessException))] // The ApplicationBase must be set before retrieving this property
		[Category ("TargetJvmNotWorking")]
		public void ConfigurationFile_Relative_NoApplicationBase ()
		{
			AppDomainSetup setup = new AppDomainSetup();
			setup.ConfigurationFile = "blar.config";
			string configFile = setup.ConfigurationFile;
			if (configFile == null) {
				// avoid compiler warning
			}
		}

		[Test]
		public void ConfigurationFile_Absolute_NoApplicationBase ()
		{
			AppDomainSetup setup = new AppDomainSetup();
			string configFile = Path.GetFullPath("blar.config");
			setup.ConfigurationFile = configFile;
			Assert.AreEqual(configFile, setup.ConfigurationFile);
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void ApplicationBase1 ()
		{
			string expected_path = tmpPath.Replace(@"\", @"/");
			AppDomainSetup setup = new AppDomainSetup ();
			string fileUri = "file://" + expected_path;
			setup.ApplicationBase = fileUri;
			// with MS 1.1 SP1 the expected_path starts with "//" but this make
			// sense only under Windows (i.e. reversed \\ for local files)
			if (RunningOnWindows)
				expected_path = "//" + expected_path;
			try {
				// under 2.0 the NotSupportedException is throw when getting 
				// (and not setting) the ApplicationBase property
				Assert.AreEqual (expected_path, setup.ApplicationBase);
			}
			catch (NotSupportedException) {
				// however the path is invalid only on Windows
				if (!RunningOnWindows)
					throw;
			}
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void ApplicationBase2 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = curDir;
			Assert.AreEqual (curDir, setup.ApplicationBase);
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void ApplicationBase3 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			string expected = Path.Combine (Environment.CurrentDirectory, "lalala");
			setup.ApplicationBase = "lalala";
			Assert.AreEqual (expected, setup.ApplicationBase);
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void ApplicationBase4 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "lala:la";
			try {
				// under 2.0 the NotSupportedException is throw when getting 
				// (and not setting) the ApplicationBase property
				Assert.AreEqual (Path.GetFullPath ("lala:la"), setup.ApplicationBase);
			}
			catch (NotSupportedException) {
				// however the path is invalid only on Windows
				// (same exceptions as Path.GetFullPath)
				if (!RunningOnWindows)
					throw;
			}
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void ApplicationBase5 ()
		{
			// This is failing because of (probably) a windows-ism, so don't worry
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "file:///lala:la";
			try {
				// under 2.0 the NotSupportedException is throw when getting 
				// (and not setting) the ApplicationBase property
				Assert.AreEqual ("/lala:la", setup.ApplicationBase);
			}
			catch (NotSupportedException) {
				// however the path is invalid only on Windows
				// (same exceptions as Path.GetFullPath)
				if (!RunningOnWindows)
					throw;
			}
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void ApplicationBase6 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "la?lala";
			// paths containing "?" are *always* bad on Windows
			// but are legal for linux so we return a full path
			if (RunningOnWindows) {
				try {
					// ArgumentException is throw when getting 
					// (and not setting) the ApplicationBase property
					Assert.Fail ("setup.ApplicationBase returned :" + setup.ApplicationBase);
				}
				catch (ArgumentException) {
				}
				catch (Exception e) {
					Assert.Fail ("Unexpected exception: " + e.ToString ());
				}
			} else {
				Assert.AreEqual (Path.GetFullPath ("la?lala"), setup.ApplicationBase);
			}
		}

		[Test]
#if MOBILE
		[Category ("NotWorking")]
#endif
		public void AppDomainInitializer1 ()
		{
			AppDomainSetup s = new AppDomainSetup ();
			s.AppDomainInitializer = AppDomainInitialized1;
			s.AppDomainInitializerArguments = new string [] {"A", "B"};
			AppDomain domain = AppDomain.CreateDomain ("MyDomain", null, s);

			object data = domain.GetData ("Initialized");
			Assert.IsNotNull (data);
			Assert.IsTrue ((bool) data);
		}

		static void AppDomainInitialized1 (string [] args)
		{
			bool initialized = true;
			initialized &= args [0] == "A";
			initialized &= args [1] == "B";
			initialized &= AppDomain.CurrentDomain.FriendlyName == "MyDomain";
			
			AppDomain.CurrentDomain.SetData ("Initialized", initialized);
		}

		public void InstanceInitializer (string [] args)
		{
		}

		[Test]
#if MOBILE
		[Category ("NotWorking")]
#else
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void AppDomainInitializerNonStaticMethod ()
		{
			AppDomainSetup s = new AppDomainSetup ();
			s.AppDomainInitializer = InstanceInitializer;
			AppDomain.CreateDomain ("MyDomain", null, s);
		}
	}
}
