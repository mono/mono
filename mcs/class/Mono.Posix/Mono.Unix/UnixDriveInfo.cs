//
// Mono.Unix/UnixDriveInfo.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004 Jonathan Pryor
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
using System.Collections;
using System.IO;
using Mono.Unix;

namespace Mono.Unix {

	public enum UnixDriveType {
		Unknown,
		NoRootDirectory,
		Removable,
		Fixed,
		Network,
		CDRom,
		Ram
	}

	// All methods & properties can throw IOException
	public sealed class UnixDriveInfo
	{
		private Native.Statvfs stat;
		private Native.Fstab   fstab;

		public UnixDriveInfo (string mountPoint)
		{
			if (mountPoint == null)
				throw new ArgumentNullException ("mountPoint");
			fstab = Native.Syscall.getfsfile (mountPoint);
			if (fstab == null)
				throw new ArgumentException ("mountPoint isn't valid: " + mountPoint);
			// throws ArgumentException if driveName isn't valid
			// though .NET also has a DriveNotFoundException class, so maybe that's
			// more appropriate?
		}

		public static UnixDriveInfo GetForSpecialFile (string specialFile)
		{
			if (specialFile == null)
				throw new ArgumentNullException ("specialFile");
			Native.Fstab f = Native.Syscall.getfsspec (specialFile);
			if (f == null)
				throw new ArgumentException ("specialFile isn't valid: " + specialFile);
			return new UnixDriveInfo (f);
		}

		private UnixDriveInfo (Native.Fstab fstab)
		{
			this.fstab = fstab;
		}

		public long AvailableFreeSpace {
			get {Refresh (); return Convert.ToInt64 (stat.f_bavail * stat.f_bsize);}
		}

		public string DriveFormat {
			get {return fstab.fs_vfstype;}
		}

		public UnixDriveType DriveType {
			get {return UnixDriveType.Unknown;}
		}

		// this throws no exceptions
		public bool IsReady {
			get {return Refresh (false);}
		}

		public string Name {
			get {return fstab.fs_file;}
		}

		public UnixDirectoryInfo RootDirectory {
			get {return new UnixDirectoryInfo (fstab.fs_file);}
		}

		public long TotalFreeSpace {
			get {Refresh (); return (long) (stat.f_bfree * stat.f_bsize);}
		}

		public long TotalSize {
			get {Refresh (); return (long) (stat.f_frsize * stat.f_blocks);}
		}

		// also throws SecurityException if caller lacks perms
		public string VolumeLabel {
			get {return fstab.fs_spec;}
			// set {}
		}

		public long MaximumFilenameLength {
			get {Refresh (); return Convert.ToInt64 (stat.f_namemax);}
		}

		public static UnixDriveInfo[] GetDrives ()
		{
			// throws IOException, UnauthorizedAccessException (no permission)
			ArrayList entries = new ArrayList ();

			lock (Native.Syscall.fstab_lock) {
				int r = Native.Syscall.setfsent ();
				if (r != 1)
					throw new IOException ("Error calling setfsent(3)", new UnixIOException ());
				try {
					Native.Fstab fs;
					while ((fs = Native.Syscall.getfsent()) != null) {
						// avoid virtual entries, such as "swap"
						if (fs.fs_file.StartsWith ("/"))
							entries.Add (new UnixDriveInfo (fs));
					}
				}
				finally {
					Native.Syscall.endfsent ();
				}
			}
			return (UnixDriveInfo[]) entries.ToArray (typeof(UnixDriveInfo));
		}

		public override string ToString ()
		{
			return VolumeLabel;
		}

		private void Refresh ()
		{
			Refresh (true);
		}

		private bool Refresh (bool throwException)
		{
			int r = Native.Syscall.statvfs (fstab.fs_file, out stat);
			if (r == -1 && throwException) {
				Native.Errno e = Native.Syscall.GetLastError ();
				throw new InvalidOperationException (
						UnixMarshal.GetErrorDescription (e),
						new UnixIOException (e));
			}
			else if (r == -1)
				return false;
			return true;
		}
	}
}

// vim: noexpandtab
