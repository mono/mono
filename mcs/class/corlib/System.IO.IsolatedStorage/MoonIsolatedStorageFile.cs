//
// System.IO.IsolatedStorage.MoonIsolatedStorageFile
//
// Moonlight's implementation for the IsolatedStorageFile
// 
// Authors
//      Miguel de Icaza (miguel@novell.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007, 2008 Novell, Inc (http://www.novell.com)
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
#if NET_2_1
using System;
using System.IO;
using System.Security;

namespace System.IO.IsolatedStorage {

	// Most of the time there will only be a single instance of both 
	// * Application Store (GetUserStoreForApplication)
	// * Site Store (GetUserStoreForSite)
	// However both can have multiple concurrent uses
	// E.g. another instance of the same application (same URL) running in another Moonlight instance
	// E.g. another application on the same site (i.e. host) for a site store

	// TODO: use shared memory (and locks) to keep the quota and used values synchronized

	// TODO: we need to be initialized by Application (System.Windows.dll) to know the correct root directories

	// FIXME: we need to tool to set quota (SL does this on a property page of the "Silverlight Configuration" menu)
	// the beta2 user interface (that I only see in IE) does not seems to differentiate application and site storage

	// notes:
	// * quota seems computed in (disk) blocks, i.e. a small file will have a (non-small) size

	public sealed class IsolatedStorageFile : IDisposable {

		private const long DefaultQuota = 1024 * 1024;
		// Since we can extend more than AvailableFreeSize we need to substract the "safety" value out of it
		private const int SafetyZone = 1024;

		static string isolated_root;
		static string isolated_appdir;
		static string isolated_sitedir;

		static object locker = new object ();
		
		static string TryDirectory (string path)
		{
			try {
				Directory.CreateDirectory (path);
				return path;
			} catch {
				return null;
			}
		}
		
		static IsolatedStorageFile ()
		{
                        string xdg_data_home = Environment.GetEnvironmentVariable ("XDG_DATA_HOME");
                        if (String.IsNullOrEmpty (xdg_data_home)) {
                                xdg_data_home = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
                        }

			string isolated_root;
			isolated_root = TryDirectory (Path.Combine (xdg_data_home, "moonlight"));
			if (isolated_root == null)
				throw new IsolatedStorageException ("No root");

			isolated_appdir = TryDirectory (Path.Combine (isolated_root, "application"));
			isolated_sitedir = TryDirectory (Path.Combine (isolated_root, "site"));
		}

		private string basedir;
		private string datafile;
		private long quota;
		private long used;
		private bool removed = false;
		private bool disposed = false;

		internal IsolatedStorageFile (string root, string dirname)
		{
			string dir = Path.Combine (root, dirname);
			basedir = TryDirectory (dir);

			datafile = Path.Combine (root, dirname + ".data");
			if (File.Exists (datafile)) {
				ReadStoreData ();
			} else {
				used = 0;
				quota = DefaultQuota;
				UpdateStoreData ();
			}
		}
		
		internal void PreCheck ()
		{
			if (disposed)
				throw new ObjectDisposedException ("Storage was disposed");
			if (removed)
				throw new IsolatedStorageException ("Storage was removed");
		}

		public static IsolatedStorageFile GetUserStoreForApplication ()
		{
			if (isolated_appdir == null)
				throw new SecurityException ();
			
			// from System.Windows.Application we made "xap_uri" correspond to
			//	 Application.Current.Host.Source.AbsoluteUri
			string app = (AppDomain.CurrentDomain.GetData ("xap_uri") as string);
			if (app == null)
				throw new SecurityException ();

			return new IsolatedStorageFile (isolated_appdir, app.Replace ("/", "%27"));
		}

		public static IsolatedStorageFile GetUserStoreForSite ()
		{
			if (isolated_sitedir == null)
				throw new SecurityException ();

			// from System.Windows.Application we made "xap_host" correspond to
			//	Application.Current.Host.Source.Host
			string site = (AppDomain.CurrentDomain.GetData ("xap_host") as string);
			if (site == null)
				throw new SecurityException ();
			// no host is defined for things like: file://home/...
			if (site.Length == 0)
				site = "localhost:file";

			return new IsolatedStorageFile (isolated_sitedir, site);
		}

		internal string Verify (string path)
		{
			// outside of try/catch since we want to get things like
			// 	ArgumentNullException for null paths
			//	ArgumentException for invalid characters
			string combined = Path.Combine (basedir, path);
			try {
				string full = Path.GetFullPath (combined);
				full = Path.GetFullPath (combined);
				if (full.StartsWith (basedir))
					return full;
			} catch {
				throw new IsolatedStorageException ();
			}
			throw new IsolatedStorageException ();
		}
		
