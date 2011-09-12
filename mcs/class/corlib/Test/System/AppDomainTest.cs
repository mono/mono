//
// AppDomainTest.cs - NUnit Test Cases for AppDomain
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Principal;

namespace MonoTests.System
{
	[TestFixture]
	public class AppDomainTest
	{
		private AppDomain ad;
		private ArrayList files = new ArrayList ();
		private string tempDir;

		[SetUp]
		public void SetUp ()
		{
			tempDir = Path.Combine (Path.GetTempPath (), Environment.UserName);
			tempDir = Path.Combine (tempDir, "MonoTests.System.AppDomainTest");
			if (!Directory.Exists (tempDir)) {
				Directory.CreateDirectory (tempDir);
			}
		}

		[TearDown]
		public void TearDown ()
		{
			if (ad != null) {
				try {
					AppDomain.Unload (ad);
					ad = null;
				} catch { } // do not affect unit test results in TearDown
			}
			foreach (string fname in files) {
				File.Delete (fname);
			}
			files.Clear ();
		}

		[Test] // bug #80934
		public void ConfigurationFile_Relative ()
		{
			// Note:
			// We use Environment.GetCommandLineArgs () to get the location of
			// the entry assembly in the default domain (since the default domain
			// is not exposed by any API)
			// 
			// MS returns a lower-case path in Environment.GetCommandLineArgs ()
			// and hence we need to perform a case-insensitive comparison
			// if the Assert involves that path

			string configFile = "test.config";
			string appBase = null;
			string expectedConfigFile = null;
			string expectedAppBase = null;

			// do not set ApplicationBase
			appBase = Path.GetDirectoryName (Environment.GetCommandLineArgs () [0]);
			expectedAppBase = appBase [appBase.Length - 1] == Path.DirectorySeparatorChar ?
				appBase : appBase + Path.DirectorySeparatorChar;
			expectedConfigFile = Path.Combine (appBase, configFile);
			AppDomainSetup setup = new AppDomainSetup();
			setup.ConfigurationFile = configFile;
			ad = CreateTestDomain (setup, true);
			CrossDomainTester cdt = CreateCrossDomainTester (ad);
			if (RunningOnUnix) {
				Assert.AreEqual (expectedConfigFile, cdt.GetConfigurationFile (), "#A1");
				Assert.AreEqual (expectedAppBase, cdt.GetApplicationBase (), "#A2");
			} else {
				Assert.IsTrue (string.Compare (expectedConfigFile, cdt.GetConfigurationFile (), true) == 0, "#A1");
				Assert.IsTrue (string.Compare (expectedAppBase, cdt.GetApplicationBase (), true) == 0, "#A2");
			}
			AppDomain.Unload (ad);

			// set ApplicationBase
			appBase = Path.GetTempPath ();
			expectedAppBase = appBase [appBase.Length - 1] == Path.DirectorySeparatorChar ?
				appBase : appBase + Path.DirectorySeparatorChar;
			expectedConfigFile = Path.Combine (appBase, configFile);
			setup = new AppDomainSetup ();
			setup.ApplicationBase = appBase;
			setup.ConfigurationFile = configFile;
			ad = CreateTestDomain (setup, true);
			cdt = CreateCrossDomainTester (ad);
			Assert.AreEqual (expectedConfigFile, cdt.GetConfigurationFile (), "#B1");
			Assert.AreEqual (expectedAppBase, cdt.GetApplicationBase (), "#B2");
			AppDomain.Unload (ad);
		}

		[Test] // bug #80934
		public void ConfigurationFile_Absolute ()
		{
			// Note:
			// We use Environment.GetCommandLineArgs () to get the location of
			// the entry assembly in the default domain (since the default domain
			// is not exposed by any API)
			// 
			// MS returns a lower-case path in Environment.GetCommandLineArgs ()
			// and hence on Windows we need to perform a case-insensitive 
			// comparison if the Assert involves that path

			string configFile = Path.Combine (tempDir, "test.config");
			string appBase = null;
			string expectedAppBase = null;

			// do not set ApplicationBase
			appBase = Path.GetDirectoryName (Environment.GetCommandLineArgs () [0]);
			expectedAppBase = appBase [appBase.Length - 1] == Path.DirectorySeparatorChar ?
				appBase : appBase + Path.DirectorySeparatorChar;
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ConfigurationFile = configFile;
			ad = CreateTestDomain (setup, true);
			CrossDomainTester cdt = CreateCrossDomainTester (ad);
			Assert.AreEqual (configFile, cdt.GetConfigurationFile (), "#A1");
			if (RunningOnUnix) {
				Assert.AreEqual (expectedAppBase, cdt.GetApplicationBase (), "#A2");
			} else {
				Assert.IsTrue (string.Compare (expectedAppBase, cdt.GetApplicationBase (), true) == 0, "#A2");
			}
			AppDomain.Unload (ad);

			// set ApplicationBase
			appBase = Path.GetTempPath ();
			expectedAppBase = appBase [appBase.Length - 1] == Path.DirectorySeparatorChar ?
				appBase : appBase + Path.DirectorySeparatorChar;
			setup = new AppDomainSetup ();
			setup.ApplicationBase = appBase;
			setup.ConfigurationFile = configFile;
			ad = CreateTestDomain (setup, true);
			cdt = CreateCrossDomainTester (ad);
			Assert.AreEqual (configFile, cdt.GetConfigurationFile (), "#B1");
			Assert.AreEqual (expectedAppBase, cdt.GetApplicationBase (), "#B2");
			AppDomain.Unload (ad);
		}

		[Test] // bug #80934
		public void ConfigurationFile_Null ()
		{
			// Note:
			// We use Environment.GetCommandLineArgs () to get the location of
			// the entry assembly in the default domain (since the default domain
			// is not exposed by any API)
			// 
			// MS returns a lower-case path in Environment.GetCommandLineArgs ()
			// and hence we need to perform a case-insensitive comparison
			// if the Assert involves that path

			string appBase = null;
			string expectedAppBase = null;
			string expectedConfigFile = null;

			// do not set ApplicationBase
			appBase = Path.GetDirectoryName (Environment.GetCommandLineArgs () [0]);
			expectedAppBase = appBase [appBase.Length - 1] == Path.DirectorySeparatorChar ?
				appBase : appBase + Path.DirectorySeparatorChar;
			expectedConfigFile = Environment.GetCommandLineArgs () [0] + ".config";
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ConfigurationFile = null;
			ad = CreateTestDomain (setup, true);
			CrossDomainTester cdt = CreateCrossDomainTester (ad);
			if (RunningOnUnix) {
				Assert.AreEqual (expectedConfigFile, cdt.GetConfigurationFile (), "#A1");
				Assert.AreEqual (expectedAppBase, cdt.GetApplicationBase (), "#A2");
			} else {
				Assert.IsTrue (string.Compare (expectedConfigFile, cdt.GetConfigurationFile (), true) == 0, "#A1");
				Assert.IsTrue (string.Compare (expectedAppBase, cdt.GetApplicationBase (), true) == 0, "#A2");
			}
			AppDomain.Unload (ad);

			// set ApplicationBase
			appBase = Path.GetTempPath ();
			expectedAppBase = appBase [appBase.Length - 1] == Path.DirectorySeparatorChar ?
				appBase : appBase + Path.DirectorySeparatorChar;
			expectedConfigFile = Path.Combine (appBase, Path.GetFileName (Environment.GetCommandLineArgs () [0]) + ".config");
			setup = new AppDomainSetup ();
			setup.ApplicationBase = appBase;
			setup.ConfigurationFile = null;
			ad = CreateTestDomain (setup, true);
			cdt = CreateCrossDomainTester (ad);
			if (RunningOnUnix) {
				Assert.AreEqual (expectedConfigFile, cdt.GetConfigurationFile (), "#B1");
			} else {
				Assert.IsTrue (string.Compare (expectedConfigFile, cdt.GetConfigurationFile (), true) == 0, "#B1");
			}
			Assert.AreEqual (expectedAppBase, cdt.GetApplicationBase (), "#B2");
			AppDomain.Unload (ad);
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess)
		public void DefineDynamicAssembly1_Access_Invalid ()
		{
			AssemblyName name = new AssemblyName ();
			name.Name = "DefineDynamicAssembly1";

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name, AssemblyBuilderAccess.Run |
					(AssemblyBuilderAccess) 666);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal enum value: 667
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("667") != -1, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("access", ex.ParamName, "#7");
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess)
		public void DefineDynamicAssembly1_Name_InvalidChars ()
		{
			string [] invalid_char_names = new string [] {
				"\tAB",
				" AB",
				"\rAB",
				"A/B",
				":AB",
				"B:A",
				"B\\A",
				"BA\\"};

			AssemblyName name = new AssemblyName ();

			foreach (string invalid_name in invalid_char_names) {
				name.Name = invalid_name;
				try {
					AppDomain.CurrentDomain.DefineDynamicAssembly (
						name,
						AssemblyBuilderAccess.Run);
					Assert.Fail ("#1:" + invalid_name);
				} catch (ArgumentException ex) {
					// Assembly names may not begin with whitespace
					// or contain the characters '/', '\' or ':'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + invalid_name);
					Assert.IsNull (ex.InnerException, "#3:" + invalid_name);
					Assert.IsNotNull (ex.Message, "#4:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'/'") != -1, "#5:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'\\'") != -1, "#6:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("':'") != -1, "#7:" + invalid_name);
					Assert.IsNull (ex.ParamName, "#8:" + invalid_name);
				}
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess)
		public void DefineDynamicAssembly1_Name_Null ()
		{
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					(AssemblyName) null,
					AssemblyBuilderAccess.Run);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			AssemblyName name = new AssemblyName ();

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			name.Name = string.Empty;

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, Evidence)
		public void DefineDynamicAssembly2_Access_Invalid ()
		{
			AssemblyName name = new AssemblyName ();
			name.Name = "DefineDynamicAssembly2";

#if NET_2_0
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name, AssemblyBuilderAccess.Run |
					(AssemblyBuilderAccess) 666,
					AppDomain.CurrentDomain.Evidence);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal enum value: 667
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("667") != -1, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("access", ex.ParamName, "#7");
			}
#else
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				name, AssemblyBuilderAccess.Run |
				(AssemblyBuilderAccess) 666,
				AppDomain.CurrentDomain.Evidence);
			Assert.IsNotNull (ab, "#1");
#endif
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, Evidence)
		public void DefineDynamicAssembly2_Name_InvalidChars ()
		{
			string [] invalid_char_names = new string [] {
				"\tAB",
				" AB",
				"\rAB",
				"A/B",
				":AB",
				"B:A",
				"B\\A",
				"BA\\"};

			AssemblyName name = new AssemblyName ();

			foreach (string invalid_name in invalid_char_names) {
				name.Name = invalid_name;
				try {
					AppDomain.CurrentDomain.DefineDynamicAssembly (
						name,
						AssemblyBuilderAccess.Run,
						AppDomain.CurrentDomain.Evidence);
					Assert.Fail ("#1:" + invalid_name);
				} catch (ArgumentException ex) {
					// Assembly names may not begin with whitespace
					// or contain the characters '/', '\' or ':'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + invalid_name);
					Assert.IsNull (ex.InnerException, "#3:" + invalid_name);
					Assert.IsNotNull (ex.Message, "#4:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'/'") != -1, "#5:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'\\'") != -1, "#6:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("':'") != -1, "#7:" + invalid_name);
					Assert.IsNull (ex.ParamName, "#8:" + invalid_name);
				}
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, Evidence)
		public void DefineDynamicAssembly2_Name_Null ()
		{
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					(AssemblyName) null,
					AssemblyBuilderAccess.Run,
					AppDomain.CurrentDomain.Evidence);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			AssemblyName name = new AssemblyName ();

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					AppDomain.CurrentDomain.Evidence);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			name.Name = string.Empty;

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					AppDomain.CurrentDomain.Evidence);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String)
		public void DefineDynamicAssembly3_Access_Invalid ()
		{
			AssemblyName name = new AssemblyName ();
			name.Name = "DefineDynamicAssembly3";

#if NET_2_0
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name, AssemblyBuilderAccess.Run |
					(AssemblyBuilderAccess) 666,
					Path.GetTempPath ());
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal enum value: 667
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("667") != -1, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("access", ex.ParamName, "#7");
			}
#else
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				name, AssemblyBuilderAccess.Run |
				(AssemblyBuilderAccess) 666,
				Path.GetTempPath ());
			Assert.IsNotNull (ab, "#1");
