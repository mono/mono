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

	// FIXME: we need to tool to set quota (SL does this on a property page of the "Silverlight Configuration" menu)
	// the beta2 user interface (that I only see in IE) does not seems to differentiate application and site storage

	public sealed class IsolatedStorageFile : IDisposable {

		// Since we can extend more than AvailableFreeSize we need to substract the "safety" value out of it
		private const int SafetyZone = 1024;

		static string isolated_root;
		static string isolated_appdir;
		static string isolated_sitedir;
		
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
			if (isolated_root == null){
				//
				// Maybe try a few others?
				//
				return;
			}

			// FIXME: we need to store quota (and any other config) outside the stores
			// - so they can't be modified by the application itself
			// - so it's not destroyed to Remove

			isolated_appdir = TryDirectory (Path.Combine (isolated_root, "application"));
			isolated_sitedir = TryDirectory (Path.Combine (isolated_root, "site"));
		}

		private string basedir;
		private long quota;
		private bool removed = false;
		private bool disposed = false;

		internal IsolatedStorageFile (string dir)
		{
			basedir = TryDirectory (dir);
			// FIXME: we need to read the quota allocated to this storage
			quota = 1024 * 1024;
			// FIXME: we need to compute the available space for this storage (and keep it updated)
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
			
			// FIXME: we need to construct something based on the application, like:
			// Application.Current.GetType ().FullName
			// of course this is outside corlib so we need prior notification of this
			return new IsolatedStorageFile (Path.Combine (isolated_appdir, "application"));
		}

		public static IsolatedStorageFile GetUserStoreForSite ()
		{
			if (isolated_sitedir == null)
				throw new SecurityException ();

			// FIXME: we need to construct something based on the site, like:
			// Application.Current.Host.Source.AbsoluteUri (or a subset of this)
			// of course this is outside corlib so we need prior notification of this
			return new IsolatedStorageFile (Path.Combine (isolated_sitedir, "site"));
		}

		internal string Verify (string path)
		{
			try {
				string full = Path.GetFullPath (Path.Combine (basedir, path));
				if (full.StartsWith (basedir + Path.DirectorySeparatorChar))
					return full;
			} catch {
				throw new IsolatedStorageException ();
			}
			throw new IsolatedStorageException ();
		}
		
		public void CreateDirectory (string dir)
		{
			Directory.CreateDirectory (Verify (dir));
		}

		public IsolatedStorageFileStream CreateFile (string path)
		{
			PreCheck ();
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path.Length == 0)
				throw new ArgumentException ("path");

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
			File.Delete (Verify (file));
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
				TryDirectory (basedir);
				removed = true;
			}
		}

		public long AvailableFreeSpace {
			get {
				PreCheck ();
				// FIXME: compute real free space
				// then substract the safety
				return 1024*1024 - SafetyZone;
			}
		}

		public long Quota {
			get {
				PreCheck ();
				return quota;
			}
		}

		public bool IncreaseQuotaTo (long newQuotaSize)
		{
			PreCheck ();
			if (newQuotaSize <= Quota)
				throw new ArgumentException ("newQuotaSize", "Only increase is possible");

			// FIXME: save new quota limit to configuration
			quota = newQuotaSize;
			return true;
		}

		internal bool CanExtend (long request)
		{
			return (request <= AvailableFreeSpace + SafetyZone);
		}
	}
}
#endif
