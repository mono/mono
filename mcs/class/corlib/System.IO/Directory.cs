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
			DirectoryInfo dInfo = getInfo (path);
			if (!dInfo.Exists)
			{
				dInfo.Create ();
			}
			return dInfo;
		}
		
		public static void Delete (string path)
		{	
			DirectoryInfo dInfo = getInfo (path);
			if (dInfo.Exists)
				dInfo.Delete ();
		}
		
		public static void Delete (string path, bool bRecurse)
		{	
			DirectoryInfo dInfo = getInfo (path);
			if (dInfo.Exists)
			{
				dInfo.Delete (bRecurse);
			}
		}
		
		public static bool Exists (string path)
		{
			return getInfo (path).Exists;
		}
		
		public static DateTime GetCreationTime (string path)
		{
			return getInfo (path).CreationTime;
		}
		
		public static string GetCurrentDirectory ()
		{	// Implementation complete 08/25/2001 14:24 except for
			// LAMESPEC: documentation specifies invalid exceptions (i think)
			//           also shouldn't need Write to getcurrrent should we?
			string str = Environment.CurrentDirectory;
			CheckPermission.Demand (FileIOPermissionAccess.Read & FileIOPermissionAccess.Write, str);
			return str;	
		}

		static bool Matches (string name, string pattern)
		{
			return true;
		}
		
		internal static ArrayList GetListing (string path, string pattern)
		{
			IntPtr handle = Wrapper.opendir (path);
			ArrayList list;
			string name;
			
			if (path == null)
				return null;

			list = new ArrayList ();
			while ((name = Wrapper.readdir (handle)) != null){
				if (pattern == null){
					list.Add (name);
					continue;
				}
				if (Matches (name, pattern))
					list.Add (name);
			}
			Wrapper.closedir (handle);

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
			foreach (string name in list){
				string full = path + Path.DirectorySeparatorChar + name;

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
			return getInfo (path).Root.FullName;
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
		
		public static DateTime GetLastAccessTime (string path)
		{
			return getInfo (path).LastAccessTime;
		}
		
		public static DateTime GetLastWriteTime (string path)
		{
			return getInfo (path).LastWriteTime;
		}
		
		[MonoTODO]
		public static string[] GetLogicalDrives ()
		{	// TODO: Implement
			return null;
		}

		[MonoTODO]
		public static DirectoryInfo GetParent (string path)
		{	// TODO: Implement
			return null;
		}

		public static void Move (string src, string dst)
		{
			 getInfo (src).MoveTo (dst);
		}
		
		public static void SetCreationTime (string path, DateTime creationTime)
		{
			getInfo (path).CreationTime = creationTime;
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
		
		public static void SetLastAccessTime (string path, DateTime accessTime)
		{
			getInfo (path).LastAccessTime = accessTime;
		}
		
		public static void SetLastWriteTime (string path, DateTime modifiedTime)
		{
			getInfo (path).LastWriteTime = modifiedTime;
		}
		
		private static DirectoryInfo getInfo (string path)
		{
			return new DirectoryInfo (path);
		}
		
		private static string[] getNames (FileSystemInfo[] arInfo)
		{
			int index = 0;
			string[] ar = new string[arInfo.Length];
						
			foreach (FileInfo fi in arInfo)
			{
				ar[index++] = fi.FullName;
			}
			return ar;
		}
	}
}
