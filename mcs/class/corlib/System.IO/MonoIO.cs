//
// System.IO.MonoIO.cs: static interface to native filesystem.
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//   Dick Porter (dick@ximian.com)
//
// (C) 2002
//

using System;
using System.Runtime.CompilerServices;

namespace System.IO
{
	internal sealed class MonoIO {
		public static readonly FileAttributes
			InvalidFileAttributes = (FileAttributes)(-1);

		public static readonly IntPtr
			InvalidHandle = (IntPtr)(-1);

		// error methods

		public static Exception GetException (MonoIOError error)
		{
			return GetException (String.Empty, error);
		}

		public static Exception GetException (string path,
						      MonoIOError error)
		{
			string message;

			switch (error) {
			// FIXME: add more exception mappings here
			case MonoIOError.ERROR_FILE_NOT_FOUND:
				message = String.Format ("Could not find file \"{0}\"", path);
				return new FileNotFoundException (message);

			case MonoIOError.ERROR_PATH_NOT_FOUND:
				message = String.Format ("Could not find a part of the path \"{0}\"", path);
				return new DirectoryNotFoundException (message);

			case MonoIOError.ERROR_ACCESS_DENIED:
				message = String.Format ("Access to the path \"{0}\" is denied.", path);
				return new UnauthorizedAccessException (message);

			default:
				message = String.Format ("Win32 IO returned {0}", error);
				return new IOException (message);
			}
		}

		// directory methods

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool CreateDirectory (string path, out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool RemoveDirectory (string path, out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static IntPtr FindFirstFile (string path, out MonoIOStat stat, out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool FindNextFile (IntPtr find, out MonoIOStat stat, out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool FindClose (IntPtr find,
						     out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static string GetCurrentDirectory (out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool SetCurrentDirectory (string path, out MonoIOError error);

		// file methods

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool MoveFile (string path, string dest,
						    out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool CopyFile (string path, string dest,
						    bool overwrite,
						    out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool DeleteFile (string path,
						      out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static FileAttributes GetFileAttributes (string path, out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool SetFileAttributes (string path, FileAttributes attrs, out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static MonoFileType GetFileType (IntPtr handle, out MonoIOError error);

		public static bool Exists (string path, out MonoIOError error)
		{
			FileAttributes attrs = GetFileAttributes (path,
								  out error);
			if (attrs == InvalidFileAttributes)
				return false;

			return true;
		}

		public static bool ExistsFile (string path,
					       out MonoIOError error)
		{
			FileAttributes attrs = GetFileAttributes (path,
								  out error);
			if (attrs == InvalidFileAttributes)
				return false;

			if ((attrs & FileAttributes.Directory) != 0)
				return false;

			return true;
		}

		public static bool ExistsDirectory (string path,
						    out MonoIOError error)
		{
			FileAttributes attrs = GetFileAttributes (path,
								  out error);
			if (attrs == InvalidFileAttributes)
				return false;

			if ((attrs & FileAttributes.Directory) == 0)
				return false;

			return true;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool GetFileStat (string path,
						       out MonoIOStat stat,
						       out MonoIOError error);

		// handle methods

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static IntPtr Open (string filename,
						  FileMode mode,
						  FileAccess access,
						  FileShare share,
						  out MonoIOError error);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool Close (IntPtr handle,
						 out MonoIOError error);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static int Read (IntPtr handle, byte [] dest,
					       int dest_offset, int count,
					       out MonoIOError error);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static int Write (IntPtr handle, byte [] src,
						int src_offset, int count,
						out MonoIOError error);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static long Seek (IntPtr handle, long offset,
						SeekOrigin origin,
						out MonoIOError error);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool Flush (IntPtr handle,
						 out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static long GetLength (IntPtr handle,
						     out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool SetLength (IntPtr handle,
						     long length,
						     out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool SetFileTime (IntPtr handle,
						       long creation_time,
						       long last_access_time,
						       long last_write_time,
						       out MonoIOError error);

		public static bool SetFileTime (string path,
						long creation_time,
						long last_access_time,
						long last_write_time,
						out MonoIOError error)
		{
			IntPtr handle;
			bool result;

			handle = Open (path, FileMode.Open,
				       FileAccess.ReadWrite,
				       FileShare.ReadWrite, out error);
			if (handle == IntPtr.Zero)
				return false;

			result = SetFileTime (handle, creation_time,
					      last_access_time,
					      last_write_time, out error);
			Close (handle, out error);
			
			return result;
		}

		// console handles

		public extern static IntPtr ConsoleOutput {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public extern static IntPtr ConsoleInput {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public extern static IntPtr ConsoleError {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		// pipe handles

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool CreatePipe (out IntPtr read_handle, out IntPtr write_handle);

		// path characters

		public extern static char VolumeSeparatorChar {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public extern static char DirectorySeparatorChar {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public extern static char AltDirectorySeparatorChar {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public extern static char PathSeparator {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public extern static char [] InvalidPathChars {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}
	}
}

