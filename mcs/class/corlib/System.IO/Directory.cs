// 
// System.IO.Directory.cs 
//
// Authors:
//   Jim Richardson  (develop@wtfo-guru.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Dan Lewis       (dihlewis@yahoo.co.uk)
//   Eduardo Garcia  (kiwnix@yahoo.es)
//   Ville Palo      (vi64pa@kolumbus.fi)
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// Copyright (C) 2002 Ximian, Inc.
// 
// Created:        Monday, August 13, 2001 
//
//------------------------------------------------------------------------------

//
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
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Runtime.InteropServices;

#if !MOONLIGHT
using System.Security.AccessControl;
#endif

namespace System.IO
{
	[ComVisible (true)]
	public static class Directory
	{

		public static DirectoryInfo CreateDirectory (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			
			if (path.Length == 0)
				throw new ArgumentException ("Path is empty");
			
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid chars");

			if (path.Trim ().Length == 0)
				throw new ArgumentException ("Only blank characters in path");

			if (File.Exists(path))
				throw new IOException ("Cannot create " + path + " because a file with the same name already exists.");
			
			// LAMESPEC: with .net 1.0 version this throw NotSupportedException and msdn says so too
			// but v1.1 throws ArgumentException.
			if (Environment.IsRunningOnWindows && path == ":")
				throw new ArgumentException ("Only ':' In path");
			
			return CreateDirectoriesInternal (path);
		}

#if !MOONLIGHT
		[MonoLimitation ("DirectorySecurity not implemented")]
		public static DirectoryInfo CreateDirectory (string path, DirectorySecurity directorySecurity)
		{
			return(CreateDirectory (path));
		}
#endif

		static DirectoryInfo CreateDirectoriesInternal (string path)
		{
#if !MOONLIGHT
			if (SecurityManager.SecurityEnabled) {
				new FileIOPermission (FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, path).Demand ();
			}
#endif
			DirectoryInfo info = new DirectoryInfo (path, true);
			if (info.Parent != null && !info.Parent.Exists)
				 info.Parent.Create ();

			MonoIOError error;
			if (!MonoIO.CreateDirectory (path, out error)) {
				// LAMESPEC: 1.1 and 1.2alpha allow CreateDirectory on a file path.
				// So CreateDirectory ("/tmp/somefile") will succeed if 'somefile' is
				// not a directory. However, 1.0 will throw an exception.
				// We behave like 1.0 here (emulating 1.1-like behavior is just a matter
				// of comparing error to ERROR_FILE_EXISTS, but it's lame to do:
				//    DirectoryInfo di = Directory.CreateDirectory (something);
				// and having di.Exists return false afterwards.
				// I hope we don't break anyone's code, as they should be catching
				// the exception anyway.
				if (error != MonoIOError.ERROR_ALREADY_EXISTS &&
				    error != MonoIOError.ERROR_FILE_EXISTS)
					throw MonoIO.GetException (path, error);
			}

			return info;
		}
		
		public static void Delete (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			
			if (path.Length == 0)
				throw new ArgumentException ("Path is empty");
			
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid chars");

			if (path.Trim().Length == 0)
				throw new ArgumentException ("Only blank characters in path");

			if (Environment.IsRunningOnWindows && path == ":")
				throw new NotSupportedException ("Only ':' In path");

			MonoIOError error;
			bool success;
			
			if (MonoIO.ExistsSymlink (path, out error)) {
				/* RemoveDirectory maps to rmdir()
				 * which fails on symlinks (ENOTDIR)
				 */
				success = MonoIO.DeleteFile (path, out error);
			} else {
				success = MonoIO.RemoveDirectory (path, out error);
			}
			
			if (!success) {
				/*
				 * FIXME:
				 * In io-layer/io.c rmdir returns error_file_not_found if directory does not exists.
				 * So maybe this could be handled somewhere else?
				 */
				if (error == MonoIOError.ERROR_FILE_NOT_FOUND) {
					if (File.Exists (path))
						throw new IOException ("Directory does not exist, but a file of the same name exists.");
					else
						throw new DirectoryNotFoundException ("Directory does not exist.");
				} else
					throw MonoIO.GetException (path, error);
			}
		}

