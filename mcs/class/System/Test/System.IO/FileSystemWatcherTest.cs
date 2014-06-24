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

namespace MonoTests.System.IO
{
	[TestFixture]
	public class FileSystemWatcherTest
	{
		string base_path;
		string path_a;

		FileSystemWatcher SetupWatcher ()
		{
			var fsw = new FileSystemWatcher ();
			fsw.Path = base_path;
			fsw.IncludeSubdirectories = true;
			fsw.EnableRaisingEvents = true;

			return fsw;
		}

		[SetUp]
		public void Setup ()
		{
			base_path = Path.GetTempPath ();
			path_a = Path.Combine (base_path, "a.txt");
		}

		[TearDown]
		public void TearDown ()
		{
			if (File.Exists (path_a))
				File.Delete (path_a);
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
			bool created = false;
			var fsw = SetupWatcher ();

			fsw.Created += (object sender, FileSystemEventArgs e) => {
				created = true;
			};

			Assert.IsFalse (created);

			File.WriteAllText (path_a, "this should create the file");

			Thread.Sleep (20);

			Assert.IsTrue (created);
		}
	}
}

#endif
