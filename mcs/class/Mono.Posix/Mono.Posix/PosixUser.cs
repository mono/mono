//
// Mono.Posix/PosixUser.cs
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
using System.Text;
using Mono.Posix;

namespace Mono.Posix {

	public sealed class PosixUser
	{
		private PosixUser () {}

		public static uint GetUserId (string user)
		{
			return new PosixUserInfo (user).UserId;
		}

		public static uint GetCurrentUser ()
		{
			return Syscall.getuid ();
		}

		public static string GetCurrentUserName ()
		{
			return GetName (GetCurrentUser());
		}

		// I would hope that this is the same as GetCurrentUserName, but it is a
		// different syscall, so who knows.
		public static string GetLogin ()
		{
			StringBuilder buf = new StringBuilder (4);
			int r;
			do {
				buf.Capacity *= 2;
				r = Syscall.getlogin_r (buf, (ulong) buf.Capacity);
			} while (r == (-1) && Syscall.GetLastError() == Error.ERANGE);
			PosixMarshal.ThrowExceptionForLastErrorIf (r);
			return buf.ToString ();
		}

		public static uint GetGroupId (string user)
		{
			return new PosixUserInfo (user).GroupId;
		}

		public static uint GetGroupId (uint user)
		{
			return new PosixUserInfo (user).GroupId;
		}

		public static string GetRealName (string user)
		{
			return new PosixUserInfo (user).RealName;
		}

		public static string GetRealName (uint user)
		{
			return new PosixUserInfo (user).RealName;
		}

		public static string GetHomeDirectory (string user)
		{
			return new PosixUserInfo (user).HomeDirectory;
		}

		public static string GetHomeDirectory (uint user)
		{
			return new PosixUserInfo (user).HomeDirectory;
		}

		public static string GetName (uint user)
		{
			return new PosixUserInfo (user).UserName;
		}

		public static string GetPassword (string user)
		{
			return new PosixUserInfo (user).Password;
		}

		public static string GetPassword (uint user)
		{
			return new PosixUserInfo (user).Password;
		}

		public static string GetShellProgram (string user)
		{
			return new PosixUserInfo (user).ShellProgram;
		}

		public static string GetShellProgram (uint user)
		{
			return new PosixUserInfo (user).ShellProgram;
		}

		public static PosixUserInfo[] GetLocalUsers ()
		{
			Syscall.SetLastError ((Error) 0);
			Syscall.setpwent ();
			if (Syscall.GetLastError () != (Error) 0) {
				PosixMarshal.ThrowExceptionForLastError ();
			}
			ArrayList entries = new ArrayList ();
			try {
				Passwd p;
				while ((p = Syscall.getpwent()) != null)
					entries.Add (new PosixUserInfo (p));
				if (Syscall.GetLastError () != (Error) 0)
					PosixMarshal.ThrowExceptionForLastError ();
			}
			finally {
				Syscall.endpwent ();
			}
			return (PosixUserInfo[]) entries.ToArray (typeof(PosixUserInfo));
		}
	}
}

// vim: noexpandtab
