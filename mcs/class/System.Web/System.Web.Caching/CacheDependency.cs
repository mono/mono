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
using System.ComponentModel;
using System.IO;
using System.Security.Permissions;
#if NET_2_0
using System.Text;
#endif

namespace System.Web.Caching
{
#if NET_2_0
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CacheDependency: IDisposable {
#else
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class CacheDependency: IDisposable {
#endif
		static readonly object dependencyChangedEvent = new object ();
		string[] cachekeys;
		CacheDependency dependency;
		DateTime start;
		Cache cache;
		FileSystemWatcher[] watchers;
		bool hasChanged;
#if NET_2_0
		bool used;
		DateTime utcLastModified;
#endif
		object locker = new object ();
		EventHandlerList events = new EventHandlerList ();
		
		internal event EventHandler DependencyChanged {
			add { events.AddHandler (dependencyChangedEvent, value); }
			remove { events.RemoveHandler (dependencyChangedEvent, value); }
		}
		
#if NET_2_0
		public CacheDependency (): this (null, null, null, DateTime.Now)
		{
		}
#endif
		
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
			if (filenames != null) {
				watchers = new FileSystemWatcher [filenames.Length];
				for (int n=0; n<filenames.Length; n++) {
					FileSystemWatcher watcher = new FileSystemWatcher ();
					if (Directory.Exists (filenames [n])) {
						watcher.Path = filenames [n];
					} else {
						string parentPath = Path.GetDirectoryName (filenames [n]);
						if (parentPath != null && Directory.Exists (parentPath)) {
							watcher.Path = parentPath;
							watcher.Filter = Path.GetFileName (filenames [n]);
						} else
							continue;
					}
					watcher.NotifyFilter |= NotifyFilters.Size;
					watcher.Created += new FileSystemEventHandler (OnChanged);
					watcher.Changed += new FileSystemEventHandler (OnChanged);
					watcher.Deleted += new FileSystemEventHandler (OnChanged);
					watcher.Renamed += new RenamedEventHandler (OnChanged);
					watcher.EnableRaisingEvents = true;
					watchers [n] = watcher;
				}
			}
			this.cachekeys = cachekeys;
			this.dependency = dependency;
			if (dependency != null)
				dependency.DependencyChanged += new EventHandler (OnChildDependencyChanged);
			this.start = start;

#if NET_2_0
			FinishInit ();
#endif
		}

#if NET_2_0
		public virtual string GetUniqueID ()
		{
			StringBuilder sb = new StringBuilder ();
			lock (locker) {
				if (watchers != null)
					foreach (FileSystemWatcher fsw in watchers)
						if (fsw != null && fsw.Path != null && fsw.Path.Length != 0)
							sb.AppendFormat ("_{0}", fsw.Path);
			}

			if (cachekeys != null)
				foreach (string key in cachekeys)
					sb.AppendFormat ("_{0}", key);
			return sb.ToString ();
		}
#endif
		
		void OnChanged (object sender, FileSystemEventArgs args)
		{
			OnDependencyChanged (sender, args);
		}

		bool DoOnChanged ()
		{
			if (DateTime.Now < start)
				return false;
			hasChanged = true;
#if NET_2_0
			utcLastModified = DateTime.UtcNow;
#endif
			DisposeWatchers ();
			
			if (cache != null)
				cache.CheckDependencies ();

			return true;
		}
		
		void DisposeWatchers ()
		{
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

#if NET_2_0
		internal virtual void DependencyDisposeInternal ()
		{
		}
#endif
		
#if NET_2_0
		protected virtual
#endif
		void DependencyDispose () 
		{
#if NET_2_0
			DependencyDisposeInternal ();
#endif
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
#if NET_2_0
			used = c != null;
#endif
		}
		
#if NET_2_0
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
#endif
		
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
		
#if NET_2_0
		protected
#else
		internal
#endif
		void NotifyDependencyChanged (object sender, EventArgs e) 
		{
			OnDependencyChanged (sender, e);
		}


	}
}
