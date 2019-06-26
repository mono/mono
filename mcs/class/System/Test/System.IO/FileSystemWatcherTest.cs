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

using MonoTests.Helpers;

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
			Assert.AreEqual (fw.Filter, "*", "#02");
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
			using (var tmp = new TempDirectory ()) {
				FileSystemWatcher fw = new FileSystemWatcher (tmp.Path, null);
			}
		}

		[Test]
		// Doesn't throw here :-?
		// [ExpectedException (typeof (ArgumentException))]
		public void CheckCtor5 ()
		{
			using (var tmp1 = new TempDirectory ()) {
				using (var tmp2 = new TempDirectory ()) {
					FileSystemWatcher fw = new FileSystemWatcher (tmp1.Path, "invalidpath|");
					fw = new FileSystemWatcher (tmp2.Path, "*");
				}
			}
		}

		[Test]
		// ...But here it does...
		[ExpectedException (typeof (ArgumentException))]
		public void CheckInvalidPath ()
		{
			using (var tmp = new TempDirectory ()) {
				FileSystemWatcher fw = new FileSystemWatcher (tmp.Path, "invalidpath|");
				fw.Path = "invalidpath|";
			}
		}

		[Test]
		// ...and here too
		[ExpectedException (typeof (ArgumentException))]
		public void CheckPathWildcard ()
		{
			using (var tmp = new TempDirectory ()) {
				FileSystemWatcher fw = new FileSystemWatcher (tmp.Path, "*");
				fw.Path = "*";
			}
		}

		[Test]
		[Category ("NotOnMac")] // creates resource exhaustion issues
		public void LargeNumberOfInstances ()
		{
			using (var tmp = new TempDirectory ()) {
				var watchers = new FileSystemWatcher [256];
				for (int x = 0; x < watchers.Length; x++)
				{
					watchers[x] = new FileSystemWatcher (tmp.Path, "*");
					watchers[x].EnableRaisingEvents = true;
				}
			}
		}
	}
}

#endif