#endif
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String)
		public void DefineDynamicAssembly3_Name_InvalidChars ()
		{
			string [] invalid_char_names = new string [] {
				"\tAB",
				" AB",
				"\rAB",
				"A/B",
				":AB",
				"B:A",
				"B\\A",
				"BA\\"};

			AssemblyName name = new AssemblyName ();

			foreach (string invalid_name in invalid_char_names) {
				name.Name = invalid_name;
				try {
					AppDomain.CurrentDomain.DefineDynamicAssembly (
						name,
						AssemblyBuilderAccess.Run,
						Path.GetTempPath ());
					Assert.Fail ("#1:" + invalid_name);
				} catch (ArgumentException ex) {
					// Assembly names may not begin with whitespace
					// or contain the characters '/', '\' or ':'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + invalid_name);
					Assert.IsNull (ex.InnerException, "#3:" + invalid_name);
					Assert.IsNotNull (ex.Message, "#4:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'/'") != -1, "#5:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'\\'") != -1, "#6:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("':'") != -1, "#7:" + invalid_name);
					Assert.IsNull (ex.ParamName, "#8:" + invalid_name);
				}
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String)
		public void DefineDynamicAssembly3_Name_Null ()
		{
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					(AssemblyName) null,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath ());
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			AssemblyName name = new AssemblyName ();

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath ());
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			name.Name = string.Empty;

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath ());
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, Evidence)
		public void DefineDynamicAssembly4_Access_Invalid ()
		{
			AssemblyName name = new AssemblyName ();
			name.Name = "DefineDynamicAssembly4";

#if NET_2_0
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name, AssemblyBuilderAccess.Run |
					(AssemblyBuilderAccess) 666,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal enum value: 667
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("667") != -1, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("access", ex.ParamName, "#7");
			}
#else
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				name, AssemblyBuilderAccess.Run |
				(AssemblyBuilderAccess) 666,
				Path.GetTempPath (),
				AppDomain.CurrentDomain.Evidence);
			Assert.IsNotNull (ab, "#1");
#endif
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, Evidence)
		public void DefineDynamicAssembly4_Name_InvalidChars ()
		{
			string [] invalid_char_names = new string [] {
				"\tAB",
				" AB",
				"\rAB",
				"A/B",
				":AB",
				"B:A",
				"B\\A",
				"BA\\"};

			AssemblyName name = new AssemblyName ();

			foreach (string invalid_name in invalid_char_names) {
				name.Name = invalid_name;
				try {
					AppDomain.CurrentDomain.DefineDynamicAssembly (
						name,
						AssemblyBuilderAccess.Run,
						Path.GetTempPath (),
						AppDomain.CurrentDomain.Evidence);
					Assert.Fail ("#1:" + invalid_name);
				} catch (ArgumentException ex) {
					// Assembly names may not begin with whitespace
					// or contain the characters '/', '\' or ':'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + invalid_name);
					Assert.IsNull (ex.InnerException, "#3:" + invalid_name);
					Assert.IsNotNull (ex.Message, "#4:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'/'") != -1, "#5:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'\\'") != -1, "#6:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("':'") != -1, "#7:" + invalid_name);
					Assert.IsNull (ex.ParamName, "#8:" + invalid_name);
				}
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, Evidence)
		public void DefineDynamicAssembly4_Name_Null ()
		{
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					(AssemblyName) null,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			AssemblyName name = new AssemblyName ();

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			name.Name = string.Empty;

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, PermissionSet, PermissionSet, PermissionSet)
		public void DefineDynamicAssembly5_Access_Invalid ()
		{
			AssemblyName name = new AssemblyName ();
			name.Name = "DefineDynamicAssembly5";

#if NET_2_0
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name, AssemblyBuilderAccess.Run |
					(AssemblyBuilderAccess) 666,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal enum value: 667
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("667") != -1, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("access", ex.ParamName, "#7");
			}
#else
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				name, AssemblyBuilderAccess.Run |
				(AssemblyBuilderAccess) 666,
				(PermissionSet) null,
				(PermissionSet) null,
				(PermissionSet) null);
			Assert.IsNotNull (ab, "#1");
#endif
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, PermissionSet, PermissionSet, PermissionSet)
		public void DefineDynamicAssembly5_Name_InvalidChars ()
		{
			string [] invalid_char_names = new string [] {
				"\tAB",
				" AB",
				"\rAB",
				"A/B",
				":AB",
				"B:A",
				"B\\A",
				"BA\\"};

			AssemblyName name = new AssemblyName ();

			foreach (string invalid_name in invalid_char_names) {
				name.Name = invalid_name;
				try {
					AppDomain.CurrentDomain.DefineDynamicAssembly (
						name,
						AssemblyBuilderAccess.Run,
						(PermissionSet) null,
						(PermissionSet) null,
						(PermissionSet) null);
					Assert.Fail ("#1:" + invalid_name);
				} catch (ArgumentException ex) {
					// Assembly names may not begin with whitespace
					// or contain the characters '/', '\' or ':'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + invalid_name);
					Assert.IsNull (ex.InnerException, "#3:" + invalid_name);
					Assert.IsNotNull (ex.Message, "#4:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'/'") != -1, "#5:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'\\'") != -1, "#6:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("':'") != -1, "#7:" + invalid_name);
					Assert.IsNull (ex.ParamName, "#8:" + invalid_name);
				}
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, PermissionSet, PermissionSet, PermissionSet)
		public void DefineDynamicAssembly5_Name_Null ()
		{
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					(AssemblyName) null,
					AssemblyBuilderAccess.Run,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			AssemblyName name = new AssemblyName ();

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			name.Name = string.Empty;

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, Evidence, PermissionSet, PermissionSet, PermissionSet)
		public void DefineDynamicAssembly6_Access_Invalid ()
		{
			AssemblyName name = new AssemblyName ();
			name.Name = "DefineDynamicAssembly6";

#if NET_2_0
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name, AssemblyBuilderAccess.Run |
					(AssemblyBuilderAccess) 666,
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal enum value: 667
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("667") != -1, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("access", ex.ParamName, "#7");
			}
#else
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				name, AssemblyBuilderAccess.Run |
				(AssemblyBuilderAccess) 666,
				AppDomain.CurrentDomain.Evidence,
				(PermissionSet) null,
				(PermissionSet) null,
				(PermissionSet) null);
			Assert.IsNotNull (ab, "#1");
#endif
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, Evidence, PermissionSet, PermissionSet, PermissionSet)
		public void DefineDynamicAssembly6_Name_InvalidChars ()
		{
			string [] invalid_char_names = new string [] {
				"\tAB",
				" AB",
				"\rAB",
				"A/B",
				":AB",
				"B:A",
				"B\\A",
				"BA\\"};

			AssemblyName name = new AssemblyName ();

			foreach (string invalid_name in invalid_char_names) {
				name.Name = invalid_name;
				try {
					AppDomain.CurrentDomain.DefineDynamicAssembly (
						name,
						AssemblyBuilderAccess.Run,
						AppDomain.CurrentDomain.Evidence,
						(PermissionSet) null,
						(PermissionSet) null,
						(PermissionSet) null);
					Assert.Fail ("#1:" + invalid_name);
				} catch (ArgumentException ex) {
					// Assembly names may not begin with whitespace
					// or contain the characters '/', '\' or ':'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + invalid_name);
					Assert.IsNull (ex.InnerException, "#3:" + invalid_name);
					Assert.IsNotNull (ex.Message, "#4:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'/'") != -1, "#5:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'\\'") != -1, "#6:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("':'") != -1, "#7:" + invalid_name);
					Assert.IsNull (ex.ParamName, "#8:" + invalid_name);
				}
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, Evidence, PermissionSet, PermissionSet, PermissionSet)
		public void DefineDynamicAssembly6_Name_Null ()
		{
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					(AssemblyName) null,
					AssemblyBuilderAccess.Run,
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			AssemblyName name = new AssemblyName ();

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			name.Name = string.Empty;

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, PermissionSet, PermissionSet, PermissionSet)
		public void DefineDynamicAssembly7_Access_Invalid ()
		{
			AssemblyName name = new AssemblyName ();
			name.Name = "DefineDynamicAssembly7";

#if NET_2_0
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name, AssemblyBuilderAccess.Run |
					(AssemblyBuilderAccess) 666,
					Path.GetTempPath (),
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal enum value: 667
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("667") != -1, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("access", ex.ParamName, "#7");
			}
#else
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				name, AssemblyBuilderAccess.Run |
				(AssemblyBuilderAccess) 666,
				Path.GetTempPath (),
				(PermissionSet) null,
				(PermissionSet) null,
				(PermissionSet) null);
			Assert.IsNotNull (ab, "#1");
#endif
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, PermissionSet, PermissionSet, PermissionSet)
		public void DefineDynamicAssembly7_Name_InvalidChars ()
		{
			string [] invalid_char_names = new string [] {
				"\tAB",
				" AB",
				"\rAB",
				"A/B",
				":AB",
				"B:A",
				"B\\A",
				"BA\\"};

			AssemblyName name = new AssemblyName ();

			foreach (string invalid_name in invalid_char_names) {
				name.Name = invalid_name;
				try {
					AppDomain.CurrentDomain.DefineDynamicAssembly (
						name,
						AssemblyBuilderAccess.Run,
						Path.GetTempPath (),
						(PermissionSet) null,
						(PermissionSet) null,
						(PermissionSet) null);
					Assert.Fail ("#1:" + invalid_name);
				} catch (ArgumentException ex) {
					// Assembly names may not begin with whitespace
					// or contain the characters '/', '\' or ':'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + invalid_name);
					Assert.IsNull (ex.InnerException, "#3:" + invalid_name);
					Assert.IsNotNull (ex.Message, "#4:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'/'") != -1, "#5:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'\\'") != -1, "#6:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("':'") != -1, "#7:" + invalid_name);
					Assert.IsNull (ex.ParamName, "#8:" + invalid_name);
				}
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, PermissionSet, PermissionSet, PermissionSet)
		public void DefineDynamicAssembly7_Name_Null ()
		{
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					(AssemblyName) null,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			AssemblyName name = new AssemblyName ();

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			name.Name = string.Empty;

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, Evidence, PermissionSet, PermissionSet, PermissionSet)
		public void DefineDynamicAssembly8_Access_Invalid ()
		{
			AssemblyName name = new AssemblyName ();
			name.Name = "DefineDynamicAssembly8";

#if NET_2_0
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name, AssemblyBuilderAccess.Run |
					(AssemblyBuilderAccess) 666,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal enum value: 667
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("667") != -1, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("access", ex.ParamName, "#7");
			}
#else
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				name, AssemblyBuilderAccess.Run |
				(AssemblyBuilderAccess) 666,
				Path.GetTempPath (),
				AppDomain.CurrentDomain.Evidence,
				(PermissionSet) null,
				(PermissionSet) null,
				(PermissionSet) null);
			Assert.IsNotNull (ab, "#1");
#endif
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, Evidence, PermissionSet, PermissionSet, PermissionSet)
		public void DefineDynamicAssembly8_Name_InvalidChars ()
		{
			string [] invalid_char_names = new string [] {
				"\tAB",
				" AB",
				"\rAB",
				"A/B",
				":AB",
				"B:A",
				"B\\A",
				"BA\\"};

			AssemblyName name = new AssemblyName ();

			foreach (string invalid_name in invalid_char_names) {
				name.Name = invalid_name;
				try {
					AppDomain.CurrentDomain.DefineDynamicAssembly (
						name,
						AssemblyBuilderAccess.Run,
						Path.GetTempPath (),
						AppDomain.CurrentDomain.Evidence,
						(PermissionSet) null,
						(PermissionSet) null,
						(PermissionSet) null);
					Assert.Fail ("#1:" + invalid_name);
				} catch (ArgumentException ex) {
					// Assembly names may not begin with whitespace
					// or contain the characters '/', '\' or ':'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + invalid_name);
					Assert.IsNull (ex.InnerException, "#3:" + invalid_name);
					Assert.IsNotNull (ex.Message, "#4:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'/'") != -1, "#5:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'\\'") != -1, "#6:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("':'") != -1, "#7:" + invalid_name);
					Assert.IsNull (ex.ParamName, "#8:" + invalid_name);
				}
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, Evidence, PermissionSet, PermissionSet, PermissionSet)
		public void DefineDynamicAssembly8_Name_Null ()
		{
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					(AssemblyName) null,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			AssemblyName name = new AssemblyName ();

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			name.Name = string.Empty;

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, Evidence, PermissionSet, PermissionSet, PermissionSet, Boolean)
		public void DefineDynamicAssembly9_Access_Invalid ()
		{
			AssemblyName name = new AssemblyName ();
			name.Name = "DefineDynamicAssembly9";

#if NET_2_0
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name, AssemblyBuilderAccess.Run |
					(AssemblyBuilderAccess) 666,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null,
					true);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal enum value: 667
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("667") != -1, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("access", ex.ParamName, "#7");
			}
#else
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				name, AssemblyBuilderAccess.Run |
				(AssemblyBuilderAccess) 666,
				Path.GetTempPath (),
				AppDomain.CurrentDomain.Evidence,
				(PermissionSet) null,
				(PermissionSet) null,
				(PermissionSet) null,
				true);
			Assert.IsNotNull (ab, "#1");
