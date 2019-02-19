//
// System.Web.Caching.CachedDependency
//
// Author(s):
//  Lluis Sanchez (lluis@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Permissions;
using System.Text;

namespace System.Web.Caching
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CacheDependency: IDisposable
	{
		static readonly object dependencyChangedEvent = new object ();
		string[] cachekeys;
		CacheDependency dependency;
		DateTime start;
		Cache cache;
		FileSystemWatcher[] watchers;
		static readonly bool useSharedWatchers = Environment.GetEnvironmentVariable ("MONO_SYSTEMWEB_CACHEDEPENDENCY_SHARED_FSW") != null;
		static readonly Dictionary<string, FileSystemWatcher> sharedWatchers = new Dictionary<string, FileSystemWatcher> ();
		bool hasChanged;
		bool used;
		DateTime utcLastModified;
		static readonly object locker = new object ();
		EventHandlerList events = new EventHandlerList ();
		
		internal event EventHandler DependencyChanged {
			add { events.AddHandler (dependencyChangedEvent, value); }
			remove { events.RemoveHandler (dependencyChangedEvent, value); }
		}

		protected
		CacheDependency (): this (null, null, null, DateTime.Now)
		{
		}
		
		public CacheDependency (string filename): this (new string[] { filename }, null, null, DateTime.Now)
		{
		}
		
		public CacheDependency (string[] filenames): this (filenames, null, null, DateTime.Now)
		{
		}
		
		public CacheDependency (string filename, DateTime start): this (new string[] { filename }, null, null, start)
		{
		}

		public CacheDependency (string [] filenames, DateTime start)
			: this (filenames, null, null, start)
		{
		}

		public CacheDependency (string[] filenames, string[] cachekeys): this (filenames, cachekeys, null, DateTime.Now)
		{
		}
		
		public CacheDependency (string[] filenames, string[] cachekeys, CacheDependency dependency): this (filenames, cachekeys, dependency, DateTime.Now)
		{
		}
		
		public CacheDependency (string[] filenames, string[] cachekeys, DateTime start): this (filenames, cachekeys, null, start)
		{
		}
		
		public CacheDependency (string[] filenames, string[] cachekeys, CacheDependency dependency, DateTime start)
		{
			int flen = filenames != null ? filenames.Length : 0;
			
			if (flen > 0) {
				watchers = new FileSystemWatcher [flen];
				string filename;
				
				for (int n = 0; n < flen; n++) {
					filename = filenames [n];
					if (String.IsNullOrEmpty (filename))
						continue;
					string path = null;
					string filter = null;
					if (Directory.Exists (filename))
						path = filename;
					else {
						string parentPath = Path.GetDirectoryName (filename);
						if (parentPath != null && Directory.Exists (parentPath)) {
							path = parentPath;
							filter = Path.GetFileName (filename);
						} else
							continue;
					}

					lock (locker) {
						FileSystemWatcher watcher;
						if (useSharedWatchers) {
							if (!sharedWatchers.TryGetValue (path, out watcher)) {
								watcher = new FileSystemWatcher ();
								watcher.Path = path;
								watcher.NotifyFilter |= NotifyFilters.Size;
								watcher.Created += new FileSystemEventHandler ((s, e) => { if (filter == null || e.Name == filter) OnChanged (s, e); });
								watcher.Changed += new FileSystemEventHandler ((s, e) => { if (filter == null || e.Name == filter) OnChanged (s, e); });
								watcher.Deleted += new FileSystemEventHandler ((s, e) => { if (filter == null || e.Name == filter) OnChanged (s, e); });
								watcher.Renamed += new RenamedEventHandler ((s, e) => { if (filter == null || e.OldName == filter) OnChanged (s, e); });
								watcher.EnableRaisingEvents = true;
								sharedWatchers [path] = watcher;
							}
						} else {
							watcher = new FileSystemWatcher ();
							watcher.Path = path;
							if (filter != null)
								watcher.Filter = filter;
							watcher.NotifyFilter |= NotifyFilters.Size;
							watcher.Created += new FileSystemEventHandler (OnChanged);
							watcher.Changed += new FileSystemEventHandler (OnChanged);
							watcher.Deleted += new FileSystemEventHandler (OnChanged);
							watcher.Renamed += new RenamedEventHandler (OnChanged);
							watcher.EnableRaisingEvents = true;
							watchers [n] = watcher;
						}
					}
				}
			}
			this.cachekeys = cachekeys;
			this.dependency = dependency;
			if (dependency != null)
				dependency.DependencyChanged += new EventHandler (OnChildDependencyChanged);
			this.start = start;

			FinishInit ();
		}

		public virtual string GetUniqueID ()
		{
			var sb = new StringBuilder ();
			
			lock (locker) {
				FileSystemWatcher[] watcherList;
				
				if (useSharedWatchers) {
					watcherList = new FileSystemWatcher[sharedWatchers.Count];
					sharedWatchers.Values.CopyTo (watcherList, 0);
				} else {
					watcherList = watchers;
				}

				if (watcherList != null)
					foreach (FileSystemWatcher fsw in watcherList)
						if (fsw != null && fsw.Path != null && fsw.Path.Length != 0)
							sb.Append ("_" + fsw.Path);
			}

			if (cachekeys != null)
				foreach (string key in cachekeys)
					sb.AppendFormat ("_" + key);
			return sb.ToString ();
		}
		
		void OnChanged (object sender, FileSystemEventArgs args)
		{
			OnDependencyChanged (sender, args);
		}

		bool DoOnChanged ()
		{
			DateTime now = DateTime.Now;
			
			if (now < start)
				return false;
			hasChanged = true;
			utcLastModified = now.ToUniversalTime ();
			DisposeWatchers ();
			
			if (cache != null)
				cache.CheckDependencies ();

			return true;
		}
		
		void DisposeWatchers ()
		{
			if (useSharedWatchers) return;

			lock (locker) {
				if (watchers != null) {
					foreach (FileSystemWatcher w in watchers)
						if (w != null)
							w.Dispose ();
				}
				watchers = null;
			}
		}

		public void Dispose ()
		{
			DependencyDispose ();
		}

		internal virtual void DependencyDisposeInternal ()
		{
		}
		
		protected virtual void DependencyDispose () 
		{
			DependencyDisposeInternal ();
			DisposeWatchers ();
			if (dependency != null) {
				dependency.DependencyChanged -= new EventHandler (OnChildDependencyChanged);
				dependency.Dispose ();
			}
			
			cache = null;
		}
		
		internal void SetCache (Cache c)
		{
			cache = c;
			used = c != null;
		}
		
		protected internal void FinishInit () 
		{
			utcLastModified = DateTime.UtcNow;
		}

		internal bool IsUsed {
			get { return used; }
		}

		internal DateTime Start {
			get { return start; }
			set { start = value; }
		}

		public DateTime UtcLastModified {
			get {
				return utcLastModified;
			}
		}

		protected void SetUtcLastModified (DateTime utcLastModified) 
		{
			this.utcLastModified = utcLastModified;
		}
		
		public bool HasChanged {
			get {
				if (hasChanged)
					return true;

				if (DateTime.Now < start)
					return false;

				if (cache != null && cachekeys != null) {
					foreach (string key in cachekeys) {
						if (cache.GetKeyLastChange (key) > start) {
							hasChanged = true;
							break;
						}
					}
				}
				if (hasChanged)
					DisposeWatchers ();

				return hasChanged;
			}
		}
		
		void OnChildDependencyChanged (object o, EventArgs e)
		{
			hasChanged = true;
			OnDependencyChanged (o, e);
		}
		
		void OnDependencyChanged (object sender, EventArgs e)
		{
			if (!DoOnChanged ())
				return;

			EventHandler eh = events [dependencyChangedEvent] as EventHandler;
			if (eh != null)
				eh (sender, e);
		}		

		protected void NotifyDependencyChanged (object sender, EventArgs e) 
		{
			OnDependencyChanged (sender, e);
		}
	}
}
