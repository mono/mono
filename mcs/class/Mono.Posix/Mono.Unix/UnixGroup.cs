//
// Mono.Unix/UnixGroup.cs
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
using Mono.Unix;

namespace Mono.Unix {

	public sealed class UnixGroup
	{
		private UnixGroup () {}

		[CLSCompliant (false)]
		[Obsolete ("The return type will change in the next release")]
		public static uint GetGroupId (string group)
		{
			return new UnixGroupInfo (group).GroupId;
		}

		public static string[] GetMembers (string group)
		{
			return new UnixGroupInfo (group).Members;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetMembers (long)")]
		public static string[] GetMembers (uint group)
		{
			return new UnixGroupInfo (group).Members;
		}

		public static string[] GetMembers (long group)
		{
			return new UnixGroupInfo (group).GetMembers ();
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetGroupName (long)")]
		public static string GetName (uint group)
		{
			return new UnixGroupInfo (group).GroupName;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetGroupName(long)")]
		public static string GetGroupName (uint group)
		{
			return new UnixGroupInfo (group).GroupName;
		}

		public static string GetGroupName (long group)
		{
			return new UnixGroupInfo (group).GroupName;
		}

		public static string GetPassword (string group)
		{
			return new UnixGroupInfo (group).Password;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetPassword(long)")]
		public static string GetPassword (uint group)
		{
			return new UnixGroupInfo (group).Password;
		}

		public static string GetPassword (long group)
		{
			return new UnixGroupInfo (group).Password;
		}

		public static UnixGroupInfo[] GetLocalGroups ()
		{
			ArrayList entries = new ArrayList ();
			Syscall.SetLastError ((Error) 0);
			lock (Syscall.grp_lock) {
				Syscall.setgrent ();
				if (Syscall.GetLastError () != (Error) 0)
					UnixMarshal.ThrowExceptionForLastError ();
				try {
					Group g;
					while ((g = Syscall.getgrent()) != null)
						entries.Add (new UnixGroupInfo (g));
					if (Syscall.GetLastError() != (Error) 0)
						UnixMarshal.ThrowExceptionForLastError ();
				}
				finally {
					Syscall.endgrent ();
				}
			}
			return (UnixGroupInfo[]) entries.ToArray (typeof(UnixGroupInfo));
		}
	}
}

// vim: noexpandtab