#endif
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, Evidence, PermissionSet, PermissionSet, PermissionSet, Boolean)
		public void DefineDynamicAssembly9_Name_InvalidChars ()
		{
			string [] invalid_char_names = new string [] {
				"\tAB",
				" AB",
				"\rAB",
				"A/B",
				":AB",
				"B:A",
				"B\\A",
				"BA\\"};

			AssemblyName name = new AssemblyName ();

			foreach (string invalid_name in invalid_char_names) {
				name.Name = invalid_name;
				try {
					AppDomain.CurrentDomain.DefineDynamicAssembly (
						name,
						AssemblyBuilderAccess.Run,
						Path.GetTempPath (),
						AppDomain.CurrentDomain.Evidence,
						(PermissionSet) null,
						(PermissionSet) null,
						(PermissionSet) null,
						true);
					Assert.Fail ("#1:" + invalid_name);
				} catch (ArgumentException ex) {
					// Assembly names may not begin with whitespace
					// or contain the characters '/', '\' or ':'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + invalid_name);
					Assert.IsNull (ex.InnerException, "#3:" + invalid_name);
					Assert.IsNotNull (ex.Message, "#4:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'/'") != -1, "#5:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'\\'") != -1, "#6:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("':'") != -1, "#7:" + invalid_name);
					Assert.IsNull (ex.ParamName, "#8:" + invalid_name);
				}
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, Evidence, PermissionSet, PermissionSet, PermissionSet, Boolean)
		public void DefineDynamicAssembly9_Name_Null ()
		{
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					(AssemblyName) null,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null,
					true);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			AssemblyName name = new AssemblyName ();

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null,
					true);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			name.Name = string.Empty;

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null,
					true);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
		}

#if NET_2_0
		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, Evidence, PermissionSet, PermissionSet, PermissionSet, Boolean, IEnumerable<CustomAttributeBuilder>)
		public void DefineDynamicAssembly10_Access_Invalid ()
		{
			AssemblyName name = new AssemblyName ();
			name.Name = "DefineDynamicAssembly10";

#if NET_2_0
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name, AssemblyBuilderAccess.Run |
					(AssemblyBuilderAccess) 666,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null,
					true,
					new List<CustomAttributeBuilder> ());
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal enum value: 667
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("667") != -1, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("access", ex.ParamName, "#7");
			}
#else
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				name, AssemblyBuilderAccess.Run |
				(AssemblyBuilderAccess) 666,
				Path.GetTempPath (),
				AppDomain.CurrentDomain.Evidence,
				(PermissionSet) null,
				(PermissionSet) null,
				(PermissionSet) null,
				true,
				new List<CustomAttributeBuilder> ());
			Assert.IsNotNull (ab, "#1");
#endif
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, Evidence, PermissionSet, PermissionSet, PermissionSet, Boolean, IEnumerable<CustomAttributeBuilder>)
		public void DefineDynamicAssembly10_Name_InvalidChars ()
		{
			string [] invalid_char_names = new string [] {
				"\tAB",
				" AB",
				"\rAB",
				"A/B",
				":AB",
				"B:A",
				"B\\A",
				"BA\\"};

			AssemblyName name = new AssemblyName ();

			foreach (string invalid_name in invalid_char_names) {
				name.Name = invalid_name;
				try {
					AppDomain.CurrentDomain.DefineDynamicAssembly (
						name,
						AssemblyBuilderAccess.Run,
						Path.GetTempPath (),
						AppDomain.CurrentDomain.Evidence,
						(PermissionSet) null,
						(PermissionSet) null,
						(PermissionSet) null,
						true,
						new List<CustomAttributeBuilder> ());
					Assert.Fail ("#1:" + invalid_name);
				} catch (ArgumentException ex) {
					// Assembly names may not begin with whitespace
					// or contain the characters '/', '\' or ':'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + invalid_name);
					Assert.IsNull (ex.InnerException, "#3:" + invalid_name);
					Assert.IsNotNull (ex.Message, "#4:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'/'") != -1, "#5:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'\\'") != -1, "#6:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("':'") != -1, "#7:" + invalid_name);
					Assert.IsNull (ex.ParamName, "#8:" + invalid_name);
				}
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, Evidence, PermissionSet, PermissionSet, PermissionSet, Boolean, IEnumerable<CustomAttributeBuilder>)
		public void DefineDynamicAssembly10_Name_Null ()
		{
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					(AssemblyName) null,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null,
					true,
					new List<CustomAttributeBuilder> ());
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			AssemblyName name = new AssemblyName ();

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null,
					true,
					new List<CustomAttributeBuilder> ());
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			name.Name = string.Empty;

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence,
					(PermissionSet) null,
					(PermissionSet) null,
					(PermissionSet) null,
					true,
					new List<CustomAttributeBuilder> ());
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess, IEnumerable<CustomAttributeBuilder>)
		public void DefineDynamicAssembly11 ()
		{
			List<CustomAttributeBuilder> cattrs;
			AssemblyBuilder ab;
			Attribute attr;
			AssemblyName name;
			string assemblyFile;
			string current_dir = Directory.GetCurrentDirectory ();

			name = new AssemblyName ();
			name.Name = "DefineDynamicAssembly11A";

			cattrs = new List<CustomAttributeBuilder> ();
			cattrs.Add (new CustomAttributeBuilder (typeof (AssemblyVersionAttribute).
				GetConstructor (new Type [] { typeof (string) }),
				new object [] { "1.2.3.4"}));
			cattrs.Add (new CustomAttributeBuilder (typeof (AssemblyCultureAttribute).
				GetConstructor (new Type [] { typeof (string) }),
				new object [] { "nl-BE"}));
			cattrs.Add (new CustomAttributeBuilder (typeof (AssemblyAlgorithmIdAttribute).
				GetConstructor (new Type [] { typeof (AssemblyHashAlgorithm) }),
				new object [] { AssemblyHashAlgorithm.MD5 }));
			cattrs.Add (new CustomAttributeBuilder (typeof (AssemblyFlagsAttribute).
				GetConstructor (new Type [] { typeof (uint) }),
				new object [] { (uint)0x0100 }));
			cattrs.Add (new CustomAttributeBuilder (typeof (CLSCompliantAttribute).
				GetConstructor (new Type [] { typeof (bool) }),
				new object [] { true }));

			ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				name, AssemblyBuilderAccess.Save, cattrs);

			ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (ComVisibleAttribute).
				GetConstructor (new Type [] { typeof (bool) }),
				new object [] { true }));

			ab.Save ("DefineDynamicAssembly11A.dll");

			assemblyFile = Path.Combine (current_dir, "DefineDynamicAssembly11A.dll");

			try {
				AssemblyName an = AssemblyName.GetAssemblyName (assemblyFile);
				Assert.AreEqual (CultureInfo.InvariantCulture, an.CultureInfo, "#A1");
				Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#A2");
				Assert.AreEqual ("DefineDynamicAssembly11A, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", an.FullName, "#A3");
				Assert.IsNull (an.GetPublicKey (), "#A4");
				Assert.AreEqual (new byte [0], an.GetPublicKeyToken (), "#A5");
				Assert.AreEqual (AssemblyHashAlgorithm.SHA1, an.HashAlgorithm, "#A6");
				Assert.IsNull (an.KeyPair, "#A7");
				Assert.AreEqual ("DefineDynamicAssembly11A", an.Name, "#A8");
				//Assert.AreEqual (ProcessorArchitecture.MSIL, an.ProcessorArchitecture, "#A9");
				Assert.AreEqual (an.FullName, an.ToString (), "#A10");
				Assert.AreEqual (new Version (0, 0, 0, 0), an.Version, "#A11");
				Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, an.VersionCompatibility, "#A12");

				Assembly a;

				using (FileStream fs = File.OpenRead (assemblyFile)) {
					byte [] buffer = new byte [fs.Length];
					fs.Read (buffer, 0, buffer.Length);
					a = Assembly.Load (buffer);
				}

				attr = Attribute.GetCustomAttribute (a, typeof (AssemblyVersionAttribute));
				Assert.IsNotNull (attr, "#A13a");
				Assert.AreEqual ("1.2.3.4", ((AssemblyVersionAttribute) attr).Version, "#A13b");
				attr = Attribute.GetCustomAttribute (a, typeof (AssemblyCultureAttribute));
				Assert.IsNotNull (attr, "#A14a");
				Assert.AreEqual ("nl-BE", ((AssemblyCultureAttribute) attr).Culture, "#A14b");
				attr = Attribute.GetCustomAttribute (a, typeof (AssemblyAlgorithmIdAttribute));
				Assert.IsNotNull (attr, "#A15a");
				Assert.AreEqual ((uint) AssemblyHashAlgorithm.MD5, ((AssemblyAlgorithmIdAttribute) attr).AlgorithmId, "#A15b");
				attr = Attribute.GetCustomAttribute (a, typeof (AssemblyFlagsAttribute));
				Assert.IsNotNull (attr, "#A16a");
				Assert.AreEqual ((uint) 0x0100, ((AssemblyFlagsAttribute) attr).Flags, "#A16b");
				attr = Attribute.GetCustomAttribute (a, typeof (CLSCompliantAttribute));
				Assert.IsNotNull (attr, "#A17a");
				Assert.IsTrue (((CLSCompliantAttribute) attr).IsCompliant, "#A17b");
				attr = Attribute.GetCustomAttribute (a, typeof (ComVisibleAttribute));
				Assert.IsNotNull (attr, "#A18a");
				Assert.IsTrue (((ComVisibleAttribute) attr).Value, "#A18b");
			} finally {
				File.Delete (assemblyFile);
			}

			name = new AssemblyName ();
			name.CultureInfo = new CultureInfo ("fr-BE");
			name.KeyPair = new StrongNameKeyPair (keyPair);
			name.Name = "DefineDynamicAssembly11B";
			name.Version = new Version (3, 2, 4, 1);

			cattrs = new List<CustomAttributeBuilder> ();
			cattrs.Add (new CustomAttributeBuilder (typeof (AssemblyVersionAttribute).
				GetConstructor (new Type [] { typeof (string) }),
				new object [] { "1.2.3.4"}));
			cattrs.Add (new CustomAttributeBuilder (typeof (AssemblyCultureAttribute).
				GetConstructor (new Type [] { typeof (string) }),
				new object [] { "nl-BE"}));
			cattrs.Add (new CustomAttributeBuilder (typeof (AssemblyAlgorithmIdAttribute).
				GetConstructor (new Type [] { typeof (AssemblyHashAlgorithm) }),
				new object [] { AssemblyHashAlgorithm.MD5 }));
			cattrs.Add (new CustomAttributeBuilder (typeof (AssemblyFlagsAttribute).
				GetConstructor (new Type [] { typeof (uint) }),
				new object [] { (uint)0x0100 }));
			cattrs.Add (new CustomAttributeBuilder (typeof (CLSCompliantAttribute).
				GetConstructor (new Type [] { typeof (bool) }),
				new object [] { true }));

			ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				name, AssemblyBuilderAccess.Save, cattrs);

			ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (ComVisibleAttribute).
				GetConstructor (new Type [] { typeof (bool) }),
				new object [] { true }));

			ab.Save ("DefineDynamicAssembly11B.dll");

			assemblyFile = Path.Combine (current_dir, "DefineDynamicAssembly11B.dll");

			try {
				AssemblyName an = AssemblyName.GetAssemblyName (assemblyFile);
				Assert.AreEqual ("fr-BE", an.CultureInfo.Name, "#B1");
				Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#B2");
				Assert.AreEqual ("DefineDynamicAssembly11B, Version=3.2.4.1, Culture=fr-BE, PublicKeyToken=ce5276d8687ec6dc", an.FullName, "#B3");
				Assert.AreEqual (publicKey, an.GetPublicKey (), "#B4");
				Assert.AreEqual (pk_token, an.GetPublicKeyToken (), "#B5");
				Assert.AreEqual (AssemblyHashAlgorithm.SHA1, an.HashAlgorithm, "#B6");
				Assert.IsNull (an.KeyPair, "#B7");
				Assert.AreEqual ("DefineDynamicAssembly11B", an.Name, "#B8");
				//Assert.AreEqual (ProcessorArchitecture.MSIL, an.ProcessorArchitecture, "#B9");
				Assert.AreEqual (an.FullName, an.ToString (), "#B10");
				Assert.AreEqual (new Version (3, 2, 4, 1), an.Version, "#B11");
				Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, an.VersionCompatibility, "#B12");

				Assembly a;

				using (FileStream fs = File.OpenRead (assemblyFile)) {
					byte [] buffer = new byte [fs.Length];
					fs.Read (buffer, 0, buffer.Length);
					a = Assembly.Load (buffer);
				}

				attr = Attribute.GetCustomAttribute (a, typeof (AssemblyVersionAttribute));
				Assert.IsNotNull (attr, "#B13a");
				Assert.AreEqual ("1.2.3.4", ((AssemblyVersionAttribute) attr).Version, "#B13b");
				attr = Attribute.GetCustomAttribute (a, typeof (AssemblyCultureAttribute));
				Assert.IsNotNull (attr, "#B14a");
				Assert.AreEqual ("nl-BE", ((AssemblyCultureAttribute) attr).Culture, "#B14b");
				attr = Attribute.GetCustomAttribute (a, typeof (AssemblyAlgorithmIdAttribute));
				Assert.IsNotNull (attr, "#B15a");
				Assert.AreEqual ((uint) AssemblyHashAlgorithm.MD5, ((AssemblyAlgorithmIdAttribute) attr).AlgorithmId, "#B15b");
				attr = Attribute.GetCustomAttribute (a, typeof (AssemblyFlagsAttribute));
				Assert.IsNotNull (attr, "#B16a");
				Assert.AreEqual ((uint) 0x0100, ((AssemblyFlagsAttribute) attr).Flags, "#B16b");
				attr = Attribute.GetCustomAttribute (a, typeof (CLSCompliantAttribute));
				Assert.IsNotNull (attr, "#B17a");
				Assert.IsTrue (((CLSCompliantAttribute) attr).IsCompliant, "#B17b");
				attr = Attribute.GetCustomAttribute (a, typeof (ComVisibleAttribute));
				Assert.IsNotNull (attr, "#B18a");
				Assert.IsTrue (((ComVisibleAttribute) attr).Value, "#B18b");
			} finally {
				File.Delete (assemblyFile);
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess, IEnumerable<CustomAttributeBuilder>)
		public void DefineDynamicAssembly11_Access_Invalid ()
		{
			AssemblyName name = new AssemblyName ();
			name.Name = "DefineDynamicAssembly11";

#if NET_2_0
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name, AssemblyBuilderAccess.Run |
					(AssemblyBuilderAccess) 666,
					new List<CustomAttributeBuilder> ());
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal enum value: 667
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("667") != -1, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("access", ex.ParamName, "#7");
			}
