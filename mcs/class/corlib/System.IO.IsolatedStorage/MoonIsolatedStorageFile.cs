//
// System.IO.IsolatedStorage.MoonIsolatedStorageFile
//
// Moonlight's implementation for the IsolatedStorageFile
// 
// Authors
//      Miguel de Icaza (miguel@novell.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
                        if (xdg_data_home == null || xdg_data_home == String.Empty){
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

		internal IsolatedStorageFile (string basedir)
		{
		}
		
		[SecuritySafeCritical]
		public static IsolatedStorageFile GetUserStoreForApplication ()
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
		
		[SecuritySafeCritical]
		public void CreateDirectory (string dir)
		{
			Verify (dir);
			Directory.CreateDirectory (dir);
		}    
		
		[SecuritySafeCritical]
		public void DeleteDirectory (string dir)
		{
			Verify (dir);
			Directory.Delete (dir);
		}

		[SecuritySafeCritical]
		public string [] GetDirectoryNames (string searchPattern)
		{
			if (searchPattern.IndexOf ('/') != -1)
				throw new IsolatedStorageException ();
			
			return Directory.GetDirectories (appdir, searchPattern);
		}

		[SecuritySafeCritical]
		public string [] GetFileNames (string searchPattern)
		{
			if (searchPattern.IndexOf ('/') != -1)
				throw new IsolatedStorageException ();
			
			return Directory.GetDirectories (appdir, searchPattern);
		}

		[SecuritySafeCritical]
		public void DeleteFile (string file)
		{
			Verify (file);
		}
		
		public void Dispose ()
		{
		}

		[SecuritySafeCritical]
		public void Close ()
		{
		}

		[CLSCompliant(false)]
		public ulong CurrentSize {
			get {
				return 0;
			}
		}

		[CLSCompliant(false)]
		public ulong MaximumSize {
			get {
				return 1024*1024;
			}
		}
	}
}
#endif