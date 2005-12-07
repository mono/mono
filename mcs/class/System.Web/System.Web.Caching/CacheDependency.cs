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
using System.IO;
using System.Security.Permissions;

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
		string[] cachekeys;
		CacheDependency dependency;
		DateTime start;
		Cache cache;
#if !TARGET_JVM
		FileSystemWatcher[] watchers;
#endif
		bool hasChanged;
		object locker = new object ();
		
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
#if !TARGET_JVM
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
					watcher.Created += new FileSystemEventHandler (OnChanged);
					watcher.Changed += new FileSystemEventHandler (OnChanged);
					watcher.Deleted += new FileSystemEventHandler (OnChanged);
					watcher.Renamed += new RenamedEventHandler (OnChanged);
					watcher.EnableRaisingEvents = true;
					watchers [n] = watcher;
				}
			}
#endif
			this.cachekeys = cachekeys;
			this.dependency = dependency;
			if (dependency != null)
				dependency.DependencyChanged += new EventHandler (OnChildDependencyChanged);
			this.start = start;
		}
		
		void OnChanged (object sender, FileSystemEventArgs args)
		{
			if (DateTime.Now < start)
				return;
			hasChanged = true;
			DisposeWatchers ();
			
			if (cache != null)
				cache.CheckExpiration ();
		}
		
#if TARGET_JVM
		void DisposeWatchers ()
		{
		}
#else
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
#endif
		public void Dispose ()
		{
			DisposeWatchers ();
			if (dependency != null)
				dependency.DependencyChanged -= new EventHandler (OnChildDependencyChanged);
			cache = null;
		}
		
		internal void SetCache (Cache c)
		{
			cache = c;
		}
		
		public bool HasChanged {
			get {
				if (hasChanged)
					return true;

				if (DateTime.Now < start)
					return false;

				if (cache != null) {
					if (cachekeys != null) {
						foreach (string key in cachekeys) {
							if (cache.GetKeyLastChange (key) > start) {
								hasChanged = true;
								break;
							}
						}
					}
				}
				if (hasChanged)
					DisposeWatchers ();

				return hasChanged;
			}
		}
		
		void OnChildDependencyChanged (object o, EventArgs a)
		{
			hasChanged = true;
			OnDependencyChanged ();
		}
		
		void OnDependencyChanged ()
		{
			if (DependencyChanged != null)
				DependencyChanged (this, null);
		}
		
		internal event EventHandler DependencyChanged;
	}
}
