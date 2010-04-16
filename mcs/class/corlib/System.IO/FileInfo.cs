//------------------------------------------------------------------------------
// 
// System.IO.FileInfo.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
//                 Dan Lewis (dihlewis@yahoo.co.uk)
// Created:        Monday, August 13, 2001 
//
//------------------------------------------------------------------------------

//
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

using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

#if !NET_2_1
using System.Security.AccessControl;
#endif

namespace System.IO {

	[Serializable]
	[ComVisible (true)]
	public sealed class FileInfo : FileSystemInfo
	{
		private bool exists;

#if MOONLIGHT
		internal FileInfo ()
		{
		}
#endif
		public FileInfo (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			CheckPath (fileName);
			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			OriginalPath = fileName;
			FullPath = Path.GetFullPath (fileName);
		}

		private FileInfo (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		internal override void InternalRefresh ()
		{
			exists = File.Exists (FullPath);
		}

		// public properties

		public override bool Exists {
			get {
				Refresh (false);

				if (stat.Attributes == MonoIO.InvalidFileAttributes)
					return false;

				if ((stat.Attributes & FileAttributes.Directory) != 0)
					return false;

				return exists;
			}
		}

		public override string Name {
			get {
				return Path.GetFileName (FullPath);
			}
		}

#if !NET_2_1
		public bool IsReadOnly {
			get {
				if (!Exists)
					throw new FileNotFoundException ("Could not find file \"" + OriginalPath + "\".", OriginalPath);
					
				return ((stat.Attributes & FileAttributes.ReadOnly) != 0);
			}
				
			set {
				if (!Exists)
					throw new FileNotFoundException ("Could not find file \"" + OriginalPath + "\".", OriginalPath);
					
				FileAttributes attrs = File.GetAttributes(FullPath);

				if (value) 
					attrs |= FileAttributes.ReadOnly;
				else
					attrs &= ~FileAttributes.ReadOnly;

				File.SetAttributes(FullPath, attrs);
			}
		}

		[MonoLimitation ("File encryption isn't supported (even on NTFS).")]
		[ComVisible (false)]
		public void Encrypt ()
		{
			// MS.NET support this only on NTFS file systems, i.e. it's a file-system (not a framework) feature.
			// otherwise it throws a NotSupportedException (or a PlatformNotSupportedException on older OS).
			// we throw the same (instead of a NotImplementedException) because most code should already be
			// handling this exception to work properly.
			throw new NotSupportedException (Locale.GetText ("File encryption isn't supported on any file system."));
		}

		[MonoLimitation ("File encryption isn't supported (even on NTFS).")]
		[ComVisible (false)]
		public void Decrypt ()
		{
			// MS.NET support this only on NTFS file systems, i.e. it's a file-system (not a framework) feature.
			// otherwise it throws a NotSupportedException (or a PlatformNotSupportedException on older OS).
			// we throw the same (instead of a NotImplementedException) because most code should already be
			// handling this exception to work properly.
			throw new NotSupportedException (Locale.GetText ("File encryption isn't supported on any file system."));
		}
#endif

		public long Length {
			get {
				if (!Exists)
					throw new FileNotFoundException ("Could not find file \"" + OriginalPath + "\".", OriginalPath);

				return stat.Length;
			}
		}

		public string DirectoryName {
			get {
				return Path.GetDirectoryName (FullPath);
			}
		}

		public DirectoryInfo Directory {
			get {
				return new DirectoryInfo (DirectoryName);
			}
		}

		// streamreader methods

		public StreamReader OpenText ()
		{
			return new StreamReader (Open (FileMode.Open, FileAccess.Read));
		}

		public StreamWriter CreateText ()
		{
			return new StreamWriter (Open (FileMode.Create, FileAccess.Write));
		}
		
		public StreamWriter AppendText ()
		{
			return new StreamWriter (Open (FileMode.Append, FileAccess.Write));
		}

		// filestream methods

		public FileStream Create ()
		{
			return File.Create (FullPath);
		}

		public FileStream OpenRead ()
		{
			return Open (FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public FileStream OpenWrite ()
		{
			return Open (FileMode.OpenOrCreate, FileAccess.Write);
		}

		public FileStream Open (FileMode mode)
		{
			return Open (mode, FileAccess.ReadWrite);
		}

		public FileStream Open (FileMode mode, FileAccess access)
		{
			return Open (mode, access, FileShare.None);
		}

		public FileStream Open (FileMode mode, FileAccess access, FileShare share)
		{
			return new FileStream (FullPath, mode, access, share);
		}

		// file methods

		public override void Delete ()
		{
			MonoIOError error;

			if (!MonoIO.Exists (FullPath, out error))
				// a weird MS.NET behaviour
				return;

			if (MonoIO.ExistsDirectory (FullPath, out error))
				throw new UnauthorizedAccessException ("Access to the path \"" + FullPath + "\" is denied.");
			if (!MonoIO.DeleteFile (FullPath, out error))
				throw MonoIO.GetException (OriginalPath, error);
		}

		public void MoveTo (string destFileName)
		{
			if (destFileName == null)
				throw new ArgumentNullException ("destFileName");
			if (destFileName == Name || destFileName == FullName)
				return;
			if (!File.Exists (FullPath))
				throw new FileNotFoundException ();

			File.Move (FullPath, destFileName);
			this.FullPath = Path.GetFullPath (destFileName);
		}

		public FileInfo CopyTo (string destFileName)
		{
			return CopyTo (destFileName, false);
		}

		public FileInfo CopyTo (string destFileName, bool overwrite)
		{
			if (destFileName == null)
				throw new ArgumentNullException ("destFileName");
			if (destFileName.Length == 0)
				throw new ArgumentException ("An empty file name is not valid.", "destFileName");

			string dest = Path.GetFullPath (destFileName);

			if (overwrite && File.Exists (dest))
				File.Delete (dest);

			File.Copy (FullPath, dest);
		
			return new FileInfo (dest);
		}

		public override string ToString ()
		{
#if NET_2_1
			// for Moonlight we *never* return paths, since ToString is not [SecurityCritical] we simply return the Name
			return Name;
#else
			return OriginalPath;
#endif
		}

#if !NET_2_1
		public FileSecurity GetAccessControl ()
		{
			throw new NotImplementedException ();
		}
		
		public FileSecurity GetAccessControl (AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}

		[ComVisible (false)]
		public FileInfo Replace (string destinationFileName,
					 string destinationBackupFileName)
		{
			string destinationFullPath = null;
			if (!Exists)
                		throw new FileNotFoundException ();
			if (destinationFileName == null)
                		throw new ArgumentNullException ("destinationFileName");
            		if (destinationFileName.Length == 0)
                		throw new ArgumentException ("An empty file name is not valid.", "destinationFileName");

			destinationFullPath = Path.GetFullPath (destinationFileName);

			if (!File.Exists (destinationFullPath))
				throw new FileNotFoundException ();

			FileAttributes attrs = File.GetAttributes (destinationFullPath);

			if ( (attrs & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
					throw new UnauthorizedAccessException (); 

            		if (destinationBackupFileName != null) {
                		if (destinationBackupFileName.Length == 0)
                    			throw new ArgumentException ("An empty file name is not valid.", "destinationBackupFileName");
                		File.Copy (destinationFullPath, Path.GetFullPath (destinationBackupFileName), true);
            		}
            		File.Copy (FullPath, destinationFullPath,true);
            		File.Delete (FullPath);
			return new FileInfo (destinationFullPath);
		}
		
		[ComVisible (false)]
		[MonoLimitation ("We ignore the ignoreMetadataErrors parameter")]
		public FileInfo Replace (string destinationFileName,
					 string destinationBackupFileName,
					 bool ignoreMetadataErrors)
		{
			return Replace (destinationFileName, destinationBackupFileName);
		}

		public void SetAccessControl (FileSecurity fileSecurity)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
