// 
// System.IO.DirectoryInfo.cs 
//
// Author:
//   Miguel de Icaza, miguel@ximian.com
//   Jim Richardson, develop@wtfo-guru.com
//   Dan Lewis, dihlewis@yahoo.co.uk
//
// Copyright (C) 2002 Ximian, Inc.
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;

namespace System.IO {
	
	[Serializable]
	public sealed class DirectoryInfo : FileSystemInfo {
	
		public DirectoryInfo (string path) {
			CheckPath (path);
		
			OriginalPath = path;
			FullPath = Path.GetFullPath (path);
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
			get {
				string result = Path.GetFileName (FullPath);
				if (result == null || result == "")
					return FullPath;
				return result;
			}
		}

		public DirectoryInfo Parent {
			get {
				string dirname = Path.GetDirectoryName (FullPath);
				if (dirname == null)
					return null;

				return new DirectoryInfo (dirname);
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

		public void Create () {
			Directory.CreateDirectory (FullPath);
		}

		public DirectoryInfo CreateSubdirectory (string name) {
			CheckPath (name);
			
			string path = Path.Combine (FullPath, name);
			Directory.CreateDirectory (path);

			return new DirectoryInfo (path);
		}

		// directory listing methods

		public FileInfo [] GetFiles () {
			return GetFiles ("*");
		}

		public FileInfo [] GetFiles (string pattern) {
			string [] names = Directory.GetFiles (FullPath, pattern);

			ArrayList infos = new ArrayList ();
			foreach (string name in names)
				infos.Add (new FileInfo (name));

			return (FileInfo []) infos.ToArray (typeof (FileInfo));
		}

		public DirectoryInfo [] GetDirectories () {
			return GetDirectories ("*");
		}

		public DirectoryInfo [] GetDirectories (string pattern) {
			string [] names = Directory.GetDirectories (FullPath, pattern);

			ArrayList infos = new ArrayList ();
			foreach (string name in names)
				infos.Add (new DirectoryInfo (name));

			return (DirectoryInfo []) infos.ToArray (typeof (DirectoryInfo));
		}

		public FileSystemInfo [] GetFileSystemInfos () {
			return GetFileSystemInfos ("*");
		}

		public FileSystemInfo [] GetFileSystemInfos (string pattern) {
			ArrayList infos = new ArrayList ();
			infos.AddRange (GetDirectories (pattern));
			infos.AddRange (GetFiles (pattern));

			return (FileSystemInfo []) infos.ToArray (typeof (FileSystemInfo));
		}

		// directory management methods

		public override void Delete () {
			Delete (false);
		}

		public void Delete (bool recurse) {
			Directory.Delete (FullPath, recurse);
		}

		public void MoveTo (string dest) {
 			Directory.Move (FullPath, Path.GetFullPath (dest));
		}

		public override string ToString () {
			return OriginalPath;
		}
	}
}
