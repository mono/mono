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
// Copyright (C) 2004, 2006 Novell, Inc (http://www.novell.com)
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
using System.Text;
#if NET_2_0
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
#endif

namespace System.IO
{
	/// <summary>
	/// 
	/// </summary>
#if NET_2_0
	[ComVisible (true)]
#endif
	public
#if NET_2_0
	static
#else
	sealed
#endif
	class File
	{

#if !NET_2_0
		private File () {}
#endif
		
#if NET_2_0
		public static void AppendAllText (string path, string contents)
		{	
			using (TextWriter w = new StreamWriter (path, true)) {
				w.Write (contents);
			}
		}

		public static void AppendAllText (string path, string contents, Encoding encoding)
		{	
			using (TextWriter w = new StreamWriter (path, true, encoding)) {
				w.Write (contents);
			}
		}
#endif

		public static StreamWriter AppendText (string path)
		{	
			return new StreamWriter (path, true);
		}

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
				throw new ArgumentException (Locale.GetText ("src is null"));
			if (dest.Trim () == "" || dest.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException (Locale.GetText ("dest is empty or contains invalid characters"));
			if (!Exists (src))
				throw new FileNotFoundException (Locale.GetText ("{0} does not exist", src), src);

			if ((GetAttributes(src) & FileAttributes.Directory) == FileAttributes.Directory){
				throw new ArgumentException(Locale.GetText ("{0} is a directory", src));
			}
			
			if (Exists (dest)) {
				if ((GetAttributes(dest) & FileAttributes.Directory) == FileAttributes.Directory){
					throw new ArgumentException (Locale.GetText ("{0} is a directory", dest));
				}
				if (!overwrite)
					throw new IOException (Locale.GetText ("{0} already exists", dest));
			}

			string DirName = Path.GetDirectoryName(dest);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException (Locale.GetText ("Destination directory not found: {0}",DirName));

			MonoIOError error;
			
			if (!MonoIO.CopyFile (src, dest, overwrite, out error)){
				string p = Locale.GetText ("{0}\" or \"{1}", src, dest);
				throw MonoIO.GetException (p, error);
			}
		}

		public static FileStream Create (string path)
		{
			return(Create (path, 8192, FileOptions.None, null));
		}

		public static FileStream Create (string path, int buffersize)
		{
			return(Create (path, buffersize, FileOptions.None,
				       null));
		}

#if NET_2_0
		[MonoTODO ("options not implemented")]
		public static FileStream Create (string path, int bufferSize,
						 FileOptions options)
		{
			return(Create (path, bufferSize, options, null));
		}
		
