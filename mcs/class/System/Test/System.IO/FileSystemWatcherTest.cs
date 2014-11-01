// FileSystemWatcherTest.cs - NUnit Test Cases for the System.IO.FileSystemWatcher class
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2004 Novell, Inc.  http://www.novell.com
// 

#if !MOBILE

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class FileSystemWatcherTest
	{
		#region Test Internals

		/// <summary>
		/// All NotifyFilters values.
		/// </summary>
		private const NotifyFilters c_notifyFiltersAll =
			NotifyFilters.Attributes
			| NotifyFilters.CreationTime
			| NotifyFilters.DirectoryName
			| NotifyFilters.FileName
			| NotifyFilters.LastAccess
			| NotifyFilters.LastWrite
			| NotifyFilters.Security
			| NotifyFilters.Size;

		/// <summary>
		/// The delay duration between method calls which mutate the filesystem (to trigger events
		/// in the FileSystemWatcher). This is used to thottle the rate at which the events are fired.
		/// </summary>
		private static readonly TimeSpan s_fileSystemOperationSleepDuration = TimeSpan.FromMilliseconds(100.0);

		/// <summary>
		/// The path to the folder used as the base path for the tests within this fixture.
		/// This folder is created within the user's temporary folder.
		/// </summary>
		private string m_testBasePath;

		[SetUp]
		public void TestSetup()
		{
			var FolderName = "FileSystemWatcherTest-" + Guid.NewGuid().ToString("D");
			var TestBasePath = Path.Combine(Path.GetTempPath(), FolderName);
			Directory.CreateDirectory(TestBasePath);
			m_testBasePath = TestBasePath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? TestBasePath : TestBasePath + Path.DirectorySeparatorChar.ToString();
		}

		#endregion

		[Test]
		public void CheckDefaults()
		{
			FileSystemWatcher fw = new FileSystemWatcher();
			Assert.AreEqual(fw.EnableRaisingEvents, false, "#01");
			Assert.AreEqual(fw.Filter, "*.*", "#02");
			Assert.AreEqual(fw.IncludeSubdirectories, false, "#03");
			Assert.AreEqual(fw.InternalBufferSize, 8192, "#04");
			Assert.AreEqual(fw.NotifyFilter, NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite, "#05");
			Assert.AreEqual(fw.Path, "", "#06");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CheckCtor1()
		{
			FileSystemWatcher fw = new FileSystemWatcher(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CheckCtor2()
		{
			FileSystemWatcher fw = new FileSystemWatcher("");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CheckCtor3()
		{
			FileSystemWatcher fw = new FileSystemWatcher("notexistsblahblah");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CheckCtor4()
		{
			FileSystemWatcher fw = new FileSystemWatcher(Path.GetTempPath(), null);
		}

		[Test]
		// Doesn't throw here :-?
		// [ExpectedException (typeof (ArgumentException))]
		public void CheckCtor5()
		{
			FileSystemWatcher fw = new FileSystemWatcher(Path.GetTempPath(), "invalidpath|");
			fw = new FileSystemWatcher(Path.GetTempPath(), "*");
		}

		[Test]
		// ...But here it does...
		[ExpectedException(typeof(ArgumentException))]
		public void CheckInvalidPath()
		{
			FileSystemWatcher fw = new FileSystemWatcher(Path.GetTempPath(), "invalidpath|");
			fw.Path = "invalidpath|";
		}

		[Test]
		// ...and here too
		[ExpectedException(typeof(ArgumentException))]
		public void CheckPathWildcard()
		{
			FileSystemWatcher fw = new FileSystemWatcher(Path.GetTempPath(), "*");
			fw.Path = "*";
		}

		[Test]
		public void NotifyOnFileChanged()
		{
			// The filenames used (in order) to create the temporary files.
			var Filenames = new string[5];
			for (var i = 0; i < Filenames.Length; i++)
			{
				Filenames[i] = Guid.NewGuid().ToString("D") + ".txt";
			}

			// Create the temporary files.
			foreach (var Filename in Filenames)
			{
				var FilePath = Path.Combine(m_testBasePath, Filename);
				File.WriteAllText(FilePath, "Testing, testing, 1, 2, 3");
			}

			// Holds data returned by the event under test.
			var EventDataList = new List<FileSystemEventArgs>();

			// Create a FileSystemWatcher instance.
			using (var watcher = new FileSystemWatcher(m_testBasePath))
			{
				// Attach an event handler which records data returned by the event.
				watcher.Changed += (sender, e) =>
				{
					lock (EventDataList)
					{
						EventDataList.Add(e);
					}
				};

				// Enable event types so we capture the events we're looking for.
				watcher.NotifyFilter = c_notifyFiltersAll;

				// Enable raising of events.
				watcher.EnableRaisingEvents = true;

				// Alter the temporary files by appending some text.
				foreach (var Filename in Filenames)
				{
					// Sleep for a short period of time to allow the events to be throttled, if necessary.
					if (s_fileSystemOperationSleepDuration != TimeSpan.Zero)
					{
						Thread.Sleep(s_fileSystemOperationSleepDuration);
					}

					var FilePath = Path.Combine(m_testBasePath, Filename);
					using (var sw = (new FileInfo(FilePath)).AppendText())
					{
						sw.WriteLine();
						sw.WriteLine("Adding some additional text to change the file...");
					}
				}

				// Sleep for a short period of time to allow the events to be throttled, if necessary.
				if (s_fileSystemOperationSleepDuration != TimeSpan.Zero)
				{
					Thread.Sleep(s_fileSystemOperationSleepDuration);
				}
			}

			// Check the captured event data to make sure it matches our expectations.
			Assert.AreEqual(Filenames.Length, EventDataList.Count,
				"The event data list does not contain the correct number of items.");
			for (var i = 0; i < Filenames.Length; i++)
			{
				var EventData = EventDataList[i];
				Assert.AreEqual(EventData.ChangeType, WatcherChangeTypes.Changed,
					String.Format("The event data at index {0} does not have the expected value for it's ChangeType property.", i));

				var FilePath = Path.Combine(m_testBasePath, Filenames[i]);
				Assert.AreEqual(FilePath, EventData.FullPath,
					String.Format("The event data at index {0} does not have the expected value for it's FullPath property.", i));

				// TODO : Check the value of the EventData.Size property.
				//
			}
		}

		[Test]
		public void NotifyOnFileCreate()
		{
			// The filenames used (in order) to create the temporary files.
			var Filenames = new string[5];
			for (var i = 0; i < Filenames.Length; i++)
			{
				Filenames[i] = Guid.NewGuid().ToString("D") + ".txt";
			}

			// Holds data returned by the event under test.
			var EventDataList = new List<FileSystemEventArgs>();

			// Create a FileSystemWatcher instance.
			using (var watcher = new FileSystemWatcher(m_testBasePath))
			{
				// Attach an event handler which records data returned by the event.
				watcher.Created += (sender, e) =>
				{
					lock (EventDataList)
					{
						EventDataList.Add(e);
					}
				};

				// Enable event types so we capture the events we're looking for.
				watcher.NotifyFilter = c_notifyFiltersAll;

				// Enable raising of events.
				watcher.EnableRaisingEvents = true;

				// Create the temporary files.
				foreach (var Filename in Filenames)
				{
					// Sleep for a short period of time to allow the events to be throttled, if necessary.
					if (s_fileSystemOperationSleepDuration != TimeSpan.Zero)
					{
						Thread.Sleep(s_fileSystemOperationSleepDuration);
					}

					var FilePath = Path.Combine(m_testBasePath, Filename);
					File.WriteAllText(FilePath, "Testing, testing, 1, 2, 3");
				}

				// Sleep for a short period of time to allow the events to be throttled, if necessary.
				if (s_fileSystemOperationSleepDuration != TimeSpan.Zero)
				{
					Thread.Sleep(s_fileSystemOperationSleepDuration);
				}
			}

			// Check the captured event data to make sure it matches our expectations.
			Assert.AreEqual(Filenames.Length, EventDataList.Count,
				"The event data list does not contain the correct number of items.");
			for (var i = 0; i < Filenames.Length; i++)
			{
				var EventData = EventDataList[i];
				Assert.AreEqual(EventData.ChangeType, WatcherChangeTypes.Created,
					String.Format("The event data at index {0} does not have the expected value for it's ChangeType property.", i));

				var FilePath = Path.Combine(m_testBasePath, Filenames[i]);
				Assert.AreEqual(FilePath, EventData.FullPath,
					String.Format("The event data at index {0} does not have the expected value for it's FullPath property.", i));
			}
		}

		[Test]
		public void NotifyOnFileDelete()
		{
			// The filenames used (in order) to create the temporary files.
			var Filenames = new string[5];
			for (var i = 0; i < Filenames.Length; i++)
			{
				Filenames[i] = Guid.NewGuid().ToString("D") + ".txt";
			}

			// Create the temporary files.
			foreach (var Filename in Filenames)
			{
				var FilePath = Path.Combine(m_testBasePath, Filename);
				File.WriteAllText(FilePath, "Testing, testing, 1, 2, 3");
			}

			// Holds data returned by the event under test.
			var EventDataList = new List<FileSystemEventArgs>();

			// Create a FileSystemWatcher instance.
			using (var watcher = new FileSystemWatcher(m_testBasePath))
			{
				// Attach an event handler which records data returned by the event.
				watcher.Deleted += (sender, e) =>
				{
					lock (EventDataList)
					{
						EventDataList.Add(e);
					}
				};

				// Enable event types so we capture the events we're looking for.
				watcher.NotifyFilter = c_notifyFiltersAll;

				// Enable raising of events.
				watcher.EnableRaisingEvents = true;

				// Delete the temporary files.
				foreach (var Filename in Filenames)
				{
					// Sleep for a short period of time to allow the events to be throttled, if necessary.
					if (s_fileSystemOperationSleepDuration != TimeSpan.Zero)
					{
						Thread.Sleep(s_fileSystemOperationSleepDuration);
					}

					var FilePath = Path.Combine(m_testBasePath, Filename);
					File.Delete(FilePath);
				}

				// Sleep for a short period of time to allow the events to be throttled, if necessary.
				if (s_fileSystemOperationSleepDuration != TimeSpan.Zero)
				{
					Thread.Sleep(s_fileSystemOperationSleepDuration);
				}
			}

			// Check the captured event data to make sure it matches our expectations.
			Assert.AreEqual(Filenames.Length, EventDataList.Count,
				"The event data list does not contain the correct number of items.");
			for (var i = 0; i < Filenames.Length; i++)
			{
				var EventData = EventDataList[i];
				Assert.AreEqual(EventData.ChangeType, WatcherChangeTypes.Deleted,
					String.Format("The event data at index {0} does not have the expected value for it's ChangeType property.", i));

				var FilePath = Path.Combine(m_testBasePath, Filenames[i]);
				Assert.AreEqual(FilePath, EventData.FullPath,
					String.Format("The event data at index {0} does not have the expected value for it's FullPath property.", i));

				// TODO : Check the value of the EventData.Size property.
				//
			}
		}

		[Test]
		public void NotifyOnFileRename()
		{
			// The filenames used (in order) to create, then rename the temporary file.
			var Filenames = new string[5];
			for (var i = 0; i < Filenames.Length; i++)
			{
				Filenames[i] = Guid.NewGuid().ToString("D") + ".txt";
			}

			// Create the temporary file.
			var FilePath = Path.Combine(m_testBasePath, Filenames[0]);
			File.WriteAllText(FilePath, "Testing, testing, 1, 2, 3");

			// Holds data returned by the event under test.
			var EventDataList = new List<RenamedEventArgs>();
			
			// Create a FileSystemWatcher instance.
			using (var watcher = new FileSystemWatcher(m_testBasePath))
			{
				// Attach an event handler which records data returned by the event.
				watcher.Renamed += (sender, e) =>
				{
					lock (EventDataList)
					{
						EventDataList.Add(e);
					}
				};

				// Enable event types so we capture the events we're looking for.
				watcher.NotifyFilter = c_notifyFiltersAll;

				// Enable raising of events.
				watcher.EnableRaisingEvents = true;

				// Rename the file a few times.
				for (var i = 1; i < Filenames.Length; i++)
				{
					// Sleep for a short period of time to allow the events to be throttled, if necessary.
					if (s_fileSystemOperationSleepDuration != TimeSpan.Zero)
					{
						Thread.Sleep(s_fileSystemOperationSleepDuration);
					}

					var CurrentFilePath = Path.Combine(m_testBasePath, Filenames[i - 1]);
					var NewFilePath = Path.Combine(m_testBasePath, Filenames[i]);

					// Rename the file by moving it within the same folder.
					File.Move(CurrentFilePath, NewFilePath);
				}

				// Sleep for a short period of time to allow the events to be throttled, if necessary.
				if (s_fileSystemOperationSleepDuration != TimeSpan.Zero)
				{
					Thread.Sleep(s_fileSystemOperationSleepDuration);
				}
			}

			// Check the captured event data to make sure it matches our expectations.
			Assert.AreEqual(Filenames.Length - 1, EventDataList.Count,
				"The event data list does not contain the correct number of items.");
			for (var i = 1; i < Filenames.Length; i++)
			{
				var EventData = EventDataList[i - 1];
				Assert.AreEqual(EventData.ChangeType, WatcherChangeTypes.Renamed,
					String.Format("The event data at index {0} does not have the expected value for it's ChangeType property.", i - 1)); 

				var OldFilePath = Path.Combine(m_testBasePath, Filenames[i - 1]);
				Assert.AreEqual(OldFilePath, EventData.OldFullPath,
					String.Format("The event data at index {0} does not have the expected value for it's OldFullPath property.", i - 1));

				var NewFilePath = Path.Combine(m_testBasePath, Filenames[i]);
				Assert.AreEqual(NewFilePath, EventData.FullPath,
					String.Format("The event data at index {0} does not have the expected value for it's FullPath property.", i - 1));
			}
		}
	}
}

#endif
