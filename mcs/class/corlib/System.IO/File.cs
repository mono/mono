// 
// System.IO.File.cs 
//
// 
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Jim Richardson  (develop@wtfo-guru.com)
//
// Copyright 2002 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
//

using System;
using System.Private;

namespace System.IO
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class File : Object
	{
		const int COPY_BLOCK_SIZE = 32 * 1024;
		
		public static StreamWriter AppendText (string path)
		{	
			return new StreamWriter (path, true);
		}
		 
		public static void Copy (string sourceFilename, string destFilename)
		{
			Copy (sourceFilename, destFilename, true);
		}
		 
		public static void Copy (string src, string dest, bool overrite)
		{	
			if (src == null || dest == null)
				throw new ArgumentNullException ();
			if (src == "" || dest == null ||
			    src.IndexOfAny (Path.InvalidPathChars) != -1 ||
			    dest.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ();

			using (FileStream src_stream = new FileStream (
				src, FileMode.Open, FileAccess.Read, FileShare.Read, COPY_BLOCK_SIZE),
			       dst_stream = new FileStream (
				       dest, FileMode.CreateNew, FileAccess.Write, FileShare.None,
				       COPY_BLOCK_SIZE)){
				byte [] buffer = new byte [COPY_BLOCK_SIZE];
				int count;
			
				while ((count = src_stream.Read (buffer, 0, COPY_BLOCK_SIZE)) != 0)
					dst_stream.Write (buffer, 0, count);
			}
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
			int code;
			
			if (path == null)
				throw new ArgumentNullException ();
			if (path == "" || path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ();
			
			code = Wrapper.unlink (path);
			if (code == Wrapper.EISDIR)
				throw new UnauthorizedAccessException ();
			else
				throw new IOException (Errno.Message (code));
		}
		
		public static bool Exists (string path)
		{
			unsafe {
				stat s;
				
				if (Wrapper.stat (path, &s) == 0)
					return true;
				return false;
			}
		}

		[MonoTODO]
		public static FileAttributes GetAttributes (string path)
		{
			throw new Exception ("Unimplemented");
		}

		[MonoTODO]
		public static DateTime GetCreationTime (string path)
		{
			throw new Exception ("Unimplemented");
		}

		[MonoTODO]
		public static DateTime GetLastAccessTime (string path)
		{
			throw new Exception ("Unimplemented");
		}

		[MonoTODO]
		public static DateTime GetLastWriteTime (string path)
		{
			throw new Exception ("Unimplemented");
		}


		public static void Move (string src, string dest)
		{
			if (src == null || dest == null)
				throw new ArgumentNullException ();
			if (src == "" || dest == null ||
			    src.IndexOfAny (Path.InvalidPathChars) != -1 ||
			    dest.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ();

			int code = Wrapper.rename (src, dest);
			if (code == 0)
				return;
			throw new Exception (Errno.Message (code));
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

		[MonoTODO]
		public static void SetAttributes (string path, FileAttributes attributes)
		{
			throw new Exception ("Unimplemented");
		}

		[MonoTODO]
		public static void SetCreationTime (string path, DateTime creationTime)
		{
			throw new Exception ("Unimplemented");
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
	}
}
