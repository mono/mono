// 
// System.IO.Directory.cs 
//
// Authors:
//   Jim Richardson  (develop@wtfo-guru.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Dan Lewis       (dihlewis@yahoo.co.uk)
//   Eduardo Garcia  (kiwnix@yahoo.es)
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
using System.Text;

namespace System.IO
{
	public sealed class Directory : Object
	{
		private Directory () {}
		
		public static DirectoryInfo CreateDirectory (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			
			if (path == "")
				throw new ArgumentException ("Path is empty");
			
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid chars");

			if (path.Trim ().Length == 0)
				throw new ArgumentException ("Only blank characters in path");
			
			if (path == ":")
				throw new NotSupportedException ("Only ':' In path");
			
			return CreateDirectoriesInternal (path);
		}

		static DirectoryInfo CreateDirectoriesInternal (string path)
		{
			DirectoryInfo info = new DirectoryInfo (path);
			if (info.Parent != null && !info.Parent.Exists)
				 info.Parent.Create ();

			MonoIOError error;
			if (!MonoIO.CreateDirectory (path, out error)) {
				if (error != MonoIOError.ERROR_ALREADY_EXISTS)
					throw MonoIO.GetException (error);
			}

			return info;
		}
		
		public static void Delete (string path)
		{
			if (path == null) {
				throw new ArgumentNullException ("path");
			}
			
			if (path == "") {
				throw new ArgumentException ("Path is empty");
			}
			
			if (path.IndexOfAny (Path.InvalidPathChars) != -1) {
				throw new ArgumentException ("Path contains invalid chars");
			}
			if (path.Trim().Length == 0)
				throw new ArgumentException ("Only blank characters in path");
			if (path == ":")
				throw new NotSupportedException ("Only ':' In path");

			

			MonoIOError error;
			
			if (!MonoIO.RemoveDirectory (path, out error)) {
				throw MonoIO.GetException (error);
			}
		}

		static void RecursiveDelete (string path)
		{
			foreach (string dir in GetDirectories (path))
				RecursiveDelete (dir);

			foreach (string file in GetFiles (path))
				File.Delete (file);

			Directory.Delete (path);
		}
		
		public static void Delete (string path, bool recurse)
		{
			if (path == null)
				throw new ArgumentNullException ();
			if (path == "")
				throw new System.ArgumentException("Path is Empty");
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid characters");
			if (path.Trim().Length == 0)
				throw new ArgumentException ("Only blank characters in path");
			if (path == ":")
				throw new NotSupportedException ("Only ':' In path");			
			
			
			if (recurse == false){
				Delete (path);
				return;
			}

			RecursiveDelete (path);
		}

		public static bool Exists (string path)
		{
			if (path == null)
				return false;
				
			MonoIOError error;
			
			return MonoIO.ExistsDirectory (path, out error);
		}

		public static DateTime GetLastAccessTime (string path)
		{
			return File.GetLastAccessTime (path);
		}
		
		public static DateTime GetLastWriteTime (string path)
		{
			return File.GetLastWriteTime (path);
		}
		
		public static DateTime GetCreationTime (string path)
		{
			if (path == null)
				throw new System.ArgumentNullException("Path is Null");
			if (path == "")
				throw new System.ArgumentException("Path is Empty");
			if (path.Trim().Length == 0)
				throw new ArgumentException ("Only blank characters in path");
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid chars");
			if (path == ":")
				throw new NotSupportedException ("Only ':' In path");

			

			return File.GetLastWriteTime (path);
		}
		
		public static string GetCurrentDirectory ()
		{
			/*
			// Implementation complete 08/25/2001 14:24 except for
			// LAMESPEC: documentation specifies invalid exceptions (i think)
			//           also shouldn't need Write to getcurrrent should we?
			string str = Environment.CurrentDirectory;
			CheckPermission.Demand (FileIOPermissionAccess.Read & FileIOPermissionAccess.Write, str);
			*/
			return Environment.CurrentDirectory;
		}
		
		public static string [] GetDirectories (string path)
		{
			return GetDirectories (path, "*");
		}
		
		public static string [] GetDirectories (string path, string pattern)
		{
			return GetFileSystemEntries (path, pattern, FileAttributes.Directory, FileAttributes.Directory);
		}
		
		public static string GetDirectoryRoot (string path)
		{
			return new String(Path.DirectorySeparatorChar,1);
		}
		
