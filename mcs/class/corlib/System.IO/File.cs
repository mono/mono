// 
// System.IO.FIle.cs 
//
// 
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Jim Richardson  (develop@wtfo-guru.com)
//   Dan Lewis       (dihlewis@yahoo.co.uk)
//
// Copyright 2002 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
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
				throw new FileNotFoundException (src + " does not exist");

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
				Exception e = MonoIO.GetException (error);
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
			
			return MonoIO.ExistsFile (path, out error);
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
			
			return MonoIO.GetFileAttributes (path, out error);
		}

		public static DateTime GetCreationTime (string path)
		{
			MonoIOStat stat;
			MonoIOError error;
			
			if (!MonoIO.GetFileStat (path, out stat, out error))
				throw new IOException (path);
			return DateTime.FromFileTime (stat.CreationTime);
		}

		public static DateTime GetLastAccessTime (string path)
		{
			MonoIOStat stat;
			MonoIOError error;
			
			if (!MonoIO.GetFileStat (path, out stat, out error))
				throw MonoIO.GetException (path, error);
			return DateTime.FromFileTime (stat.LastAccessTime);
		}

		public static DateTime GetLastWriteTime (string path)
		{
			MonoIOStat stat;
			MonoIOError error;
			
			if (!MonoIO.GetFileStat (path, out stat, out error))
				throw MonoIO.GetException (path, error);
			return DateTime.FromFileTime (stat.LastWriteTime);
		}

		public static void Move (string src, string dest)
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
				throw new FileNotFoundException (src + " does not exist");
			if (Exists (dest) && ((GetAttributes(dest) & FileAttributes.Directory) == FileAttributes.Directory))
					throw new ArgumentException(dest + " is a directory");	

			string DirName;
			DirName = Path.GetDirectoryName(src);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException("Source directory not found: " + DirName);
			DirName = Path.GetDirectoryName(dest);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException("Destination directory not found: " + DirName);

			MonoIOError error;
			
			if (!MonoIO.MoveFile (src, dest, out error))
				throw MonoIO.GetException (error);
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
			
			if (!MonoIO.SetFileAttributes (path, attributes,
						       out error)) {
				throw MonoIO.GetException (path, error);
			}
		}

		public static void SetCreationTime (string path,
						    DateTime creation_time)
		{
			MonoIOError error;
			
			if (!MonoIO.SetFileTime (path, creation_time.ToFileTime(),
						 -1, -1, out error)) {
				throw MonoIO.GetException (path, error);
			}
		}

		public static void SetLastAccessTime (string path,DateTime last_access_time)
		{
			MonoIOError error;
			
			if (!MonoIO.SetFileTime (path, -1,
						 last_access_time.ToFileTime(), -1,
						 out error)) {
				throw MonoIO.GetException (path, error);
			}
		}

		public static void SetLastWriteTime (string path,
						     DateTime last_write_time)
		{
			MonoIOError error;
			
			if (!MonoIO.SetFileTime (path, -1, -1,
						 last_write_time.ToFileTime(),
						 out error)) {
				throw MonoIO.GetException (path, error);
			}
		}
	}
}
