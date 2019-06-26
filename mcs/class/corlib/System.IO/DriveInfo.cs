//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.IO {
	[SerializableAttribute] 
	[ComVisibleAttribute(true)] 
	public sealed class DriveInfo : ISerializable {
		string drive_format;
		string path;

		DriveInfo (string path, string fstype)
		{
			this.drive_format = fstype;
			this.path = path;
		}

		public DriveInfo (string driveName)
		{
			if (!Environment.IsUnix) {
				if (driveName == null || driveName.Length == 0)
					throw new ArgumentException ("The drive name is null or empty", "driveName");

				if (driveName.Length >= 2 && driveName [1] != ':')
					throw new ArgumentException ("Invalid drive name", "driveName");

				// Convert the path to a standard format so we can find it later.
				driveName = String.Concat (Char.ToUpperInvariant (driveName [0]).ToString (), ":\\");
			}

			DriveInfo [] drives = GetDrives ();
			Array.Sort (drives, (DriveInfo di1, DriveInfo di2) => String.Compare (di2.path, di1.path, true));
			foreach (DriveInfo d in drives){
				if (driveName.StartsWith (d.path, StringComparison.OrdinalIgnoreCase)){
					this.path = d.path;
					this.drive_format = d.drive_format;
					return;
				}
			}
			throw new ArgumentException ("The drive name does not exist", "driveName");
		}
		
		static void GetDiskFreeSpace (string path, out ulong availableFreeSpace, out ulong totalSize, out ulong totalFreeSpace)
		{
			MonoIOError error;
			if (!GetDiskFreeSpaceInternal (path, out availableFreeSpace, out totalSize, out totalFreeSpace, out error))
				throw MonoIO.GetException (path, error);
		}

		public long AvailableFreeSpace {
			get {
				ulong availableFreeSpace;
				ulong totalSize;
				ulong totalFreeSpace;

				GetDiskFreeSpace (path, out availableFreeSpace, out totalSize, out totalFreeSpace);
				return availableFreeSpace > long.MaxValue ?  long.MaxValue : (long) availableFreeSpace;
			}
		}

		public long TotalFreeSpace {
			get {
				ulong availableFreeSpace;
				ulong totalSize;
				ulong totalFreeSpace;

				GetDiskFreeSpace (path, out availableFreeSpace, out totalSize, out totalFreeSpace);
				return totalFreeSpace > long.MaxValue ?  long.MaxValue : (long) totalFreeSpace;
			}
		}

		public long TotalSize {
			get {
				ulong availableFreeSpace;
				ulong totalSize;
				ulong totalFreeSpace;

				GetDiskFreeSpace (path, out availableFreeSpace, out totalSize, out totalFreeSpace);
				return totalSize > long.MaxValue ?  long.MaxValue : (long) totalSize;
			}
		}

		[MonoTODO ("Currently get only works on Mono/Unix; set not implemented")]
		public string VolumeLabel {
			get {
				return path;
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public string DriveFormat {
			get {
				return drive_format;
			}
		}

		public DriveType DriveType {
			get {
				return (DriveType) GetDriveTypeInternal (path);
			}
		}

		public string Name {
			get {
				return path;
			}
		}

		public DirectoryInfo RootDirectory {
			get {
				return new DirectoryInfo (path);
			}
		}

		public bool IsReady {
			get {
				return Directory.Exists (Name);
			}
		}
		
		[MonoTODO("In windows, alldrives are 'Fixed'")]
		public static DriveInfo[] GetDrives ()
		{
			var drives = Environment.GetLogicalDrives ();
			DriveInfo [] infos = new DriveInfo [drives.Length];
			int i = 0;
			foreach (string s in drives)
				infos [i++] = new DriveInfo (s, GetDriveFormat (s));

			return infos;
		}

		void ISerializable.GetObjectData (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return(Name);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe extern static bool GetDiskFreeSpaceInternal (char *pathName, int pathName_length, out ulong freeBytesAvail,
							     out ulong totalNumberOfBytes, out ulong totalNumberOfFreeBytes,
							     out MonoIOError error);

		unsafe static bool GetDiskFreeSpaceInternal (string pathName, out ulong freeBytesAvail,
						     out ulong totalNumberOfBytes, out ulong totalNumberOfFreeBytes,
						     out MonoIOError error)
		{
			// FIXME Check for embedded nuls here or in native.
			fixed (char *fixed_pathName = pathName) {
				return GetDiskFreeSpaceInternal (fixed_pathName, pathName?.Length ?? 0,
								 out freeBytesAvail, out totalNumberOfBytes, out totalNumberOfFreeBytes,
								 out error);
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe extern static uint GetDriveTypeInternal (char *rootPathName, int rootPathName_length);

		unsafe static uint GetDriveTypeInternal (string rootPathName)
		{
			// FIXME Check for embedded nuls here or in native.
			fixed (char *fixed_rootPathName = rootPathName) {
				return GetDriveTypeInternal (fixed_rootPathName, rootPathName?.Length ?? 0);
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe extern static string GetDriveFormatInternal (char *rootPathName, int rootPathName_length);

		unsafe static string GetDriveFormat (string rootPathName)
		{
			// FIXME Check for embedded nuls here or in native.
			fixed (char *fixed_rootPathName = rootPathName) {
				return GetDriveFormatInternal (fixed_rootPathName, rootPathName?.Length ?? 0);
			}
		}
	}
}
