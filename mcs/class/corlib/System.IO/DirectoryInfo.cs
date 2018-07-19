// 
// System.IO.DirectoryInfo.cs 
//
// Authors:
//   Miguel de Icaza, miguel@ximian.com
//   Jim Richardson, develop@wtfo-guru.com
//   Dan Lewis, dihlewis@yahoo.co.uk
//   Sebastien Pouliot  <sebastien@ximian.com>
//   Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2002 Ximian, Inc.
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2014 Xamarin, Inc (http://www.xamarin.com)
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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Security.AccessControl;

using Microsoft.Win32.SafeHandles;

namespace System.IO {
	
	[Serializable]
	[ComVisible (true)]
	public sealed class DirectoryInfo : FileSystemInfo {

		private string current;
		private string parent;
	
		public DirectoryInfo (string path) : this (path, false)
		{
		}

		internal DirectoryInfo (string path, bool simpleOriginalPath)
		{
			CheckPath (path);

			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			FullPath = Path.GetFullPath (path);
			if (simpleOriginalPath)
				OriginalPath = Path.GetFileName (FullPath);
			else
				OriginalPath = path;

			Initialize ();
		}

		private DirectoryInfo (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			Initialize ();
		}

		void Initialize ()
		{
			int len = FullPath.Length - 1;
			if ((len > 1) && (FullPath [len] == Path.DirectorySeparatorChar))
				len--;
			int last = FullPath.LastIndexOf (Path.DirectorySeparatorChar, len);
			if ((last == -1) || ((last == 0) && (len == 0))) {
				current = FullPath;
				parent = null;
			} else {
				current = FullPath.Substring (last + 1, len - last);
				if (last == 0 && !Environment.IsRunningOnWindows)
					parent = Path.DirectorySeparatorStr;
				else
					parent = FullPath.Substring (0, last);
				// adjust for drives, i.e. a special case for windows
				if (Environment.IsRunningOnWindows) {
					if ((parent.Length == 2) && (parent [1] == ':') && Char.IsLetter (parent [0]))
						parent += Path.DirectorySeparatorChar;
				}
			}
		}

		// properties

		public override bool Exists {
			get {
				if (_dataInitialised == -1)
					Refresh ();

				if (_data.fileAttributes == MonoIO.InvalidFileAttributes)
					return false;

				if ((_data.fileAttributes & FileAttributes.Directory) == 0)
					return false;

				return true;
			}
		}

		public override string Name {
			get { return current; }
		}

		public DirectoryInfo Parent {
			get {
				if ((parent == null) || (parent.Length == 0))
					return null;
				return new DirectoryInfo (parent);
			}
		}

		public DirectoryInfo Root {
			get {
				string root = Path.GetPathRoot (FullPath);
				if (root == null)
					return null;

				return new DirectoryInfo (root);
			}
		}

		// creational methods

		public void Create ()
		{
			Directory.CreateDirectory (FullPath);
		}

		public DirectoryInfo CreateSubdirectory (string path)
		{
			CheckPath (path);

			path = Path.Combine (FullPath, path);
			Directory.CreateDirectory (path);
			return new DirectoryInfo (path);
		}

		// directory listing methods

		public FileInfo [] GetFiles ()
		{
			return GetFiles ("*");
		}

		public FileInfo [] GetFiles (string searchPattern)
		{
			if (searchPattern == null)
				throw new ArgumentNullException ("searchPattern");

			string [] names = Directory.GetFiles (FullPath, searchPattern);

			FileInfo[] infos = new FileInfo [names.Length];
			int i = 0;
			foreach (string name in names)
				infos [i++] = new FileInfo (name);

			return infos;
		}

		public DirectoryInfo [] GetDirectories ()
		{
			return GetDirectories ("*");
		}

		public DirectoryInfo [] GetDirectories (string searchPattern)
		{
			if (searchPattern == null)
				throw new ArgumentNullException ("searchPattern");

			string [] names = Directory.GetDirectories (FullPath, searchPattern);

			DirectoryInfo[] infos = new DirectoryInfo [names.Length];
			int i = 0;
			foreach (string name in names)
				infos [i++] = new DirectoryInfo (name);

			return infos;
		}

		public FileSystemInfo [] GetFileSystemInfos ()
		{
			return GetFileSystemInfos ("*");
		}

		public FileSystemInfo [] GetFileSystemInfos (string searchPattern)
		{
			return GetFileSystemInfos (searchPattern, SearchOption.TopDirectoryOnly);
		}

		public
		FileSystemInfo [] GetFileSystemInfos (string searchPattern, SearchOption searchOption)
		{
			if (searchPattern == null)
				throw new ArgumentNullException ("searchPattern");
			if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
				throw new ArgumentOutOfRangeException ("searchOption", "Must be TopDirectoryOnly or AllDirectories");
			if (!Directory.Exists (FullPath))
				throw new IOException ("Invalid directory");

			List<FileSystemInfo> infos = new List<FileSystemInfo> ();
			InternalGetFileSystemInfos (searchPattern, searchOption, infos);
			return infos.ToArray ();
		}

		void InternalGetFileSystemInfos (string searchPattern, SearchOption searchOption, List<FileSystemInfo> infos)
		{
			// UnauthorizedAccessExceptions might happen here and break everything for SearchOption.AllDirectories
			string [] dirs = Directory.GetDirectories (FullPath, searchPattern);
			string [] files = Directory.GetFiles (FullPath, searchPattern);

			Array.ForEach<string> (dirs, (dir) => { infos.Add (new DirectoryInfo (dir)); });
			Array.ForEach<string> (files, (file) => { infos.Add (new FileInfo (file)); });
			if (dirs.Length == 0 || searchOption == SearchOption.TopDirectoryOnly)
				return;

			foreach (string dir in dirs) {
				DirectoryInfo dinfo = new DirectoryInfo (dir);
				dinfo.InternalGetFileSystemInfos (searchPattern, searchOption, infos);
			}
		}

		// directory management methods

		public override void Delete ()
		{
			Delete (false);
		}

		public void Delete (bool recursive)
		{
			Directory.Delete (FullPath, recursive);
		}

		public void MoveTo (string destDirName)
		{
			if (destDirName == null)
				throw new ArgumentNullException ("destDirName");
			if (destDirName.Length == 0)
				throw new ArgumentException ("An empty file name is not valid.", "destDirName");

			Directory.Move (FullPath, Path.GetFullPath (destDirName));
			FullPath = OriginalPath = destDirName;
			Initialize ();
		}

		public override string ToString ()
		{
			return OriginalPath;
		}

		public DirectoryInfo[] GetDirectories (string searchPattern, SearchOption searchOption)
		{
		    //NULL-check of searchPattern is done in Directory.GetDirectories
			string [] names = Directory.GetDirectories (FullPath, searchPattern, searchOption);
			//Convert the names to DirectoryInfo instances
			DirectoryInfo[] infos = new DirectoryInfo [names.Length];
			for (int i = 0; i<names.Length; ++i){
				string name = names[i];
				infos [i] = new DirectoryInfo (name);
			}
			return infos;
		}	

		internal int GetFilesSubdirs (ArrayList l, string pattern)
		{
			int count;
			FileInfo [] thisdir = null;

			try {
				thisdir = GetFiles (pattern);
			} catch (System.UnauthorizedAccessException){
				return 0;
			}
			
			count = thisdir.Length;
			l.Add (thisdir);

			foreach (DirectoryInfo subdir in GetDirectories ()){
				count += subdir.GetFilesSubdirs (l, pattern);
			}
			return count;
		}
		
		public FileInfo[] GetFiles (string searchPattern, SearchOption searchOption)
		{
			switch (searchOption) {
			case SearchOption.TopDirectoryOnly:
				return GetFiles (searchPattern);
			case SearchOption.AllDirectories: {
				ArrayList groups = new ArrayList ();
				int count = GetFilesSubdirs (groups, searchPattern);
				int current = 0;
				
				FileInfo [] all = new FileInfo [count];
				foreach (FileInfo [] p in groups){
					p.CopyTo (all, current);
					current += p.Length;
				}
				return all;
			}
			default:
				string msg = Locale.GetText ("Invalid enum value '{0}' for '{1}'.", searchOption, "SearchOption");
				throw new ArgumentOutOfRangeException ("searchOption", msg);
			}
		}

		// access control methods

		[MonoLimitation ("DirectorySecurity isn't implemented")]
		public void Create (DirectorySecurity directorySecurity)
		{
			if (directorySecurity != null)
				throw new UnauthorizedAccessException ();
			Create ();
		}

		[MonoLimitation ("DirectorySecurity isn't implemented")]
		public DirectoryInfo CreateSubdirectory (string path, DirectorySecurity directorySecurity)
		{
			if (directorySecurity != null)
				throw new UnauthorizedAccessException ();
			return CreateSubdirectory (path);
		}

		public DirectorySecurity GetAccessControl ()
		{
			return Directory.GetAccessControl (FullPath);
		}

		public DirectorySecurity GetAccessControl (AccessControlSections includeSections)
		{
			return Directory.GetAccessControl (FullPath, includeSections);
		}

		public void SetAccessControl (DirectorySecurity directorySecurity)
		{
			Directory.SetAccessControl (FullPath, directorySecurity);
		}


		public IEnumerable<DirectoryInfo> EnumerateDirectories ()
		{
			return EnumerateDirectories ("*", SearchOption.TopDirectoryOnly);
		}

		public IEnumerable<DirectoryInfo> EnumerateDirectories (string searchPattern)
		{
			return EnumerateDirectories (searchPattern, SearchOption.TopDirectoryOnly);
		}

		public IEnumerable<DirectoryInfo> EnumerateDirectories (string searchPattern, SearchOption searchOption)
		{
			if (searchPattern == null)
				throw new ArgumentNullException ("searchPattern");

			return CreateEnumerateDirectoriesIterator (searchPattern, searchOption);
		}

		IEnumerable<DirectoryInfo> CreateEnumerateDirectoriesIterator (string searchPattern, SearchOption searchOption)
		{
			foreach (string name in Directory.EnumerateDirectories (FullPath, searchPattern, searchOption))
				yield return new DirectoryInfo (name);
		}

		public IEnumerable<FileInfo> EnumerateFiles ()
		{
			return EnumerateFiles ("*", SearchOption.TopDirectoryOnly);
		}

		public IEnumerable<FileInfo> EnumerateFiles (string searchPattern)
		{
			return EnumerateFiles (searchPattern, SearchOption.TopDirectoryOnly);
		}

		public IEnumerable<FileInfo> EnumerateFiles (string searchPattern, SearchOption searchOption)
		{
			if (searchPattern == null)
				throw new ArgumentNullException ("searchPattern");

			return CreateEnumerateFilesIterator (searchPattern, searchOption);
		}

		IEnumerable<FileInfo> CreateEnumerateFilesIterator (string searchPattern, SearchOption searchOption)
		{
			foreach (string name in Directory.EnumerateFiles (FullPath, searchPattern, searchOption))
				yield return new FileInfo (name);
		}

		public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos ()
		{
			return EnumerateFileSystemInfos ("*", SearchOption.TopDirectoryOnly);
		}

		public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos (string searchPattern)
		{
			return EnumerateFileSystemInfos (searchPattern, SearchOption.TopDirectoryOnly);
		}

		public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos (string searchPattern, SearchOption searchOption)
		{
			if (searchPattern == null)
				throw new ArgumentNullException ("searchPattern");
			if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
				throw new ArgumentOutOfRangeException ("searchoption");

			return EnumerateFileSystemInfos (FullPath, searchPattern, searchOption);
		}

		static internal IEnumerable<FileSystemInfo> EnumerateFileSystemInfos (string basePath, string searchPattern, SearchOption searchOption)
		{
			Path.Validate (basePath);

			SafeFindHandle findHandle = null;

			try {
				string filePath;
				int nativeAttrs;

				string basePathWithPattern = Path.Combine (basePath, searchPattern);

				int nativeError;
				try {} finally {
					findHandle = new SafeFindHandle (MonoIO.FindFirstFile (basePathWithPattern, out filePath, out nativeAttrs, out nativeError));
				}

				if (findHandle.IsInvalid) {
					MonoIOError error = (MonoIOError) nativeError;
					if (error != MonoIOError.ERROR_FILE_NOT_FOUND)
						throw MonoIO.GetException (Path.GetDirectoryName (basePathWithPattern), error);

					yield break;
				}

				do {
					if (filePath == null)
						yield break;

					if (filePath == "." || filePath == "..")
						continue;

					FileAttributes attrs = (FileAttributes) nativeAttrs;

					string fullPath = Path.Combine (basePath, filePath);

					if ((attrs & FileAttributes.ReparsePoint) == 0) {
						if ((attrs & FileAttributes.Directory) != 0)
							yield return new DirectoryInfo (fullPath);
						else
							yield return new FileInfo (fullPath);
					}

					if ((attrs & FileAttributes.Directory) != 0 && searchOption == SearchOption.AllDirectories) {
						foreach (FileSystemInfo child in EnumerateFileSystemInfos (fullPath, searchPattern, searchOption))
							yield return child;
					}
				} while (MonoIO.FindNextFile (findHandle.DangerousGetHandle (), out filePath, out nativeAttrs, out int _));
			} finally {
				if (findHandle != null)
					findHandle.Dispose ();
			}
		}
		
		internal void CheckPath (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path.Length == 0)
				throw new ArgumentException ("An empty file name is not valid.");
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path.");
			if (Environment.IsRunningOnWindows) {
				int idx = path.IndexOf (':');
				if (idx >= 0 && idx != 1)
					throw new ArgumentException ("path");
			}
		}
	}
}
