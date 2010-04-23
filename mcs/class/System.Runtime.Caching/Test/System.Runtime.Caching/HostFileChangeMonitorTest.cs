//
// HostFileChangeMonitorTest.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.Caching.Hosting;
using System.Text;

using NUnit.Framework;
using MonoTests.Common;

namespace MonoTests.System.Runtime.Caching
{	
	[TestFixture]
	public class HostFileChangeMonitorTest
	{
		[Test]
		public void Constructor_Exceptions ()
		{
			HostFileChangeMonitor monitor;
			string relPath = Path.Combine ("relative", "file", "path");
			var paths = new List<string> {
				relPath
			};

			AssertExtensions.Throws<ArgumentException> (() => {
				monitor = new HostFileChangeMonitor (paths);
			}, "#A1");

			paths.Clear ();
			paths.Add (null);
			AssertExtensions.Throws<ArgumentException> (() => {
				monitor = new HostFileChangeMonitor (paths);
			}, "#A2");

			paths.Clear ();
			paths.Add (String.Empty);
			AssertExtensions.Throws<ArgumentException> (() => {
				monitor = new HostFileChangeMonitor (paths);
			}, "#A3");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				monitor = new HostFileChangeMonitor (null);
			}, "#A4");

			paths.Clear ();
			AssertExtensions.Throws<ArgumentException> (() => {
				monitor = new HostFileChangeMonitor (paths);
			}, "#A5");
		}

		[Test]
		public void Constructor_MissingFiles ()
		{
			AppDomainTools.RunInSeparateDomain (Constructor_MissingFiles_Handler, "Constructor_MissingFiles");
		}

		static void Constructor_MissingFiles_Handler ()
		{
			HostFileChangeMonitor monitor;
			PlatformID pid = Environment.OSVersion.Platform;
			bool runningOnWindows = ((int) pid != 128 && pid != PlatformID.Unix && pid != PlatformID.MacOSX);
			string missingFile = Path.Combine ("missing", "file", "path");

			if (runningOnWindows)
				missingFile = "c:\\" + missingFile;
			else
				missingFile = "/" + missingFile;

			var paths = new List<string> {
				missingFile
			};

			// Actually thrown by FileSystemWatcher constructor - note that the exception message suggests the file's
			// parent directory is being watched, not the file itself:
			//
			// MonoTests.System.Runtime.Caching.HostFileChangeMonitorTest.Constructor_MissingFiles:
			// System.ArgumentException : The directory name c:\missing\file is invalid.
			// at System.IO.FileSystemWatcher..ctor(String path, String filter)
			// at System.IO.FileSystemWatcher..ctor(String path)
			// at System.Runtime.Caching.FileChangeNotificationSystem.System.Runtime.Caching.Hosting.IFileChangeNotificationSystem.StartMonitoring(String filePath, OnChangedCallback onChangedCallback, Object& state, DateTimeOffset& lastWriteTime, Int64& fileSize)
			// at System.Runtime.Caching.HostFileChangeMonitor.InitDisposableMembers()
			// at System.Runtime.Caching.HostFileChangeMonitor..ctor(IList`1 filePaths)
			// at MonoTests.System.Runtime.Caching.HostFileChangeMonitorTest.Constructor_MissingFiles() in c:\users\grendel\documents\visual studio 2010\Projects\System.Runtime.Caching.Test\System.Runtime.Caching.Test\System.Runtime.Caching\HostFileChangeMonitorTest.cs:line 68
			AssertExtensions.Throws<ArgumentException> (() => {
				monitor = new HostFileChangeMonitor (paths);
			}, "#A1");

			if (runningOnWindows)
				missingFile = "c:\\file.txt";
			else
				missingFile = "/file.txt";

			paths.Clear ();
			paths.Add (missingFile);
			monitor = new HostFileChangeMonitor (paths);
			Assert.AreEqual (1, monitor.FilePaths.Count, "#A2-1");
			Assert.AreEqual (missingFile, monitor.FilePaths [0], "#A2-2");
			Assert.AreEqual (missingFile + "701CE1722770000FFFFFFFFFFFFFFFF", monitor.UniqueId, "#A2-4");

			paths.Add (missingFile);
			monitor = new HostFileChangeMonitor (paths);
			Assert.AreEqual (2, monitor.FilePaths.Count, "#A3-1");
			Assert.AreEqual (missingFile, monitor.FilePaths [0], "#A3-2");
			Assert.AreEqual (missingFile, monitor.FilePaths [1], "#A3-3");
			Assert.AreEqual (missingFile + "701CE1722770000FFFFFFFFFFFFFFFF", monitor.UniqueId, "#A3-4");
		}

		[Test]
		public void Constructor_Duplicates ()
		{
			HostFileChangeMonitor monitor;
			PlatformID pid = Environment.OSVersion.Platform;
			bool runningOnWindows = ((int) pid != 128 && pid != PlatformID.Unix && pid != PlatformID.MacOSX);
			string missingFile = Path.Combine ("missing", "file", "path");

			if (runningOnWindows)
				missingFile = "c:\\file.txt";
			else
				missingFile = "/file.txt";

			var paths = new List<string> {
				missingFile,
				missingFile
			};

			// Just checks if it doesn't throw any exception for dupes
			monitor = new HostFileChangeMonitor (paths);
		}

		static Tuple <string, string, string, IList <string>> SetupMonitoring ()
		{
			string testPath = Path.Combine (Path.GetTempPath (), "Dispose_Calls_StopMonitoring");
			if (!Directory.Exists (testPath))
				Directory.CreateDirectory (testPath);

			string firstFile = Path.Combine (testPath, "FirstFile.txt");
			string secondFile = Path.Combine (testPath, "SecondFile.txt");

			File.WriteAllText (firstFile, "I am the first file.");
			File.WriteAllText (secondFile, "I am the second file.");

			var paths = new List<string> {
				firstFile,
				secondFile
			};

			return new Tuple<string, string, string, IList<string>> (testPath, firstFile, secondFile, paths);
		}

		static void CleanupMonitoring (Tuple<string, string, string, IList<string>> setup)
		{
			string testPath = setup != null ? setup.Item1 : null;
			if (String.IsNullOrEmpty (testPath) || !Directory.Exists (testPath))
				return;

			foreach (string f in Directory.EnumerateFiles(testPath)) {
				try {
					File.Delete (f);
				} catch {
					// ignore
				}
			}
		}

		[Test]
		public void Constructor_Calls_StartMonitoring ()
		{
			AppDomainTools.RunInSeparateDomain (Constructor_Calls_StartMonitoring_Handler, "Constructor_Calls_StartMonitoring_Handler");
		}

		static void Constructor_Calls_StartMonitoring_Handler ()
		{
			Tuple<string, string, string, IList<string>> setup = null;
			try {
				var tns = new TestNotificationSystem ();
				ObjectCache.Host = tns;
				setup = SetupMonitoring ();
				var monitor = new HostFileChangeMonitor (setup.Item4);

				Assert.IsTrue (tns.StartMonitoringCalled, "#A1-1");
				Assert.AreEqual (2, tns.StartMonitoringCallCount, "#A1-2");
			} finally {
				CleanupMonitoring (setup);
			}
		}

		[Test]
		public void Dispose_Calls_StopMonitoring ()
		{
			AppDomainTools.RunInSeparateDomain (Dispose_Calls_StopMonitoring_Handler, "Dispose_Calls_StopMonitoring_Handler");
		}

		static void Dispose_Calls_StopMonitoring_Handler ()
		{
			Tuple<string, string, string, IList<string>> setup = null;
			try {
				var tns = new TestNotificationSystem ();
				ObjectCache.Host = tns;
				setup = SetupMonitoring ();
				var monitor = new HostFileChangeMonitor (setup.Item4);
				tns.FakeChanged (setup.Item2);

				Assert.IsTrue (tns.StopMonitoringCalled, "#A1-1");
				Assert.AreEqual (2, tns.StopMonitoringCallCount, "#A1-2");
			} finally {
				CleanupMonitoring (setup);
			}
		}

		[Test]
		public void Dispose_NullState_NoStopMonitoring ()
		{
			AppDomainTools.RunInSeparateDomain (Dispose_NullState_NoStopMonitoring_Handler, "Dispose_NullState_NoStopMonitoring_Handler");
		}

		static void Dispose_NullState_NoStopMonitoring_Handler ()
		{
			Tuple<string, string, string, IList<string>> setup = null;
			try {
				var tns = new TestNotificationSystem ();
				tns.UseNullState = true;
				ObjectCache.Host = tns;
				setup = SetupMonitoring ();
				var monitor = new HostFileChangeMonitor (setup.Item4);
				tns.FakeChanged (setup.Item2);

				Assert.IsFalse (tns.StopMonitoringCalled, "#A1-1");
				Assert.AreEqual (0, tns.StopMonitoringCallCount, "#A1-2");
			} finally {
				CleanupMonitoring (setup);
			}
		}

		[Test]
		public void UniqueId ()
		{
			Tuple<string, string, string, IList<string>> setup = null;
			try {
				setup = SetupMonitoring ();
				FileInfo fi;
				var monitor = new HostFileChangeMonitor (setup.Item4);
				var sb = new StringBuilder ();

				fi = new FileInfo (setup.Item2);
				sb.AppendFormat ("{0}{1:X}{2:X}",
					setup.Item2,
					fi.LastWriteTimeUtc.Ticks,
					fi.Length);

				fi = new FileInfo (setup.Item3);
				sb.AppendFormat ("{0}{1:X}{2:X}",
					setup.Item3,
					fi.LastWriteTimeUtc.Ticks,
					fi.Length);

				Assert.AreEqual (sb.ToString (), monitor.UniqueId, "#A1");

				var list = new List<string> (setup.Item4);
				list.Add (setup.Item1);

				monitor = new HostFileChangeMonitor (list);
				var di = new DirectoryInfo (setup.Item1);
				sb.AppendFormat ("{0}{1:X}{2:X}",
					setup.Item1,
					di.LastWriteTimeUtc.Ticks,
					-1L);
				Assert.AreEqual (sb.ToString (), monitor.UniqueId, "#A2");

				list.Add (setup.Item1);
				monitor = new HostFileChangeMonitor (list);
				Assert.AreEqual (sb.ToString (), monitor.UniqueId, "#A3");
			} finally {
				CleanupMonitoring (setup);
			}
		}
	}
}
