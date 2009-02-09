//
// Mono.Unix/UnixGroupInfo.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004-2005 Jonathan Pryor
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
using System.Text;
using Mono.Unix;

namespace Mono.Unix {

	public sealed class UnixGroupInfo
	{
		private Native.Group group;

		public UnixGroupInfo (string group)
		{
			this.group = new Native.Group ();
			Native.Group gr;
			int r = Native.Syscall.getgrnam_r (group, this.group, out gr);
			if (r != 0 || gr == null)
				throw new ArgumentException (Locale.GetText ("invalid group name"), "group");
		}

		public UnixGroupInfo (long group)
		{
			this.group = new Native.Group ();
			Native.Group gr;
			int r = Native.Syscall.getgrgid_r (Convert.ToUInt32 (group), this.group, out gr);
			if (r != 0 || gr == null)
				throw new ArgumentException (Locale.GetText ("invalid group id"), "group");
		}

		public UnixGroupInfo (Native.Group group)
		{
			this.group = CopyGroup (group);
		}

		private static Native.Group CopyGroup (Native.Group group)
		{
			Native.Group g = new Native.Group ();

			g.gr_gid    = group.gr_gid;
			g.gr_mem    = group.gr_mem;
			g.gr_name   = group.gr_name;
			g.gr_passwd = group.gr_passwd;

			return g;
		}

		public string GroupName {
			get {return group.gr_name;}
		}

		public string Password {
			get {return group.gr_passwd;}
		}

		public long GroupId {
			get {return group.gr_gid;}
		}

		public UnixUserInfo[] GetMembers ()
		{
			ArrayList members = new ArrayList (group.gr_mem.Length);
			for (int i = 0; i < group.gr_mem.Length; ++i) {
				try {
					members.Add (new UnixUserInfo (group.gr_mem [i]));
				} catch (ArgumentException) {
					// ignore invalid users
				}
			}
			return (UnixUserInfo[]) members.ToArray (typeof (UnixUserInfo));
		}

		public string[] GetMemberNames ()
		{
			return (string[]) group.gr_mem.Clone ();
		}

		public override int GetHashCode ()
		{
			return group.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType())
				return false;
			return group.Equals (((UnixGroupInfo) obj).group);
		}

		public override string ToString ()
		{
			return group.ToString();
		}

		public Native.Group ToGroup ()
		{
			return CopyGroup (group);
		}

		public static UnixGroupInfo[] GetLocalGroups ()
		{
			ArrayList entries = new ArrayList ();
			lock (Native.Syscall.grp_lock) {
				if (Native.Syscall.setgrent () != 0)
					UnixMarshal.ThrowExceptionForLastError ();
				try {
					Native.Group g;
					while ((g = Native.Syscall.getgrent()) != null)
						entries.Add (new UnixGroupInfo (g));
					if (Native.Syscall.GetLastError() != (Native.Errno) 0)
						UnixMarshal.ThrowExceptionForLastError ();
				}
				finally {
					Native.Syscall.endgrent ();
				}
			}
			return (UnixGroupInfo[]) entries.ToArray (typeof(UnixGroupInfo));
		}
	}
}

// vim: noexpandtab
