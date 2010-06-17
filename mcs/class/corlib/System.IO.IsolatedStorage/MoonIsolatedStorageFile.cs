//
// System.IO.IsolatedStorage.MoonIsolatedStorageFile
//
// Moonlight's implementation for the IsolatedStorageFile
// 
// Authors
//      Miguel de Icaza (miguel@novell.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007, 2008, 2009 Novell, Inc (http://www.novell.com)
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
#if MOONLIGHT
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace System.IO.IsolatedStorage {

	// Most of the time there will only be a single instance of both 
	// * Application Store (GetUserStoreForApplication)
	// * Site Store (GetUserStoreForSite)
	// However both can have multiple concurrent uses, e.g.
	// * another instance of the same application (same URL) running in another Moonlight instance
	// * another application on the same site (i.e. host) for a site store
	// and share the some quota, i.e. a site and all applications on the sites share the same space

	// notes:
	// * quota seems computed in (disk) blocks, i.e. a small file will have a (non-small) size
	// e.g. every files and directories entries takes 1KB

	public sealed class IsolatedStorageFile : IDisposable {

		static object locker = new object ();
	
		private string basedir;
		private long used;
		private bool removed = false;
		private bool disposed = false;

		internal IsolatedStorageFile (string root)
		{
			basedir = root;
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
			return new IsolatedStorageFile (IsolatedStorage.ApplicationPath);
		}

		public static IsolatedStorageFile GetUserStoreForSite ()
		{
			return new IsolatedStorageFile (IsolatedStorage.SitePath);
		}

		internal string Verify (string path)
		{
			// special case: 'path' would be returned (instead of combined)
			if ((path.Length > 0) && (path [0] == '/'))
				path = path.Substring (1, path.Length - 1);

			// outside of try/catch since we want to get things like
			//	ArgumentException for invalid characters
			string combined = Path.Combine (basedir, path);
			try {
				string full = Path.GetFullPath (combined);
				if (full.StartsWith (basedir))
					return full;
			} catch {
				// we do not supply an inner exception since it could contains details about the path
				throw new IsolatedStorageException ();
			}
			throw new IsolatedStorageException ();
		}
		
		public static bool IsEnabled {
			get {
				Console.WriteLine ("NIEX: System.IO.IsolatedStorage.IsolatedStorageFile:get_IsEnabled");
				throw new NotImplementedException ();
			}
		}

		public void CreateDirectory (string dir)
		{
			PreCheck ();
			if (dir == null)
				throw new ArgumentNullException ("dir");
			// empty dir is ignored
			if (dir.Length > 0)
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
			if (dir == null)
				throw new ArgumentNullException ("dir");
			Directory.Delete (Verify (dir));
		}

		public void DeleteFile (string file)
		{
			PreCheck ();
			if (file == null)
				throw new ArgumentNullException ("file");
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

		private void CheckSearchPattern (string searchPattern)
		{
			if (searchPattern == null)
				throw new ArgumentNullException ("searchPattern");
			if (searchPattern.Length == 0)
				throw new IsolatedStorageException ("searchPattern");
			if (searchPattern.IndexOfAny (Path.GetInvalidPathChars ()) != -1)
				throw new ArgumentException ("searchPattern");
		}

		public string [] GetDirectoryNames ()
		{
			return HideAppDirs (Directory.GetDirectories (basedir));
		}

		public string [] GetDirectoryNames (string searchPattern)
		{
			CheckSearchPattern (searchPattern);

			// note: IsolatedStorageFile accept a "dir/file" pattern which is not allowed by DirectoryInfo
			// so we need to split them to get the right results
			string path = Path.GetDirectoryName (searchPattern);
			string pattern = Path.GetFileName (searchPattern);
			string [] afi = null;

			if (path == null || path.Length == 0) {
				return HideAppDirs (Directory.GetDirectories (basedir, searchPattern));
			} else {
				// we're looking for a single result, identical to path (no pattern here)
				// we're also looking for something under the current path (not outside isolated storage)

				string [] subdirs = Directory.GetDirectories (basedir, path);
				if (subdirs.Length != 1 || subdirs [0].IndexOf (basedir) < 0)
					throw new IsolatedStorageException ();

				DirectoryInfo dir = new DirectoryInfo (subdirs [0]);
				if (dir.Name != path)
					throw new IsolatedStorageException ();

				return GetNames (dir.GetDirectories (pattern));
			}
		}

		public string [] GetFileNames ()
		{
			return HideAppDirs (Directory.GetFiles (basedir));
		}

		public string [] GetFileNames (string searchPattern)
		{
			CheckSearchPattern (searchPattern);

			// note: IsolatedStorageFile accept a "dir/file" pattern which is not allowed by DirectoryInfo
			// so we need to split them to get the right results
			string path = Path.GetDirectoryName (searchPattern);
			string pattern = Path.GetFileName (searchPattern);
			string [] afi = null;

			if (path == null || path.Length == 0) {
				return HideAppDirs (Directory.GetFiles (basedir, searchPattern));
			} else {
				// we're looking for a single result, identical to path (no pattern here)
				// we're also looking for something under the current path (not outside isolated storage)

				string [] subdirs = Directory.GetDirectories (basedir, path);
				if (subdirs.Length != 1 || subdirs [0].IndexOf (basedir) < 0)
					throw new IsolatedStorageException ();

				DirectoryInfo dir = new DirectoryInfo (subdirs [0]);
				if (dir.Name != path)
					throw new IsolatedStorageException ();

				return GetNames (dir.GetFiles (pattern));
			}
		}

		// Return the file name portion of a full path
		private string[] GetNames (FileSystemInfo[] afsi)
		{
			string[] r = new string[afsi.Length];
			for (int i = 0; i != afsi.Length; ++i)
				r[i] = afsi[i].Name;
			return r;
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
			IsolatedStorage.Remove (basedir);
			removed = true;
		}

		// note: available free space could be changed from another application (same URL, another ML instance) or
		// another application on the same site
		public long AvailableFreeSpace {
			get {
				PreCheck ();
				return IsolatedStorage.AvailableFreeSpace;
			}
		}

		// note: quota could be changed from another application (same URL, another ML instance) or
		// another application on the same site
		public long Quota {
			get {
				PreCheck ();
				return IsolatedStorage.Quota;
			}
		}

		[DllImport ("moon")]
		[return: MarshalAs (UnmanagedType.Bool)]
		extern static bool isolated_storage_increase_quota_to (string primary_text, string secondary_text);

		const long mb = 1024 * 1024;

		public bool IncreaseQuotaTo (long newQuotaSize)
		{
			PreCheck ();

			if (newQuotaSize <= Quota)
				throw new ArgumentException ("newQuotaSize", "Only increases are possible");

			string message = String.Format ("This web site, <u>{0}</u>, is requesting an increase of its local storage capacity on your computer. It is currently using <b>{1:F1} MB</b> out of a maximum of <b>{2:F1} MB</b>.",
				IsolatedStorage.Site, IsolatedStorage.Current / mb, IsolatedStorage.Quota / mb);
			string question = String.Format ("Do you want to increase the web site quota to a new maximum of <b>{0:F1} MB</b> ?", 
				newQuotaSize / mb);
			bool result = isolated_storage_increase_quota_to (message, question);
			if (result)
				IsolatedStorage.Quota = newQuotaSize;
			return result;
		}
	}
}
#endif