#else
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				name, AssemblyBuilderAccess.Run |
				(AssemblyBuilderAccess) 666,
				new List<CustomAttributeBuilder> ());
			Assert.IsNotNull (ab, "#1");
#endif
		}

		[Test] // DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess, IEnumerable<CustomAttributeBuilder>)
		public void DefineDynamicAssembly11_Name_InvalidChars ()
		{
			string [] invalid_char_names = new string [] {
				"\tAB",
				" AB",
				"\rAB",
				"A/B",
				":AB",
				"B:A",
				"B\\A",
				"BA\\"};

			AssemblyName name = new AssemblyName ();

			foreach (string invalid_name in invalid_char_names) {
				name.Name = invalid_name;
				try {
					AppDomain.CurrentDomain.DefineDynamicAssembly (
						name,
						AssemblyBuilderAccess.Run,
						new List<CustomAttributeBuilder> ());
					Assert.Fail ("#1:" + invalid_name);
				} catch (ArgumentException ex) {
					// Assembly names may not begin with whitespace
					// or contain the characters '/', '\' or ':'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + invalid_name);
					Assert.IsNull (ex.InnerException, "#3:" + invalid_name);
					Assert.IsNotNull (ex.Message, "#4:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'/'") != -1, "#5:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("'\\'") != -1, "#6:" + invalid_name);
					Assert.IsTrue (ex.Message.IndexOf ("':'") != -1, "#7:" + invalid_name);
					Assert.IsNull (ex.ParamName, "#8:" + invalid_name);
				}
			}
		}

		[Test] // DefineDynamicAssembly (AssemblyName, AssemblyBuilderAccess, String, Evidence, PermissionSet, PermissionSet, PermissionSet, Boolean, IEnumerable<CustomAttributeBuilder>)
		public void DefineDynamicAssembly11_Name_Null ()
		{
			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					(AssemblyName) null,
					AssemblyBuilderAccess.Run,
					new List<CustomAttributeBuilder> ());
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("name", ex.ParamName, "#A6");
			}

			AssemblyName name = new AssemblyName ();

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					new List<CustomAttributeBuilder> ());
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			name.Name = string.Empty;

			try {
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name,
					AssemblyBuilderAccess.Run,
					new List<CustomAttributeBuilder> ());
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// AssemblyName.Name cannot be null or an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}
		}

		[Test] // ExecuteAssemblyByName (String)
		public void ExecuteAssemblyByName1_NoEntryPoint ()
		{
			try {
				AppDomain.CurrentDomain.ExecuteAssemblyByName ("mscorlib");
				Assert.Fail ("#1");
			} catch (MissingMethodException ex) {
				// Entry point not found in assembly '...'
				Assert.AreEqual (typeof (MissingMethodException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (Consts.AssemblyCorlib) != -1, "#5");
			}
		}

		[Test] // ExecuteAssemblyByName (String, Evidence)
		public void ExecuteAssemblyByName2_NoEntryPoint ()
		{
			try {
				AppDomain.CurrentDomain.ExecuteAssemblyByName (
					"mscorlib", (Evidence) null);
				Assert.Fail ("#1");
			} catch (MissingMethodException ex) {
				// Entry point not found in assembly '...'
				Assert.AreEqual (typeof (MissingMethodException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (Consts.AssemblyCorlib) != -1, "#5");
			}
		}

		[Test] // ExecuteAssemblyByName (String, Evidence, String [])
		public void ExecuteAssemblyByName3_NoEntryPoint ()
		{
			try {
				AppDomain.CurrentDomain.ExecuteAssemblyByName (
					"mscorlib", (Evidence) null,
					new string [0]);
				Assert.Fail ("#1");
			} catch (MissingMethodException ex) {
				// Entry point not found in assembly '...'
				Assert.AreEqual (typeof (MissingMethodException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (Consts.AssemblyCorlib) != -1, "#5");
			}
		}

		[Test] // ExecuteAssemblyByName (AssemblyName, Evidence, String [])
		public void ExecuteAssemblyByName4_NoEntryPoint ()
		{
			AssemblyName aname = new AssemblyName ("mscorlib");

			try {
				AppDomain.CurrentDomain.ExecuteAssemblyByName (
					aname, (Evidence) null, new string [0]);
				Assert.Fail ("#1");
			} catch (MissingMethodException ex) {
				// Entry point not found in assembly '...'
				Assert.AreEqual (typeof (MissingMethodException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (Consts.AssemblyCorlib) != -1, "#5");
			}
		}
#endif

		[Test]
		public void SetThreadPrincipal ()
		{
			IIdentity i = new GenericIdentity ("sebastien@ximian.com", "rfc822");
			IPrincipal p = new GenericPrincipal (i, null);
			ad = AppDomain.CreateDomain ("SetThreadPrincipal");
			ad.SetThreadPrincipal (p);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetThreadPrincipalNull ()
		{
			AppDomain.CurrentDomain.SetThreadPrincipal (null);
		}

		[Test]
		[ExpectedException (typeof (PolicyException))]
		public void SetThreadPrincipalTwice ()
		{
			IIdentity i = new GenericIdentity ("sebastien@ximian.com", "rfc822");
			IPrincipal p = new GenericPrincipal (i, null);
			ad = AppDomain.CreateDomain ("SetThreadPrincipalTwice");
			ad.SetThreadPrincipal (p);
			// you only live twice (or so James told me ;-)
			ad.SetThreadPrincipal (p);
		}

		[Test]
		[ExpectedException (typeof (AppDomainUnloadedException))]
		public void SetThreadPrincipalUnloaded ()
		{
			ad = AppDomain.CreateDomain ("Ximian");
			AppDomain.Unload (ad);
			IIdentity i = new GenericIdentity ("sebastien@ximian.com", "rfc822");
			IPrincipal p = new GenericPrincipal (i, null);
			ad.SetThreadPrincipal (p);
		}

		[Test]
		public void SetPrincipalPolicy_NoPrincipal ()
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.NoPrincipal);
		}

		[Test]
		public void SetPrincipalPolicy_UnauthenticatedPrincipal ()
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.UnauthenticatedPrincipal);
		}

		[Test]
		public void SetPrincipalPolicy_WindowsPrincipal ()
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.WindowsPrincipal);
		}

		[Test]
		[ExpectedException (typeof (AppDomainUnloadedException))]
		public void SetPrincipalPolicyUnloaded ()
		{
			ad = AppDomain.CreateDomain ("Ximian");
			AppDomain.Unload (ad);
			ad.SetPrincipalPolicy (PrincipalPolicy.NoPrincipal);
		}

		[Test]
		public void CreateDomain_String ()
		{
			ad = AppDomain.CreateDomain ("CreateDomain_String");
			Assert.IsNotNull (ad.Evidence, "Evidence");
			// Evidence are copied (or referenced?) from default app domain
			// we can't get default so we use the current (which should have copied the default)
			Assert.AreEqual (AppDomain.CurrentDomain.Evidence.Count, ad.Evidence.Count, "Evidence.Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateDomain_String_Null ()
		{
			ad = AppDomain.CreateDomain (null);
		}

		[Test]
		[Category ("NotDotNet")]
		public void CreateDomain_StringEvidence ()
		{
			Evidence e = new Evidence ();
			ad = AppDomain.CreateDomain ("CreateDomain_StringEvidence", e);
			Assert.IsNotNull (ad.Evidence, "Evidence");
			Assert.AreEqual (0, ad.Evidence.Count, "Evidence.Count");

			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.AreEqual (0, ad.Evidence.Count, "Evidence.Count");
			// evidence isn't copied but referenced
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateDomain_StringNullEvidence ()
		{
			ad = AppDomain.CreateDomain (null, new Evidence ());
		}

		[Test]
		public void CreateDomain_StringEvidenceNull ()
		{
			ad = AppDomain.CreateDomain ("CreateDomain_StringEvidenceNull", null);
			Assert.IsNotNull (ad.Evidence, "Evidence");
			// Evidence are copied (or referenced?) from default app domain
			// we can't get default so we use the current (which should have copied the default)
			Evidence e = AppDomain.CurrentDomain.Evidence;
			Assert.AreEqual (e.Count, ad.Evidence.Count, "Evidence.Count-1");
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.AreEqual (e.Count - 1, ad.Evidence.Count, "Evidence.Count-2");
			// evidence are copied
		}

		[Test]
		[Category ("NotDotNet")]
		public void CreateDomain_StringEvidenceAppDomainSetup ()
		{
			Evidence e = new Evidence ();
			AppDomainSetup info = new AppDomainSetup ();
			info.ApplicationName = "ApplicationName";

			ad = AppDomain.CreateDomain ("CreateDomain_StringEvidenceAppDomainSetup", e, info);
			Assert.IsNotNull (ad.Evidence, "Evidence");
			Assert.AreEqual (0, ad.Evidence.Count, "Evidence.Count");
			Assert.IsNotNull (ad.SetupInformation, "SetupInformation");
			Assert.AreEqual ("ApplicationName", ad.SetupInformation.ApplicationName);

			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.AreEqual (0, ad.Evidence.Count, "Evidence.Count");
			// evidence isn't copied but referenced
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateDomain_StringNullEvidenceAppDomainSetup ()
		{
			AppDomainSetup info = new AppDomainSetup ();
			ad = AppDomain.CreateDomain (null, new Evidence (), info);
		}

		[Test]
		public void CreateDomain_StringEvidenceNullAppDomainSetup ()
		{
			AppDomainSetup info = new AppDomainSetup ();
			info.ApplicationName = "ApplicationName";
			ad = AppDomain.CreateDomain ("CreateDomain_StringEvidenceNullAppDomainSetup", null, info);
			Assert.IsNotNull (ad.Evidence, "Evidence");
			// Evidence are copied (or referenced?) from default app domain
			// we can't get default so we use the current (which should have copied the default)
			Assert.AreEqual (AppDomain.CurrentDomain.Evidence.Count, ad.Evidence.Count, "Evidence.Count");
			Assert.AreEqual ("ApplicationName", ad.SetupInformation.ApplicationName, "ApplicationName-1");
			info.ApplicationName = "Test";
			Assert.AreEqual ("Test", info.ApplicationName, "ApplicationName-2");
			Assert.AreEqual ("ApplicationName", ad.SetupInformation.ApplicationName, "ApplicationName-3");
			// copied
		}

		[Test]
		[Category ("NotDotNet")]
		public void CreateDomain_StringEvidenceAppDomainSetupNull ()
		{
			Evidence e = new Evidence ();
			ad = AppDomain.CreateDomain ("CreateDomain_StringEvidenceAppDomainSetupNull", e, null);
			Assert.IsNotNull (ad.Evidence, "Evidence");
			Assert.AreEqual (0, ad.Evidence.Count, "Evidence.Count");
			// SetupInformation is copied from default app domain
			Assert.IsNotNull (ad.SetupInformation, "SetupInformation");
		}

		[Test] // ExecuteAssembly (String)
		public void ExecuteAssembly1_NoEntryPoint ()
		{
			Assembly assembly = typeof (AppDomainTest).Assembly;

			try {
				AppDomain.CurrentDomain.ExecuteAssembly (
					assembly.Location);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (MissingMethodException ex) {
				// Entry point not found in assembly '...'
				Assert.AreEqual (typeof (MissingMethodException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (assembly.FullName) != -1, "#5");
			}
#else
			} catch (COMException ex) {
				// Unspecified error
				Assert.AreEqual (typeof (COMException), ex.GetType (), "#2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
			}
#endif
		}

		[Test] // ExecuteAssembly (String, Evidence)
		public void ExecuteAssembly2_NoEntryPoint ()
		{
			Assembly assembly = typeof (AppDomainTest).Assembly;

			try {
				AppDomain.CurrentDomain.ExecuteAssembly (
					assembly.Location,
					(Evidence) null);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (MissingMethodException ex) {
				// Entry point not found in assembly '...'
				Assert.AreEqual (typeof (MissingMethodException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (assembly.FullName) != -1, "#5");
			}
#else
			} catch (COMException ex) {
				// Unspecified error
				Assert.AreEqual (typeof (COMException), ex.GetType (), "#2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
			}
#endif
		}

		[Test] // ExecuteAssembly (String, Evidence, String [])
		public void ExecuteAssembly3_NoEntryPoint ()
		{
			Assembly assembly = typeof (AppDomainTest).Assembly;

			try {
				AppDomain.CurrentDomain.ExecuteAssembly (
					assembly.Location,
					(Evidence) null,
					new string [0]);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (MissingMethodException ex) {
				// Entry point not found in assembly '...'
				Assert.AreEqual (typeof (MissingMethodException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (assembly.FullName) != -1, "#5");
			}
#else
			} catch (COMException ex) {
				// Unspecified error
				Assert.AreEqual (typeof (COMException), ex.GetType (), "#2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
			}
#endif
		}

		[Test] // ExecuteAssembly (String, Evidence, String [], Byte [], AssemblyHashAlgorithm)
		[Category ("NotWorking")] // Not implemented
		public void ExecuteAssembly4_NoEntryPoint ()
		{
			Assembly assembly = typeof (AppDomainTest).Assembly;

			try {
				AppDomain.CurrentDomain.ExecuteAssembly (
					assembly.Location,
					(Evidence) null,
					new string [0],
					(byte []) null,
					AssemblyHashAlgorithm.SHA1);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (MissingMethodException ex) {
				// Entry point not found in assembly '...'
				Assert.AreEqual (typeof (MissingMethodException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (assembly.FullName) != -1, "#5");
			}
#else
			} catch (COMException ex) {
				// Unspecified error
				Assert.AreEqual (typeof (COMException), ex.GetType (), "#2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
			}
#endif
		}

		[Test] // bug #79720
		[Category ("NotWorking")]
		public void Load_Loaded_Ignore ()
		{
			int assemblyStartCount = AppDomain.CurrentDomain.GetAssemblies ().Length;

			// PART A

			string assemblyFile = Path.Combine (tempDir, "bug79720A.dll");
			AssemblyName aname = new AssemblyName ();
			aname.Name = "bug79720A";
			aname.Version = new Version (2, 4);

			GenerateAssembly (aname, assemblyFile);

			Assert.AreEqual (assemblyStartCount, AppDomain.CurrentDomain.GetAssemblies ().Length, "#A1");

			aname = new AssemblyName ();
			aname.Name = "bug79720A";
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A2");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720A";
			aname.Version = new Version (0, 0, 0, 0);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A3");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720A";
			aname.Version = new Version (2, 4);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A4");
			} catch (FileNotFoundException) {
			}

			Assert.AreEqual (assemblyStartCount, AppDomain.CurrentDomain.GetAssemblies ().Length, "#A5");

			Assembly.LoadFrom (assemblyFile);

			Assert.AreEqual (assemblyStartCount + 1, AppDomain.CurrentDomain.GetAssemblies ().Length, "#A6");

			aname = new AssemblyName ();
			aname.Name = "bug79720A";
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A7");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720A";
			aname.Version = new Version (0, 0, 0, 0);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A8");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720A";
			aname.Version = new Version (2, 4);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A9");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720A";
			aname.Version = new Version (2, 4);
			aname.CultureInfo = CultureInfo.InvariantCulture;
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A10");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720A";
			aname.Version = new Version (2, 4, 0, 0);
			aname.CultureInfo = CultureInfo.InvariantCulture;
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A11");
			} catch (FileNotFoundException) {
			}

			Assert.AreEqual (assemblyStartCount + 1, AppDomain.CurrentDomain.GetAssemblies ().Length, "#A12");

			// PART B

			assemblyFile = Path.Combine (tempDir, "bug79720B.dll");
			aname = new AssemblyName ();
			aname.Name = "bug79720B";
			aname.Version = new Version (2, 4, 1);
			aname.CultureInfo = new CultureInfo ("nl-BE");

			GenerateAssembly (aname, assemblyFile);

			Assert.AreEqual (assemblyStartCount + 1, AppDomain.CurrentDomain.GetAssemblies ().Length, "#B1");

			aname = new AssemblyName ();
			aname.Name = "bug79720B";
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B2");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720B";
			aname.Version = new Version (0, 0, 0, 0);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B3");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720B";
			aname.Version = new Version (2, 4, 1);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B4");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720B";
			aname.Version = new Version (2, 4, 1);
			aname.CultureInfo = new CultureInfo ("nl-BE");
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B5");
			} catch (FileNotFoundException) {
			}

			Assert.AreEqual (assemblyStartCount + 1, AppDomain.CurrentDomain.GetAssemblies ().Length, "#B6");

			Assembly.LoadFrom (assemblyFile);

			Assert.AreEqual (assemblyStartCount + 2, AppDomain.CurrentDomain.GetAssemblies ().Length, "#B7");

			aname = new AssemblyName ();
			aname.Name = "bug79720B";
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B8");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720B";
			aname.Version = new Version (0, 0, 0, 0);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B9");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720B";
			aname.Version = new Version (2, 4, 1);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B10");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720B";
			aname.Version = new Version (2, 4, 1);
			aname.CultureInfo = new CultureInfo ("nl-BE");
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B11");
			} catch (FileNotFoundException) {
			}

			Assert.AreEqual (assemblyStartCount + 2, AppDomain.CurrentDomain.GetAssemblies ().Length, "#B12");

			// PART C

			assemblyFile = Path.Combine (tempDir, "bug79720C.dll");
			aname = new AssemblyName ();
			aname.Name = "bug79720C";
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Version = new Version (2, 4);
			aname.KeyPair = new StrongNameKeyPair (keyPair);

			GenerateAssembly (aname, assemblyFile);

			Assert.AreEqual (assemblyStartCount + 2, AppDomain.CurrentDomain.GetAssemblies ().Length, "#C1");

			aname = new AssemblyName ();
			aname.Name = "bug79720C";
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C2");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720C";
			aname.Version = new Version (0, 0, 0, 0);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C3");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720C";
			aname.Version = new Version (2, 4, 1);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C4");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720C";
			aname.Version = new Version (2, 4, 1);
			aname.CultureInfo = new CultureInfo ("nl-BE");
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C5");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720C";
			aname.Version = new Version (2, 4, 1);
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.SetPublicKey (publicKey);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C6");
			} catch (FileNotFoundException) {
			}

			Assert.AreEqual (assemblyStartCount + 2, AppDomain.CurrentDomain.GetAssemblies ().Length, "#C7");

			Assembly.LoadFrom (assemblyFile);

			Assert.AreEqual (assemblyStartCount + 3, AppDomain.CurrentDomain.GetAssemblies ().Length, "#C8");

			aname = new AssemblyName ();
			aname.Name = "bug79720C";
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C9");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720C";
			aname.Version = new Version (0, 0, 0, 0);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C10");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720C";
			aname.Version = new Version (2, 4);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C11");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720C";
			aname.Version = new Version (2, 4);
			aname.CultureInfo = new CultureInfo ("nl-BE");
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C12");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79720C";
			aname.Version = new Version (2, 4);
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.SetPublicKey (publicKey);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C13");
			} catch (FileNotFoundException) {
			}

			Assert.AreEqual (assemblyStartCount + 3, AppDomain.CurrentDomain.GetAssemblies ().Length, "#C14");
		}

		[Test]
		[Category ("NotWorking")]
		public void Load_Loaded_Multiple ()
		{
			string cultureDir = Path.Combine (tempDir, "nl-BE");
			if (!Directory.Exists (cultureDir))
				Directory.CreateDirectory (cultureDir);

			AppDomain ad = CreateTestDomain (tempDir, true);
			try {
				CrossDomainTester cdt = CreateCrossDomainTester (ad);

				int assemblyCount = cdt.AssemblyCount;

				// PART A

				AssemblyName aname = new AssemblyName ();
				aname.Name = "multipleA";
				aname.Version = new Version (1, 2, 3, 4);
				cdt.GenerateAssembly (aname, Path.Combine (tempDir, "multipleA.dll"));

				Assert.AreEqual (assemblyCount + 1, cdt.AssemblyCount, "#A1");

				aname = new AssemblyName ();
				aname.Name = "multipleA";
				Assert.IsTrue (cdt.AssertLoad (aname), "#A2");

				Assert.AreEqual (assemblyCount + 2, cdt.AssemblyCount, "#A3");

				aname = new AssemblyName ();
				aname.Name = "multipleA";
				Assert.IsTrue (cdt.AssertLoad (aname), "#A4");

				aname = new AssemblyName ();
				aname.Name = "multipleA";
				aname.CultureInfo = CultureInfo.InvariantCulture;
				Assert.IsTrue (cdt.AssertLoad (aname), "#A5");

				aname = new AssemblyName ();
				aname.Name = "multipleA";
				aname.CultureInfo = CultureInfo.InvariantCulture;
				Assert.IsTrue (cdt.AssertLoad (aname), "#A6");

				aname = new AssemblyName ();
				aname.Name = "multipleA";
				aname.CultureInfo = CultureInfo.InvariantCulture;
				aname.Version = new Version (1, 2, 3, 4);
				Assert.IsTrue (cdt.AssertLoad (aname), "#A7");

				aname = new AssemblyName ();
				aname.Name = "multipleA";
				aname.CultureInfo = CultureInfo.InvariantCulture;
				aname.Version = new Version (1, 2, 3, 4);
				Assert.IsTrue (cdt.AssertLoad (aname), "#A8");

				Assert.AreEqual (assemblyCount + 2, cdt.AssemblyCount, "#A9");

				// PART B

				aname = new AssemblyName ();
				aname.Name = "multipleB";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				aname.Version = new Version (2, 4, 1, 0);
				cdt.GenerateAssembly (aname, Path.Combine (tempDir, "nl-BE/multipleB.dll"));

				Assert.AreEqual (assemblyCount + 3, cdt.AssemblyCount, "#B1");

				aname = new AssemblyName ();
				aname.Name = "multipleB";
				Assert.IsTrue (cdt.AssertLoad (aname), "#B2");

				Assert.AreEqual (assemblyCount + 4, cdt.AssemblyCount, "#B3");

				aname = new AssemblyName ();
				aname.Name = "multipleB";
				Assert.IsTrue (cdt.AssertLoad (aname), "#B4");

				aname = new AssemblyName ();
				aname.Name = "multipleB";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				Assert.IsTrue (cdt.AssertLoad (aname), "#B5");

				aname = new AssemblyName ();
				aname.Name = "multipleB";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				Assert.IsTrue (cdt.AssertLoad (aname), "#B6");

				aname = new AssemblyName ();
				aname.Name = "multipleB";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				aname.Version = new Version (2, 4, 1, 0);
				Assert.IsTrue (cdt.AssertLoad (aname), "#B7");

				aname = new AssemblyName ();
				aname.Name = "multipleB";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				aname.Version = new Version (2, 4, 1, 0);
				Assert.IsTrue (cdt.AssertLoad (aname), "#B8");

				Assert.AreEqual (assemblyCount + 4, cdt.AssemblyCount, "#B9");

				// PART C

				aname = new AssemblyName ();
				aname.Name = "multipleC";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				aname.Version = new Version (2, 4, 0, 0);
				aname.KeyPair = new StrongNameKeyPair (keyPair);
				cdt.GenerateAssembly (aname, Path.Combine (tempDir, "nl-BE/multipleC.dll"));

				Assert.AreEqual (assemblyCount + 5, cdt.AssemblyCount, "#C1");

				aname = new AssemblyName ();
				aname.Name = "multipleC";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				aname.Version = new Version (2, 4, 0, 0);
				aname.SetPublicKey (publicKey);
				Assert.IsTrue (cdt.AssertLoad (aname), "#C2");

				Assert.AreEqual (assemblyCount + 6, cdt.AssemblyCount, "#C3");

				aname = new AssemblyName ();
				aname.Name = "multipleC";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				aname.Version = new Version (2, 4, 0, 0);
				aname.SetPublicKey (publicKey);
				Assert.IsTrue (cdt.AssertLoad (aname), "#C4");

				Assert.AreEqual (assemblyCount + 6, cdt.AssemblyCount, "#C5");
			} finally {
				AppDomain.Unload (ad);
			}
		}

		[Test] // bug #79522
		[Category ("NotWorking")]
		public void Load_Manifest_Mismatch ()
		{
			string assemblyFile = Path.Combine (tempDir, "bug79522A.dll");
			AssemblyName aname = new AssemblyName ();
			aname.Name = "bug79522A";
			aname.Version = new Version (2, 4);

			GenerateAssembly (aname, assemblyFile);

			aname = new AssemblyName ();
			aname.CodeBase = assemblyFile;
			aname.Name = "whateveryouwant";
			aname.Version = new Version (1, 1);

			// despite the fact that no assembly with the specified name
			// exists, the assembly pointed to by the CodeBase of the
			// AssemblyName will be loaded
			//
			// however the display name of the loaded assembly does not
			// match the display name of the AssemblyName, and as a result
			// a FileLoadException is thrown
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A1");
			} catch (FileLoadException) {
			}

			// if we set CodeBase to some garbage, then we'll get a
			// FileNotFoundException instead
			aname.CodeBase = "whatever";
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A2");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79522A";
			aname.CodeBase = assemblyFile;
#if NET_2_0
			AppDomain.CurrentDomain.Load (aname);
#else
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A3");
			} catch (FileLoadException) {
			}
#endif

			aname = new AssemblyName ();
			aname.Name = "bug79522A";
			aname.CodeBase = assemblyFile;
			aname.Version = new Version (2, 5);
#if NET_2_0
			// the version number is not considered when comparing the manifest
			// of the assembly found using codebase
			AppDomain.CurrentDomain.Load (aname);
#else
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A4");
			} catch (FileLoadException) {
			}
#endif

			aname = new AssemblyName ();
			aname.Name = "bug79522A";
			aname.CodeBase = assemblyFile;
			aname.Version = new Version (2, 4, 1);
#if NET_2_0
			// the version number is not considered when comparing the manifest
			// of the assembly found using codebase
			AppDomain.CurrentDomain.Load (aname);
#else
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A5");
			} catch (FileLoadException) {
			}
#endif

			// if version is set, then culture must also be set
			aname = new AssemblyName ();
			aname.Name = "bug79522A";
			aname.CodeBase = assemblyFile;
			aname.Version = new Version (2, 4);
#if NET_2_0
			AppDomain.CurrentDomain.Load (aname);
#else
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A6");
			} catch (FileLoadException) {
			}
#endif

			// version number does not need to be set
			aname = new AssemblyName ();
			aname.Name = "bug79522A";
			aname.CodeBase = assemblyFile;
			aname.CultureInfo = CultureInfo.InvariantCulture;
			AppDomain.CurrentDomain.Load (aname);

			// if set, the version number must match exactly
			aname = new AssemblyName ();
			aname.Name = "bug79522A";
			aname.CodeBase = assemblyFile;
			aname.CultureInfo = CultureInfo.InvariantCulture;
			aname.Version = new Version (2, 4);
			AppDomain.CurrentDomain.Load (aname);

			// if both culture and version are set, then the version diff
			// is ignored
			aname = new AssemblyName ();
			aname.Name = "bug79522A";
			aname.CodeBase = assemblyFile;
			aname.CultureInfo = CultureInfo.InvariantCulture;
			aname.Version = new Version (2, 5);
			AppDomain.CurrentDomain.Load (aname);

			// loaded assembly is not signed
			aname = new AssemblyName ();
			aname.Name = "bug79522A";
			aname.CodeBase = assemblyFile;
			aname.CultureInfo = CultureInfo.InvariantCulture;
			aname.Version = new Version (2, 4);
			aname.SetPublicKey (publicKey);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A7");
			} catch (FileLoadException) {
			}

			// if set, the culture must match
			aname = new AssemblyName ();
			aname.Name = "bug79522A";
			aname.CodeBase = assemblyFile;
			aname.Version = new Version (2, 4);
			aname.CultureInfo = new CultureInfo ("en-US");
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#A8");
			} catch (FileLoadException) {
			}

			// PART B

			assemblyFile = Path.Combine (tempDir, "bug79522B.dll");
			aname = new AssemblyName ();
			aname.Name = "bug79522B";
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Version = new Version (2, 4, 1);

			GenerateAssembly (aname, assemblyFile);

			aname = new AssemblyName ();
			aname.CodeBase = assemblyFile;
			aname.Name = "whateveryouwant";
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Version = new Version (1, 1);

			// despite the fact that no assembly with the specified name
			// exists, the assembly pointed to by the CodeBase of the
			// AssemblyName will be loaded
			//
			// however the display name of the loaded assembly does not
			// match the display name of the AssemblyName, and as a result
			// a FileLoadException is thrown
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B1");
			} catch (FileLoadException) {
			}

			// if we set CodeBase to some garbage, then we'll get a
			// FileNotFoundException instead
			aname.CodeBase = "whatever";
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B2");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79522B";
			aname.CodeBase = assemblyFile;
