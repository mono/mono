//
// ProcessTest.cs - NUnit Test Cases for System.Diagnostics.Process
//
// Authors:
//   Gert Driesen (drieseng@users.sourceforge.net)
//   Robert Jordan <robertj@gmx.net>
//
// (C) 2007 Gert Driesen
// 

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class ProcessTest
	{
		static bool RunningOnUnix {
			get {
				int p = (int)Environment.OSVersion.Platform;
				return ((p == 128) || (p == 4) || (p == 6));
			}
		}

		[Test]
		public void GetProcessById_MachineName_Null ()
		{
			try {
				Process.GetProcessById (1, (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNotNull (ex.ParamName, "#4");
				Assert.AreEqual ("machineName", ex.ParamName, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
		}

		[Test]
		public void GetProcesses_MachineName_Null ()
		{
			try {
				Process.GetProcesses ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNotNull (ex.ParamName, "#4");
				Assert.AreEqual ("machineName", ex.ParamName, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
		}

		[Test] // Covers #26363
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void GetProcesses_StartTime ()
		{
			foreach (var p in Process.GetProcesses ()) {
				if (!p.HasExited && p.StartTime.Year < 1800)
					Assert.Fail ("Process should not be started since the 18th century.");
			}
		}

		[Test]
		public void PriorityClass_NotStarted ()
		{
			Process process = new Process ();
			try {
				process.PriorityClass = ProcessPriorityClass.Normal;
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// No process is associated with this object
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				Assert.Fail ("#B1:" + process.PriorityClass);
			} catch (InvalidOperationException ex) {
				// No process is associated with this object
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void PriorityClass_Invalid ()
		{
			Process process = new Process ();
			try {
				process.PriorityClass = (ProcessPriorityClass) 666;
				Assert.Fail ("#1");
			} catch (InvalidEnumArgumentException ex) {
				Assert.AreEqual (typeof (InvalidEnumArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("666") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (ProcessPriorityClass).Name) != -1, "#6");
				Assert.IsNotNull (ex.ParamName, "#7");
				Assert.AreEqual ("value", ex.ParamName, "#8");
			}
		}

#if MONO_FEATURE_PROCESS_START
		[Test] // Start ()
		public void Start1_FileName_Empty ()
		{
			Process process = new Process ();
			process.StartInfo = new ProcessStartInfo (string.Empty);

			// no shell
			process.StartInfo.UseShellExecute = false;
			try {
				process.Start ();
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Cannot start process because a file name has
				// not been provided
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			// shell
			process.StartInfo.UseShellExecute = true;
			try {
				process.Start ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Cannot start process because a file name has
				// not been provided
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // Start ()
		public void Start1_FileName_InvalidPathCharacters ()
		{
			if (RunningOnUnix)
				// on unix, all characters are allowed
				Assert.Ignore ("Running on Unix.");

			string systemDir = Environment.GetFolderPath (Environment.SpecialFolder.System);
			string exe = "\"" + Path.Combine (systemDir, "calc.exe") + "\"";

			Process process = new Process ();
			process.StartInfo = new ProcessStartInfo (exe);

			// no shell
			process.StartInfo.UseShellExecute = false;
			Assert.IsTrue (process.Start ());
			process.Kill ();

			// shell
			process.StartInfo.UseShellExecute = true;
			Assert.IsTrue (process.Start ());
			process.Kill ();
		}

		[Test] // Start ()
		public void Start1_FileName_NotFound ()
		{
			Process process = new Process ();
			string exe = RunningOnUnix ? exe = "/usr/bin/shouldnoteverexist"
				: @"c:\shouldnoteverexist.exe";

			// absolute path, no shell
			process.StartInfo = new ProcessStartInfo (exe);
			process.StartInfo.UseShellExecute = false;
			try {
				process.Start ();
				Assert.Fail ("#A1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#A2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
				Assert.IsNotNull (ex.Message, "#A5");
				Assert.AreEqual (2, ex.NativeErrorCode, "#A6");
			}

			// relative path, no shell
			process.StartInfo.FileName = "shouldnoteverexist.exe";
			process.StartInfo.UseShellExecute = false;
			try {
				process.Start ();
				Assert.Fail ("#B1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#B2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#B3");
				Assert.IsNull (ex.InnerException, "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
				Assert.AreEqual (2, ex.NativeErrorCode, "#B6");
			}

			// absolute path, shell
			process.StartInfo.FileName = exe;
			process.StartInfo.UseShellExecute = true;
			try {
				process.Start ();
				Assert.Fail ("#C1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#C2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#C3");
				Assert.IsNull (ex.InnerException, "#C4");
				Assert.IsNotNull (ex.Message, "#C5");
				Assert.AreEqual (2, ex.NativeErrorCode, "#C6");
			}

			// relative path, shell
			process.StartInfo.FileName = "shouldnoteverexist.exe";
			process.StartInfo.UseShellExecute = true;
			try {
				process.Start ();
				Assert.Fail ("#D1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#D2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#D3");
				Assert.IsNull (ex.InnerException, "#D4");
				Assert.IsNotNull (ex.Message, "#D5");
				Assert.AreEqual (2, ex.NativeErrorCode, "#D6");
			}
		}

		[Test] // Start ()
		public void Start1_FileName_Null ()
		{
			Process process = new Process ();
			process.StartInfo = new ProcessStartInfo ((string) null);

			// no shell
			process.StartInfo.UseShellExecute = false;
			try {
				process.Start ();
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Cannot start process because a file name has
				// not been provided
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			// shell
			process.StartInfo.UseShellExecute = true;
			try {
				process.Start ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Cannot start process because a file name has
				// not been provided
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // Start ()
		public void Start1_FileName_Whitespace ()
		{
			Process process = new Process ();
			process.StartInfo = new ProcessStartInfo (" ");
			process.StartInfo.UseShellExecute = false;
			try {
				process.Start ();
				Assert.Fail ("#1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				// TODO: On windows we get ACCESS_DENIED (5) instead of FILE_NOT_FOUND (2) and .NET
				// gives ERROR_INVALID_PARAMETER (87). See https://bugzilla.xamarin.com/show_bug.cgi?id=44514
				Assert.IsTrue (ex.NativeErrorCode == 2 || ex.NativeErrorCode == 5 || ex.NativeErrorCode == 87, "#6");
			}
		}

		[Test] // Start (ProcessStartInfo)
		public void Start2_FileName_Empty ()
		{
			ProcessStartInfo startInfo = new ProcessStartInfo (string.Empty);

			// no shell
			startInfo.UseShellExecute = false;
			try {
				Process.Start (startInfo);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Cannot start process because a file name has
				// not been provided
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			// shell
			startInfo.UseShellExecute = true;
			try {
				Process.Start (startInfo);
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Cannot start process because a file name has
				// not been provided
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // Start (ProcessStartInfo)
		public void Start2_FileName_NotFound ()
		{
			ProcessStartInfo startInfo = new ProcessStartInfo ();
			string exe = RunningOnUnix ? exe = "/usr/bin/shouldnoteverexist"
				: @"c:\shouldnoteverexist.exe";

			// absolute path, no shell
			startInfo.FileName = exe;
			startInfo.UseShellExecute = false;
			try {
				Process.Start (startInfo);
				Assert.Fail ("#A1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#A2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
				Assert.IsNotNull (ex.Message, "#A5");
				Assert.AreEqual (2, ex.NativeErrorCode, "#A6");
			}

			// relative path, no shell
			startInfo.FileName = "shouldnoteverexist.exe";
			startInfo.UseShellExecute = false;
			try {
				Process.Start (startInfo);
				Assert.Fail ("#B1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#B2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#B3");
				Assert.IsNull (ex.InnerException, "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
				Assert.AreEqual (2, ex.NativeErrorCode, "#B6");
			}

			if (RunningOnUnix)
				Assert.Ignore ("On Unix and Mac OS X, we try " +
					"to open any file (using xdg-open, ...)" +
					" and we do not report an exception " +
					"if this fails.");

			// absolute path, shell
			startInfo.FileName = exe;
			startInfo.UseShellExecute = true;
			try {
				Process.Start (startInfo);
				Assert.Fail ("#C1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#C2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#C3");
				Assert.IsNull (ex.InnerException, "#C4");
				Assert.IsNotNull (ex.Message, "#C5");
				Assert.AreEqual (2, ex.NativeErrorCode, "#C6");
			}

			// relative path, shell
			startInfo.FileName = "shouldnoteverexist.exe";
			startInfo.UseShellExecute = true;
			try {
				Process.Start (startInfo);
				Assert.Fail ("#D1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#D2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#D3");
				Assert.IsNull (ex.InnerException, "#D4");
				Assert.IsNotNull (ex.Message, "#D5");
				Assert.AreEqual (2, ex.NativeErrorCode, "#D6");
			}
		}

		[Test] // Start (ProcessStartInfo)
		public void Start2_FileName_Null ()
		{
			ProcessStartInfo startInfo = new ProcessStartInfo ((string) null);

			// no shell
			startInfo.UseShellExecute = false;
			try {
				Process.Start (startInfo);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Cannot start process because a file name has
				// not been provided
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			// shell
			startInfo.UseShellExecute = true;
			try {
				Process.Start (startInfo);
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Cannot start process because a file name has
				// not been provided
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // Start (ProcessStartInfo)
		public void Start2_FileName_Whitespace ()
		{
			ProcessStartInfo startInfo = new ProcessStartInfo (" ");
			startInfo.UseShellExecute = false;
			try {
				Process.Start (startInfo);
				Assert.Fail ("#1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				// TODO: On windows we get ACCESS_DENIED (5) instead of FILE_NOT_FOUND (2) and .NET
				// gives ERROR_INVALID_PARAMETER (87). See https://bugzilla.xamarin.com/show_bug.cgi?id=44514
				Assert.IsTrue (ex.NativeErrorCode == 2 || ex.NativeErrorCode == 5 || ex.NativeErrorCode == 87, "#6");
			}
		}

		[Test] // Start (ProcessStartInfo)
		public void Start2_StartInfo_Null ()
		{
			try {
				Process.Start ((ProcessStartInfo) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("startInfo", ex.ParamName, "#6");
			}
		}

		[Test] // Start (string)
		public void Start3_FileName_Empty ()
		{
			try {
				Process.Start (string.Empty);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot start process because a file name has
				// not been provided
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // Start (string)
		public void Start3_FileName_NotFound ()
		{
			if (RunningOnUnix)
				Assert.Ignore ("On Unix and Mac OS X, we try " +
					"to open any file (using xdg-open, ...)" +
					" and we do not report an exception " +
					"if this fails.");

			string exe = @"c:\shouldnoteverexist.exe";

			// absolute path, no shell
			try {
				Process.Start (exe);
				Assert.Fail ("#A1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#A2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
				Assert.IsNotNull (ex.Message, "#A5");
				Assert.AreEqual (2, ex.NativeErrorCode, "#A6");
			}

			// relative path, no shell
			try {
				Process.Start ("shouldnoteverexist.exe");
				Assert.Fail ("#B1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#B2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#B3");
				Assert.IsNull (ex.InnerException, "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
				Assert.AreEqual (2, ex.NativeErrorCode, "#B6");
			}
		}

		[Test] // Start (string)
		public void Start3_FileName_Null ()
		{
			try {
				Process.Start ((string) null);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot start process because a file name has
				// not been provided
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // Start (string, string)
		public void Start4_Arguments_Null ()
		{
			if (RunningOnUnix)
				Assert.Ignore ("On Unix and Mac OS X, we try " +
					"to open any file (using xdg-open, ...)" +
					" and we do not report an exception " +
					"if this fails.");

			string exe = @"c:\shouldnoteverexist.exe";

			try {
				Process.Start ("whatever.exe", (string) null);
				Assert.Fail ("#1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#B2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#B3");
				Assert.IsNull (ex.InnerException, "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
				Assert.AreEqual (2, ex.NativeErrorCode, "#B6");
			}
		}

		[Test] // Start (string, string)
		public void Start4_FileName_Empty ()
		{
			try {
				Process.Start (string.Empty, string.Empty);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot start process because a file name has
				// not been provided
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // Start (string, string)
		public void Start4_FileName_NotFound ()
		{
			if (RunningOnUnix)
				Assert.Ignore ("On Unix and Mac OS X, we try " +
					"to open any file (using xdg-open, ...)" +
					" and we do not report an exception " +
					"if this fails.");

			string exe = @"c:\shouldnoteverexist.exe";

			// absolute path, no shell
			try {
				Process.Start (exe, string.Empty);
				Assert.Fail ("#A1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#A2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
				Assert.IsNotNull (ex.Message, "#A5");
				Assert.AreEqual (2, ex.NativeErrorCode, "#A6");
			}

			// relative path, no shell
			try {
				Process.Start ("shouldnoteverexist.exe", string.Empty);
				Assert.Fail ("#B1");
			} catch (Win32Exception ex) {
				// The system cannot find the file specified
				Assert.AreEqual (typeof (Win32Exception), ex.GetType (), "#B2");
				Assert.AreEqual (-2147467259, ex.ErrorCode, "#B3");
				Assert.IsNull (ex.InnerException, "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
				Assert.AreEqual (2, ex.NativeErrorCode, "#B6");
			}
		}

		[Test]
		public void Start_UseShellExecuteWithEmptyUserName ()
		{
			if (RunningOnUnix)
				Assert.Ignore ("On Unix and Mac OS X, we try " +
					"to open any file (using xdg-open, ...)" +
					" and we do not report an exception " +
					"if this fails.");

			string exe = @"c:\shouldnoteverexist.exe";

			try {
				Process p = new Process ();
				p.StartInfo.FileName = exe;
				p.StartInfo.UseShellExecute = true;
				p.StartInfo.UserName = "";
				p.Start ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
				Assert.Fail ("#2");
			} catch (Win32Exception) {
			}

			try {
				Process p = new Process ();
				p.StartInfo.FileName = exe;
				p.StartInfo.UseShellExecute = true;
				p.StartInfo.UserName = null;
				p.Start ();
				Assert.Fail ("#3");				
			} catch (InvalidOperationException) {
				Assert.Fail ("#4");
			} catch (Win32Exception) {
			}
		}
		
		[Test] // Start (string, string)
		public void Start4_FileName_Null ()
		{
			try {
				Process.Start ((string) null, string.Empty);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot start process because a file name has
				// not been provided
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void StartInfo ()
		{
			ProcessStartInfo startInfo = new ProcessStartInfo ();

			Process p = new Process ();
			Assert.IsNotNull (p.StartInfo, "#A1");
			p.StartInfo = startInfo;
			Assert.AreSame (startInfo, p.StartInfo, "#A2");
		}

		[Test]
		public void StartInfo_Null ()
		{
			Process p = new Process ();
			try {
				p.StartInfo = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		[Test]
		[NUnit.Framework.Category ("NotDotNet")]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void TestRedirectedOutputIsAsync ()
		{
			// Test requires cygwin, so we just bail out for now.
			if (Path.DirectorySeparatorChar == '\\')
				Assert.Ignore ("Test requires cygwin.");
			
			Process p = new Process ();
			p.StartInfo = new ProcessStartInfo ("/bin/sh", "-c \"sleep 2; echo hello\"");
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.UseShellExecute = false;
			p.Start ();

			Stream stdout = p.StandardOutput.BaseStream;

			byte [] buffer = new byte [200];

			// start async Read operation
			var sw = Stopwatch.StartNew ();
			IAsyncResult ar = stdout.BeginRead (buffer, 0, buffer.Length,
							    new AsyncCallback (Read), stdout);

			Assert.IsTrue (sw.ElapsedMilliseconds < 1000, "#01 BeginRead was not async");
			p.WaitForExit ();
			Assert.AreEqual (0, p.ExitCode, "#02 script failure");

			/*
			ar.AsyncWaitHandle.WaitOne (2000, false);
			if (bytesRead < "hello".Length)
				Assert.Fail ("#03 got {0} bytes", bytesRead);
			Assert.AreEqual ("hello", Encoding.Default.GetString (buffer, 0, 5), "#04");
			*/
		}
		
		void Read (IAsyncResult ar)
		{
			Stream stm = (Stream) ar.AsyncState;
			bytesRead = stm.EndRead (ar);
		}

		public int bytesRead = -1;

		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void TestEnvironmentVariablesClearedDoNotInherit ()
		{
			if (!RunningOnUnix)
				Assert.Ignore ("env not available on Windows");

			var stdout = new StringBuilder ();

			Process p = new Process ();
			p.StartInfo = new ProcessStartInfo ("/usr/bin/env");
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.OutputDataReceived += (s, e) => { if (e.Data != null) stdout.AppendLine (e.Data); };
			p.StartInfo.Environment.Clear (); // i.e. don't inherit
			p.StartInfo.Environment["HELLO"] = "123";

			p.Start ();
			p.BeginOutputReadLine ();
			p.WaitForExit ();

			Assert.AreEqual ("HELLO=123\n", stdout.ToString ());
		}

		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void TestEnvironmentVariablesClearedDoNotInheritEmpty ()
		{
			if (!RunningOnUnix)
				Assert.Ignore ("env not available on Windows");

			var stdout = new StringBuilder ();

			Process p = new Process ();
			p.StartInfo = new ProcessStartInfo ("/usr/bin/env");
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.OutputDataReceived += (s, e) => { if (e.Data != null)	stdout.AppendLine (e.Data); };
			p.StartInfo.Environment.Clear (); // i.e. don't inherit

			p.Start ();
			p.BeginOutputReadLine ();
			p.WaitForExit ();

			Assert.AreEqual ("", stdout.ToString ());
		}

		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		// This was for bug #459450
		public void TestEventRaising ()
		{
			EventWaitHandle errorClosed = new ManualResetEvent(false);
			EventWaitHandle outClosed = new ManualResetEvent(false);
			EventWaitHandle exited = new ManualResetEvent(false);

			Process p = new Process();
			
			p.StartInfo = GetCrossPlatformStartInfo ();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardInput = false;
			p.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
				if (e.Data == null) {
					outClosed.Set();
				}
			};
			
			p.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => {
				if (e.Data == null) {
					errorClosed.Set();
				}
			};
			
			p.Exited += (object sender, EventArgs e) => {
				exited.Set ();
			};
			
			p.EnableRaisingEvents = true;

			p.Start();

			p.BeginErrorReadLine();
			p.BeginOutputReadLine();

			Console.WriteLine("started, waiting for handles");
			bool r = WaitHandle.WaitAll(new WaitHandle[] { errorClosed, outClosed, exited }, 10000, false);

			Assert.AreEqual (true, r, "Null Argument Events Raised");
		}

		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void TestEnableEventsAfterExitedEvent ()
		{
			Process p = new Process ();
			
			p.StartInfo = GetCrossPlatformStartInfo ();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;

			var exitedCalledCounter = 0;
			var exited = new ManualResetEventSlim ();
			p.Exited += (object sender, EventArgs e) => {
				exitedCalledCounter++;
				Assert.IsTrue (p.HasExited);
				exited.Set ();
			};

			p.EnableRaisingEvents = true;

			p.Start ();
			p.BeginErrorReadLine ();
			p.BeginOutputReadLine ();
			p.WaitForExit ();

			exited.Wait (10000);
			Assert.AreEqual (1, exitedCalledCounter);
			Thread.Sleep (50);
			Assert.AreEqual (1, exitedCalledCounter);
		}

		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void TestEnableEventsBeforeExitedEvent ()
		{
			Process p = new Process ();
			
			p.StartInfo = GetCrossPlatformStartInfo ();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;

			p.EnableRaisingEvents = true;

			var exitedCalledCounter = 0;
			var exited = new ManualResetEventSlim ();
			p.Exited += (object sender, EventArgs e) => {
				exitedCalledCounter++;
				Assert.IsTrue (p.HasExited);
				exited.Set ();
			};

			p.Start ();
			p.BeginErrorReadLine ();
			p.BeginOutputReadLine ();
			p.WaitForExit ();

			Assert.IsTrue (exited.Wait (10000));
			Assert.AreEqual (1, exitedCalledCounter);
			Thread.Sleep (50);
			Assert.AreEqual (1, exitedCalledCounter);
		}

		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void TestDisableEventsBeforeExitedEvent ()
		{
			Process p = new Process ();
			
			p.StartInfo = GetCrossPlatformStartInfo ();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;

			p.EnableRaisingEvents = false;

			ManualResetEvent mre = new ManualResetEvent (false);
			p.Exited += (object sender, EventArgs e) => {
				mre.Set ();
			};

			p.Start ();
			p.BeginErrorReadLine ();
			p.BeginOutputReadLine ();
			p.WaitForExit ();

			Assert.IsFalse (mre.WaitOne (1000));
		}

		ProcessStartInfo GetCrossPlatformStartInfo ()
		{
			if (RunningOnUnix) {
				string path;
#if MONODROID
				path = "/system/bin/ls";
#else
				path = "/bin/ls";
#endif
				return new ProcessStartInfo (path, "/");
			} else
				return new ProcessStartInfo ("help", "");
		}

		ProcessStartInfo GetEchoCrossPlatformStartInfo ()
		{
			if (RunningOnUnix) {
				string path;
#if MONODROID
				path = "/system/bin/cat";
#else
				path = "/bin/cat";
#endif
				return new ProcessStartInfo (path);
			} else {
				var psi = new ProcessStartInfo ("findstr");
				psi.Arguments = "\"^\"";
				return psi;
			}
		}
#endif // MONO_FEATURE_PROCESS_START

		[Test]
		public void ProcessName_NotStarted ()
		{
			Process p = new Process ();
			Exception e = null;
			try {
				String.IsNullOrEmpty (p.ProcessName);
			} catch (Exception ex) {
				e = ex;
			}
			
			Assert.IsNotNull (e, "ProcessName should raise if process was not started");
			
			//msg should be "No process is associated with this object"
			Assert.AreEqual (e.GetType (), typeof (InvalidOperationException),
			                 "exception should be IOE, I got: " + e.GetType ().Name);
			
			Assert.IsNull (e.InnerException, "IOE inner exception should be null");
		}
		
#if MONO_FEATURE_PROCESS_START
		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void ProcessName_AfterExit ()
		{
			Process p = new Process ();
			p.StartInfo = GetCrossPlatformStartInfo ();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			p.Start ();
			p.BeginErrorReadLine();
			p.BeginOutputReadLine();
			p.WaitForExit ();
			String.IsNullOrEmpty (p.ExitCode + "");
			
			Exception e = null;
			try {
				String.IsNullOrEmpty (p.ProcessName);
			} catch (Exception ex) {
				e = ex;
			}
			
			Assert.IsNotNull (e, "ProcessName should raise if process was finished");
			
			//msg should be "Process has exited, so the requested information is not available"
			Assert.AreEqual (e.GetType (), typeof (InvalidOperationException),
			                 "exception should be IOE, I got: " + e.GetType ().Name);
			
			Assert.IsNull (e.InnerException, "IOE inner exception should be null");
		}
#endif // MONO_FEATURE_PROCESS_START

		[Test]
		public void Handle_ThrowsOnNotStarted ()
		{
			Process p = new Process ();
			try {
				var x = p.Handle;
				Assert.Fail ("Handle should throw for unstated procs, but returned " + x);
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void HasExitedCurrent () {
			Assert.IsFalse (Process.GetCurrentProcess ().HasExited);
		}

#if MONO_FEATURE_PROCESS_START
		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void DisposeWithDisposedStreams ()
		{
			var psi = GetCrossPlatformStartInfo ();
			psi.RedirectStandardInput = true;
			psi.RedirectStandardOutput = true;
			psi.UseShellExecute = false;

			var p = Process.Start (psi);
			p.StandardInput.BaseStream.Dispose ();
			p.StandardOutput.BaseStream.Dispose ();
			p.Dispose ();
		}

		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void StandardInputWrite ()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("AIX")))
			{
				// This test is broken on AIX because the fork child seems to become comatose.
				Assert.Ignore ("Skipping on AIX/i");
			}

			var psi = GetEchoCrossPlatformStartInfo ();
			psi.RedirectStandardInput = true;
			psi.RedirectStandardOutput = true;
			psi.UseShellExecute = false;

			using (var p = Process.Start (psi)) {
				// drain stdout
				p.OutputDataReceived += (s, e) => {};
				p.BeginOutputReadLine ();

				for (int i = 0; i < 1024 * 9; ++i) {
					p.StandardInput.Write ('x');
					if (i > 0 && i % 128 == 0)
						p.StandardInput.WriteLine ();
				}

				p.StandardInput.Close ();

				p.WaitForExit ();
			}
		}
#endif // MONO_FEATURE_PROCESS_START

		[Test]
		public void Modules () {
			var modules = Process.GetCurrentProcess ().Modules;
			foreach (var a in AppDomain.CurrentDomain.GetAssemblies ()) {
				var found = false;
				var name = a.GetName ();

				StringBuilder sb = new StringBuilder ();
				sb.AppendFormat ("Could not found: {0} {1}\n", name.Name, name.Version);
				sb.AppendLine ("Looked in modules:");

				foreach (var o in modules) {
					var m = (ProcessModule) o;

					sb.AppendFormat ("   {0} {1}.{2}.{3}\n", m.FileName.ToString (),
							m.FileVersionInfo.FileMajorPart,
							m.FileVersionInfo.FileMinorPart,
							m.FileVersionInfo.FileBuildPart);

					if (!m.FileName.StartsWith ("[In Memory] " + name.Name))
						continue;

					var fv = m.FileVersionInfo;
					if (fv.FileBuildPart != name.Version.Build ||
						fv.FileMinorPart != name.Version.Minor ||
						fv.FileMajorPart != name.Version.Major)
						continue;

					found = true;
				}

				Assert.IsTrue (found, sb.ToString ());
			}
		}

#if MONO_FEATURE_PROCESS_START
		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestDoubleBeginOutputReadLine ()
		{
			using (Process p = new Process ()) {
				p.StartInfo = GetCrossPlatformStartInfo ();
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;

				p.Start ();

				p.BeginOutputReadLine ();
				p.BeginOutputReadLine ();

				Assert.Fail ();
			}
		}

		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestDoubleBeginErrorReadLine ()
		{
			using (Process p = new Process ()) {
				p.StartInfo = GetCrossPlatformStartInfo ();
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;

				p.Start ();

				p.BeginErrorReadLine ();
				p.BeginErrorReadLine ();

				Assert.Fail ();
			}
		}

		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void TestExitedRaisedTooSoon ()
		{
			if (!RunningOnUnix)
				Assert.Ignore ("using sleep command, only available on unix");

			int sleeptime = 5;

			using (Process p = Process.Start("sleep", sleeptime.ToString ())) {
				ManualResetEvent mre = new ManualResetEvent (false);

				p.EnableRaisingEvents = true;
				p.Exited += (sender, e) => {
					mre.Set ();
				};

				Assert.IsFalse (mre.WaitOne ((sleeptime - 2) * 1000), "Exited triggered before the process returned");
			}
		}
#endif // MONO_FEATURE_PROCESS_START

		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void GetProcessesByName()
		{
			// This should return Process[0] or a Process[] with all the "foo" programs running
			Process.GetProcessesByName ("foo");
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")] //Getting the name of init works fine on Android and Linux, but fails on OSX, SELinux and iOS
		public void HigherPrivilegeProcessName ()
		{
			if (!RunningOnUnix)
				Assert.Ignore ("accessing pid 1, only available on unix");

			string v = Process.GetProcessById (1).ProcessName;
		}

		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void NonChildProcessWaitForExit ()
		{
			if (!RunningOnUnix)
				Assert.Ignore ("accessing parent pid, only available on unix");

			using (Process process = Process.GetProcessById (getppid ()))
			using (ManualResetEvent mre = new ManualResetEvent (false))
			{
				Assert.IsFalse (process.WaitForExit (10), "#1");
				Assert.IsFalse (process.HasExited, "#2");
				Assert.Throws<InvalidOperationException>(delegate { int exitCode = process.ExitCode; }, "#3");

				process.Exited += (s, e) => mre.Set ();
				process.EnableRaisingEvents = true;
				Assert.IsFalse (mre.WaitOne (100), "#4");

				Assert.IsFalse (process.WaitForExit (10), "#5");
				Assert.IsFalse (process.HasExited, "#6");
				Assert.Throws<InvalidOperationException>(delegate { int exitCode = process.ExitCode; }, "#7");
			}
		}

		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void NonChildProcessName ()
		{
			if (!RunningOnUnix)
				Assert.Ignore ("accessing parent pid, only available on unix");

			using (Process process = Process.GetProcessById (getppid ()))
			{
				string pname = process.ProcessName;
				Assert.IsNotNull (pname, "#1");
				AssertHelper.IsNotEmpty (pname, "#2");
			}
		}

		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")]
		public void NonChildProcessId ()
		{
			if (!RunningOnUnix)
				Assert.Ignore ("accessing parent pid, only available on unix");

			int ppid;
			using (Process process = Process.GetProcessById (ppid = getppid ()))
			{
				int pid = process.Id;
				Assert.AreEqual (ppid, pid, "#1");
				AssertHelper.Greater (pid, 0, "#2");
			}
		}

		[DllImport ("libc")]
		static extern int getppid();
	}
}