		public void CreateDirectory (string dir)
		{
			PreCheck ();
			Directory.CreateDirectory (Verify (dir));
		}

		public IsolatedStorageFileStream CreateFile (string path)
		{
			PreCheck ();
			try {
				return new IsolatedStorageFileStream (path, FileMode.Create, this);
			}
			catch (DirectoryNotFoundException) {
				// this can happen if the supplied path includes an unexisting directory
				throw new IsolatedStorageException ();
			}
		}
		
		public void DeleteDirectory (string dir)
		{
			PreCheck ();
			Directory.Delete (Verify (dir));
		}

		public void DeleteFile (string file)
		{
			PreCheck ();
			string checked_filename = Verify (file);
			if (!File.Exists (checked_filename))
				throw new IsolatedStorageException ("File does not exists");
			File.Delete (checked_filename);
		}

		public void Dispose ()
		{
			disposed = true;
		}

		public bool DirectoryExists (string path)
		{
			PreCheck ();
			return Directory.Exists (Verify (path));
		}

		public bool FileExists (string path)
		{
			PreCheck ();
			return File.Exists (Verify (path));
		}

		private string HideAppDir (string path)
		{
			// remove the "isolated" part of the path (and the extra '/')
			return path.Substring (basedir.Length + 1);
		}

		private string [] HideAppDirs (string[] paths)
		{
			for (int i=0; i < paths.Length; i++)
				paths [i] = HideAppDir (paths [i]);
			return paths;
		}

		public string [] GetDirectoryNames ()
		{
			return HideAppDirs (Directory.GetDirectories (basedir));
		}

		public string [] GetDirectoryNames (string searchPattern)
		{
			if (searchPattern.IndexOf ('/') != -1)
				throw new IsolatedStorageException ();
			
			return HideAppDirs (Directory.GetDirectories (basedir, searchPattern));
		}

		public string [] GetFileNames ()
		{
			return HideAppDirs (Directory.GetFiles (basedir));
		}

		public string [] GetFileNames (string searchPattern)
		{
			if (searchPattern.IndexOf ('/') != -1)
				throw new IsolatedStorageException ();
			
			return HideAppDirs (Directory.GetFiles (basedir, searchPattern));
		}

		public IsolatedStorageFileStream OpenFile (string path, FileMode mode)
		{
			return OpenFile (path, mode, FileAccess.ReadWrite, FileShare.None);
		}

		public IsolatedStorageFileStream OpenFile (string path, FileMode mode, FileAccess access)
		{
			return OpenFile (path, mode, access, FileShare.None);
		}

		public IsolatedStorageFileStream OpenFile (string path, FileMode mode, FileAccess access, FileShare share)
		{
			PreCheck ();
			return new IsolatedStorageFileStream (path, mode, access, share, this);
		}

		public void Remove ()
		{
			PreCheck ();
			try {
				Directory.Delete (basedir, true);
			}
			finally {
				used = 0;
				quota = DefaultQuota;
				UpdateStoreData ();
				TryDirectory (basedir);
				removed = true;
			}
		}

		// note: available free space could be changed from another application (same URL, another ML instance) or
		// another application on the same site
		public long AvailableFreeSpace {
			get {
				PreCheck ();
				ReadStoreData ();
				return quota - used - SafetyZone;
			}
		}

		// note: quota could be changed from another application (same URL, another ML instance) or
		// another application on the same site
		public long Quota {
			get {
				PreCheck ();
				ReadStoreData ();
				return quota;
			}
		}

		public bool IncreaseQuotaTo (long newQuotaSize)
		{
			PreCheck ();

			if (newQuotaSize <= quota)
				throw new ArgumentException ("newQuotaSize", "Only increases are possible");

			// FIXME: we must ensure this is called from an event handler (or return false)
			// we need to find out where it's valid (e.g. Page.Loaded and Page.MouseLeftButtonUp are not)

			// FIXME: need plugin UI to confirm the change

			return false;
		}

		internal bool CanExtend (long request)
		{
			bool result = (request <= AvailableFreeSpace + SafetyZone);
			if (result) {
				lock (locker) {
					used += request;
				}
			}
			return result;
		}

		// TEMPORARY - not thread (or cross process) safe

		private byte [] data = new byte [16];

		internal void ReadStoreData ()
		{
			using (FileStream fs = new FileStream (datafile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				fs.Read (data, 0, 16);
				quota = BitConverter.ToInt64 (data, 0);
				used = BitConverter.ToInt64 (data, 8);
			}
		}

		internal void UpdateStoreData ()
		{
			using (FileStream fs = new FileStream (datafile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)) {
				fs.Write (BitConverter.GetBytes (quota), 0, 8);
				fs.Write (BitConverter.GetBytes (used), 0, 8);
			}
		}
	}
}
#endif