#if NET_2_0
			// the version number is not considered when comparing the manifest
			// of the assembly found using codebase
			AppDomain.CurrentDomain.Load (aname);
#else
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B3");
			} catch (FileLoadException) {
			}
#endif

			aname = new AssemblyName ();
			aname.Name = "bug79522B";
			aname.CodeBase = assemblyFile;
			aname.Version = new Version (5, 5);
#if NET_2_0
			// the version number is not considered when comparing the manifest
			// of the assembly found using codebase
			AppDomain.CurrentDomain.Load (aname);
#else
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B3");
			} catch (FileLoadException) {
			}
#endif

			aname = new AssemblyName ();
			aname.Name = "bug79522B";
			aname.CodeBase = assemblyFile;
			aname.Version = new Version (2, 4, 1);
#if NET_2_0
			AppDomain.CurrentDomain.Load (aname);
#else
			// when the loaded assembly has a specific culture, then that
			// culture must be set if you set the Version on the aname
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B4");
			} catch (FileLoadException) {
			}
#endif

			// version does not need to be set
			aname = new AssemblyName ();
			aname.Name = "bug79522B";
			aname.CodeBase = assemblyFile;
			aname.CultureInfo = new CultureInfo ("nl-BE");
			AppDomain.CurrentDomain.Load (aname);

			// if both culture and version are set, then the version diff
			// is ignored
			aname = new AssemblyName ();
			aname.Name = "bug79522B";
			aname.CodeBase = assemblyFile;
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Version = new Version (6, 5);
			AppDomain.CurrentDomain.Load (aname);

			// loaded assembly is not signed
			aname = new AssemblyName ();
			aname.Name = "bug79522B";
			aname.CodeBase = assemblyFile;
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.SetPublicKey (publicKey);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B5");
			} catch (FileLoadException) {
			}

			// if set, the culture must match
			aname = new AssemblyName ();
			aname.Name = "bug79522B";
			aname.CodeBase = assemblyFile;
			aname.Version = new Version (2, 4, 1);
			aname.CultureInfo = new CultureInfo ("en-US");
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#B6");
			} catch (FileLoadException) {
			}

			// PART C

			assemblyFile = Path.Combine (tempDir, "bug79522C.dll");
			aname = new AssemblyName ();
			aname.Name = "bug79522C";
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Version = new Version (2, 4);
			aname.KeyPair = new StrongNameKeyPair (keyPair);

			GenerateAssembly (aname, assemblyFile);

			aname = new AssemblyName ();
			aname.CodeBase = assemblyFile;
			aname.Name = "whateveryouwant";
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Version = new Version (1, 1);
			aname.SetPublicKey (publicKey);

			// despite the fact that no assembly with the specified name
			// exists, the assembly pointed to by the CodeBase of the
			// AssemblyName will be loaded
			//
			// however the display name of the loaded assembly does not
			// match the display name of the AssemblyName, and as a result
			// a FileLoadException is thrown
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C1");
			} catch (FileLoadException) {
			}

			// if we set CodeBase to some garbage, then we'll get a
			// FileNotFoundException instead
			aname.CodeBase = "whatever";
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C2");
			} catch (FileNotFoundException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79522C";
			aname.CodeBase = assemblyFile;
#if NET_2_0
			AppDomain.CurrentDomain.Load (aname);
#else
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C3");
			} catch (FileLoadException) {
			}
