//
// FileChangeNotificationSystemEntry.cs
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
using System.Runtime.Caching.Hosting;
using System.Text;

namespace System.Runtime.Caching
{
	// This class is NOT thread-safe - the caller needs to assure access serialization when
	// calling Add/Remove
	sealed class FileChangeNotificationSystemEntry : IDisposable
	{
		string watchedDirPath;
		FileChangeNotificationSystem owner;
		FileSystemWatcher watcher;
		SortedDictionary <string, bool> filePaths;

		SortedDictionary <string, bool> FilePaths {
			get {
				if (filePaths == null)
					filePaths = new SortedDictionary <string, bool> (Helpers.StringComparer);
				return filePaths;
			}
		}

		public bool IsEmpty {
			get { return (filePaths == null || filePaths.Count == 0); }
		}
		
		public FileChangeNotificationSystemEntry (string dirPath, FileChangeNotificationSystem owner)
		{
			if (String.IsNullOrEmpty (dirPath))
				throw new ArgumentNullException ("dirPath");

			if (owner == null)
				throw new ArgumentNullException ("owner");
			
			watchedDirPath = dirPath;
			this.owner = owner;
		}
		
		public void Add (string filePath)
		{
			if (String.IsNullOrEmpty (filePath))
				return;

			bool start = false;
			try {
				if (watcher == null) {
					watcher = new FileSystemWatcher (watchedDirPath);
					watcher.IncludeSubdirectories = false;
					watcher.Filter = String.Empty;
					watcher.NotifyFilter = NotifyFilters.FileName |
						NotifyFilters.DirectoryName |
						NotifyFilters.Size |
						NotifyFilters.LastWrite |
						NotifyFilters.CreationTime;
					watcher.Changed += OnChanged;
					watcher.Created += OnChanged;
					watcher.Deleted += OnChanged;
					watcher.Renamed += OnRenamed;
				} else if (watcher.EnableRaisingEvents) {
					Stop ();
					start = true;
				}
			
				SortedDictionary <string, bool> filePaths = FilePaths;
				if (filePaths.ContainsKey (filePath))
					return;

				filePaths.Add (filePath, true);
			} finally {
				if (start)
					Start ();
			}
		}

		public void Remove (string filePath)
		{
			if (filePath == null)
				return;

			bool start = false;
			try {
				if (watcher != null && watcher.EnableRaisingEvents) {
					Stop ();
					start = true;
				}
				
				SortedDictionary <string, bool> filePaths = FilePaths;
				if (!filePaths.ContainsKey (filePath))
					return;

				filePaths.Remove (filePath);
			} finally {
				if (start)
					Start ();
			}
		}
		
		public void Start ()
		{
			if (watcher == null)
				return;

			watcher.EnableRaisingEvents = true;
		}

		public void Stop ()
		{
			if (watcher == null)
				return;

			watcher.EnableRaisingEvents = false;
		}

		public void Dispose ()
		{
			if (watcher == null)
				return;

			try {
				Stop ();
			} finally {	
				watcher.Dispose ();
			}
		}

		void IDisposable.Dispose ()
		{
			Dispose ();
		}
		
		void OnChanged (object source, FileSystemEventArgs e)
		{
			string fullPath = e.FullPath;
			if (owner == null || filePaths == null || !filePaths.ContainsKey (fullPath))
				return;
			
			owner.OnChanged (e.ChangeType, fullPath, null);
		}

		void OnRenamed (object source, RenamedEventArgs e)
		{
			string fullPath = e.FullPath;
			if (owner == null || filePaths == null || !filePaths.ContainsKey (fullPath))
				return;

			owner.OnChanged (e.ChangeType, e.OldFullPath, e.FullPath);
		}
	}
}