		static void RecursiveDelete (string path)
		{
			MonoIOError error;
			
			foreach (string dir in GetDirectories (path)) {
				if (MonoIO.ExistsSymlink (dir, out error)) {
					MonoIO.DeleteFile (dir, out error);
				} else {
					RecursiveDelete (dir);
				}
			}

			foreach (string file in GetFiles (path))
				File.Delete (file);

			Directory.Delete (path);
		}
		
		public static void Delete (string path, bool recursive)
		{
			CheckPathExceptions (path);
			
			if (recursive)
				RecursiveDelete (path);
			else
				Delete (path);
		}

		public static bool Exists (string path)
		{
			if (path == null)
				return false;
				
			MonoIOError error;
			bool exists;
			
			exists = MonoIO.ExistsDirectory (path, out error);
			/* This should not throw exceptions */
			return exists;
		}

		public static DateTime GetLastAccessTime (string path)
		{
			return File.GetLastAccessTime (path);
		}

		public static DateTime GetLastAccessTimeUtc (string path)
		{
			return GetLastAccessTime (path).ToUniversalTime ();
		}

		public static DateTime GetLastWriteTime (string path)
		{
			return File.GetLastWriteTime (path);
		}
		
		public static DateTime GetLastWriteTimeUtc (string path)
		{
			return GetLastWriteTime (path).ToUniversalTime ();
		}

		public static DateTime GetCreationTime (string path)
		{
			return File.GetCreationTime (path);
		}

		public static DateTime GetCreationTimeUtc (string path)
		{
			return GetCreationTime (path).ToUniversalTime ();
		}

		public static string GetCurrentDirectory ()
		{
			MonoIOError error;
				
			string result = MonoIO.GetCurrentDirectory (out error);
			if (error != MonoIOError.ERROR_SUCCESS)
				throw MonoIO.GetException (error);
#if !MOONLIGHT
			if ((result != null) && (result.Length > 0) && SecurityManager.SecurityEnabled) {
				new FileIOPermission (FileIOPermissionAccess.PathDiscovery, result).Demand ();
			}
#endif
			return result;
		}
		
		public static string [] GetDirectories (string path)
		{
			return GetDirectories (path, "*");
		}
		
		public static string [] GetDirectories (string path, string searchPattern)
		{
			return GetFileSystemEntries (path, searchPattern, FileAttributes.Directory, FileAttributes.Directory);
		}
		
#if !MOONLIGHT
		public static string [] GetDirectories (string path, string searchPattern, SearchOption searchOption)
		{
			if (searchOption == SearchOption.TopDirectoryOnly)
				return GetDirectories (path, searchPattern);
			ArrayList all = new ArrayList ();
			GetDirectoriesRecurse (path, searchPattern, all);
			return (string []) all.ToArray (typeof (string));
		}
		
		static void GetDirectoriesRecurse (string path, string searchPattern, ArrayList all)
		{
			all.AddRange (GetDirectories (path, searchPattern));
			foreach (string dir in GetDirectories (path))
				GetDirectoriesRecurse (dir, searchPattern, all);
		}
#endif

		public static string GetDirectoryRoot (string path)
		{
			return new String(Path.DirectorySeparatorChar,1);
		}
		
		public static string [] GetFiles (string path)
		{
			return GetFiles (path, "*");
		}
		
