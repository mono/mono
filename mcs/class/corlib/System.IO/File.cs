// 
// System.IO.FIle.cs 
//
// 
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Jim Richardson  (develop@wtfo-guru.com)
//   Dan Lewis       (dihlewis@yahoo.co.uk)
//   Ville Palo      (vi64pa@kolumbus.fi)
//
// Copyright 2002 Ximian, Inc. http://www.ximian.com
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

namespace System.IO
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class File
	{
		private File () {}

		
		
		public static StreamWriter AppendText (string path)
		{	
			return new StreamWriter (path, true);
		}

		[MonoTODO("Security Permision Checks")]
		public static void Copy (string sourceFilename, string destFilename)
		{
			Copy (sourceFilename, destFilename, false);
		}

		public static void Copy (string src, string dest, bool overwrite)
		{	
			if (src == null)
				throw new ArgumentNullException ("src");
			if (dest == null)
				throw new ArgumentNullException ("dest");
			if (src.Trim () == "" || src.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("src");
			if (dest.Trim () == "" || dest.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("dest");
			if (!Exists (src))
				throw new FileNotFoundException (src + " does not exist", src);

			if ((GetAttributes(src) & FileAttributes.Directory) == FileAttributes.Directory){
				throw new ArgumentException(src + " is a directory");
			}
			
			if (Exists (dest)) {
				if ((GetAttributes(dest) & FileAttributes.Directory) == FileAttributes.Directory){
					throw new ArgumentException(dest + " is a directory");	
				}
				if (!overwrite)
					throw new IOException (dest + " already exists");
			}

			string DirName = Path.GetDirectoryName(dest);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException("Destination directory not found: " + DirName);

			MonoIOError error;
			
			if (!MonoIO.CopyFile (src, dest, overwrite, out error))
				throw MonoIO.GetException (error);
		}

		public static FileStream Create (string path)
		{
			return Create (path, 8192);
		}

		public static FileStream Create (string path, int buffersize)
		{
			if (null == path)
				throw new ArgumentNullException("path");
			if (String.Empty == path.Trim() || path.IndexOfAny(Path.InvalidPathChars) >= 0)
				throw new ArgumentException("path");

			string DirName = Path.GetDirectoryName(path);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException("Destination directory not found: " + DirName);
			if (Exists(path)){
				if ((GetAttributes(path) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly){
					throw new UnauthorizedAccessException(path + " is a read-only");	
				}
			}

			return new FileStream (path, FileMode.Create, FileAccess.ReadWrite,
					       FileShare.None, buffersize);
		}

		public static StreamWriter CreateText(string path)
		
		{
			return new StreamWriter (path, false);
		
		}
		
		
		
		public static void Delete (string path)
		{
			if (null == path)
				throw new ArgumentNullException("path");
			if (String.Empty == path.Trim() || path.IndexOfAny(Path.InvalidPathChars) >= 0)
				throw new ArgumentException("path");
			if (Directory.Exists (path))
				throw new UnauthorizedAccessException("path is a directory");

			string DirName = Path.GetDirectoryName(path);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException("Destination directory not found: " + DirName);

			MonoIOError error;
			
			if (!MonoIO.DeleteFile (path, out error)){
				Exception e = MonoIO.GetException (path, error);
				if (! (e is FileNotFoundException))
					throw e;
			}
		}

		public static bool Exists (string path)
		{
			// For security reasons no exceptions are
			// thrown, only false is returned if there is
			// any problem with the path or permissions.
			// Minimizes what information can be
			// discovered by using this method.
			if (null == path || String.Empty == path.Trim()
			    || path.IndexOfAny(Path.InvalidPathChars) >= 0) {
				return false;
			}

			MonoIOError error;
			bool exists;
			
			exists = MonoIO.ExistsFile (path, out error);
			if (error != MonoIOError.ERROR_SUCCESS &&
			    error != MonoIOError.ERROR_FILE_NOT_FOUND &&
			    error != MonoIOError.ERROR_PATH_NOT_FOUND) {
				throw MonoIO.GetException (path, error);
			}
			
			return(exists);
		}

		public static FileAttributes GetAttributes (string path)
		{
			if (null == path) {
				throw new ArgumentNullException("path");
			}
			
			if (String.Empty == path.Trim()) {
				throw new ArgumentException("Path is empty");
			}

			if (path.IndexOfAny(Path.InvalidPathChars) >= 0) {
				throw new ArgumentException("Path contains invalid chars");
			}

			MonoIOError error;
			FileAttributes attrs;
			
			attrs = MonoIO.GetFileAttributes (path, out error);
			if (error != MonoIOError.ERROR_SUCCESS) {
				throw MonoIO.GetException (path, error);
			}

			return(attrs);
		}

		public static DateTime GetCreationTime (string path)
		{
			MonoIOStat stat;
			MonoIOError error;
			CheckPathExceptions (path);
			
			if (!MonoIO.GetFileStat (path, out stat, out error))
				throw new IOException (path);
			return DateTime.FromFileTime (stat.CreationTime);
		}

		public static DateTime GetCreationTimeUtc (string path)
		{
			return GetCreationTime (path).ToUniversalTime ();
		}

		public static DateTime GetLastAccessTime (string path)
		{
			MonoIOStat stat;
			MonoIOError error;
			CheckPathExceptions (path);

			if (!MonoIO.GetFileStat (path, out stat, out error))
				throw new IOException (path);
			return DateTime.FromFileTime (stat.LastAccessTime);
		}

		public static DateTime GetLastAccessTimeUtc (string path)
		{
			return GetLastAccessTime (path).ToUniversalTime ();
		}

		public static DateTime GetLastWriteTime (string path)
		{
			MonoIOStat stat;
			MonoIOError error;
			CheckPathExceptions (path);

			if (!MonoIO.GetFileStat (path, out stat, out error))
				throw new IOException (path);
			return DateTime.FromFileTime (stat.LastWriteTime);
		}

		public static DateTime GetLastWriteTimeUtc (string path)
		{
			return GetLastWriteTime (path).ToUniversalTime ();
		}

		public static void Move (string src, string dest)
		{
			MonoIOError error;

			if (src == null)
				throw new ArgumentNullException ("src");
			if (dest == null)
				throw new ArgumentNullException ("dest");
			if (src.Trim () == "" || src.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("src");
			if (dest.Trim () == "" || dest.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("dest");
			if (!MonoIO.Exists (src, out error))
				throw new FileNotFoundException (src + " does not exist", src);
			if (MonoIO.ExistsDirectory (dest, out error))
					throw new IOException (dest + " is a directory");	

			// Don't check for this error here to allow the runtime to check if src and dest
			// are equal. Comparing src and dest is not enough.
			//if (MonoIO.Exists (dest, out error))
			//	throw new IOException (dest + " already exists");

			string DirName;
			DirName = Path.GetDirectoryName(src);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException("Source directory not found: " + DirName);
			DirName = Path.GetDirectoryName(dest);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException("Destination directory not found: " + DirName);

			if (!MonoIO.MoveFile (src, dest, out error)) {
				if (error == MonoIOError.ERROR_ALREADY_EXISTS)
					throw MonoIO.GetException (dest, error);
 				throw MonoIO.GetException (error);
			}

		}
		
		public static FileStream Open (string path, FileMode mode)
		{	
			return new FileStream (path, mode, FileAccess.ReadWrite, FileShare.None);
		}
		
		public static FileStream Open (string path, FileMode mode, FileAccess access)
		{	
			return new FileStream (path, mode, access, FileShare.None);
		}

		public static FileStream Open (string path, FileMode mode, FileAccess access,
					       FileShare share)
		{
			return new FileStream (path, mode, access, share);
		}
		
		public static FileStream OpenRead (string path)
		{	
			return new FileStream (path, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public static StreamReader OpenText (string path)
		{
			return new StreamReader (path);
		}

		public static FileStream OpenWrite (string path)
		{
			return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		}

		public static void SetAttributes (string path,
						  FileAttributes attributes)
		{
			MonoIOError error;
			CheckPathExceptions (path);
			
			if (!MonoIO.SetFileAttributes (path, attributes,
						       out error)) {
				throw MonoIO.GetException (path, error);
			}
		}

		public static void SetCreationTime (string path,
						    DateTime creation_time)
		{
			MonoIOError error;
			CheckPathExceptions (path);
			if (!MonoIO.Exists (path, out error))
				throw MonoIO.GetException (path, error);
			
			if (!MonoIO.SetCreationTime (path, creation_time, out error)) {
				throw MonoIO.GetException (path, error);
			}
		}

		public static void SetCreationTimeUtc (string path,
						    DateTime creation_time)
		{
			SetCreationTime (path, creation_time.ToLocalTime ());
		}

		public static void SetLastAccessTime (string path,DateTime last_access_time)
		{
			MonoIOError error;
			CheckPathExceptions (path);
			if (!MonoIO.Exists (path, out error))
				throw MonoIO.GetException (path, error);

			if (!MonoIO.SetLastAccessTime (path, last_access_time, out error)) {
				throw MonoIO.GetException (path, error);
			}
		}

		public static void SetLastAccessTimeUtc (string path,DateTime last_access_time)
		{
			SetLastAccessTime (path, last_access_time.ToLocalTime ());
		}

		public static void SetLastWriteTime (string path,
						     DateTime last_write_time)
		{
			MonoIOError error;
			CheckPathExceptions (path);
			if (!MonoIO.Exists (path, out error))
				throw MonoIO.GetException (path, error);

			if (!MonoIO.SetLastWriteTime (path, last_write_time, out error)) {
				throw MonoIO.GetException (path, error);
			}
		}

		public static void SetLastWriteTimeUtc (string path,
						     DateTime last_write_time)
		{
			SetLastWriteTime (path, last_write_time.ToLocalTime ());
		}

		#region Private

		private static void CheckPathExceptions (string path)
		{
			if (path == null)
				throw new System.ArgumentNullException("Path is Null");
			if (path == "")
				throw new System.ArgumentException("Path is Empty");
			if (path.Trim().Length == 0)
				throw new ArgumentException ("Only blank characters in path");
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Path contains invalid chars");
		}

		#endregion
	}
}
