// 
// System.IO.File.cs 
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
	public sealed class File : Object
	{
		public static StreamWriter AppendText (string path)
		{	
			return new StreamWriter (path, true);
		}
		 
		public static void Copy (string sourceFilename, string destFilename)
		{
			Copy (sourceFilename, destFilename, true);
		}
		 
		public static void Copy (string src, string dest, bool overwrite)
		{	
			if (src == null || dest == null)
				throw new ArgumentNullException ();
			if (src == "" || dest == null ||
			    src.IndexOfAny (Path.InvalidPathChars) != -1 ||
			    dest.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ();

			if (!MonoIO.CopyFile (src, dest, overwrite))
				throw MonoIO.GetException ();
		}

		public static FileStream Create (string path)
		{
			return Create (path, 8192);
		}

		public static FileStream Create (string path, int buffersize)
		{
			return new FileStream (path, FileMode.Create, FileAccess.ReadWrite,
					       FileShare.None, buffersize);
		}
		
		public static void Delete (string path)
		{
			if (path == null)
				throw new ArgumentNullException ();
			if (path == "" || path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ();
			
			if (!MonoIO.DeleteFile (path))
				throw MonoIO.GetException ();
		}
		
		public static bool Exists (string path)
		{
			return MonoIO.ExistsFile (path);
		}

		public static FileAttributes GetAttributes (string path)
		{
			return MonoIO.GetFileAttributes (path);
		}

		public static DateTime GetCreationTime (string path)
		{
			MonoIOStat stat;

			MonoIO.GetFileStat (path, out stat);
			return DateTime.FromFileTime (stat.CreationTime);
		}

		public static DateTime GetLastAccessTime (string path)
		{
			MonoIOStat stat;

			MonoIO.GetFileStat (path, out stat);
			return DateTime.FromFileTime (stat.LastAccessTime);
		}

		public static DateTime GetLastWriteTime (string path)
		{
			MonoIOStat stat;

			MonoIO.GetFileStat (path, out stat);
			return DateTime.FromFileTime (stat.LastWriteTime);
		}

		public static void Move (string src, string dest)
		{
			if (src == null || dest == null)
				throw new ArgumentNullException ();
			if (src == "" || dest == null ||
			    src.IndexOfAny (Path.InvalidPathChars) != -1 ||
			    dest.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ();

			if (!MonoIO.MoveFile (src, dest))
				throw MonoIO.GetException ();
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

		public FileStream OpenWrite (string path)
		{
			return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		}

		public static void SetAttributes (string path, FileAttributes attributes)
		{
			if (!MonoIO.SetFileAttributes (path, attributes))
				throw MonoIO.GetException (path);
		}

		public static void SetCreationTime (string path, DateTime creation_time)
		{
			if (!MonoIO.SetFileTime (path, creation_time.Ticks, -1, -1))
				throw MonoIO.GetException (path);
		}

		public static void SetLastAccessTime (string path, DateTime last_access_time)
		{
			if (!MonoIO.SetFileTime (path, -1, last_access_time.Ticks, -1))
				throw MonoIO.GetException (path);
		}

		public static void SetLastWriteTime (string path, DateTime last_write_time)
		{
			if (!MonoIO.SetFileTime (path, -1, -1, last_write_time.Ticks))
				throw MonoIO.GetException (path);
		}
	}
}
