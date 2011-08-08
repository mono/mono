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
				driveName = String.Concat (Char.ToUpper (driveName [0]).ToString (), ":\\");
			}

			DriveInfo [] drives = GetDrives ();
			foreach (DriveInfo d in drives){
				if (d.path == driveName){
					this.path = d.path;
					this.drive_format = d.drive_format;
					this.path = d.path;
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

		[MonoTODO("It always returns true")]
		public bool IsReady {
			get {
				return true;
			}
		}
		
		static StreamReader TryOpen (string name)
		{
			if (File.Exists (name))
				return new StreamReader (name);
			return null;
		}

		static char [] space = { ' ' };
		static DriveInfo [] LinuxGetDrives ()
		{
			using (StreamReader mounts = TryOpen ("/proc/mounts")){
				var drives = new List<DriveInfo> ();
				string line;
				
				while ((line = mounts.ReadLine ()) != null){
					if (line.StartsWith ("rootfs"))
						continue;
					string [] parts = line.Split (space, 4);
					if (parts.Length < 3)
						continue;
					string path = Unescape (parts [1]);
					string fstype = parts [2];
					drives.Add (new DriveInfo (path, fstype));
				}

				return drives.ToArray ();
			}
		}

		static string Unescape (string path)
		{
			StringBuilder sb = null;
			int start = 0;
			do {
				int slash = path.IndexOf ('\\', start);
				if (slash >= 0) {
					if (sb == null)
						sb = new StringBuilder ();
					sb.Append (path.Substring (start, slash - start));
					char c = (char) ((path [slash + 1] - '0') << 6);
					c += (char) ((path [slash + 2] - '0') << 3);
					c += (char) (path [slash + 3] - '0');
					sb.Append (c);
					start = slash + 4;
					continue;
				}
				if (start == 0)
					return path;
				sb.Append (path.Substring (start));
				return sb.ToString ();
			} while (true);
		}
		
		static DriveInfo [] UnixGetDrives ()
		{
			DriveInfo [] di = null;

			try {
				using (StreamReader linux_ostype = TryOpen ("/proc/sys/kernel/ostype")){
					if (linux_ostype != null){
						string line = linux_ostype.ReadLine ();
						if (line == "Linux")
							di = LinuxGetDrives ();
					}
				}
				
				if (di != null)
					return di;
			} catch (Exception) {
				// If anything happens.
			}
			
			DriveInfo [] unknown = new DriveInfo [1];
			unknown [0]= new DriveInfo ("/", "unixfs");

			return unknown;
		}

		static DriveInfo [] RuntimeGetDrives ()
		{
			var drives = Environment.GetLogicalDrives ();
			DriveInfo [] infos = new DriveInfo [drives.Length];
			int i = 0;
			foreach (string s in drives) {
				infos [i++] = new DriveInfo (s, GetDriveFormat (s));
			}

			return infos;
		}
		
		[MonoTODO("In windows, alldrives are 'Fixed'")]
		public static DriveInfo[] GetDrives ()
		{
			if (!Environment.IsUnix || Environment.IsMacOS)
				return RuntimeGetDrives ();
			
			return UnixGetDrives ();
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
		extern static bool GetDiskFreeSpaceInternal (string pathName, out ulong freeBytesAvail,
							     out ulong totalNumberOfBytes, out ulong totalNumberOfFreeBytes,
							     out MonoIOError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static uint GetDriveTypeInternal (string rootPathName);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static string GetDriveFormat (string rootPathName);
	}
}
