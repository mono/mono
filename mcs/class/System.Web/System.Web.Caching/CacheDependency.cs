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
		string [] filenames;
		bool changed;
		bool disposed;
		CacheEntry [] entries;
		CacheItemRemovedCallback removedDelegate;
		EventHandler changedDelegate;

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
			this.start = start;
			if (filenames == null)
				this.filenames = noStrings;
				
			foreach (string file in filenames) {
				if (file == null)
					throw new ArgumentNullException ("filenames");
			}

			this.filenames = filenames;
			
			if (cachekeys == null)
				cachekeys = noStrings;

			foreach (string ck in cachekeys) {
				if (ck == null)
					throw new ArgumentNullException ("cachekeys");
			}

			if (dependency == null)
				dependency = noDependency;


			this.changed = dependency.changed;
			if (changed == true)
				return;

			int nentries = cachekeys.Length + ((dependency.entries == null) ? 0 :
								  dependency.entries.Length);
			if (nentries != 0) {
				this.removedDelegate = new CacheItemRemovedCallback (CacheItemRemoved);
				this.entries = new CacheEntry [nentries];
				if (dependency.entries != null) {
					int i = 0;
					foreach (CacheEntry entry in dependency.entries) {
						entry._onRemoved += removedDelegate;
						i++;
					}
				}
			}

			if (filenames.Length > 0) {
				this.changedDelegate = new EventHandler (OnChanged);
				foreach (string s in filenames)
					Watcher.AddWatch (s, changedDelegate);
			}
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

