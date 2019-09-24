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
using System.Reflection;

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

		[Test]
		public void CreateTwoAndDispose ()
		{
			// Create two FSW instances and dispose them.  Verify
			// that the backend IFileWatcher's Dispose
			// (watcher_handle) method got called.

			// FIXME: This only works for the
			// CoreFXFileSystemWatcherProxy not the other backends.

			using (var tmp = new TempDirectory ()) {
				// have to use reflection to poke at the private fields of FileSystemWatcher.
				var watcherField = typeof (FileSystemWatcher).GetField ("watcher", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.IsNotNull (watcherField);
				var watcherHandleField = typeof (FileSystemWatcher).GetField ("watcher_handle", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.IsNotNull (watcherHandleField);
				var proxyType = typeof (FileSystemWatcher).Assembly.GetType ("System.IO.CoreFXFileSystemWatcherProxy");
				Assert.IsNotNull (proxyType);
				// the "internal_map" maps watcher handles to backend CoreFX FSW instances
				var proxyTypeInternalMapField = proxyType.GetField ("internal_map", BindingFlags.Static | BindingFlags.NonPublic);
				Assert.IsNotNull (proxyTypeInternalMapField);

				var fsw1 = new FileSystemWatcher (tmp.Path, "*");
				var fsw2 = new FileSystemWatcher (tmp.Path, "*");
				// at this point watcher and watcher_handle should be set

				global::System.Collections.Generic.IDictionary<object, global::System.IO.CoreFX.FileSystemWatcher> internal_map = null;
				object handle1 = null;
				object handle2 = null;

				// using "using" to ensure that Dispose gets called even if we throw an exception
				using (var fsw11 = fsw1)
				using (var fsw22 = fsw2) {

					// Once at least one FSW is initialized, watcher should be set.  But if the
					// wrong backend is getting used, ignore this test because the other checks
					// (internal_map in particular) won't be valid.
					var watcher = watcherField.GetValue (fsw1);
					Assert.IsNotNull (watcher);
					if (!proxyType.IsAssignableFrom (watcher.GetType ()))
						Assert.Ignore ("Testing only CoreFXFileSystemWatcherProxy FSW backend");

					handle1 = watcherHandleField.GetValue (fsw1);
					handle2 = watcherHandleField.GetValue (fsw2);

					Assert.IsNotNull (handle1);
					Assert.IsNotNull (handle2);

					// Can't check for internal_map earlier - it is lazily created when the first
					// FSW instance is created
					internal_map = proxyTypeInternalMapField.GetValue (null)
						as global::System.Collections.Generic.IDictionary<object, global::System.IO.CoreFX.FileSystemWatcher>;
					Assert.IsNotNull (internal_map);

					// Both of handles should be in the internal map while the file system watchers
					// are not disposed.
					Assert.IsTrue (internal_map.ContainsKey (handle1));
					Assert.IsTrue (internal_map.ContainsKey (handle2));

				}

				// Dispose was called, now watcher_handle should be null

				Assert.IsNull (watcherHandleField.GetValue (fsw1));
				Assert.IsNull (watcherHandleField.GetValue (fsw2));

				// This pair are the critical checks: after we call Dispose on fsw1 and fsw2, the
				// backend's internal map shouldn't have anything keyed on handle1 and handle2.
				// Therefore System.IO.CoreFX.FileSystemWatcher instances will be disposed of, too.
				Assert.IsFalse (internal_map.ContainsKey (handle1));
				Assert.IsFalse (internal_map.ContainsKey (handle2));
			}
		}

	}
}

#endif
