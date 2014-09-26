// FileSystemWatcherTest.cs - NUnit Test Cases for the System.IO.FileSystemWatcher class
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2004 Novell, Inc.  http://www.novell.com
// 

#if !MOBILE

using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class FileSystemWatcherTest
	{
		static string base_path = Path.Combine (Path.GetTempPath (), "FSWTest");
		static string path_a = Path.Combine (base_path, "a.txt");

		AutoResetEvent eventFired = new AutoResetEvent (false);
		WatcherChangeTypes lastChangeType;
		string lastName, lastFullPath;

		[TestFixtureSetUp]
		public void FixtureSetup ()
		{
			if (!Directory.Exists (base_path))
			   Directory.CreateDirectory (base_path);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			if (Directory.Exists (base_path))
			   Directory.Delete (base_path, true);
		}

		[Test]
		public void CheckDefaults ()
		{
			FileSystemWatcher fw = new FileSystemWatcher ();
			Assert.AreEqual (fw.EnableRaisingEvents, false, "#01");
			Assert.AreEqual (fw.Filter, "*.*", "#02");
			Assert.AreEqual (fw.IncludeSubdirectories, false, "#03");
			Assert.AreEqual (fw.InternalBufferSize, 8192, "#04");
			Assert.AreEqual (fw.NotifyFilter, NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite, "#05");
			Assert.AreEqual (fw.Path, "", "#06");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CheckCtor1 ()
		{
			FileSystemWatcher fw = new FileSystemWatcher (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CheckCtor2 ()
		{
			FileSystemWatcher fw = new FileSystemWatcher ("");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CheckCtor3 ()
		{
			FileSystemWatcher fw = new FileSystemWatcher ("notexistsblahblah");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CheckCtor4 ()
		{
			FileSystemWatcher fw = new FileSystemWatcher (Path.GetTempPath (), null);
		}

		[Test]
		// Doesn't throw here :-?
		// [ExpectedException (typeof (ArgumentException))]
		public void CheckCtor5 ()
		{
			FileSystemWatcher fw = new FileSystemWatcher (Path.GetTempPath (), "invalidpath|");
			fw = new FileSystemWatcher (Path.GetTempPath (), "*");
		}

		[Test]
		// ...But here it does...
		[ExpectedException (typeof (ArgumentException))]
		public void CheckInvalidPath ()
		{
			FileSystemWatcher fw = new FileSystemWatcher (Path.GetTempPath (), "invalidpath|");
			fw.Path = "invalidpath|";
		}

		[Test]
		// ...and here too
		[ExpectedException (typeof (ArgumentException))]
		public void CheckPathWildcard ()
		{
			FileSystemWatcher fw = new FileSystemWatcher (Path.GetTempPath (), "*");
			fw.Path = "*";
		}


		[Test]
		public void TestWatchPathForFileCreation ()
		{
			FileSystemEventHandler createdDelegate = delegate (object o, FileSystemEventArgs e) {
				eventFired.Set ();
				lastChangeType = WatcherChangeTypes.Created;
			};

			var fsw = new FileSystemWatcher (base_path);
			fsw.IncludeSubdirectories = true;
			fsw.Created += createdDelegate;
			fsw.EnableRaisingEvents = true;
			Thread.Sleep (1000);

			Assert.IsFalse (File.Exists (path_a));

			File.WriteAllText (path_a, "this should create the file");
			bool gotEvent = eventFired.WaitOne (20000, true);

			Assert.IsTrue (File.Exists (path_a));

			Assert.IsTrue (gotEvent);
			Assert.IsTrue ((lastChangeType == WatcherChangeTypes.Created));

			fsw.EnableRaisingEvents = false;
			fsw.Created -= createdDelegate;
			fsw.Dispose ();
		}

		[Test]
		public void TestWatchPathForFileDelete ()
		{
			var basePath = Path.Combine (Path.GetTempPath (), "FSWTestDelete");
			var fileToDelete = Path.Combine (basePath, "deleteMe.txt");

			if (!Directory.Exists (basePath))
			   Directory.CreateDirectory (basePath);

			File.WriteAllText (fileToDelete, "this file will be deleted");
			Thread.Sleep (500);

			FileSystemEventHandler deletedDelegate = delegate (object o, FileSystemEventArgs e) {
				eventFired.Set ();
				lastChangeType = WatcherChangeTypes.Deleted;
			};

			var fsw = new FileSystemWatcher (basePath);
			fsw.IncludeSubdirectories = true;
			fsw.Deleted += deletedDelegate;
			fsw.EnableRaisingEvents = true;
			Thread.Sleep (1000);

			Assert.IsTrue (File.Exists (fileToDelete));

			File.Delete (fileToDelete);
			bool gotEvent = eventFired.WaitOne (5000, true);

			Assert.IsTrue (gotEvent);
			Assert.IsTrue ((lastChangeType == WatcherChangeTypes.Deleted));

			fsw.EnableRaisingEvents = false;
			fsw.Deleted -= deletedDelegate;
			fsw.Dispose ();

			Directory.Delete (basePath);
		}

		[Test]
		public void TestWatchPathForFileModify ()
		{
			var basePath = Path.Combine (Path.GetTempPath (), "FSWTestModify");
			var fileToModify = Path.Combine (basePath, "modifyMe.txt");

			if (!Directory.Exists (basePath))
				Directory.CreateDirectory (basePath);

			File.WriteAllText (fileToModify, "this file will be changed");
			Thread.Sleep (1000);

			FileSystemEventHandler changedDelegate = delegate (object o, FileSystemEventArgs e) {
				eventFired.Set ();
				lastChangeType = WatcherChangeTypes.Changed;
			};

			var fsw = new FileSystemWatcher (basePath);
			fsw.Changed += changedDelegate;
			fsw.IncludeSubdirectories = true;
			fsw.EnableRaisingEvents = true;
			Thread.Sleep (1000);

			Assert.IsTrue (File.Exists (fileToModify));

			// XXX
			// This isn't portable to Windowws, but nothing else seems to work
			// on OSX.
			Process.Start ("touch", fileToModify);

			//File.AppendAllText (fileToModify, "change is scary");

			/* using (StreamWriter sw = File.AppendText (fileToModify)) */
			/* { */
			/*   sw.WriteLine("change is scary"); */
			/*   sw.Flush (); */
			/* } */

			/* using (var sw = new StreamWriter (fileToModify, true)) { */
			/*   sw.WriteLine ("change is scary"); */
			/* } */

			bool gotEvent = eventFired.WaitOne (4000, true);

			Assert.IsTrue (gotEvent);
			Assert.IsTrue ((lastChangeType == WatcherChangeTypes.Changed));

			fsw.EnableRaisingEvents = false;
			fsw.Changed -= changedDelegate;
			fsw.Dispose ();

			File.Delete (fileToModify);
			Directory.Delete (basePath);
		}

		[Test]
		public void TestWatchPathForFileRename ()
		{
			var basePath = Path.Combine (Path.GetTempPath (), "FSWTestRename");
			var sourceFile = Path.Combine (basePath, "renameMe-src.txt");
			var destFile = Path.Combine (basePath, "renameMe-dest.txt");

			if (!Directory.Exists (basePath))
				Directory.CreateDirectory (basePath);

			File.WriteAllText (sourceFile, "this file will be renamed");
			Thread.Sleep (1000);

			RenamedEventHandler renamedDelegate = delegate (object o, RenamedEventArgs e) {
				eventFired.Set ();
				lastChangeType = WatcherChangeTypes.Renamed;
			};

			var fsw = new FileSystemWatcher (basePath);
			fsw.Renamed += renamedDelegate;
			fsw.IncludeSubdirectories = true;
			fsw.EnableRaisingEvents = true;
			Thread.Sleep (1000);

			Assert.IsTrue (File.Exists (sourceFile));
			Assert.IsFalse (File.Exists (destFile));

			// XXX
			// This isn't portable to Windows.
			Process.Start ("mv", sourceFile + " " + destFile);

			bool gotEvent = eventFired.WaitOne (4000, true);

			Assert.IsTrue (gotEvent);
			Assert.IsTrue ((lastChangeType == WatcherChangeTypes.Renamed));
			Assert.IsTrue (File.Exists (destFile));
			Assert.IsFalse (File.Exists (sourceFile));

			fsw.EnableRaisingEvents = false;
			fsw.Renamed -= renamedDelegate;
			fsw.Dispose ();

			File.Delete (destFile);
			Directory.Delete (basePath);
		}
	}
}

#endif
