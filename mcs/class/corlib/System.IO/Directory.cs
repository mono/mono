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
using System.Security.AccessControl;

namespace System.IO
{
	public static partial class Directory
	{
		public static string GetDirectoryRoot (string path)
		{
			Path.Validate (path);
			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			// FIXME nice hack but that does not work under windows
			return new String(Path.DirectorySeparatorChar, 1);
		}

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

			// after validations but before File.Exists to avoid an oracle
			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			if (File.Exists(path))
				throw new IOException ("Cannot create " + path + " because a file with the same name already exists.");
			
			if (Environment.IsRunningOnWindows && path == ":")
				throw new ArgumentException ("Only ':' In path");
			
			return CreateDirectoriesInternal (path);
		}

		[MonoLimitation ("DirectorySecurity not implemented")]
		public static DirectoryInfo CreateDirectory (string path, DirectorySecurity directorySecurity)
		{
			return(CreateDirectory (path));
		}

		static DirectoryInfo CreateDirectoriesInternal (string path)
		{
#if !MOBILE
			if (SecurityManager.SecurityEnabled) {
				new FileIOPermission (FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, path).Demand ();
			}
#endif
			DirectoryInfo info = new DirectoryInfo (path, true);
			if (info.Parent != null && !info.Parent.Exists)
				 info.Parent.Create ();

			MonoIOError error;
			if (!MonoIO.CreateDirectory (info.FullName, out error)) {
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
			Path.Validate (path);

			if (Environment.IsRunningOnWindows && path == ":")
				throw new NotSupportedException ("Only ':' In path");

			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

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
				 * In io-layer/io.c rmdir returns error_file_not_found if directory does not exist.
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
			Path.Validate (path);			
			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			if (recursive)
				RecursiveDelete (path);
			else
				Delete (path);
		}

		public static bool Exists (string path)
		{
			if (path == null)
				return false;

			// on Moonlight this does not throw but returns false
			if (!SecurityManager.CheckElevatedPermissions ())
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
			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			string result = InsecureGetCurrentDirectory();
#if !MOBILE
			if ((result != null) && (result.Length > 0) && SecurityManager.SecurityEnabled) {
				new FileIOPermission (FileIOPermissionAccess.PathDiscovery, result).Demand ();
			}
#endif
			return result;
		}

		internal static string InsecureGetCurrentDirectory()
		{
			MonoIOError error;
			string result = MonoIO.GetCurrentDirectory(out error);

			if (error != MonoIOError.ERROR_SUCCESS)
				throw MonoIO.GetException(error);

			return result;
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
			Path.Validate (path);			

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

			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			if (Exists (destDirName))
				throw new IOException (destDirName + " already exists.");

			if (!Exists (sourceDirName) && !File.Exists (sourceDirName))
				throw new DirectoryNotFoundException (sourceDirName + " does not exist");

			MonoIOError error;
			if (!MonoIO.MoveFile (sourceDirName, destDirName, out error))
				throw MonoIO.GetException (error);
		}

		public static void SetAccessControl (string path, DirectorySecurity directorySecurity)
		{
			if (null == directorySecurity)
				throw new ArgumentNullException ("directorySecurity");
				
			directorySecurity.PersistModifications (path);
		}

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

		public static DirectorySecurity GetAccessControl (string path, AccessControlSections includeSections)
		{
			return new DirectorySecurity (path, includeSections);
		}

		public static DirectorySecurity GetAccessControl (string path)
		{
			// AccessControlSections.Audit requires special permissions.
			return GetAccessControl (path,
						 AccessControlSections.Owner |
						 AccessControlSections.Group |
						 AccessControlSections.Access);
		}

#region Copied from reference source
        internal static String GetDemandDir(string fullPath, bool thisDirOnly)
        {
            String demandPath;

            if (thisDirOnly) {
                if (fullPath.EndsWith( Path.DirectorySeparatorChar ) 
                    || fullPath.EndsWith( Path.AltDirectorySeparatorChar ) )
                    demandPath = fullPath + ".";
                else
                    demandPath = fullPath + Path.DirectorySeparatorCharAsString + ".";
            }
            else {
                if (!(fullPath.EndsWith( Path.DirectorySeparatorChar ) 
                    || fullPath.EndsWith( Path.AltDirectorySeparatorChar )) )
                    demandPath = fullPath + Path.DirectorySeparatorCharAsString;
                else
                    demandPath = fullPath;
            }
            return demandPath;
        }
#endregion
	}
}
