// FileSystemWatcherTest.cs - NUnit Test Cases for the System.IO.FileSystemWatcher class
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2004 Novell, Inc.  http://www.novell.com
// 

using NUnit.Framework;
using System;
using System.IO;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class FileSystemWatcherTest : Assertion
	{
		[Test]
		public void CheckDefaults ()
		{
			FileSystemWatcher fw = new FileSystemWatcher ();
			AssertEquals ("#01", fw.EnableRaisingEvents, false);
			AssertEquals ("#02", fw.Filter, "*.*");
			AssertEquals ("#03", fw.IncludeSubdirectories, false);
			AssertEquals ("#04", fw.InternalBufferSize, 8192);
			AssertEquals ("#05", fw.NotifyFilter, NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite);
			AssertEquals ("#06", fw.Path, "");
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
	}
}

