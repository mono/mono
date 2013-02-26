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

using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class ProcessTest
	{
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
				return;

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

			if (RunningOnUnix)
				Assert.Ignore ("On Unix and Mac OS X, we try " +
					"to open any file (using xdg-open, ...)" +
					" and we do not report an exception " +
					"if this fails.");

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
				Assert.AreEqual (2, ex.NativeErrorCode, "#6");
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
				Assert.AreEqual (2, ex.NativeErrorCode, "#6");
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

#if NET_2_0		
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
#endif
		
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
				return;
			
			Process p = new Process ();
			p.StartInfo = new ProcessStartInfo ("/bin/sh", "-c \"sleep 2; echo hello\"");
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.UseShellExecute = false;
			p.Start ();

			Stream stdout = p.StandardOutput.BaseStream;

			byte [] buffer = new byte [200];

			// start async Read operation
			DateTime start = DateTime.Now;
			IAsyncResult ar = stdout.BeginRead (buffer, 0, buffer.Length,
							    new AsyncCallback (Read), stdout);

			Assert.IsTrue ((DateTime.Now - start).TotalMilliseconds < 1000, "#01 BeginRead was not async");
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

		static bool RunningOnUnix {
			get {
				int p = (int)Environment.OSVersion.Platform;
				return ((p == 128) || (p == 4) || (p == 6));
			}
		}

		int bytesRead = -1;

#if NET_2_0
// Not technically a 2.0 only test, but I use lambdas, so I need gmcs

		[Test]
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
		
		private ProcessStartInfo GetCrossPlatformStartInfo ()
		{
			return RunningOnUnix ? new ProcessStartInfo ("/bin/ls", "/") : new ProcessStartInfo ("help", "");
		}
		
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
		
		[Test]
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
#endif
	}
}