#endif

			aname = new AssemblyName ();
			aname.Name = "bug79522C";
			aname.CodeBase = assemblyFile;
			aname.Version = new Version (5, 5);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C3");
			} catch (FileLoadException) {
			}

			aname = new AssemblyName ();
			aname.Name = "bug79522C";
			aname.CodeBase = assemblyFile;
			aname.Version = new Version (2, 4);
#if NET_2_0
			AppDomain.CurrentDomain.Load (aname);
#else
			// when the loaded assembly has a specific culture/publickey,
			// then that culture/publickey must be set if you set the
			// Version on the aname
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C4");
			} catch (FileLoadException) {
			}
#endif

			aname = new AssemblyName ();
			aname.Name = "bug79522C";
			aname.CodeBase = assemblyFile;
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Version = new Version (2, 4);
#if NET_2_0
			AppDomain.CurrentDomain.Load (aname);
#else
			// if loaded assembly is signed, then the public key must be set
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C5");
			} catch (FileLoadException) {
			}
#endif

			aname = new AssemblyName ();
			aname.Name = "bug79522C";
			aname.CodeBase = assemblyFile;
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.SetPublicKey (publicKey);
#if NET_2_0
			AppDomain.CurrentDomain.Load (aname);
#else
			// if public key is set, then version must be set
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C6");
			} catch (FileLoadException) {
			}
#endif

			aname = new AssemblyName ();
			aname.Name = "bug79522C";
			aname.CodeBase = assemblyFile;
			aname.CultureInfo = new CultureInfo ("nl-BE");
#if NET_2_0
			AppDomain.CurrentDomain.Load (aname);
#else
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C7");
			} catch (FileLoadException) {
			}
#endif

			// if culture and version are set, then the version must match
			aname = new AssemblyName ();
			aname.Name = "bug79522C";
			aname.CodeBase = assemblyFile;
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.SetPublicKey (publicKey);
			aname.Version = new Version (5, 6);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C8");
			} catch (FileLoadException) {
			}

			// publickey must match
			aname = new AssemblyName ();
			aname.Name = "bug79522C";
			aname.CodeBase = assemblyFile;
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Version = new Version (2, 4);
			aname.SetPublicKey (publicKey2);
			try {
				AppDomain.CurrentDomain.Load (aname);
				Assert.Fail ("#C9");
#if NET_2_0
			} catch (SecurityException) {
				// Invalid assembly public key
			}
#else
			} catch (FileLoadException) {
			}
