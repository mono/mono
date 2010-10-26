// 
// System.IO.DirectoryInfo.cs 
//
// Authors:
//   Miguel de Icaza, miguel@ximian.com
//   Jim Richardson, develop@wtfo-guru.com
//   Dan Lewis, dihlewis@yahoo.co.uk
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2002 Ximian, Inc.
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
#if NET_2_0 && !NET_2_1 && !MICRO_LIB
using System.Security.AccessControl;
#endif

namespace System.IO {
	
	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public sealed class DirectoryInfo : FileSystemInfo {

		private string current;
		private string parent;
	
		public DirectoryInfo (string path) : this (path, false)
		{
		}

		internal DirectoryInfo (string path, bool simpleOriginalPath)
		{
			CheckPath (path);

			FullPath = Path.GetFullPath (path);
			if (simpleOriginalPath)
				OriginalPath = Path.GetFileName (path);
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
				Refresh (false);

				if (stat.Attributes == MonoIO.InvalidFileAttributes)
					return false;

				if ((stat.Attributes & FileAttributes.Directory) == 0)
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
			if (searchPattern == null)
				throw new ArgumentNullException ("searchPattern");

			if (!Directory.Exists (FullPath))
				throw new IOException ("Invalid directory");
			string [] dirs = Directory.GetDirectories (FullPath, searchPattern);
			string [] files = Directory.GetFiles (FullPath, searchPattern);

			FileSystemInfo[] infos = new FileSystemInfo [dirs.Length + files.Length];
			int i = 0;
			foreach (string dir in dirs)
				infos [i++] = new DirectoryInfo (dir);
			foreach (string file in files)
				infos [i++] = new FileInfo (file);

			return infos;
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
		}

		public override string ToString ()
		{
			return OriginalPath;
		}

#if NET_2_0 && !NET_2_1 || UNITY
		public DirectoryInfo[] GetDirectories (string searchPattern, SearchOption searchOption)
		{
			switch (searchOption) {
			case SearchOption.TopDirectoryOnly:
				return GetDirectories (searchPattern);
			case SearchOption.AllDirectories:
				Queue workq = new Queue(GetDirectories(searchPattern));
				Queue doneq = new Queue();
				while (workq.Count > 0)
					{
						DirectoryInfo cinfo = (DirectoryInfo) workq.Dequeue();
						DirectoryInfo[] cinfoDirs = cinfo.GetDirectories(searchPattern);
						foreach (DirectoryInfo i in cinfoDirs) workq.Enqueue(i);
						doneq.Enqueue(cinfo);
					}

				DirectoryInfo[] infos = new DirectoryInfo[doneq.Count];
				doneq.CopyTo(infos, 0);
				return infos;
			default:
				string msg = Locale.GetText ("Invalid enum value '{0}' for '{1}'.", searchOption, "SearchOption");
				throw new ArgumentOutOfRangeException ("searchOption", msg);
			}
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
#endif
#if NET_2_0 && !NET_2_1
#if !MICRO_LIB
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

		[MonoNotSupported ("DirectorySecurity isn't implemented")]
		public DirectorySecurity GetAccessControl ()
		{
			throw new UnauthorizedAccessException ();
		}

		[MonoNotSupported ("DirectorySecurity isn't implemented")]
		public DirectorySecurity GetAccessControl (AccessControlSections includeSections)
		{
			throw new UnauthorizedAccessException ();
		}

		[MonoLimitation ("DirectorySecurity isn't implemented")]
		public void SetAccessControl (DirectorySecurity directorySecurity)
		{
			if (directorySecurity != null)
				throw new ArgumentNullException ("directorySecurity");
			throw new UnauthorizedAccessException ();
		}
#endif
#endif
	}
}
