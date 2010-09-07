//
// EnvironmentCas.cs - CAS unit tests for System.Environment
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using NUnit.Framework;

using System;
using System.Collections;
using System.IO;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System {

	[TestFixture]
	[Category ("CAS")]
	public class EnvironmentCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// Partial Trust Tests - i.e. call "normal" unit with reduced privileges

		[Test]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PartialTrust_PermitOnly_Environment ()
		{
			MonoTests.System.EnvironmentTest et = new MonoTests.System.EnvironmentTest ();
			// call most (all but arguments checking) unit tests from EnvironmentTest
			et.ExpandEnvironmentVariables_UnknownVariable ();
			et.ExpandEnvironmentVariables_KnownVariable ();
			et.ExpandEnvironmentVariables_NotVariable ();
			et.ExpandEnvironmentVariables_Alone ();
			et.ExpandEnvironmentVariables_End ();
			et.ExpandEnvironmentVariables_None ();
			et.ExpandEnvironmentVariables_EmptyVariable ();
			et.ExpandEnvironmentVariables_Double ();
			et.ExpandEnvironmentVariables_ComplexExpandable ();
			et.ExpandEnvironmentVariables_ExpandableAndNonExpandable ();
			et.ExpandEnvironmentVariables_ExpandableWithTrailingPercent ();
			et.ExpandEnvironmentVariables_ComplexExpandable2 ();
			et.GetEnvironmentVariables ();
			et.GetCommandLineArgs ();
		}		

		// test Demand by denying it's caller from the required privileges

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "PATH")]
		[ExpectedException (typeof (SecurityException))]
		public void CommandLine ()
		{
			// now that the stack is set, call the method
			string s = Environment.CommandLine;
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Unrestricted = true)]
		public void CurrentDirectory_EnvironmentPermission ()
		{
			// now that the stack is set, call the methods
			string cd = Environment.CurrentDirectory;
			Environment.CurrentDirectory = cd;
			// nothing to do with EnvironmentPermission
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CurrentDirectory_SecurityPermission ()
		{
			string cd = null;
			try {
				cd = Environment.CurrentDirectory;
			}
			catch (SecurityException) {
				// this isn't the part that should fail
				Assert.Fail ("shouldn't fail when getting current dir");
			}
			// now that the stack is set, call the method
			Environment.CurrentDirectory = cd;
		}

#if !RUN_ONDOTNET || NET_4_0 // Disabled because .net 2 fails to load dll with "Failure decoding embedded permission set object" due to "/" path

		[Test]
		[FileIOPermission (SecurityAction.Deny, PathDiscovery = "/")]
		[ExpectedException (typeof (SecurityException))]
		public void CurrentDirectory_Get_FileIOPermission ()
		{
			// now that the stack is set, call the method
			string cd = Environment.CurrentDirectory;
		}
		
		[Test]
		[FileIOPermission (SecurityAction.Deny, PathDiscovery = "/")]
		[ExpectedException (typeof (SecurityException))]
		public void GetFolderPath ()
		{
			// now that the stack is set, call the method
			string s = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
		}		
#endif

		[Test]
		public void CurrentDirectory_Set_FileIOPermission ()
		{
			// this test will change the current directory
			// and cause tests failures elsewhere...
			string cd = Environment.CurrentDirectory;
			try {
				CurrentDirectory_Set_FileIOPermission_Restricted ();
			}
			finally {
				// ... unless we return to the original directory
				Environment.CurrentDirectory = cd;
			}
		}

		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		private void CurrentDirectory_Set_FileIOPermission_Restricted ()
		{
			// now that the stack is set, call the method
#if RUN_ONDOTNET
			Environment.CurrentDirectory = "C:\\";
#else
			Environment.CurrentDirectory = "/";
#endif
			// no rights are required (as we already know the path)
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Exit ()
		{
			// now that the stack is set, call the method
			Environment.Exit (1);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void ExitCode ()
		{
			// now that the stack is set, call the method
			int ec = Environment.ExitCode;
			Environment.ExitCode = ec;
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "PATH")]
		[ExpectedException (typeof (SecurityException))]
		public void ExpandEnvironmentVariables ()
		{
			// now that the stack is set, call the method
			string s = Environment.ExpandEnvironmentVariables ("%PATH%");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[Ignore ("Protected by a LinkDemand, will throw an ExecutionEngineException to stop process")]
		public void FailFast ()
		{
			// now that the stack is set, call the method
			Environment.FailFast ("bye bye");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "PATH")]
		[ExpectedException (typeof (SecurityException))]
		public void GetCommandLineArgs ()
		{
			// now that the stack is set, call the method
			string[] s = Environment.GetCommandLineArgs ();
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "PATH")]
		[ExpectedException (typeof (SecurityException))]
		public void GetEnvironmentVariable ()
		{
			// now that the stack is set, call the method
			string s = Environment.GetEnvironmentVariable ("PATH");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "MONO")]
		public void GetEnvironmentVariable_Target_Process ()
		{
			// now that the stack is set, call the method
			string s = Environment.GetEnvironmentVariable ("PATH",
				EnvironmentVariableTarget.Process);
			// it doesn't takes Unrestricted access to read from 
			// Process environment variables (like older API)
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void GetEnvironmentVariable_Target ()
		{
			// now that the stack is set, call the method
			string s = Environment.GetEnvironmentVariable ("PATH",
				EnvironmentVariableTarget.Machine);
			// it takes Unrestricted access to read from Machine
			// and User environment variables
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Write = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void GetEnvironmentVariable_Target2 ()
		{
			// note that be removing write access we're no longer
			// unrestricted, so this should fail even for a read
			string s = Environment.GetEnvironmentVariable ("PATH",
				EnvironmentVariableTarget.Machine);
			// it takes Unrestricted access to read from Machine
			// and User environment variables
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "PATH")]
		[ExpectedException (typeof (SecurityException))]
		public void GetEnvironmentVariables ()
		{
			// note that whis wouldn't fail if no PATH variable
			// was part of the environment variables
			IDictionary d = Environment.GetEnvironmentVariables ();
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "PLEASE_NEVER_DEFINE_THIS_VARIABLE")]
		// Before 2.0 this required Unrestricted EnvironmentPermission
		[ExpectedException (typeof (SecurityException))]
		public void GetEnvironmentVariables_Pass ()
		{
			// this will work as long as PLEASE_NEVER_DEFINE_THIS_VARIABLE
			// isn't an environment variable
			IDictionary d = Environment.GetEnvironmentVariables ();
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "PATH")]
		[ExpectedException (typeof (SecurityException))]
		public void GetEnvironmentVariables_Target ()
		{
			// now that the stack is set, call the method
			IDictionary d = Environment.GetEnvironmentVariables (
				EnvironmentVariableTarget.Machine);
			// it takes Unrestricted access to read from Machine
			// and User environment variables
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Write = "PATH")]
		[ExpectedException (typeof (SecurityException))]
		public void GetEnvironmentVariables_Target2 ()
		{
			// note that be removing write access we're no longer
			// unrestricted, so this should fail even for a read
			IDictionary d = Environment.GetEnvironmentVariables (
				EnvironmentVariableTarget.Machine);
			// it takes Unrestricted access to read from Machine
			// and User environment variables
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Unrestricted = true)]
		public void GetFolderPath_EnvironmentPermission ()
		{
			// we can get all folders without any EnvironmentPermission
			// note: Mono use some environment variable to create the paths
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "MyDocuments");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.Desktop), "Desktop");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.MyComputer), "MyComputer");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Personal");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.Favorites), "Favorites");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.Startup), "Startup");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.Recent), "Recent");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.SendTo), "SendTo");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.StartMenu), "StartMenu");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.MyMusic), "MyMusic");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory), "DesktopDirectory");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.Templates), "Templates");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "ApplicationData");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "LocalApplicationData");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.InternetCache), "InternetCache");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.Cookies), "Cookies");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.History), "History");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.CommonApplicationData), "CommonApplicationData");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.System), "System");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.ProgramFiles), "ProgramFiles");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.MyPictures), "MyPictures");
			Assert.IsNotNull (Environment.GetFolderPath (Environment.SpecialFolder.CommonProgramFiles), "CommonProgramFiles");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Write = "PATH")]
		[ExpectedException (typeof (SecurityException))]
		public void GetLogicalDrives ()
		{
			// note that be removing write access we're no longer
			// unrestricted, so this should fail even if it's not
			// related to environment variables at all!
			string[] s = Environment.GetLogicalDrives ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void HasShutdownStarted ()
		{
			// now that the stack is set, call the methods
			bool b = Environment.HasShutdownStarted;
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "COMPUTERNAME")]
		[ExpectedException (typeof (SecurityException))]
		public void MachineName_EnvironmentPermission ()
		{
			// now that the stack is set, call the method
			string s = Environment.MachineName;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode =true)]
		[ExpectedException (typeof (SecurityException))]
		public void MachineName_SecurityPermission ()
		{
			// now that the stack is set, call the method
			string s = Environment.MachineName;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void NewLine ()
		{
			// now that the stack is set, call the methods
			string s = Environment.NewLine;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void OSVersion ()
		{
			// now that the stack is set, call the methods
			OperatingSystem os = Environment.OSVersion;
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "NUMBER_OF_PROCESSORS")]
		[ExpectedException (typeof (SecurityException))]
		public void ProcessorCount ()
		{
			// now that the stack is set, call the methods
			int i = Environment.ProcessorCount;
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Write = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void SetEnvironmentVariable ()
		{
			// now that the stack is set, call the method
			Environment.SetEnvironmentVariable ("MONO", "GO");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Write = "PATH")]
		[ExpectedException (typeof (SecurityException))]
		public void SetEnvironmentVariable2 ()
		{
			// Note: strangely (but documented as such) it takes
			// unrestricted access to call SetEnvironmentVariable
			// so denying write to PATH also deny write to any
			// other environment variable, like the MONO variable
			Environment.SetEnvironmentVariable ("MONO", "GO");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Write = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void SetEnvironmentVariable_Target ()
		{
			// now that the stack is set, call the method
			Environment.SetEnvironmentVariable ("MONO", "GO",
				EnvironmentVariableTarget.Machine);
			// it takes Unrestricted access to read from Machine
			// and User environment variables
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void SetEnvironmentVariable_Target2 ()
		{
			// note that be removing read access we're no longer
			// unrestricted, so this should fail even for a write
			Environment.SetEnvironmentVariable ("MONO", "GO",
				EnvironmentVariableTarget.Machine);
			// it takes Unrestricted access to read from Machine
			// and User environment variables
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Write = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void StackTrace ()
		{
			// note that be removing write access we're no longer
			// unrestricted, so this should fail even if the call
			// stack isn't related to writing in environment variables
			string s = Environment.StackTrace;
		}

		[Test]
#if false && RUN_ONDOTNET
		[FileIOPermission (SecurityAction.Deny, PathDiscovery = "C:\\")]
		[ExpectedException (typeof (SecurityException))]
#endif
		public void SystemDirectory ()
		{
			// now that the stack is set, call the method
			string s = Environment.SystemDirectory;
			// note: Under Linux SystemDirectory is empty (so it's not a path)
			Assert.AreEqual (String.Empty, s);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void TickCount ()
		{
			// now that the stack is set, call the methods
			int i = Environment.TickCount;
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERDOMAINNAME")]
		[ExpectedException (typeof (SecurityException))]
		public void UserDomainName ()
		{
			// now that the stack is set, call the methods
			string s = Environment.UserDomainName;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void UserInteractive ()
		{
			// now that the stack is set, call the methods
			bool b = Environment.UserInteractive;
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		[ExpectedException (typeof (SecurityException))]
		public void UserName ()
		{
			// now that the stack is set, call the methods
			string s = Environment.UserName;
			// note: the UserName property doesn't really read the 
			// USERNAME variable. You can test this by impersonating
			// another user (see unit tests)
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Version ()
		{
			// now that the stack is set, call the methods
			Version v = Environment.Version;
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		[ExpectedException (typeof (SecurityException))]
		public void WorkingSet ()
		{
			// note that be removing read access to USERNAME we're 
			// no longer unrestricted, so this should fail even if
			// the working set is unrelated to the username
			long l = Environment.WorkingSet;
		}
	}
}