		public static string [] GetFiles (string path, string searchPattern)
		{
			return GetFileSystemEntries (path, searchPattern, FileAttributes.Directory, 0);
		}

#if !MOONLIGHT
		public static string[] GetFiles (string path, string searchPattern, SearchOption searchOption)
		{
			if (searchOption == SearchOption.TopDirectoryOnly)
				return GetFiles (path, searchPattern);
			ArrayList all = new ArrayList ();
			GetFilesRecurse (path, searchPattern, all);
			return (string []) all.ToArray (typeof (string));
		}
		
		static void GetFilesRecurse (string path, string searchPattern, ArrayList all)
		{
			all.AddRange (GetFiles (path, searchPattern));
			foreach (string dir in GetDirectories (path))
				GetFilesRecurse (dir, searchPattern, all);
		}
#endif

		public static string [] GetFileSystemEntries (string path)
		{
			return GetFileSystemEntries (path, "*");
		}

		public static string [] GetFileSystemEntries (string path, string searchPattern)
		{
			return GetFileSystemEntries (path, searchPattern, 0, 0);
		}
		
		public static string[] GetLogicalDrives ()
		{ 
			return Environment.GetLogicalDrives ();
		}

		static bool IsRootDirectory (string path)
		{
			// Unix
			if (Path.DirectorySeparatorChar == '/' && path == "/")
				return true;

			// Windows
			if (Path.DirectorySeparatorChar == '\\')
				if (path.Length == 3 && path.EndsWith (":\\"))
					return true;

			return false;
		}

		public static DirectoryInfo GetParent (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid characters");
			if (path.Length == 0)
				throw new ArgumentException ("The Path do not have a valid format");

			// return null if the path is the root directory
			if (IsRootDirectory (path))
				return null;

			string parent_name = Path.GetDirectoryName (path);
			if (parent_name.Length == 0)
				parent_name = GetCurrentDirectory();

			return new DirectoryInfo (parent_name);
		}

