//
// System.IO.MonoIO.cs: static interface to native filesystem.
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//   Dick Porter (dick@ximian.com)
//
// (C) 2002
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.IO
{
	unsafe internal sealed class MonoIO {
		public static readonly FileAttributes
			InvalidFileAttributes = (FileAttributes)(-1);

		public static readonly IntPtr
			InvalidHandle = (IntPtr)(-1L);

		public static readonly bool SupportsAsync = GetSupportsAsync ();

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
				return new FileNotFoundException (message,
								  path);

			case MonoIOError.ERROR_PATH_NOT_FOUND:
				message = String.Format ("Could not find a part of the path \"{0}\"", path);
				return new DirectoryNotFoundException (message);

			case MonoIOError.ERROR_ACCESS_DENIED:
				message = String.Format ("Access to the path \"{0}\" is denied.", path);
				return new UnauthorizedAccessException (message);

			case MonoIOError.ERROR_FILE_EXISTS:
				message = String.Format ("Could not create file \"{0}\". File already exists.", path);
				return new IOException (message);

			case MonoIOError.ERROR_FILENAME_EXCED_RANGE:
				message = String.Format ("Path is too long. Path: {0}", path); 
				return new PathTooLongException (message);

			case MonoIOError.ERROR_INVALID_PARAMETER:
				message = String.Format ("Invalid parameter");
				return new IOException (message);

			default:
				message = String.Format ("Win32 IO returned {0}. Path: {1}", error, path);
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

		// aio_* methods
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static bool GetSupportsAsync ();

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
								  
			// Actually, we are looking for a directory, not a file
			if (error == MonoIOError.ERROR_FILE_NOT_FOUND)
				error = MonoIOError.ERROR_PATH_NOT_FOUND;
				
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
						  bool async,
						  out MonoIOError error);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool Close (IntPtr handle,
						 out MonoIOError error);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static int Read (IntPtr handle, byte [] dest,
					       int dest_offset, int count,
					       out MonoIOError error);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static int Write (IntPtr handle, [In] byte [] src,
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
			return SetFileTime (path,
				0,
				creation_time,
				last_access_time,
				last_write_time,
				DateTime.MinValue,
				out error);
		}

		public static bool SetCreationTime (string path,
						DateTime dateTime,
						out MonoIOError error)
		{
			return SetFileTime (path, 1, -1, -1, -1, dateTime, out error);
		}

		public static bool SetLastAccessTime (string path,
						DateTime dateTime,
						out MonoIOError error)
		{
			return SetFileTime (path, 2, -1, -1, -1, dateTime, out error);
		}

		public static bool SetLastWriteTime (string path,
						DateTime dateTime,
						out MonoIOError error)
		{
			return SetFileTime (path, 3, -1, -1, -1, dateTime, out error);
		}

		public static bool SetFileTime (string path,
						int type,
						long creation_time,
						long last_access_time,
						long last_write_time,
						DateTime dateTime,
						out MonoIOError error)
		{
			IntPtr handle;
			bool result;

			handle = Open (path, FileMode.Open,
				       FileAccess.ReadWrite,
				       FileShare.ReadWrite, false, out error);
			if (handle == MonoIO.InvalidHandle)
				return false;

			switch (type) {
			case 1:
				creation_time = dateTime.ToFileTime ();
				break;
			case 2:
				last_access_time = dateTime.ToFileTime ();
				break;
			case 3:
				last_write_time = dateTime.ToFileTime ();
				break;
			}

			result = SetFileTime (handle, creation_time,
					      last_access_time,
					      last_write_time, out error);

			MonoIOError ignore_error;
			Close (handle, out ignore_error);
			
			return result;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void Lock (IntPtr handle,
						long position, long length,
						out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void Unlock (IntPtr handle,
						  long position, long length,
						  out MonoIOError error);

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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static int GetTempPath(out string path);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void BeginWrite (IntPtr handle,FileStreamAsyncResult ares);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void BeginRead (IntPtr handle, FileStreamAsyncResult ares);

	}
}

