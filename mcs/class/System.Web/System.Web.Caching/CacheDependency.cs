// 
// System.Web.Caching.CacheDependency
//
// Authors:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Copyright Patrik Torstensson, 2001
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//
using System;
using System.IO;
using System.Web;

namespace System.Web.Caching
{
	internal class CacheDependencyChangedArgs : EventArgs
	{
		string key;

		public CacheDependencyChangedArgs (string key)
		{
			this.key = key;
		}

		public string Key {
			get { return key; }
		}
	}

	internal delegate void CacheDependencyChangedHandler (object sender, CacheDependencyChangedArgs args);

	public sealed class CacheDependency : IDisposable
	{
		static string [] noStrings = new string [0];
		static CacheDependency noDependency = new CacheDependency ();
		DateTime start;
		bool changed;
		bool disposed;
		CacheEntry [] entries;
		CacheItemRemovedCallback removedDelegate;
		FileSystemWatcher [] watchers;

		private CacheDependency ()
		{
		}

		public CacheDependency (string filename)
			: this (filename, DateTime.MaxValue)
		{
		}

		public CacheDependency (string filename, DateTime start)
			: this (new string [] {filename}, null, null, start)
		{
		}

		public CacheDependency (string [] filenames)
			: this (filenames, null, null, DateTime.MaxValue)
		{
		}

		public CacheDependency (string [] filenames, DateTime start)
			: this (filenames, null, null, start)
		{
		}

		public CacheDependency (string [] filenames, string [] cachekeys)
			: this (filenames, cachekeys, null, DateTime.MaxValue)
		{
		}

		public CacheDependency (string [] filenames, string [] cachekeys, DateTime start)
			: this (filenames, cachekeys, null, start)
		{
		}

		public CacheDependency (string [] filenames, string [] cachekeys, CacheDependency dependency)
			: this (filenames, cachekeys, dependency, DateTime.MaxValue)
		{
		}

		public CacheDependency (string [] filenames,
					string [] cachekeys,
					CacheDependency dependency,
					DateTime start)
		{
			Cache cache = HttpRuntime.Cache;

			this.start = start;
			if (filenames == null)
				filenames = noStrings;

			foreach (string file in filenames) {
				if (file == null)
					throw new ArgumentNullException ("filenames");
			}

			if (cachekeys == null)
				cachekeys = noStrings;

			int missing_keys = 0;
			foreach (string ck in cachekeys) {
				if (ck == null)
					throw new ArgumentNullException ("cachekeys");
				if (cache.GetEntry (ck) == null)
					missing_keys++;
			}

			if (dependency == null)
				dependency = noDependency;


			this.changed = dependency.changed;
			if (changed == true)
				return;

			int nentries = cachekeys.Length + ((dependency.entries == null) ? 0 :
								  dependency.entries.Length) - missing_keys;

			if (nentries != 0) {
				this.removedDelegate = new CacheItemRemovedCallback (CacheItemRemoved);
				this.entries = new CacheEntry [nentries];
				
				int i = 0;
				if (dependency.entries != null) {
					foreach (CacheEntry entry in dependency.entries) {
						entry._onRemoved += removedDelegate;
						entries [i++] = entry;
					}
				}

				for (int c=0; c<cachekeys.Length; c++) {
					CacheEntry entry = cache.GetEntry (cachekeys [c]);
					if (entry == null)
						continue;
					entry._onRemoved += removedDelegate;
					entries [i++] = entry;
				}
			}

			if (filenames.Length > 0) {
				watchers = new FileSystemWatcher [filenames.Length];
				for (int i=0; i<filenames.Length; i++)
					watchers [i] = CreateWatcher (filenames [i]);
			}
		}

		private FileSystemWatcher CreateWatcher (string file)
		{
			FileSystemWatcher watcher = new FileSystemWatcher ();

			watcher.Path = Path.GetFullPath (Path.GetDirectoryName (file));
			watcher.Filter = Path.GetFileName (file);

			watcher.Changed += new FileSystemEventHandler (OnFileChanged);
			watcher.Created += new FileSystemEventHandler (OnFileChanged);
			watcher.Deleted += new FileSystemEventHandler (OnFileChanged);

			watcher.EnableRaisingEvents = true;

			return watcher;
		}

		private void OnFileChanged (object sender, FileSystemEventArgs e)
		{
			OnChanged (sender, e);
		}

		void CacheItemRemoved (string key, object value, CacheItemRemovedReason reason)
		{
			OnChanged (this, EventArgs.Empty);
		}

		void OnChanged (object sender, EventArgs args)
		{
			if (changed || disposed)
				return;

			changed = true;
			if (Changed != null)
				Changed (this, new CacheDependencyChangedArgs (null));
		}

		public void Dispose ()
		{
			for (int i=0; i<watchers.Length; i++)
				watchers [i].Dispose ();
		}

		public bool HasChanged
		{
			get { return changed; }
		}

		internal CacheEntry [] GetCacheEntries ()
		{
			return entries;
		}

		internal event CacheDependencyChangedHandler Changed;
	}
}