		public static void Move (string sourceDirName, string destDirName)
		{
			if (sourceDirName == null)
				throw new ArgumentNullException ("sourceDirName");

			if (destDirName == null)
				throw new ArgumentNullException ("destDirName");

			if (sourceDirName.Trim ().Length == 0 || sourceDirName.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Invalid source directory name: " + sourceDirName, "sourceDirName");

			if (destDirName.Trim ().Length == 0 || destDirName.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Invalid target directory name: " + destDirName, "destDirName");

			if (sourceDirName == destDirName)
				throw new IOException ("Source and destination path must be different.");

			if (Exists (destDirName))
				throw new IOException (destDirName + " already exists.");

			if (!Exists (sourceDirName) && !File.Exists (sourceDirName))
				throw new DirectoryNotFoundException (sourceDirName + " does not exist");

			MonoIOError error;
			if (!MonoIO.MoveFile (sourceDirName, destDirName, out error))
				throw MonoIO.GetException (error);
		}

#if !MOONLIGHT
		public static void SetAccessControl (string path, DirectorySecurity directorySecurity)
		{
			throw new NotImplementedException ();
		}
#endif

		public static void SetCreationTime (string path, DateTime creationTime)
		{
			File.SetCreationTime (path, creationTime);
		}

		public static void SetCreationTimeUtc (string path, DateTime creationTimeUtc)
		{
			SetCreationTime (path, creationTimeUtc.ToLocalTime ());
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public static void SetCurrentDirectory (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path.Trim ().Length == 0)
				throw new ArgumentException ("path string must not be an empty string or whitespace string");

			MonoIOError error;
				
			if (!Exists (path))
				throw new DirectoryNotFoundException ("Directory \"" +
									path + "\" not found.");

			MonoIO.SetCurrentDirectory (path, out error);
			if (error != MonoIOError.ERROR_SUCCESS)
				throw MonoIO.GetException (path, error);
		}

		public static void SetLastAccessTime (string path, DateTime lastAccessTime)
		{
			File.SetLastAccessTime (path, lastAccessTime);
		}

		public static void SetLastAccessTimeUtc (string path, DateTime lastAccessTimeUtc)
		{
			SetLastAccessTime (path, lastAccessTimeUtc.ToLocalTime ());
		}

		public static void SetLastWriteTime (string path, DateTime lastWriteTime)
		{
			File.SetLastWriteTime (path, lastWriteTime);
		}

		public static void SetLastWriteTimeUtc (string path, DateTime lastWriteTimeUtc)
		{
			SetLastWriteTime (path, lastWriteTimeUtc.ToLocalTime ());
		}

		// private
		
		private static void CheckPathExceptions (string path)
		{
			if (path == null)
				throw new System.ArgumentNullException("path");
			if (path.Length == 0)
				throw new System.ArgumentException("Path is Empty");
			if (path.Trim().Length == 0)
				throw new ArgumentException ("Only blank characters in path");
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid chars");
		}

		// Does the common validation, searchPattern has already been checked for not-null
		static string ValidateDirectoryListing (string path, string searchPattern, out bool stop)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (path.Trim ().Length == 0)
				throw new ArgumentException ("The Path does not have a valid format");

			string wild = Path.Combine (path, searchPattern);
			string wildpath = Path.GetDirectoryName (wild);
			if (wildpath.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid characters");

			if (wildpath.IndexOfAny (Path.InvalidPathChars) != -1) {
				if (path.IndexOfAny (SearchPattern.InvalidChars) == -1)
					throw new ArgumentException ("Path contains invalid characters", "path");

				throw new ArgumentException ("Pattern contains invalid characters", "pattern");
			}

			MonoIOError error;
			if (!MonoIO.ExistsDirectory (wildpath, out error)) {
				if (error == MonoIOError.ERROR_SUCCESS) {
					MonoIOError file_error;
					if (MonoIO.ExistsFile (wildpath, out file_error)) {
						stop = true;
						return wildpath;
					}
				}

				if (error != MonoIOError.ERROR_PATH_NOT_FOUND)
					throw MonoIO.GetException (wildpath, error);

				if (wildpath.IndexOfAny (SearchPattern.WildcardChars) == -1)
					throw new DirectoryNotFoundException ("Directory '" + wildpath + "' not found.");

				if (path.IndexOfAny (SearchPattern.WildcardChars) == -1)
					throw new ArgumentException ("Pattern is invalid", "searchPattern");

				throw new ArgumentException ("Path is invalid", "path");
			}

			stop = false;
			return Path.Combine (wildpath, searchPattern);
		}
		
		private static string [] GetFileSystemEntries (string path, string searchPattern, FileAttributes mask, FileAttributes attrs)
		{
			if (searchPattern == null)
				throw new ArgumentNullException ("searchPattern");
			if (searchPattern.Length == 0)
				return new string [] {};
			bool stop;
			string path_with_pattern = ValidateDirectoryListing (path, searchPattern, out stop);
			if (stop)
				return new string [] { path_with_pattern };

			MonoIOError error;
			string [] result = MonoIO.GetFileSystemEntries (path, path_with_pattern, (int) attrs, (int) mask, out error);
			if (error != 0)
				throw MonoIO.GetException (Path.GetDirectoryName (Path.Combine (path, searchPattern)), error);
			
			return result;
		}

		internal static void ValidatePath (string path)
		{
#if MOONLIGHT
			// On Moonlight (SL4+) this is possible, with limitations, in "Elevated Trust"
			throw new SecurityException ("we're not ready to enable this SL4 feature yet");
#endif
		}

#if NET_4_0 || MOONLIGHT
		public static string[] GetFileSystemEntries (string path, string searchPattern, SearchOption searchOption)
		{
			// Take the simple way home:
			return new List<string> (EnumerateFileSystemEntries (path, searchPattern, searchOption)).ToArray ();
		}
						       
		internal static IEnumerable<string> EnumerateKind (string path, string searchPattern, SearchOption searchOption, FileAttributes kind)
		{
			if (searchPattern == null)
				throw new ArgumentNullException ("searchPattern");

			if (searchPattern.Length == 0)
				yield break;

			if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
				throw new ArgumentOutOfRangeException ("searchoption");

			ValidatePath (path);

			bool stop;
			string path_with_pattern = ValidateDirectoryListing (path, searchPattern, out stop);
			if (stop){
				yield return path_with_pattern;
				yield break;
			}
			
			IntPtr handle;
			MonoIOError error;
			FileAttributes rattr;
			bool subdirs = searchOption == SearchOption.AllDirectories;
			
			string s = MonoIO.FindFirst (path, path_with_pattern, out rattr, out error, out handle);
			if (s == null)
				yield break;
			if (error != 0)
				throw MonoIO.GetException (Path.GetDirectoryName (Path.Combine (path, searchPattern)), (MonoIOError) error);

			try {
				if (((rattr & FileAttributes.ReparsePoint) == 0) && ((rattr & kind) != 0))
					yield return s;
				
				while ((s = MonoIO.FindNext (handle, out rattr, out error)) != null){
					if ((rattr & FileAttributes.ReparsePoint) != 0)
						continue;
					if ((rattr & kind) != 0)
						yield return s;
					
					if (((rattr & FileAttributes.Directory) != 0) && subdirs)
						foreach (string child in EnumerateKind (s, searchPattern, searchOption, kind))
							yield return child;
				}
			} finally {
				MonoIO.FindClose (handle);
			}
		}

		public static IEnumerable<string> EnumerateDirectories (string path, string searchPattern, SearchOption searchOption)
		{
			return EnumerateKind (path, searchPattern, searchOption, FileAttributes.Directory);
		}
		
		public static IEnumerable<string> EnumerateDirectories (string path, string searchPattern)
		{
			return EnumerateKind (path, searchPattern, SearchOption.TopDirectoryOnly, FileAttributes.Directory);
		}

		public static IEnumerable<string> EnumerateDirectories (string path)
		{
			return EnumerateKind (path, "*", SearchOption.TopDirectoryOnly, FileAttributes.Directory);
		}

		public static IEnumerable<string> EnumerateFiles (string path, string searchPattern, SearchOption searchOption)
		{
			return EnumerateKind (path, searchPattern, searchOption, FileAttributes.Normal);
		}

		public static IEnumerable<string> EnumerateFiles (string path, string searchPattern)
		{
			return EnumerateKind (path, searchPattern, SearchOption.TopDirectoryOnly, FileAttributes.Normal);
		}

		public static IEnumerable<string> EnumerateFiles (string path)
		{
			return EnumerateKind (path, "*", SearchOption.TopDirectoryOnly, FileAttributes.Normal);
		}

		public static IEnumerable<string> EnumerateFileSystemEntries (string path, string searchPattern, SearchOption searchOption)
		{
			return EnumerateKind (path, searchPattern, searchOption, FileAttributes.Normal | FileAttributes.Directory);
		}

		public static IEnumerable<string> EnumerateFileSystemEntries (string path, string searchPattern)
		{
			return EnumerateKind (path, searchPattern, SearchOption.TopDirectoryOnly, FileAttributes.Normal | FileAttributes.Directory);
		}

		public static IEnumerable<string> EnumerateFileSystemEntries (string path)
		{
			return EnumerateKind (path, "*", SearchOption.TopDirectoryOnly, FileAttributes.Normal | FileAttributes.Directory);
		}
		
#endif

#if !MOONLIGHT
		[MonoNotSupported ("DirectorySecurity isn't implemented")]
		public static DirectorySecurity GetAccessControl (string path, AccessControlSections includeSections)
		{
			throw new PlatformNotSupportedException ();
		}

		[MonoNotSupported ("DirectorySecurity isn't implemented")]
		public static DirectorySecurity GetAccessControl (string path)
		{
			throw new PlatformNotSupportedException ();
		}
#endif
	}
}
