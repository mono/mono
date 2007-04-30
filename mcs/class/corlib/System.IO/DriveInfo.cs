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

#if NET_2_0
using System;
using System.Collections;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace System.IO {
	[SerializableAttribute] 
	[ComVisibleAttribute(true)] 
	public sealed class DriveInfo : ISerializable {
		_DriveType _drive_type;
		DriveType drive_type;
		string drive_format;
		string path;

		DriveInfo (_DriveType _drive_type, string path, string fstype)
		{
			this._drive_type = _drive_type;
			this.drive_format = fstype;
			this.path = path;

			this.drive_type = ToDriveType (_drive_type, fstype);
		}

		public DriveInfo (string driveName)
		{
			DriveInfo [] drives = GetDrives ();

			foreach (DriveInfo d in drives){
				if (d.path == driveName){
					this.path = d.path;
					this.drive_type = d.drive_type;
					this.drive_format = d.drive_format;
					this.path = d.path;
					this._drive_type = d._drive_type;
					return;
				}
			}
			throw new ArgumentException ("The drive name does not exist", "driveName");
		}
		
		enum _DriveType {
			GenericUnix,
			Linux,
			Windows,
		}
		
		[MonoTODO("Always returns infinite")]
		public long AvailableFreeSpace {
			get {
				if (DriveType == DriveType.CDRom || DriveType == DriveType.Ram || DriveType == DriveType.Unknown)
					return 0;
				return Int64.MaxValue;
			}
		}

		[MonoTODO("Always returns infinite")]
		public long TotalFreeSpace {
			get {
				if (DriveType == DriveType.CDRom || DriveType == DriveType.Ram || DriveType == DriveType.Unknown)
					return 0;
				return Int64.MaxValue;
			}
		}

		[MonoTODO("Always returns infinite")]
		public long TotalSize {
			get {
				return Int64.MaxValue;
			}
		}

		[MonoTODO ("Currently get only works on Mono/Unix; set not implemented")]
		public string VolumeLabel {
			get {
				if (_drive_type != _DriveType.Windows)
					return path;
				else
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

		static DriveType ToDriveType (_DriveType drive_type, string drive_format)
		{
			if (drive_type == _DriveType.Linux){
				switch (drive_format){
				case "tmpfs":
				case "ramfs":
					return DriveType.Ram;
				case "iso9660":
					return DriveType.CDRom;
				case "ext2":
				case "ext3":
				case "sysv":
				case "reiserfs":
				case "ufs":
				case "vfat":
				case "udf":
				case "hfs":
				case "hpfs":
				case "qnx4":
					return DriveType.Fixed;
				case "smbfs":
				case "fuse":
				case "nfs":
				case "nfs4":
				case "cifs":
				case "ncpfs":
				case "coda":
				case "afs":
					return DriveType.Network;
				case "proc":
				case "sysfs":
				case "debugfs":
				case "devpts":
				case "securityfs":
					return DriveType.Ram;
				default:
					return DriveType.Unknown;
				}
			} else {
				return DriveType.Fixed;
			}
		}
		
		public DriveType DriveType {
			get {
				return drive_type;
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
				if (_drive_type != _DriveType.Windows)
					return true;

				// Do something for Windows here.
				return true;
			}
		}
		
		static StreamReader TryOpen (string name)
		{
			if (File.Exists (name))
				return new StreamReader (name, Encoding.ASCII);
			return null;
		}

		static DriveInfo [] LinuxGetDrives ()
		{
			using (StreamReader mounts = TryOpen ("/proc/mounts")){
				ArrayList drives = new ArrayList ();
				string line;
				
				while ((line = mounts.ReadLine ()) != null){
					if (line.StartsWith ("rootfs"))
						continue;
					int p;

					p = line.IndexOf (' ');
					if (p == -1)
						continue;
					string rest = line.Substring (p+1);
					p = rest.IndexOf (' ');
					if (p == -1)
						continue;
					string path = rest.Substring (0, p);
					rest = rest.Substring (p+1);
					p = rest.IndexOf (' ');
					if (p == -1)
						continue;
					string fstype = rest.Substring (0, p);
					drives.Add (new DriveInfo (_DriveType.Linux, path, fstype));
				}

				return (DriveInfo []) drives.ToArray (typeof (DriveInfo));
			}
		}
		
		static DriveInfo [] UnixGetDrives ()
		{
			DriveInfo [] di = null;

			try {
				using (StreamReader linux_ostype = TryOpen ("/proc/sys/kernel/ostype")){
					Console.WriteLine ("here {0}", linux_ostype);
					if (linux_ostype != null){
						string line = linux_ostype.ReadLine ();

						Console.WriteLine ("L: {0}", line);
						if (line == "Linux")
							di = LinuxGetDrives ();
					}
				}
				
				if (di != null)
					return di;
			} catch (Exception e) {
				Console.WriteLine ("Got {0}", e);
				// If anything happens.
			}
			
			DriveInfo [] unknown = new DriveInfo [1];
			unknown [0]= new DriveInfo (_DriveType.GenericUnix, "/", "unixfs");

			return unknown;
		}

		static DriveInfo [] WindowsGetDrives ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO("Currently only implemented on Mono/Linux")]
		public static DriveInfo[] GetDrives ()
		{
			int platform = (int) Environment.Platform;

			if (platform == 4 || platform == 128)
				return UnixGetDrives ();
			else
				return WindowsGetDrives ();
		}

		void ISerializable.GetObjectData (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return(Name);
		}
	}
}

#endif
