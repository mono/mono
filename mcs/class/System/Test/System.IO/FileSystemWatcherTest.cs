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
		// Doesn't throw any exceptions
		public void DeleteSelfTest ()
		{
			var path = Path.GetTempPath () + "/Subdir";
			Directory.CreateDirectory (path);
			using (var fw = new FileSystemWatcher (path, "*")) {
				fw.IncludeSubdirectories = true;
				fw.EnableRaisingEvents = true;

				var file = path + "/file.txt";
				File.Create (file);
				Thread.Sleep (500);

				var subdir = new DirectoryInfo (path);
				subdir.Delete (true);

				Thread.Sleep (500);
			}
		}

		[Test]
		// Doesn't throw any exceptions
		public void MoveTest ()
		{
			var tempPath = Path.GetTempPath ();
			var root = tempPath + "/Ext";
			var nested = root + "/n1";
			var root2 = tempPath + "/n1";
			try {
				Directory.CreateDirectory (root);
				Directory.CreateDirectory (nested);
				File.Create (nested + "/file.txt");

				using (var fw = new FileSystemWatcher (root, "*")) {
					fw.IncludeSubdirectories = true;
					fw.EnableRaisingEvents = true;

					// MOVE TEMP/ext/n to TEMP/n
					Directory.Move (nested, root2);

					Thread.Sleep (500);
				}
			} finally {
				var rootdir = new DirectoryInfo (root);
				if (rootdir.Exists)
					rootdir.Delete (true);

				var root2dir = new DirectoryInfo (root2);
				if (root2dir.Exists)
					root2dir.Delete (true);
			}
		}
	}
}

#endif