		public static string [] GetFiles (string path)
		{
			return GetFiles (path, "*");
		}
		
		public static string [] GetFiles (string path, string pattern)
		{
			return GetFileSystemEntries (path, pattern, FileAttributes.Directory, 0);
		}

		public static string [] GetFileSystemEntries (string path)
		{
			return GetFileSystemEntries (path, "*");
		}

		public static string [] GetFileSystemEntries (string path, string pattern)
		{
			return GetFileSystemEntries (path, pattern, 0, 0);
		}
		
		public static string[] GetLogicalDrives ()
		{ 
			//FIXME: Hardcoded Paths
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				return new string[] { "/" };
			else
				return new string [] { "A:\\", "C:\\" };
		}

		public static DirectoryInfo GetParent (string path)
		{
			if (path == null)
				throw new ArgumentNullException ();
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid characters");
			if (path == "")
				throw new ArgumentException ("The Path do not have a valid format");
			
			return new DirectoryInfo (Path.GetDirectoryName (path));
		}

		public static void Move (string src, string dest)
		{
			if (src == null)
				throw new ArgumentNullException ("src");

			if (dest == null)
				throw new ArgumentNullException ("dest");

			if (src.Trim () == "" || src.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Invalid source directory name: " + src, "src");

			if (dest.Trim () == "" || dest.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Invalid target directory name: " + dest, "dest");

			if (!Exists (src))
				throw new DirectoryNotFoundException (src + " does not exist");

			MonoIOError error;
			if (!MonoIO.MoveFile (src, dest, out error))
				throw MonoIO.GetException (error);
		}

		public static void SetCreationTime (string path, DateTime creation_time)
		{
			File.SetCreationTime (path, creation_time);
		}
		
		public static void SetCurrentDirectory (string path)
		{
			/*
			// Implementation complete 08/25/2001 14:24 except for
			// LAMESPEC: documentation specifies invalid exceptions IOException (i think)
			CheckArgument.Path (path, true);
			CheckPermission.Demand (FileIOPermissionAccess.Read & FileIOPermissionAccess.Write, path);	
			*/
			if (!Exists (path))
			{
				throw new DirectoryNotFoundException ("Directory \"" + path + "\" not found.");
			}
			Environment.CurrentDirectory = path;
		}

		public static void SetLastAccessTime (string path, DateTime last_access_time)
		{
			File.SetLastAccessTime (path, last_access_time);
		}
		
		public static void SetLastWriteTime (string path, DateTime last_write_time)
		{
			File.SetLastWriteTime (path, last_write_time);
		}
		
		// private

		private static string [] GetFileSystemEntries (string path, string pattern, FileAttributes mask, FileAttributes attrs)
		{
			SearchPattern search;
			MonoIOStat stat;
			IntPtr find;

			if (path == null)
				throw new ArgumentNullException ();

			if (path == "")
				throw new ArgumentException ("The Path do not have a valid format");

			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid characters");

			if (!Directory.Exists (path)) {
				throw new DirectoryNotFoundException ("Directory '" + path + "' not found.");
			}

			search = new SearchPattern (pattern);

			MonoIOError error;
			
			find = MonoIO.FindFirstFile (Path.Combine (path , "*"), out stat, out error);
			if (find == MonoIO.InvalidHandle) {
				switch (error) {
				case MonoIOError.ERROR_FILE_NOT_FOUND:
				case MonoIOError.ERROR_PATH_NOT_FOUND:
					string message = String.Format ("Could not find a part of the path \"{0}\"", path);
					throw new DirectoryNotFoundException (message);
				case MonoIOError.ERROR_NO_MORE_FILES:
					return new string [0];

				default:
					throw MonoIO.GetException (path,
								   error);
				}
			}
			
			ArrayList entries = new ArrayList ();

			while (true) {
				// Ignore entries of "." and ".." -
				// the documentation doesn't mention
				// it (surprise!) but empirical
				// testing indicates .net never
				// returns "." or ".." in a
				// GetDirectories() list.
				if ((stat.Attributes & mask) == attrs &&
				    search.IsMatch (stat.Name) &&
				    stat.Name != "." &&
				    stat.Name != "..")
					entries.Add (Path.Combine (path, stat.Name));

				if (!MonoIO.FindNextFile (find, out stat,
							  out error))
					break;
			}
			MonoIO.FindClose (find, out error);

			return (string []) entries.ToArray (typeof (string));
		}
	}
}
