//
// System.CodeDom.Compiler TempFileCollection Class implementation
//
// Author:
//	Dick Porter (dick@ximian.com)
//
// (C) Copyright 2003 Ximian, Inc.
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace System.CodeDom.Compiler {

#if NET_2_0
	[Serializable]
#endif
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public class TempFileCollection:ICollection, IEnumerable, IDisposable
	{
		Hashtable filehash;
		string tempdir;
		bool keepfiles;
		string basepath;
		Random rnd;
		string ownTempDir;
		
		public TempFileCollection ()
			: this (String.Empty, false)
		{
		}

		public TempFileCollection(string tempDir)
			: this (tempDir, false)
		{
		}

		public TempFileCollection(string tempDir, bool keepFiles)
		{
			filehash=new Hashtable();
			tempdir = (tempDir == null) ? String.Empty : tempDir;
			keepfiles=keepFiles;
		}

		public string BasePath
		{
			get {
				if(basepath==null) {
				
					if (rnd == null)
						rnd = new Random ();

					// note: this property *cannot* change TempDir property
					string temp = tempdir;
					if (temp.Length == 0)
						temp = GetOwnTempDir ();

					// Create a temporary file at the target directory. This ensures
					// that the generated file name is unique.
					FileStream f = null;
					do {
						int num = rnd.Next ();
						num++;
						basepath = Path.Combine (temp, num.ToString("x"));
						string path = basepath + ".tmp";

						try {
							f = new FileStream (path, FileMode.CreateNew);
						}
						catch (System.IO.IOException) {
							f = null;
							continue;
						}
						catch {
							// avoid endless loop
							throw;
						}
					} while (f == null);
					
					f.Close ();
					
					// and you must have discovery access to the combined path
					// note: the cache behaviour is tested in the CAS tests
					if (SecurityManager.SecurityEnabled) {
						new FileIOPermission (FileIOPermissionAccess.PathDiscovery, basepath).Demand ();
					}
				}

				return(basepath);
			}
		}
		
		string GetOwnTempDir ()
		{
			if (ownTempDir != null)
				return ownTempDir;

			// this call ensure the Environment permissions check
			string basedir = Path.GetTempPath ();
			
			// Create a subdirectory with the correct user permissions
			int res = -1;
			bool win32 = false;
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Win32S:
			case PlatformID.Win32Windows:
			case PlatformID.Win32NT:
			case PlatformID.WinCE:
				win32 = true;
				res = 0;
				break;
			}

			do {
				int num = rnd.Next ();
				num++;
				ownTempDir = Path.Combine (basedir, num.ToString("x"));
				if (Directory.Exists (ownTempDir))
					continue;
				if (win32)
					Directory.CreateDirectory (ownTempDir);
				else
					res = mkdir (ownTempDir, 0x1c0);
				if (res != 0) {
					if (!Directory.Exists (ownTempDir))
						throw new IOException ();
					// Somebody already created the dir, keep trying
				}
			} while (res != 0);
			return ownTempDir;
		}

		int ICollection.Count {
			get {
				return filehash.Count;
			}
		}
		
		public int Count
		{
			get {
				return(filehash.Count);
			}
		}

		public bool KeepFiles
		{
			get {
				return(keepfiles);
			}
			set {
				keepfiles=value;
			}
		}

		public string TempDir
		{
			get {
				// note: we only return what we were supplied so there
				// is no permission protecting this information
				return tempdir;
			}
		}

		public string AddExtension(string fileExtension)
		{
			return(AddExtension(fileExtension, keepfiles));
		}

		public string AddExtension(string fileExtension, bool keepFile)
		{
			string filename=BasePath+"."+fileExtension;
			AddFile(filename, keepFile);
			return(filename);
		}

		public void AddFile(string fileName, bool keepFile)
		{
			filehash.Add(fileName, keepFile);
		}

		public void CopyTo(string[] fileNames, int start)
		{
			filehash.Keys.CopyTo(fileNames, start);
		}

		void ICollection.CopyTo(Array array, int start)
		{
			filehash.Keys.CopyTo(array, start);
		}

		object ICollection.SyncRoot {
			get {
				return null;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return(false);
			}
		}
		
		void IDisposable.Dispose() 
		{
			Dispose(true);
		}
		
		public void Delete()
		{
			bool allDeleted = true;
			string[] filenames = new string[filehash.Count];
			filehash.Keys.CopyTo (filenames, 0);

			foreach(string file in filenames) {
				if((bool)filehash[file]==false) {
					File.Delete(file);
					filehash.Remove(file);
				} else
					allDeleted = false;
			}
			if (basepath != null) {
				string tmpFile = basepath + ".tmp";
				File.Delete (tmpFile);
				basepath = null;
			}
			if (allDeleted && ownTempDir != null) {
				Directory.Delete (ownTempDir, true);
				ownTempDir = null;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return(filehash.Keys.GetEnumerator());
		}
		
		public IEnumerator GetEnumerator()
		{
			return(filehash.Keys.GetEnumerator());
		}

		protected virtual void Dispose(bool disposing)
		{
			Delete();
			if (disposing) {
				GC.SuppressFinalize (true);
			}
		}

		~TempFileCollection()
		{
			Dispose(false);
		}
		
		[DllImport ("libc")] private static extern int mkdir (string olpath, uint mode);
	}
}
