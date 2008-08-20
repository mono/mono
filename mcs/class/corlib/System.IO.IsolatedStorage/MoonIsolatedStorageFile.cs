//
// System.IO.IsolatedStorage.MoonIsolatedStorageFile
//
// Moonlight's implementation for the IsolatedStorageFile
// 
// Authors
//      Miguel de Icaza (miguel@novell.com)
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

	public sealed class IsolatedStorageFile : IDisposable {
		static string appdir;
		
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

			string basedir;
			basedir = TryDirectory (Path.Combine (xdg_data_home, "moonlight"));
			if (basedir == null){
				//
				// Maybe try a few others?
				//
				return;
			}

			// FIXME: Use the actual url from the plugin for this.
			appdir = Path.Combine (basedir, "url");
			try {
				Directory.CreateDirectory (appdir);
			} catch {
				appdir = null;
			}
		}


		private bool removed = false;
		private bool disposed = false;

		internal IsolatedStorageFile (string basedir)
		{
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
			if (appdir == null)
				throw new SecurityException ();
			
			return new IsolatedStorageFile (appdir);
		}

		public static IsolatedStorageFile GetUserStoreForSite ()
		{
			if (appdir == null)
				throw new SecurityException ();
			
			return new IsolatedStorageFile (appdir);
		}

		internal static string Verify (string path)
		{
			try {
				string full = Path.GetFullPath (Path.Combine (appdir, path));
				if (full.StartsWith (appdir + Path.DirectorySeparatorChar))
					return full;
			} catch {
				throw new IsolatedStorageException ();
			}
			throw new IsolatedStorageException ();
		}
		
		public void CreateDirectory (string dir)
		{
			Verify (dir);
			Directory.CreateDirectory (dir);
		}

		public IsolatedStorageFileStream CreateFile (string path)
		{
			PreCheck ();
			if (path == null)
				throw new ArgumentNullException ("path");

			return new IsolatedStorageFileStream (path, FileMode.Create, this);
		}
		
		public void DeleteDirectory (string dir)
		{
			PreCheck ();
			Verify (dir);
			Directory.Delete (dir);
		}

		public bool DirectoryExists (string path)
		{
			PreCheck ();
			Verify (path);
			return Directory.Exists (path);
		}

		public bool FileExists (string path)
		{
			PreCheck ();
			Verify (path);
			return File.Exists (path);
		}

		public string [] GetDirectoryNames ()
		{
			return Directory.GetFiles (appdir);
		}

		public string [] GetDirectoryNames (string searchPattern)
		{
			if (searchPattern.IndexOf ('/') != -1)
				throw new IsolatedStorageException ();
			
			return Directory.GetDirectories (appdir, searchPattern);
		}

		public string [] GetFileNames ()
		{
			return Directory.GetFiles (appdir);
		}

		public string [] GetFileNames (string searchPattern)
		{
			if (searchPattern.IndexOf ('/') != -1)
				throw new IsolatedStorageException ();
			
			return Directory.GetFiles (appdir, searchPattern);
		}

		public void DeleteFile (string file)
		{
			Verify (file);
			File.Delete (file);
		}
		
		public void Dispose ()
		{
			disposed = true;
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
			if (path == null)
				throw new ArgumentNullException ("path");

			return new IsolatedStorageFileStream (path, mode, access, share, this);
		}

		public void Remove ()
		{
			PreCheck ();
			try {
				// TODO - try to clean out everything
			}
			finally {
				removed = true;
			}
		}

		public long AvailableFreeSpace {
			get {
				PreCheck ();
				return 1024*1024;
			}
		}

		public long Quota {
			get {
				PreCheck ();
				return 1024*1024;
			}
		}

		public bool IncreaseQuotaTo (long newQuotaSize)
		{
			PreCheck ();
			if (newQuotaSize <= Quota)
				throw new ArgumentException ("newQuotaSize", "Only increase is possible");

			return true;
		}
	}
}
#endif
