// 
// System.IO.Directory.cs 
//
// Authors:
//   Jim Richardson  (develop@wtfo-guru.com)
//   Miguel de Icaza (miguel@ximian.com)
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// Copyright (C) 2002 Ximian, Inc.
// 
// Created:        Monday, August 13, 2001 
//
//------------------------------------------------------------------------------

using System;
using System.Security.Permissions;
using System.Collections;
using System.Private;

namespace System.IO
{
	public sealed class Directory : Object
	{
		public static DirectoryInfo CreateDirectory (string path)
		{
			if (path == null)
				throw new ArgumentNullException ();
			if (path == "" || path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ();

			int code;
			
			if ((code = Wrapper.mkdir (path, 0777)) != 0)
				throw new IOException (Errno.Message (code));

			return new DirectoryInfo (path);
		}

		public static void Delete (string path)
		{
			if (path == null)
				throw new ArgumentNullException ();
			if (path == "" || path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ();

			int code = Wrapper.rmdir (path);

			if (code != 0)
				throw new IOException (Errno.Message (code));
		}

		static void RecursiveDelete (string path)
		{
			ArrayList list = GetListing (path, "*");
			if (list == null)
				throw new DirectoryNotFoundException ();

			if (!path.EndsWith (Path.DirectorySeparatorStr))
				path = path + Path.DirectorySeparatorChar;
			
			foreach (string n in list){
				string full = path + n;
				
				unsafe {
					stat s;
					
					if (Wrapper.stat (full, &s) != 0)
						continue;

					if ((s.st_mode & Wrapper.S_IFDIR) != 0){
						RecursiveDelete (full);
					} else {
						Wrapper.unlink (full);
					}
				}
			}
		}
		
		public static void Delete (string path, bool recurse)
		{
			if (path == null)
				throw new ArgumentNullException ();
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid characters");

			if (recurse == false){
				Delete (path);
				return;
			}

			RecursiveDelete (path);
		}
		
		public static bool Exists (string path)
		{
			unsafe {
				stat s;

				if (Wrapper.stat (path, &s) == 0)
					return true;
				else
					return false;
			}
		}

		enum Time { Creation, Modified, Access }
			
		static DateTime GetTime (string path, Time kind)
		{
			
			if (path == null)
				throw new ArgumentNullException ();
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid characters");

			unsafe {
				stat s;
				int code;

				code = Wrapper.stat (path, &s);

				if (code == 0){
					switch (kind){
					case Time.Creation:
						return new DateTime (DateTime.UnixEpoch + s.st_ctime);
					case Time.Modified:
						return new DateTime (DateTime.UnixEpoch + s.st_mtime);
					case Time.Access:
						return new DateTime (DateTime.UnixEpoch + s.st_atime);
					}
				}
				if (code == Wrapper.ENOTDIR)
					throw new DirectoryNotFoundException ();
				throw new IOException (Errno.Message (code));
			}
		}
		
		public static DateTime GetLastAccessTime (string path)
		{
			return GetTime (path, Time.Access);
		}
		
		public static DateTime GetLastWriteTime (string path)
		{
			return GetTime (path, Time.Modified);
		}
		
		public static DateTime GetCreationTime (string path)
		{
			return GetTime (path, Time.Creation);
		}
		
		public static string GetCurrentDirectory ()
		{	// Implementation complete 08/25/2001 14:24 except for
			// LAMESPEC: documentation specifies invalid exceptions (i think)
			//           also shouldn't need Write to getcurrrent should we?
			string str = Environment.CurrentDirectory;
			CheckPermission.Demand (FileIOPermissionAccess.Read & FileIOPermissionAccess.Write, str);
			return str;	
		}

		internal static ArrayList GetListing (string path, string pattern)
		{
			IntPtr dir_handle;
			IntPtr glob_handle = (IntPtr) 0;
			ArrayList list;
			string name, subdir = "";
			int slashpos;
			
			if (path == null)
				return null;

			if (!path.EndsWith (Path.DirectorySeparatorStr))
				path = path + Path.DirectorySeparatorChar;

			if (pattern == "*")
				pattern = null;
			else {
				// do we need to handle globbing in directory names, too?
				if ((slashpos = pattern.LastIndexOf (Path.DirectorySeparatorStr)) >= 0) {
					subdir = pattern.Substring (0, slashpos + 1);
					path += subdir;
					pattern = pattern.Substring (slashpos + 1);
				}
				glob_handle = Wrapper.mono_glob_compile (pattern);
			}
			
			dir_handle = Wrapper.opendir (path);
			list = new ArrayList ();
			while ((name = Wrapper.readdir (dir_handle)) != null){
				if (pattern == null){
					list.Add (name);
					continue;
				}

				if (Wrapper.mono_glob_match (glob_handle, name) != 0)
					list.Add (subdir + name);
			}
			if (pattern != null)
				Wrapper.mono_glob_dispose (glob_handle);
			Wrapper.closedir (dir_handle);

			return list;
		}
		
		public static string[] GetDirectories (string path)
		{
			return GetDirectories (path, "*");
		}

		enum Kind {
			Files = 1,
			Dirs  = 2,
			All   = Files | Dirs
		}
		
		static string [] GetFileListing (ArrayList list, string path, Kind kind)
		{
			ArrayList result_list = new ArrayList ();

			if (!path.EndsWith (Path.DirectorySeparatorStr))
				path = path + Path.DirectorySeparatorChar;

			foreach (string name in list){
				string full = path + name;

				unsafe {
					stat s;
					
					if (Wrapper.stat (full, &s) != 0)
						continue;

					if ((s.st_mode & Wrapper.S_IFDIR) != 0){
						if ((kind & Kind.Dirs) != 0)
							result_list.Add (full);
					} else {
						if ((kind & Kind.Files) != 0)
							result_list.Add (full);
					}
				}
			}
			string [] names = new string [result_list.Count];
			result_list.CopyTo (names);
			return names;
		}
		
		public static string[] GetDirectories (string path, string mask)
		{
			if (path == null || mask == null)
				throw new ArgumentNullException ();
			
			ArrayList list = GetListing (path, mask);
			if (list == null)
				throw new DirectoryNotFoundException ();
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid characters");

			return GetFileListing (list, path, Kind.Dirs);
		}
		
		public static string GetDirectoryRoot (string path)
		{
			return "" + Path.DirectorySeparatorChar;
		}
		
		public static string[] GetFiles (string path)
		{
			return GetFiles (path, "*");
		}
		
		public static string[] GetFiles (string path, string mask)
		{
			if (path == null || mask == null)
				throw new ArgumentNullException ();
			
			ArrayList list = GetListing (path, mask);
			if (list == null)
				throw new DirectoryNotFoundException ();
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid characters");

			return GetFileListing (list, path, Kind.Files);
		}

		public static string[] GetFileSystemEntries (string path)
		{	
			return GetFileSystemEntries (path, "*");
		}

		public static string[] GetFileSystemEntries (string path, string mask)
		{
			if (path == null || mask == null)
				throw new ArgumentNullException ();
			
			ArrayList list = GetListing (path, mask);
			if (list == null)
				throw new DirectoryNotFoundException ();
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid characters");

			return GetFileListing (list, path, Kind.All);
		}
		
		public static string[] GetLogicalDrives ()
		{	
			return new string [] { "A:\\", "C:\\" };
		}

		[MonoTODO]
		public static DirectoryInfo GetParent (string path)
		{	// TODO: Implement
			return null;
		}

		public static void Move (string src, string dst)
		{
			if (src == null || dst == null)
				throw new ArgumentNullException ();
			if (src.IndexOfAny (Path.InvalidPathChars) != -1 ||
			    dst.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ();
			
			int code;
			
			code = Wrapper.rename (src, dst);
			if (code == 0)
				return;
			
			throw new IOException (Errno.Message (code));
		}

		[MonoTODO]
		public static void SetCreationTime (string path, DateTime creationTime)
		{
			if (path == null)
				throw new ArgumentNullException ();
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ();

			long ticks = creationTime.Ticks;
			if (ticks < DateTime.UnixEpoch)
				throw new ArgumentOutOfRangeException ();

			long res = ticks - DateTime.UnixEpoch;
			if (res > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException ();

			throw new Exception ("Unimplemented");
		}
		
		public static void SetCurrentDirectory (string path)
		{	// Implementation complete 08/25/2001 14:24 except for
			// LAMESPEC: documentation specifies invalid exceptions IOException (i think)
			CheckArgument.Path (path, true);
			CheckPermission.Demand (FileIOPermissionAccess.Read & FileIOPermissionAccess.Write, path);	
			if (!Exists (path))
			{
				throw new DirectoryNotFoundException ("Directory \"" + path + "\" not found.");
			}
			Environment.CurrentDirectory = path;
		}

		[MonoTODO]
		public static void SetLastAccessTime (string path, DateTime accessTime)
		{
			throw new Exception ("Unimplemented");
		}
		
		[MonoTODO]
		public static void SetLastWriteTime (string path, DateTime modifiedTime)
		{
			throw new Exception ("Unimplemented");
		}
		
		private static DirectoryInfo getInfo (string path)
		{
			return new DirectoryInfo (path);
		}
	}
}
