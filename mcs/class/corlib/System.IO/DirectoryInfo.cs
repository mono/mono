// 
// System.IO.DirectoryInfo.cs 
//
// Author:
//   Miguel de Icaza, miguel@ximian.com
//   Jim Richardson, develop@wtfo-guru.com
//
// Copyright (C) 2002 Ximian, Inc.
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 

using System;
using System.Collections;
using System.Private;

namespace System.IO
{
	public sealed class DirectoryInfo : FileSystemInfo
	{
		string path;
		
		public DirectoryInfo (string path)
		{
			if (path == null)
				throw new ArgumentNullException ();
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Invalid characters in pathname");

			this.path = path;
			
			unsafe {
				stat s;
				int code;
				
				code = Wrapper.stat (path, &s);
				if (code != 0)
					throw new DirectoryNotFoundException ();
			}
		}

		public override bool Exists
		{
			get {
				unsafe {
					stat s;
					int code;
					
					code = Wrapper.stat (path, &s);
					return (code == 0);
				}
			}
		}

		public DirectoryInfo Parent {
			get {
				return new DirectoryInfo (path + Path.PathSeparator + "..");
			}
		}

		public override string Name
		{
			get {
				return path;
			}
		}

		public DirectoryInfo Root
		{
			get {
				return new DirectoryInfo ("" + Path.PathSeparator);
			}
		}

		public void Create ()
		{
			Wrapper.mkdir (path, 0777);
		}

		[MonoTODO]
		DirectoryInfo CreateSubdirectory (string path)
		{
			return null;
		}

		public override void Delete ()
		{
			if (Wrapper.rmdir (path) == Wrapper.ENOTEMPTY)
				throw new IOException ("Directory not empty");
		}

		[MonoTODO]
		public void Delete (bool recursive)
		{
			if (recursive == false)
				Delete ();
		}

		public DirectoryInfo[] GetDirectories (string path)
		{
			return GetDirectories ("*");
		}

		public static DirectoryInfo[] GetDirectories (string path, string mask)
		{
			if (path == null)
				throw new ArgumentNullException ("path is null");
			if (mask == null)
				throw new ArgumentNullException ("mask is null");
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Invalid characters in path");
			
			ArrayList list = Directory.GetListing (path, "*");
			if (list == null)
				throw new DirectoryNotFoundException ();
			
			ArrayList dir_list = new ArrayList ();
			
			foreach (string name in list){
				string full = path + Path.DirectorySeparatorChar + name;

				unsafe {
					stat s;
					
					if (Wrapper.stat (full, &s) != 0)
						continue;

					if ((s.st_mode & Wrapper.S_IFDIR) != 0)
						dir_list.Add (new DirectoryInfo (full));
				}
			}

			DirectoryInfo [] di_array = new DirectoryInfo [dir_list.Count];
			dir_list.CopyTo (di_array);
			
			return di_array;
		}

		public static FileInfo[] GetFiles (string path)
		{
			return GetFiles (path, "*");
		}

		public static FileInfo[] GetFiles (string path, string mask)
		{
			if (path == null)
				throw new ArgumentNullException ("path is null");
			if (mask == null)
				throw new ArgumentNullException ("mask is null");
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Invalid characters in path");
			
			ArrayList list = Directory.GetListing (path, "*");
			if (list == null)
				throw new DirectoryNotFoundException ();

			ArrayList file_list = new ArrayList ();
			
			foreach (string name in list){
				string full = path + Path.DirectorySeparatorChar + name;

				unsafe {
					stat buf;
					
					if (Wrapper.stat (full, &buf) != 0)
						continue;

					if ((buf.st_mode & Wrapper.S_IFDIR) == 0)
						file_list.Add (new FileInfo (full));
				}
			}
			FileInfo [] fi_array = new FileInfo [file_list.Count];
			file_list.CopyTo (fi_array);
			
			return fi_array;
		}

		public static FileSystemInfo[] GetFileSystemInfos (string path)
		{
			return GetFileSystemInfos (path, "*");
		}

		public static FileSystemInfo[] GetFileSystemInfos (string path, string mask)
		{
			if (path == null)
				throw new ArgumentNullException ("path is null");
			if (mask == null)
				throw new ArgumentNullException ("mask is null");
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Invalid characters in path");

			ArrayList list = Directory.GetListing (path, "*");
			if (list == null)
				throw new DirectoryNotFoundException ();

			FileSystemInfo [] fi = new FileSystemInfo [list.Count];
			int i = 0;
			foreach (string s in list){
				string name = path + Path.DirectorySeparatorChar + s;

				unsafe {
					stat buf;
					
					if (Wrapper.stat (name, &buf) != 0)
						continue;
					if ((buf.st_mode & Wrapper.S_IFDIR) == 0)
						fi [i++] = new FileInfo (name);
					else
						fi [i++] = new DirectoryInfo (name);
				}
			}
			
			return fi;
		}

		public void MoveTo (string dest)
		{
			if (dest == null)
				throw new ArgumentNullException ();
			if (dest == "")
				throw new ArgumentException ();

			if (Wrapper.rename (path, dest) != 0)
				throw new IOException ();
		}

		public override string ToString ()
		{
			return FullName;
		}
	}
}