#endif

			aname = new AssemblyName ();
			aname.Name = "bug79522C";
			aname.CodeBase = assemblyFile;
			aname.SetPublicKey (publicKey);
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Version = new Version (2, 4);
			AppDomain.CurrentDomain.Load (aname);
		}

		[Test] // bug #79715
		public void Load_PartialVersion ()
		{
			AppDomain ad = CreateTestDomain (tempDir, true);
			try {
				CrossDomainTester cdt = CreateCrossDomainTester (ad);

				AssemblyName aname = new AssemblyName ();
				aname.Name = "bug79715";
				aname.Version = new Version (1, 2, 3, 4);
				cdt.GenerateAssembly (aname, Path.Combine (tempDir, "bug79715.dll"));

				aname = new AssemblyName ();
				aname.Name = "bug79715";
				aname.Version = new Version (1, 2);
				Assert.IsTrue (cdt.AssertLoad (aname), "#A1");
				Assert.IsTrue (cdt.AssertLoad (aname.FullName), "#A2");

				aname = new AssemblyName ();
				aname.Name = "bug79715";
				aname.Version = new Version (1, 2, 3);
				Assert.IsTrue (cdt.AssertLoad (aname), "#B1");
				Assert.IsTrue (cdt.AssertLoad (aname.FullName), "#B2");

				aname = new AssemblyName ();
				aname.Name = "bug79715";
				aname.Version = new Version (1, 2, 3, 4);
				Assert.IsTrue (cdt.AssertLoad (aname), "#C1");
				Assert.IsTrue (cdt.AssertLoad (aname.FullName), "#C2");
			} finally {
				AppDomain.Unload (ad);
			}
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Load_EmptyString ()
		{
			AppDomain.CurrentDomain.Load ("");
		}

		[Test]
		public void SetAppDomainPolicy ()
		{
			ad = AppDomain.CreateDomain ("SetAppDomainPolicy_Null");
			ad.SetAppDomainPolicy (PolicyLevel.CreateAppDomainLevel ());
			// not much to see
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetAppDomainPolicy_Null ()
		{
			ad = AppDomain.CreateDomain ("SetAppDomainPolicy_Null");
			ad.SetAppDomainPolicy (null);
		}

		[Test]
		[ExpectedException (typeof (PolicyException))]
		public void SetAppDomainPolicy_Dual ()
		{
			ad = AppDomain.CreateDomain ("SetAppDomainPolicy_Dual");
			PolicyLevel pl = PolicyLevel.CreateAppDomainLevel ();
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			pl.RootCodeGroup.PolicyStatement = new PolicyStatement (ps);
			ad.SetAppDomainPolicy (pl);

			// only one time!
			pl = PolicyLevel.CreateAppDomainLevel ();
			ps = new PermissionSet (PermissionState.None);
			pl.RootCodeGroup.PolicyStatement = new PolicyStatement (ps);
			ad.SetAppDomainPolicy (pl);
		}

		[Test]
		[ExpectedException (typeof (AppDomainUnloadedException))]
		public void SetAppDomainPolicy_Unloaded ()
		{
			ad = AppDomain.CreateDomain ("SetAppDomainPolicy_Unloaded");
			AppDomain.Unload (ad);
			ad.SetAppDomainPolicy (PolicyLevel.CreateAppDomainLevel ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetData_Null ()
		{
			AppDomain.CurrentDomain.GetData (null);
		}

		[Test]
		public void SetData ()
		{
			AppDomain.CurrentDomain.SetData ("data", "data");
			Assert.AreEqual ("data", AppDomain.CurrentDomain.GetData ("data"), "GetData");
			AppDomain.CurrentDomain.SetData ("data", null);
			Assert.IsNull (AppDomain.CurrentDomain.GetData ("data"), "GetData-Null");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetData_Null ()
		{
			AppDomain.CurrentDomain.SetData (null, "data");
		}

#if NET_2_0
		[Test]
		public void ApplyPolicy ()
		{
			ad = AppDomain.CreateDomain ("ApplyPolicy");
			string fullname = Assembly.GetExecutingAssembly ().FullName;
			string result = ad.ApplyPolicy (fullname);
			Assert.AreEqual (fullname, result, "ApplyPolicy");
			// doesn't even requires an assembly name
			Assert.AreEqual ("123", ad.ApplyPolicy ("123"), "Invalid FullName");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ApplyPolicy_Empty ()
		{
			ad = AppDomain.CreateDomain ("ApplyPolicy_Empty");
			ad.ApplyPolicy (String.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ApplyPolicy_Null ()
		{
			ad = AppDomain.CreateDomain ("ApplyPolicy_Null");
			ad.ApplyPolicy (null);
		}

		[Test]
		public void DomainManager ()
		{
			Assert.IsNull (AppDomain.CurrentDomain.DomainManager, "CurrentDomain.DomainManager");
			ad = AppDomain.CreateDomain ("DomainManager");
			Assert.IsNull (ad.DomainManager, "ad.DomainManager");
		}

		[Test]
		public void IsDefaultAppDomain ()
		{
			ad = AppDomain.CreateDomain ("ReflectionOnlyGetAssemblies");
			Assert.IsFalse (ad.IsDefaultAppDomain (), "IsDefaultAppDomain");
			// we have no public way to get the default appdomain
		}

		[Test]
		public void ReflectionOnlyGetAssemblies ()
		{
			ad = AppDomain.CreateDomain ("ReflectionOnlyGetAssemblies");
			Assembly [] a = ad.ReflectionOnlyGetAssemblies ();
			Assert.IsNotNull (a, "ReflectionOnlyGetAssemblies");
			Assert.AreEqual (0, a.Length, "Count");			

			string assemblyFile = Path.Combine (tempDir, "bug499013.dll");
			AssemblyName aname = new AssemblyName ();
			aname.Name = "bug499013";
			aname.Version = new Version (2, 4);

			GenerateAssembly (aname, assemblyFile);

			Assembly.ReflectionOnlyLoadFrom (assemblyFile);
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies ())
				Assert.IsTrue (assembly.GetName ().Name != "bug499013");
		}

		[Test]
		public void ReflectionOnlyAssemblyResolve ()
		{
			AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
			Assembly asm = Assembly.ReflectionOnlyLoad(Assembly.LoadWithPartialName("System").FullName);
			asm.GetTypes();
		}

        [Test]
		public void ResourceResolve ()
		{
			bool called = false;

			ResolveEventHandler del = delegate (object sender, ResolveEventArgs args) { 
					called = true; 
					return null;
			};
			AppDomain.CurrentDomain.ResourceResolve += del;
			Stream st = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("NOT_EXISTING");
			Assert.IsTrue (called);
			AppDomain.CurrentDomain.ResourceResolve -= del;
		}			

		private static Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
		{
			return Assembly.ReflectionOnlyLoad(args.Name);
		}
#endif

		private static AppDomain CreateTestDomain (string baseDirectory, bool assemblyResolver)
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = baseDirectory;
			setup.ApplicationName = "testdomain";
			return CreateTestDomain (setup, assemblyResolver);
		}

		private static AppDomain CreateTestDomain (AppDomainSetup setup, bool assemblyResolver)
		{
			AppDomain ad = AppDomain.CreateDomain ("testdomain",
				AppDomain.CurrentDomain.Evidence, setup);

			if (assemblyResolver) {
				Assembly ea = Assembly.GetExecutingAssembly ();
				ad.CreateInstanceFrom (ea.CodeBase,
					typeof (AssemblyResolveHandler).FullName,
					false,
					BindingFlags.Public | BindingFlags.Instance,
					null,
					new object [] { ea.Location, ea.FullName },
					CultureInfo.InvariantCulture,
					null,
					null);
			}

			return ad;
		}


		private static CrossDomainTester CreateCrossDomainTester (AppDomain domain)
		{
			Type testerType = typeof (CrossDomainTester);
			return (CrossDomainTester) domain.CreateInstanceAndUnwrap (
				testerType.Assembly.FullName, testerType.FullName, false,
				BindingFlags.Public | BindingFlags.Instance, null, new object [0],
				CultureInfo.InvariantCulture, new object [0], null);
		}

		private static void GenerateAssembly (AssemblyName aname, string path)
		{
			AppDomain ad = CreateTestDomain (AppDomain.CurrentDomain.BaseDirectory,
				false);
			try {
				CrossDomainTester cdt = CreateCrossDomainTester (ad);
				cdt.GenerateAssembly (aname, path);
			} finally {
				AppDomain.Unload (ad);
			}
		}

		private bool RunningOnUnix {
			get {
				// check for Unix platforms - see FAQ for more details
				// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
				int platform = (int) Environment.OSVersion.Platform;
				return ((platform == 4) || (platform == 128) || (platform == 6));
			}
		}

		private class CrossDomainTester : MarshalByRefObject
		{
			public void GenerateAssembly (AssemblyName aname, string path)
			{
				AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
					aname, AssemblyBuilderAccess.Save, Path.GetDirectoryName (path));
				ab.Save (Path.GetFileName (path));
			}

			public int AssemblyCount {
				get {
					return AppDomain.CurrentDomain.GetAssemblies ().Length;
				}
			}

			public string GetApplicationBase ()
			{
				return AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
			}

			public string GetConfigurationFile ()
			{
				return AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			}

			public void Load (AssemblyName assemblyRef)
			{
				AppDomain.CurrentDomain.Load (assemblyRef);
			}

			public void LoadFrom (string assemblyFile)
			{
				Assembly.LoadFrom (assemblyFile);
			}

			public bool AssertLoad (AssemblyName assemblyRef)
			{
				try {
					AppDomain.CurrentDomain.Load (assemblyRef);
					return true;
				} catch {
					return false;
				}
			}

			public bool AssertLoad (string assemblyString)
			{
				try {
					AppDomain.CurrentDomain.Load (assemblyString);
					return true;
				} catch {
					return false;
				}
			}

			public bool AssertFileLoadException (AssemblyName assemblyRef)
			{
				try {
					AppDomain.CurrentDomain.Load (assemblyRef);
					return false;
				} catch (FileLoadException) {
					return true;
				}
			}

			public bool AssertFileNotFoundException (AssemblyName assemblyRef)
			{
				try {
					AppDomain.CurrentDomain.Load (assemblyRef);
					return false;
				} catch (FileNotFoundException) {
					return true;
				}
			}
		}

		[Serializable ()]
		private class AssemblyResolveHandler
		{
			public AssemblyResolveHandler (string assemblyFile, string assemblyName)
			{
				_assemblyFile = assemblyFile;
				_assemblyName = assemblyName;

				AppDomain.CurrentDomain.AssemblyResolve +=
					new ResolveEventHandler (ResolveAssembly);
			}

			private Assembly ResolveAssembly (object sender, ResolveEventArgs args)
			{
				if (args.Name == _assemblyName)
					return Assembly.LoadFrom (_assemblyFile);

				return null;
			}

			private readonly string _assemblyFile;
			private readonly string _assemblyName;
		}

		static byte [] keyPair = {
			0x07, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41,
			0x32, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x3D, 0xBD,
			0x72, 0x08, 0xC6, 0x2B, 0x0E, 0xA8, 0xC1, 0xC0, 0x58, 0x07, 0x2B,
			0x63, 0x5F, 0x7C, 0x9A, 0xBD, 0xCB, 0x22, 0xDB, 0x20, 0xB2, 0xA9,
			0xDA, 0xDA, 0xEF, 0xE8, 0x00, 0x64, 0x2F, 0x5D, 0x8D, 0xEB, 0x78,
			0x02, 0xF7, 0xA5, 0x36, 0x77, 0x28, 0xD7, 0x55, 0x8D, 0x14, 0x68,
			0xDB, 0xEB, 0x24, 0x09, 0xD0, 0x2B, 0x13, 0x1B, 0x92, 0x6E, 0x2E,
			0x59, 0x54, 0x4A, 0xAC, 0x18, 0xCF, 0xC9, 0x09, 0x02, 0x3F, 0x4F,
			0xA8, 0x3E, 0x94, 0x00, 0x1F, 0xC2, 0xF1, 0x1A, 0x27, 0x47, 0x7D,
			0x10, 0x84, 0xF5, 0x14, 0xB8, 0x61, 0x62, 0x1A, 0x0C, 0x66, 0xAB,
			0xD2, 0x4C, 0x4B, 0x9F, 0xC9, 0x0F, 0x3C, 0xD8, 0x92, 0x0F, 0xF5,
			0xFF, 0xCE, 0xD7, 0x6E, 0x5C, 0x6F, 0xB1, 0xF5, 0x7D, 0xD3, 0x56,
			0xF9, 0x67, 0x27, 0xA4, 0xA5, 0x48, 0x5B, 0x07, 0x93, 0x44, 0x00,
			0x4A, 0xF8, 0xFF, 0xA4, 0xCB, 0x73, 0xC0, 0x6A, 0x62, 0xB4, 0xB7,
			0xC8, 0x92, 0x58, 0x87, 0xCD, 0x07, 0x0C, 0x7D, 0x6C, 0xC1, 0x4A,
			0xFC, 0x82, 0x57, 0x0E, 0x43, 0x85, 0x09, 0x75, 0x98, 0x51, 0xBB,
			0x35, 0xF5, 0x64, 0x83, 0xC7, 0x79, 0x89, 0x5C, 0x55, 0x36, 0x66,
			0xAB, 0x27, 0xA4, 0xD9, 0xD4, 0x7E, 0x6B, 0x67, 0x64, 0xC1, 0x54,
			0x4E, 0x37, 0xF1, 0x4E, 0xCA, 0xB3, 0xE5, 0x63, 0x91, 0x57, 0x12,
			0x14, 0xA6, 0xEA, 0x8F, 0x8F, 0x2B, 0xFE, 0xF3, 0xE9, 0x16, 0x08,
			0x2B, 0x86, 0xBC, 0x26, 0x0D, 0xD0, 0xC6, 0xC4, 0x1A, 0x72, 0x43,
			0x76, 0xDC, 0xFF, 0x28, 0x52, 0xA1, 0xDE, 0x8D, 0xFA, 0xD5, 0x1F,
			0x0B, 0xB5, 0x4F, 0xAF, 0x06, 0x79, 0x11, 0xEE, 0xA8, 0xEC, 0xD3,
			0x74, 0x55, 0xA2, 0x80, 0xFC, 0xF8, 0xD9, 0x50, 0x69, 0x48, 0x01,
			0xC2, 0x5A, 0x04, 0x56, 0xB4, 0x3E, 0x24, 0x32, 0x20, 0xB5, 0x2C,
			0xDE, 0xBB, 0xBD, 0x13, 0xFD, 0x13, 0xF7, 0x03, 0x3E, 0xE3, 0x37,
			0x84, 0x74, 0xE7, 0xD0, 0x5E, 0x9E, 0xB6, 0x26, 0xAE, 0x6E, 0xB0,
			0x55, 0x6A, 0x52, 0x63, 0x6F, 0x5A, 0x9D, 0xF2, 0x67, 0xD6, 0x61,
			0x4F, 0x7A, 0x45, 0xEE, 0x5C, 0x3D, 0x2B, 0x7C, 0xB2, 0x40, 0x79,
			0x54, 0x84, 0xD1, 0xBE, 0x61, 0x3E, 0x5E, 0xD6, 0x18, 0x8E, 0x14,
			0x98, 0xFC, 0x35, 0xBF, 0x5F, 0x1A, 0x20, 0x2E, 0x1A, 0xD8, 0xFF,
			0xC4, 0x6B, 0xC0, 0xC9, 0x7D, 0x06, 0xEF, 0x09, 0xF9, 0xF3, 0x69,
			0xFC, 0xBC, 0xA2, 0xE6, 0x80, 0x22, 0xB9, 0x79, 0x7E, 0xEF, 0x57,
			0x9F, 0x49, 0xE1, 0xBC, 0x0D, 0xB6, 0xA1, 0xFE, 0x8D, 0xBC, 0xBB,
			0xA3, 0x05, 0x02, 0x6B, 0x04, 0x45, 0xF7, 0x5D, 0xEE, 0x43, 0x06,
			0xD6, 0x9C, 0x94, 0x48, 0x1A, 0x0B, 0x9C, 0xBC, 0xB4, 0x4E, 0x93,
			0x60, 0x87, 0xCD, 0x58, 0xD6, 0x9A, 0x39, 0xA6, 0xC0, 0x7F, 0x8E,
			0xFF, 0x25, 0xC1, 0xD7, 0x2C, 0xF6, 0xF4, 0x6F, 0x24, 0x52, 0x0B,
			0x39, 0x42, 0x1B, 0x0D, 0x04, 0xC1, 0x93, 0x2A, 0x19, 0x1C, 0xF0,
			0xB1, 0x9B, 0xC1, 0x24, 0x6D, 0x1B, 0x0B, 0xDA, 0x1C, 0x8B, 0x72,
			0x48, 0xF0, 0x3E, 0x52, 0xBF, 0x0A, 0x84, 0x3A, 0x9B, 0xC8, 0x6D,
			0x13, 0x1E, 0x72, 0xF4, 0x46, 0x93, 0x88, 0x1A, 0x5F, 0x4C, 0x3C,
			0xE5, 0x9D, 0x6E, 0xBB, 0x4E, 0xDD, 0x5D, 0x1F, 0x11, 0x40, 0xF4,
			0xD7, 0xAF, 0xB3, 0xAB, 0x9A, 0x99, 0x15, 0xF0, 0xDC, 0xAA, 0xFF,
			0x9F, 0x2D, 0x9E, 0x56, 0x4F, 0x35, 0x5B, 0xBA, 0x06, 0x99, 0xEA,
			0xC6, 0xB4, 0x48, 0x51, 0x17, 0x1E, 0xD1, 0x95, 0x84, 0x81, 0x18,
			0xC0, 0xF1, 0x71, 0xDE, 0x44, 0x42, 0x02, 0x06, 0xAC, 0x0E, 0xA8,
			0xE2, 0xF3, 0x1F, 0x96, 0x1F, 0xBE, 0xB6, 0x1F, 0xB5, 0x3E, 0xF6,
			0x81, 0x05, 0x20, 0xFA, 0x2E, 0x40, 0x2E, 0x4D, 0xA0, 0x0E, 0xDA,
			0x42, 0x9C, 0x05, 0xAA, 0x9E, 0xAF, 0x5C, 0xF7, 0x3A, 0x3F, 0xBB,
			0x91, 0x73, 0x45, 0x27, 0xA8, 0xA2, 0x07, 0x4A, 0xEF, 0x59, 0x1E,
			0x97, 0x9D, 0xE0, 0x30, 0x5A, 0x83, 0xCE, 0x1E, 0x57, 0x32, 0x89,
			0x43, 0x41, 0x28, 0x7D, 0x14, 0x8D, 0x8B, 0x41, 0x1A, 0x56, 0x76,
			0x43, 0xDB, 0x64, 0x86, 0x41, 0x64, 0x8D, 0x4C, 0x91, 0x83, 0x4E,
			0xF5, 0x6C };

		static byte [] publicKey2 = {
			0x07, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41,
			0x32, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x7F, 0x7C,
			0xEA, 0x4A, 0x28, 0x33, 0xD8, 0x3C, 0x86, 0x90, 0x86, 0x91, 0x11,
			0xBB, 0x30, 0x0D, 0x3D, 0x69, 0x04, 0x4C, 0x48, 0xF5, 0x4F, 0xE7,
			0x64, 0xA5, 0x82, 0x72, 0x5A, 0x92, 0xC4, 0x3D, 0xC5, 0x90, 0x93,
			0x41, 0xC9, 0x1D, 0x34, 0x16, 0x72, 0x2B, 0x85, 0xC1, 0xF3, 0x99,
			0x62, 0x07, 0x32, 0x98, 0xB7, 0xE4, 0xFA, 0x75, 0x81, 0x8D, 0x08,
			0xB9, 0xFD, 0xDB, 0x00, 0x25, 0x30, 0xC4, 0x89, 0x13, 0xB6, 0x43,
			0xE8, 0xCC, 0xBE, 0x03, 0x2E, 0x1A, 0x6A, 0x4D, 0x36, 0xB1, 0xEB,
			0x49, 0x26, 0x6C, 0xAB, 0xC4, 0x29, 0xD7, 0x8F, 0x25, 0x11, 0xA4,
			0x7C, 0x81, 0x61, 0x97, 0xCB, 0x44, 0x2D, 0x80, 0x49, 0x93, 0x48,
			0xA7, 0xC9, 0xAB, 0xDB, 0xCF, 0xA3, 0x34, 0xCB, 0x6B, 0x86, 0xE0,
			0x4D, 0x27, 0xFC, 0xA7, 0x4F, 0x36, 0xCA, 0x13, 0x42, 0xD3, 0x83,
			0xC4, 0x06, 0x6E, 0x12, 0xE0, 0xA1, 0x3D, 0x9F, 0xA9, 0xEC, 0xD1,
			0xC6, 0x08, 0x1B, 0x3D, 0xF5, 0xDB, 0x4C, 0xD4, 0xF0, 0x2C, 0xAA,
			0xFC, 0xBA, 0x18, 0x6F, 0x48, 0x7E, 0xB9, 0x47, 0x68, 0x2E, 0xF6,
			0x1E, 0x67, 0x1C, 0x7E, 0x0A, 0xCE, 0x10, 0x07, 0xC0, 0x0C, 0xAD,
			0x5E, 0xC1, 0x53, 0x70, 0xD5, 0xE7, 0x25, 0xCA, 0x37, 0x5E, 0x49,
			0x59, 0xD0, 0x67, 0x2A, 0xBE, 0x92, 0x36, 0x86, 0x8A, 0xBF, 0x3E,
			0x17, 0x04, 0xFB, 0x1F, 0x46, 0xC8, 0x10, 0x5C, 0x93, 0x02, 0x43,
			0x14, 0x96, 0x6A, 0xD9, 0x87, 0x17, 0x62, 0x7D, 0x3A, 0x45, 0xBE,
			0x35, 0xDE, 0x75, 0x0B, 0x2A, 0xCE, 0x7D, 0xF3, 0x19, 0x85, 0x4B,
			0x0D, 0x6F, 0x8D, 0x15, 0xA3, 0x60, 0x61, 0x28, 0x55, 0x46, 0xCE,
			0x78, 0x31, 0x04, 0x18, 0x3C, 0x56, 0x4A, 0x3F, 0xA4, 0xC9, 0xB1,
			0x41, 0xED, 0x22, 0x80, 0xA1, 0xB3, 0xE2, 0xC7, 0x1B, 0x62, 0x85,
			0xE4, 0x81, 0x39, 0xCB, 0x1F, 0x95, 0xCC, 0x61, 0x61, 0xDF, 0xDE,
			0xF3, 0x05, 0x68, 0xB9, 0x7D, 0x4F, 0xFF, 0xF3, 0xC0, 0x0A, 0x25,
			0x62, 0xD9, 0x8A, 0x8A, 0x9E, 0x99, 0x0B, 0xFB, 0x85, 0x27, 0x8D,
			0xF6, 0xD4, 0xE1, 0xB9, 0xDE, 0xB4, 0x16, 0xBD, 0xDF, 0x6A, 0x25,
			0x9C, 0xAC, 0xCD, 0x91, 0xF7, 0xCB, 0xC1, 0x81, 0x22, 0x0D, 0xF4,
			0x7E, 0xEC, 0x0C, 0x84, 0x13, 0x5A, 0x74, 0x59, 0x3F, 0x3E, 0x61,
			0x00, 0xD6, 0xB5, 0x4A, 0xA1, 0x04, 0xB5, 0xA7, 0x1C, 0x29, 0xD0,
			0xE1, 0x11, 0x19, 0xD7, 0x80, 0x5C, 0xEE, 0x08, 0x15, 0xEB, 0xC9,
			0xA8, 0x98, 0xF5, 0xA0, 0xF0, 0x92, 0x2A, 0xB0, 0xD3, 0xC7, 0x8C,
			0x8D, 0xBB, 0x88, 0x96, 0x4F, 0x18, 0xF0, 0x8A, 0xF9, 0x31, 0x9E,
			0x44, 0x94, 0x75, 0x6F, 0x78, 0x04, 0x10, 0xEC, 0xF3, 0xB0, 0xCE,
			0xA0, 0xBE, 0x7B, 0x25, 0xE1, 0xF7, 0x8A, 0xA8, 0xD4, 0x63, 0xC2,
			0x65, 0x47, 0xCC, 0x5C, 0xED, 0x7D, 0x8B, 0x07, 0x4D, 0x76, 0x29,
			0x53, 0xAC, 0x27, 0x8F, 0x5D, 0x78, 0x56, 0xFA, 0x99, 0x45, 0xA2,
			0xCC, 0x65, 0xC4, 0x54, 0x13, 0x9F, 0x38, 0x41, 0x7A, 0x61, 0x0E,
			0x0D, 0x34, 0xBC, 0x11, 0xAF, 0xE2, 0xF1, 0x8B, 0xFA, 0x2B, 0x54,
			0x6C, 0xA3, 0x6C, 0x09, 0x1F, 0x0B, 0x43, 0x9B, 0x07, 0x95, 0x83,
			0x3F, 0x97, 0x99, 0x89, 0xF5, 0x51, 0x41, 0xF6, 0x8E, 0x5D, 0xEF,
			0x6D, 0x24, 0x71, 0x41, 0x7A, 0xAF, 0xBE, 0x81, 0x71, 0xAB, 0x76,
			0x2F, 0x1A, 0x5A, 0xBA, 0xF3, 0xA6, 0x65, 0x7A, 0x80, 0x50, 0xCE,
			0x23, 0xC3, 0xC7, 0x53, 0xB0, 0x7C, 0x97, 0x77, 0x27, 0x70, 0x98,
			0xAE, 0xB5, 0x24, 0x66, 0xE1, 0x60, 0x39, 0x41, 0xDA, 0x54, 0x01,
			0x64, 0xFB, 0x10, 0x33, 0xCE, 0x8B, 0xBE, 0x27, 0xD4, 0x21, 0x57,
			0xCC, 0x0F, 0x1A, 0xC1, 0x3D, 0xF3, 0xCC, 0x39, 0xF0, 0x2F, 0xAE,
			0xF1, 0xC0, 0xCD, 0x3B, 0x23, 0x87, 0x49, 0x7E, 0x40, 0x32, 0x6A,
			0xD3, 0x96, 0x4A, 0xE5, 0x5E, 0x6E, 0x26, 0xFD, 0x8A, 0xCF, 0x7E,
			0xFC, 0x37, 0xDE, 0x39, 0x0C, 0x53, 0x81, 0x75, 0x08, 0xAF, 0x6B,
			0x39, 0x6C, 0xFB, 0xC9, 0x79, 0xC0, 0x9B, 0x5F, 0x34, 0x86, 0xB2,
			0xDE, 0xC4, 0x19, 0x84, 0x5F, 0x0E, 0xED, 0x9B, 0xB8, 0xD3, 0x17,
			0xDA, 0x78 };

		static byte [] publicKey = {
			0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00,
			0x00, 0x06, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53,
			0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x3d,
			0xbd, 0x72, 0x08, 0xc6, 0x2b, 0x0e, 0xa8, 0xc1, 0xc0, 0x58, 0x07,
			0x2b, 0x63, 0x5f, 0x7c, 0x9a, 0xbd, 0xcb, 0x22, 0xdb, 0x20, 0xb2,
			0xa9, 0xda, 0xda, 0xef, 0xe8, 0x00, 0x64, 0x2f, 0x5d, 0x8d, 0xeb,
			0x78, 0x02, 0xf7, 0xa5, 0x36, 0x77, 0x28, 0xd7, 0x55, 0x8d, 0x14,
			0x68, 0xdb, 0xeb, 0x24, 0x09, 0xd0, 0x2b, 0x13, 0x1b, 0x92, 0x6e,
			0x2e, 0x59, 0x54, 0x4a, 0xac, 0x18, 0xcf, 0xc9, 0x09, 0x02, 0x3f,
			0x4f, 0xa8, 0x3e, 0x94, 0x00, 0x1f, 0xc2, 0xf1, 0x1a, 0x27, 0x47,
			0x7d, 0x10, 0x84, 0xf5, 0x14, 0xb8, 0x61, 0x62, 0x1a, 0x0c, 0x66,
			0xab, 0xd2, 0x4c, 0x4b, 0x9f, 0xc9, 0x0f, 0x3c, 0xd8, 0x92, 0x0f,
			0xf5, 0xff, 0xce, 0xd7, 0x6e, 0x5c, 0x6f, 0xb1, 0xf5, 0x7d, 0xd3,
			0x56, 0xf9, 0x67, 0x27, 0xa4, 0xa5, 0x48, 0x5b, 0x07, 0x93, 0x44,
			0x00, 0x4a, 0xf8, 0xff, 0xa4, 0xcb };

		static byte [] pk_token = { 0xce, 0x52, 0x76, 0xd8, 0x68, 0x7e, 0Xc6, 0xdc };
	}
}
