//
// Mono.Posix/PosixGroup.cs
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
using Mono.Posix;

namespace Mono.Posix {

	public sealed class PosixGroup
	{
		private PosixGroup () {}

		public static uint GetGroupId (string group)
		{
			return new PosixGroupInfo (group).GroupId;
		}

		public static string[] GetMembers (string group)
		{
			return new PosixGroupInfo (group).Members;
		}

		public static string[] GetMembers (uint group)
		{
			return new PosixGroupInfo (group).Members;
		}

		public static string GetName (uint group)
		{
			return new PosixGroupInfo (group).GroupName;
		}

		public static string GetPassword (string group)
		{
			return new PosixGroupInfo (group).Password;
		}

		public static string GetPassword (uint group)
		{
			return new PosixGroupInfo (group).Password;
		}

		public static PosixGroupInfo[] GetLocalGroups ()
		{
			Syscall.SetLastError ((Error) 0);
			Syscall.setgrent ();
			if (Syscall.GetLastError () != (Error) 0)
				PosixMarshal.ThrowExceptionForLastError ();
			ArrayList entries = new ArrayList ();
			try {
				Group g;
				while ((g = Syscall.getgrent()) != null)
					entries.Add (new PosixGroupInfo (g));
				if (Syscall.GetLastError() != (Error) 0)
					PosixMarshal.ThrowExceptionForLastError ();
			}
			finally {
				Syscall.endgrent ();
			}
			return (PosixGroupInfo[]) entries.ToArray (typeof(PosixGroupInfo));
		}
	}
}

// vim: noexpandtab
