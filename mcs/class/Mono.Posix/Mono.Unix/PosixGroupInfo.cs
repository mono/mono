//
// Mono.Posix/PosixGroupInfo.cs
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
using System.Text;
using Mono.Posix;

namespace Mono.Posix {

	public sealed class PosixGroupInfo
	{
		private Group group;

		public PosixGroupInfo (string group)
		{
			this.group = new Group ();
			Group gr;
			int r = Syscall.getgrnam_r (group, this.group, out gr);
			if (r != 0 || gr == null)
				throw new ArgumentException (Locale.GetText ("invalid group name"), "group");
		}

		public PosixGroupInfo (uint group)
		{
			this.group = new Group ();
			Group gr;
			int r = Syscall.getgrgid_r (group, this.group, out gr);
			if (r != 0 || gr == null)
				throw new ArgumentException (Locale.GetText ("invalid group id"), "group");
		}

		public PosixGroupInfo (Group group)
		{
			this.group = group;
		}

		public string GroupName {
			get {return group.gr_name;}
		}

		public string Password {
			get {return group.gr_passwd;}
		}

		public uint GroupId {
			get {return group.gr_gid;}
		}

		public string[] Members {
			get {return group.gr_mem;}
		}

		public override int GetHashCode ()
		{
			return group.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType())
				return false;
			return group.Equals (((PosixGroupInfo) obj).group);
		}

		public override string ToString ()
		{
			return group.ToString();
		}
	}
}

// vim: noexpandtab
