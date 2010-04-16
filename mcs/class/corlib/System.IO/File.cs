// 
// System.IO.File.cs 
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
// Copyright (C) 2004, 2006, 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Text;
using System.Runtime.InteropServices;

#if !NET_2_1
using System.Security.AccessControl;
#endif

namespace System.IO
{
	[ComVisible (true)]
	public static class File
	{
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

		public static StreamWriter AppendText (string path)
		{
			return new StreamWriter (path, true);
		}

		public static void Copy (string sourceFileName, string destFileName)
		{
			Copy (sourceFileName, destFileName, false);
		}

		public static void Copy (string sourceFileName, string destFileName, bool overwrite)
		{
			MonoIOError error;

			if (sourceFileName == null)
				throw new ArgumentNullException ("sourceFileName");
			if (destFileName == null)
				throw new ArgumentNullException ("destFileName");
			if (sourceFileName.Length == 0)
				throw new ArgumentException ("An empty file name is not valid.", "sourceFileName");
			if (sourceFileName.Trim ().Length == 0 || sourceFileName.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("The file name is not valid.");
			if (destFileName.Length == 0)
				throw new ArgumentException ("An empty file name is not valid.", "destFileName");
			if (destFileName.Trim ().Length == 0 || destFileName.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("The file name is not valid.");

			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			if (!MonoIO.Exists (sourceFileName, out error))
				throw new FileNotFoundException (Locale.GetText ("{0} does not exist", sourceFileName), sourceFileName);
			if ((GetAttributes (sourceFileName) & FileAttributes.Directory) == FileAttributes.Directory)
				throw new ArgumentException (Locale.GetText ("{0} is a directory", sourceFileName));

			if (MonoIO.Exists (destFileName, out error)) {
				if ((GetAttributes (destFileName) & FileAttributes.Directory) == FileAttributes.Directory)
					throw new ArgumentException (Locale.GetText ("{0} is a directory", destFileName));
				if (!overwrite)
					throw new IOException (Locale.GetText ("{0} already exists", destFileName));
			}

			string DirName = Path.GetDirectoryName (destFileName);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException (Locale.GetText ("Destination directory not found: {0}",DirName));

			if (!MonoIO.CopyFile (sourceFileName, destFileName, overwrite, out error)) {
				string p = Locale.GetText ("{0}\" or \"{1}", sourceFileName, destFileName);
				throw MonoIO.GetException (p, error);
			}
		}

		public static FileStream Create (string path)
		{
			return Create (path, 8192);
		}

		public static FileStream Create (string path, int bufferSize)
		{
			return new FileStream (path, FileMode.Create, FileAccess.ReadWrite,
				FileShare.None, bufferSize);
		}

#if !NET_2_1
		[MonoLimitation ("FileOptions are ignored")]
		public static FileStream Create (string path, int bufferSize,
						 FileOptions options)
		{
			return Create (path, bufferSize, options, null);
		}
		
		[MonoLimitation ("FileOptions and FileSecurity are ignored")]
		public static FileStream Create (string path, int bufferSize,
						 FileOptions options,
						 FileSecurity fileSecurity)
		{
			return new FileStream (path, FileMode.Create, FileAccess.ReadWrite,
				FileShare.None, bufferSize, options);
		}
#endif

		public static StreamWriter CreateText (string path)
		{
			return new StreamWriter (path, false);
		}

		public static void Delete (string path)
		{
			Path.Validate (path);
			if (Directory.Exists (path))
				throw new UnauthorizedAccessException(Locale.GetText ("{0} is a directory", path));

			string DirName = Path.GetDirectoryName(path);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException (Locale.GetText ("Could not find a part of the path \"{0}\".", path));

			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

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
			if (String.IsNullOrWhiteSpace (path) || path.IndexOfAny(Path.InvalidPathChars) >= 0)
				return false;

			// on Moonlight this does not throw but returns false
			if (!SecurityManager.CheckElevatedPermissions ())
				return false;

			MonoIOError error;
			return MonoIO.ExistsFile (path, out error);
		}

#if !NET_2_1
		public static FileSecurity GetAccessControl (string path)
		{
			throw new NotImplementedException ();
		}
		
		public static FileSecurity GetAccessControl (string path, AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}
#endif

		public static FileAttributes GetAttributes (string path)
		{
			Path.Validate (path);
			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			MonoIOError error;
			FileAttributes attrs;
			
			attrs = MonoIO.GetFileAttributes (path, out error);
			if (error != MonoIOError.ERROR_SUCCESS)
				throw MonoIO.GetException (path, error);
			return attrs;
		}

		public static DateTime GetCreationTime (string path)
		{
			MonoIOStat stat;
			MonoIOError error;
			Path.Validate (path);
			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			if (!MonoIO.GetFileStat (path, out stat, out error)) {
				if (error == MonoIOError.ERROR_PATH_NOT_FOUND || error == MonoIOError.ERROR_FILE_NOT_FOUND)
					return DefaultLocalFileTime;
				else
					throw new IOException (path);
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
			Path.Validate (path);
			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			if (!MonoIO.GetFileStat (path, out stat, out error)) {
				if (error == MonoIOError.ERROR_PATH_NOT_FOUND || error == MonoIOError.ERROR_FILE_NOT_FOUND)
					return DefaultLocalFileTime;
				else
					throw new IOException (path);
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
			Path.Validate (path);
			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			if (!MonoIO.GetFileStat (path, out stat, out error)) {
				if (error == MonoIOError.ERROR_PATH_NOT_FOUND || error == MonoIOError.ERROR_FILE_NOT_FOUND)
					return DefaultLocalFileTime;
				else
					throw new IOException (path);
			}
			return DateTime.FromFileTime (stat.LastWriteTime);
		}

		public static DateTime GetLastWriteTimeUtc (string path)
		{
			return GetLastWriteTime (path).ToUniversalTime ();
		}

		public static void Move (string sourceFileName, string destFileName)
		{
			if (sourceFileName == null)
				throw new ArgumentNullException ("sourceFileName");
			if (destFileName == null)
				throw new ArgumentNullException ("destFileName");
			if (sourceFileName.Length == 0)
				throw new ArgumentException ("An empty file name is not valid.", "sourceFileName");
			if (sourceFileName.Trim ().Length == 0 || sourceFileName.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("The file name is not valid.");
			if (destFileName.Length == 0)
				throw new ArgumentException ("An empty file name is not valid.", "destFileName");
			if (destFileName.Trim ().Length == 0 || destFileName.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("The file name is not valid.");

			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			MonoIOError error;
			if (!MonoIO.Exists (sourceFileName, out error))
				throw new FileNotFoundException (Locale.GetText ("{0} does not exist", sourceFileName), sourceFileName);

			// Don't check for this error here to allow the runtime
			// to check if sourceFileName and destFileName are equal.
			// Comparing sourceFileName and destFileName is not enough.
			//if (MonoIO.Exists (destFileName, out error))
			//	throw new IOException (Locale.GetText ("{0} already exists", destFileName));

			string DirName;
			DirName = Path.GetDirectoryName (destFileName);
			if (DirName != String.Empty && !Directory.Exists (DirName))
				throw new DirectoryNotFoundException (Locale.GetText ("Could not find a part of the path."));

			if (!MonoIO.MoveFile (sourceFileName, destFileName, out error)) {
				if (error == MonoIOError.ERROR_ALREADY_EXISTS)
					throw MonoIO.GetException (error);
				else if (error == MonoIOError.ERROR_SHARING_VIOLATION)
					throw MonoIO.GetException (sourceFileName, error);
				
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

		public static void Replace (string sourceFileName,
					    string destinationFileName,
					    string destinationBackupFileName)
		{
			Replace (sourceFileName, destinationFileName, destinationBackupFileName, false);
		}
		
		public static void Replace (string sourceFileName,
					    string destinationFileName,
					    string destinationBackupFileName,
					    bool ignoreMetadataErrors)
		{
			MonoIOError error;

			if (sourceFileName == null)
				throw new ArgumentNullException ("sourceFileName");
			if (destinationFileName == null)
				throw new ArgumentNullException ("destinationFileName");
			if (sourceFileName.Trim ().Length == 0 || sourceFileName.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("sourceFileName");
			if (destinationFileName.Trim ().Length == 0 || destinationFileName.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("destinationFileName");

			string fullSource = Path.GetFullPath (sourceFileName);
			string fullDest = Path.GetFullPath (destinationFileName);
			if (MonoIO.ExistsDirectory (fullSource, out error))
				throw new IOException (Locale.GetText ("{0} is a directory", sourceFileName));
			if (MonoIO.ExistsDirectory (fullDest, out error))
				throw new IOException (Locale.GetText ("{0} is a directory", destinationFileName));

			if (!Exists (fullSource))
				throw new FileNotFoundException (Locale.GetText ("{0} does not exist", sourceFileName), 
								 sourceFileName);
			if (!Exists (fullDest))
				throw new FileNotFoundException (Locale.GetText ("{0} does not exist", destinationFileName), 
								 destinationFileName);
			if (fullSource == fullDest) 
				throw new IOException (Locale.GetText ("Source and destination arguments are the same file."));

			string fullBackup = null;
			if (destinationBackupFileName != null) {
				if (destinationBackupFileName.Trim ().Length == 0 || 
				    destinationBackupFileName.IndexOfAny (Path.InvalidPathChars) != -1)
					throw new ArgumentException ("destinationBackupFileName");

				fullBackup = Path.GetFullPath (destinationBackupFileName);
				if (MonoIO.ExistsDirectory (fullBackup, out error))
					throw new IOException (Locale.GetText ("{0} is a directory", destinationBackupFileName));
				if (fullSource == fullBackup)
					throw new IOException (Locale.GetText ("Source and backup arguments are the same file."));
				if (fullDest == fullBackup)
					throw new IOException (Locale.GetText (
							       "Destination and backup arguments are the same file."));
			}

			if (!MonoIO.ReplaceFile (fullSource, fullDest, fullBackup, 
						 ignoreMetadataErrors, out error)) {
				throw MonoIO.GetException (error);
			}
		}

#if !NET_2_1
		public static void SetAccessControl (string path,
						     FileSecurity fileSecurity)
		{
			throw new NotImplementedException ();
		}
#endif

		public static void SetAttributes (string path,
						  FileAttributes fileAttributes)
		{
			MonoIOError error;
			Path.Validate (path);

			if (!MonoIO.SetFileAttributes (path, fileAttributes, out error))
				throw MonoIO.GetException (path, error);
		}

		public static void SetCreationTime (string path, DateTime creationTime)
		{
			MonoIOError error;
			Path.Validate (path);
			if (!MonoIO.Exists (path, out error))
				throw MonoIO.GetException (path, error);
			if (!MonoIO.SetCreationTime (path, creationTime, out error))
				throw MonoIO.GetException (path, error);
		}

		public static void SetCreationTimeUtc (string path, DateTime creationTimeUtc)
		{
			SetCreationTime (path, creationTimeUtc.ToLocalTime ());
		}

		public static void SetLastAccessTime (string path, DateTime lastAccessTime)
		{
			MonoIOError error;
			Path.Validate (path);
			if (!MonoIO.Exists (path, out error))
				throw MonoIO.GetException (path, error);
			if (!MonoIO.SetLastAccessTime (path, lastAccessTime, out error))
				throw MonoIO.GetException (path, error);
		}

		public static void SetLastAccessTimeUtc (string path, DateTime lastAccessTimeUtc)
		{
			SetLastAccessTime (path, lastAccessTimeUtc.ToLocalTime ());
		}

		public static void SetLastWriteTime (string path,
						     DateTime lastWriteTime)
		{
			MonoIOError error;
			Path.Validate (path);
			if (!MonoIO.Exists (path, out error))
				throw MonoIO.GetException (path, error);
			if (!MonoIO.SetLastWriteTime (path, lastWriteTime, out error))
				throw MonoIO.GetException (path, error);
		}

		public static void SetLastWriteTimeUtc (string path,
						     DateTime lastWriteTimeUtc)
		{
			SetLastWriteTime (path, lastWriteTimeUtc.ToLocalTime ());
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
			using (FileStream s = OpenRead (path)) {
				long size = s.Length;
				// limited to 2GB according to http://msdn.microsoft.com/en-us/library/system.io.file.readallbytes.aspx
				if (size > Int32.MaxValue)
					throw new IOException ("Reading more than 2GB with this call is not supported");

				int pos = 0;
				int count = (int) size;
				byte [] result = new byte [size];
				while (count > 0) {
					int n = s.Read (result, pos, count);
					if (n == 0)
						throw new IOException ("Unexpected end of stream");
					pos += n;
					count -= n;
				}
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
			using (StreamReader sr = new StreamReader (path)) {
				return sr.ReadToEnd ();
			}
		}

		public static string ReadAllText (string path, Encoding encoding)
		{
			using (StreamReader sr = new StreamReader (path, encoding)) {
				return sr.ReadToEnd ();
			}
		}

		public static void WriteAllBytes (string path, byte [] bytes)
		{
			using (Stream stream = File.Create (path)) {
				stream.Write (bytes, 0, bytes.Length);
			}
		}

		public static void WriteAllLines (string path, string [] contents)
		{
			using (StreamWriter writer = new StreamWriter (path)) {
				WriteAllLines (writer, contents);
			}
		}

		public static void WriteAllLines (string path, string [] contents, Encoding encoding)
		{
			using (StreamWriter writer = new StreamWriter (path, false, encoding)) {
				WriteAllLines (writer, contents);
			}
		}

		static void WriteAllLines (StreamWriter writer, string [] contents)
		{
			foreach (string line in contents)
				writer.WriteLine (line);
		}

		public static void WriteAllText (string path, string contents)
		{
			WriteAllText (path, contents, Encoding.UTF8Unmarked);
		}

		public static void WriteAllText (string path, string contents, Encoding encoding)
		{
			using (StreamWriter sw = new StreamWriter (path, false, encoding)) {
				sw.Write (contents);
			}
		}

		static DateTime? defaultLocalFileTime;
		static DateTime DefaultLocalFileTime {
			get {
				if (defaultLocalFileTime == null)
					defaultLocalFileTime = new DateTime (1601, 1, 1).ToLocalTime ();
					
				return defaultLocalFileTime.Value;
			}
		}


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

#if MOONLIGHT || NET_4_0
		public static IEnumerable<string> ReadLines (string path)
		{
			using (StreamReader reader = File.OpenText (path)) {
				return ReadLines (reader);
			}
		}

		public static IEnumerable<string> ReadLines (string path, Encoding encoding)
		{
			using (StreamReader reader = new StreamReader (path, encoding)) {
				return ReadLines (reader);
			}
		}

		// refactored in order to avoid compiler-generated names for Moonlight tools
		static IEnumerable<string> ReadLines (StreamReader reader)
		{
			string s;
			while ((s = reader.ReadLine ()) != null)
				yield return s;
		}

		public static void AppendAllLines (string path, IEnumerable<string> contents)
		{
			Path.Validate (path);

			if (contents == null)
				return;

			using (TextWriter w = new StreamWriter (path, true)) {
				foreach (var line in contents)
					w.Write (line);
			}
		}

		public static void AppendAllLines (string path, IEnumerable<string> contents, Encoding encoding)
		{
			Path.Validate (path);

			if (contents == null)
				return;

			using (TextWriter w = new StreamWriter (path, true, encoding)) {
				foreach (var line in contents)
					w.Write (line);
			}
		}

		public static void WriteAllLines (string path, IEnumerable<string> contents)
		{
			Path.Validate (path);

			if (contents == null)
				return;

			using (TextWriter w = new StreamWriter (path, false)) {
				foreach (var line in contents)
					w.Write (line);
			}
		}

		public static void WriteAllLines (string path, IEnumerable<string> contents, Encoding encoding)
		{
			Path.Validate (path);

			if (contents == null)
				return;

			using (TextWriter w = new StreamWriter (path, false, encoding)) {
				foreach (var line in contents)
					w.Write (line);
			}
		}
#endif
	}
}
