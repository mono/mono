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
				return Path.DirectorySeparatorChar == '\\';
			}
		}
		private bool RunningOnMono {
			get {
				return (Type.GetType ("System.MonoType", false) != null);
			}
		}

		[Test]
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
		public void ApplicationBase1 ()
		{
			string expected_path = tmpPath;
			AppDomainSetup setup = new AppDomainSetup ();
			string fileUri = "file://" + tmpPath.Replace(@"\", @"/");
			setup.ApplicationBase = fileUri;
			try {
				// under .NET the NotSupportedException is throw when getting 
				// (and not setting) the ApplicationBase property
				Assert.AreEqual (expected_path, setup.ApplicationBase);
			}
			catch (NotSupportedException) {
				// however the path is invalid only on .NET
				if (RunningOnMono)
					throw;
			}
		}

		[Test]
		public void ApplicationBase2 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = curDir;
			Assert.AreEqual (curDir, setup.ApplicationBase);
		}

		[Test]
		public void ApplicationBase3 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			string expected = Path.Combine (Environment.CurrentDirectory, "lalala");
			setup.ApplicationBase = "lalala";
			Assert.AreEqual (expected, setup.ApplicationBase);
		}

		[Test]
		public void ApplicationBase4 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "lala:la";
			if (!RunningOnWindows) {
				Assert.AreEqual (Path.GetFullPath ("lala:la"), setup.ApplicationBase);
			} else {
				// On Windows we expect a NotSupportedException to be thrown because
				// of the illegal character (:) in the path
				try {
					Assert.Fail ("NotSupportedException expected but setup.ApplicationBase returned:" + setup.ApplicationBase);
				}
				catch (NotSupportedException) {
					// Expected
				}
			}
		}

		[Test]
		public void ApplicationBase5 ()
		{
			// This is failing because of (probably) a windows-ism, so don't worry
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "file:///lala:la";
			string expected = "/lala:la";
			if (!RunningOnWindows) {
				Assert.AreEqual (expected, setup.ApplicationBase);
			} else {
				// On Windows we expect a NotSupportedException to be thrown because
				// of the illegal character (:) in the path
				try {
					Assert.Fail ("NotSupportedException expected but setup.ApplicationBase returned:" + setup.ApplicationBase);
				}
				catch (NotSupportedException) {
					// Expected
				}
			}
		}

		[Test]
		public void ApplicationBase6 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "la?lala";
			// paths containing "?" are *always* bad on Windows
			// but are legal for linux so we return a full path
			if (!RunningOnWindows) {
				Assert.AreEqual (Path.GetFullPath ("la?lala"), setup.ApplicationBase);
			} else {
				// On Windows we expect a ArgumentException to be thrown because
				// of the illegal character (?) in the path
				try {
					Assert.Fail ("ArgumentException expected but setup.ApplicationBase returned:" + setup.ApplicationBase);
				}
				catch (ArgumentException) {
					// Expected
				}
			}
		}

		[Test]
		public void ApplicationBase7 ()
		{
			if (RunningOnWindows) {
				// Extended paths are Windows only
				AppDomainSetup setup = new AppDomainSetup ();
				string expected = @"\\?\" + curDir;
				setup.ApplicationBase = expected;
				Assert.AreEqual (expected, setup.ApplicationBase);
			}
		}

		[Test]
		public void ApplicationBase8 ()
		{
			if (RunningOnWindows) {
				// Extended paths are Windows only
				AppDomainSetup setup = new AppDomainSetup ();
				setup.ApplicationBase = @"\\?\C:\lala:la";
				try {
					Assert.Fail ("NotSupportedException expected but setup.ApplicationBase returned:" + setup.ApplicationBase);
				}
				catch (NotSupportedException) {
					// Expected
				}
			}
		}

		[Test]
		public void ApplicationBase9 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			string url = "file://";
			if (RunningOnWindows)
			{
				url += "/" + Environment.CurrentDirectory;
				setup.ApplicationBase = url;
				Assert.AreEqual (Environment.CurrentDirectory, setup.ApplicationBase);
			}
			else
			{
				url += "/home";
				setup.ApplicationBase = url;
				Assert.AreEqual ("/home", setup.ApplicationBase);
			}
		}

#if MONO_FEATURE_MULTIPLE_APPDOMAINS
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
#endif // MONO_FEATURE_MULTIPLE_APPDOMAINS

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

#if MONO_FEATURE_MULTIPLE_APPDOMAINS
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
#endif // MONO_FEATURE_MULTIPLE_APPDOMAINS
	}
}