		[MonoTODO ("options and fileSecurity not implemented")]
		public static FileStream Create (string path, int bufferSize,
						 FileOptions options,
						 FileSecurity fileSecurity)
#else
		private static FileStream Create (string path, int bufferSize,
						  FileOptions options,
						  object fileSecurity)
#endif
		{
			if (null == path)
				throw new ArgumentNullException ("path");
			if (String.Empty == path.Trim() || path.IndexOfAny(Path.InvalidPathChars) >= 0)
				throw new ArgumentException (Locale.GetText ("path is invalid"));

			string DirName = Path.GetDirectoryName(path);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException (Locale.GetText ("Destination directory not found: {0}", DirName));
			if (Exists(path)){
				if ((GetAttributes(path) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly){
					throw new UnauthorizedAccessException (Locale.GetText ("{0} is read-only", path));
				}
			}

			return new FileStream (path, FileMode.Create, FileAccess.ReadWrite,
					       FileShare.None, bufferSize);
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
				throw new UnauthorizedAccessException(Locale.GetText ("{0} is a directory", path));

			string DirName = Path.GetDirectoryName(path);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException (Locale.GetText ("Destination directory not found: {0}", DirName));

			MonoIOError error;
			
			if (!MonoIO.DeleteFile (path, out error)){
				if (error != MonoIOError.ERROR_FILE_NOT_FOUND)
					throw MonoIO.GetException (path, error);
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
				throw new ArgumentException (Locale.GetText ("Path is empty"));
			}

			if (path.IndexOfAny(Path.InvalidPathChars) >= 0) {
				throw new ArgumentException(Locale.GetText ("Path contains invalid chars"));
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

			if (!MonoIO.GetFileStat (path, out stat, out error)) {
#if NET_2_0
				if (error == MonoIOError.ERROR_PATH_NOT_FOUND || error == MonoIOError.ERROR_FILE_NOT_FOUND)
					return _defaultLocalFileTime;
				else
					throw new IOException (path);
#else
				throw new IOException (path);
#endif
			}
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

			if (!MonoIO.GetFileStat (path, out stat, out error)) {
#if NET_2_0
				if (error == MonoIOError.ERROR_PATH_NOT_FOUND || error == MonoIOError.ERROR_FILE_NOT_FOUND)
					return _defaultLocalFileTime;
				else
					throw new IOException (path);
#else
				throw new IOException (path);
#endif
			}
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

			if (!MonoIO.GetFileStat (path, out stat, out error)) {
#if NET_2_0
				if (error == MonoIOError.ERROR_PATH_NOT_FOUND || error == MonoIOError.ERROR_FILE_NOT_FOUND)
					return _defaultLocalFileTime;
				else
					throw new IOException (path);
#else
				throw new IOException (path);
#endif
			}
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
				throw new FileNotFoundException (Locale.GetText ("{0} does not exist", src), src);
			if (MonoIO.ExistsDirectory (dest, out error))
					throw new IOException (Locale.GetText ("{0} is a directory", dest));	

			// Don't check for this error here to allow the runtime to check if src and dest
			// are equal. Comparing src and dest is not enough.
			//if (MonoIO.Exists (dest, out error))
			//	throw new IOException (Locale.GetText ("{0} already exists", dest));

			string DirName;
			DirName = Path.GetDirectoryName(src);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException(Locale.GetText ("Source directory not found: {0}", DirName));
			DirName = Path.GetDirectoryName(dest);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException(Locale.GetText ("Destination directory not found: {0}", DirName));

			if (!MonoIO.MoveFile (src, dest, out error)) {
				if (error == MonoIOError.ERROR_ALREADY_EXISTS)
					throw MonoIO.GetException (dest, error);
				throw MonoIO.GetException (error);
			}
		}
		
		public static FileStream Open (string path, FileMode mode)
		{	
			return new FileStream (path, mode, mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
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
				throw new System.ArgumentNullException("path");
			if (path == "")
				throw new System.ArgumentException(Locale.GetText ("Path is empty"));
			if (path.Trim().Length == 0)
				throw new ArgumentException (Locale.GetText ("Path is empty"));
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException (Locale.GetText ("Path contains invalid chars"));
		}

		#endregion

#if NET_2_0
		static File() {
			_defaultLocalFileTime = new DateTime (1601, 1, 1);
			_defaultLocalFileTime = _defaultLocalFileTime.ToLocalTime ();
		}

		//
		// The documentation for this method is most likely wrong, it
		// talks about doing a "binary read", but the remarks say
		// that this "detects the encoding".
		//
		// This can not detect and do anything useful with the encoding
		// since the result is a byte [] not a char [].
		//
		public static byte [] ReadAllBytes (string path)
		{
			using (FileStream s = Open (path, FileMode.Open, FileAccess.Read, FileShare.Read)){
				long size = s.Length;

				//
				// Is this worth supporting?
				// 
				if (size > Int32.MaxValue)
					throw new ArgumentException ("Reading more than 4gigs with this call is not supported");
				
				byte [] result = new byte [s.Length];

				s.Read (result, 0, (int) size);

				return result;
			}
		}

		public static string [] ReadAllLines (string path)
		{
			using (StreamReader reader = File.OpenText (path)) {
				return ReadAllLines (reader);
			}
		}

		public static string [] ReadAllLines (string path, Encoding encoding)
		{
			using (StreamReader reader = new StreamReader (path, encoding)) {
				return ReadAllLines (reader);
			}
		}

		static string [] ReadAllLines (StreamReader reader)
		{
			List<string> list = new List<string> ();
			while (!reader.EndOfStream)
				list.Add (reader.ReadLine ());
			return list.ToArray ();
		}

		public static string ReadAllText (string path)
		{
			return ReadAllText (path, Encoding.UTF8Unmarked);
		}

		public static string ReadAllText (string path, Encoding enc)
		{
			using (StreamReader sr = new StreamReader (path, enc)) {
				return sr.ReadToEnd ();
			}
		}

		public static void WriteAllBytes (string path, byte [] data)
		{
			using (Stream stream = File.Create (path)) {
				stream.Write (data, 0, data.Length);
			}
		}

		public static void WriteAllLines (string path, string [] lines)
		{
			using (StreamWriter writer = new StreamWriter (path)) {
				WriteAllLines (writer, lines);
			}
		}

		public static void WriteAllLines (string path, string [] lines, Encoding encoding)
		{
			using (StreamWriter writer = new StreamWriter (path, false, encoding)) {
				WriteAllLines (writer, lines);
			}
		}

		static void WriteAllLines (StreamWriter writer, string [] lines)
		{
			foreach (string line in lines)
				writer.WriteLine (line);
		}

		public static void WriteAllText (string path, string contents)
		{
			WriteAllText (path, contents, Encoding.UTF8Unmarked);
		}

		public static void WriteAllText (string path, string contents, Encoding enc)
		{
			using (StreamWriter sw = new StreamWriter (path, false, enc)) {
				sw.Write (contents);
			}
		}

		private static readonly DateTime _defaultLocalFileTime;

		[MonoLimitation ("File encryption isn't supported (even on NTFS).")]
		public static void Encrypt (string path)
		{
			// MS.NET support this only on NTFS file systems, i.e. it's a file-system (not a framework) feature.
			// otherwise it throws a NotSupportedException (or a PlatformNotSupportedException on older OS).
			// we throw the same (instead of a NotImplementedException) because most code should already be
			// handling this exception to work properly.
			throw new NotSupportedException (Locale.GetText ("File encryption isn't supported on any file system."));
		}

		[MonoLimitation ("File encryption isn't supported (even on NTFS).")]
		public static void Decrypt (string path)
		{
			// MS.NET support this only on NTFS file systems, i.e. it's a file-system (not a framework) feature.
			// otherwise it throws a NotSupportedException (or a PlatformNotSupportedException on older OS).
			// we throw the same (instead of a NotImplementedException) because most code should already be
			// handling this exception to work properly.
			throw new NotSupportedException (Locale.GetText ("File encryption isn't supported on any file system."));
		}
#endif
	}
}
