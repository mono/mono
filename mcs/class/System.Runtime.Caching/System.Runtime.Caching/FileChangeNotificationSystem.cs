//
// FileChangeNotificationSystem.cs
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
	sealed class FileChangeNotificationSystem : IFileChangeNotificationSystem, IDisposable
	{
		static readonly object watchers_lock = new object ();
		
		Dictionary <string, FileChangeNotificationSystemEntry> watchers;
		OnChangedCallback callback;

		Dictionary <string, FileChangeNotificationSystemEntry> Watchers {
			get {
				if (watchers == null)
					watchers = new Dictionary <string, FileChangeNotificationSystemEntry> (Helpers.StringEqualityComparer);

				return watchers;
			}
		}
		
		public void StartMonitoring (string filePath, OnChangedCallback onChangedCallback, out object state, out DateTimeOffset lastWriteTime, out long fileSize)
		{
			if (String.IsNullOrEmpty (filePath))
				throw new ArgumentNullException ("filePath");
			
			callback = onChangedCallback;

			string parentDir;
			if (File.Exists (filePath)) {
				var fi = new FileInfo (filePath);
				lastWriteTime = DateTimeOffset.FromFileTime (fi.LastWriteTimeUtc.Ticks);
				fileSize = fi.Length;
				parentDir = Path.GetDirectoryName (filePath);
			} else if (Directory.Exists (filePath)) {
				var di = new DirectoryInfo (filePath);
				lastWriteTime = DateTimeOffset.FromFileTime (di.LastWriteTimeUtc.Ticks);
				fileSize = -1L;
				parentDir = filePath;
			} else {
				lastWriteTime = DateTimeOffset.MaxValue;
				fileSize = -1L;

				if (filePath.LastIndexOf (Path.DirectorySeparatorChar) != -1)
					parentDir = Path.GetDirectoryName (filePath);
				else
					parentDir = filePath;
			}

			FileChangeNotificationSystemEntry entry;
			lock (watchers_lock) {
				Dictionary <string, FileChangeNotificationSystemEntry> watchers = Watchers;

				if (!watchers.TryGetValue (parentDir, out entry)) {
					entry = new FileChangeNotificationSystemEntry (parentDir, this);
					watchers.Add (parentDir, entry);
				}

				entry.Add (filePath);
				entry.Start ();
			}

			state = entry;
		}

		public void StopMonitoring (string filePath, object state)
		{
			if (String.IsNullOrEmpty (filePath))
				return;

			var entry = state as FileChangeNotificationSystemEntry;
			if (entry == null)
				return;

			lock (watchers_lock) {
				entry.Remove (filePath);
				if (entry.IsEmpty) {
					if (watchers != null && watchers.ContainsKey (filePath))
						watchers.Remove (filePath);
					
					entry.Stop ();
					entry.Dispose ();
				}
			}
		}

		public void OnChanged (WatcherChangeTypes changeType, string filePath, string newFilePath)
		{
			if (callback == null)
				return;
			
			FileChangeNotificationSystemEntry entry;

			if (watchers == null || !watchers.TryGetValue (filePath, out entry))
				entry = null;

			callback (entry);
		}
		
		public void Dispose ()
		{
			lock (watchers_lock) {
				if (watchers == null || watchers.Count == 0)
					return;

				FileChangeNotificationSystemEntry entry;
				foreach (var de in watchers) {
					entry = de.Value;
					if (entry == null)
						continue;

					entry.Stop ();
					entry.Dispose ();
				}

				watchers.Clear ();
			}
		}

		void IDisposable.Dispose ()
		{
			Dispose ();
		}
	}
}
