//
// Mono.Posix/PosixEnvironment.cs
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

	public sealed /* static */ class PosixEnvironment
	{
		private PosixEnvironment () {}

		public static string CurrentDirectory {
			get {
				return PosixDirectory.GetCurrentDirectory ();
			}
			set {
				PosixDirectory.SetCurrentDirectory (value);
			}
		}

		public static string MachineName {
			get {
				StringBuilder buf = new StringBuilder (8);
				int r = 0;
				Error e;
				do {
					buf.Capacity *= 2;
					r = Syscall.gethostname (buf);
				} while (r == (-1) && ((e = Syscall.GetLastError()) == Error.EINVAL) || 
						(e == Error.ENAMETOOLONG));
				if (r == (-1))
					PosixMarshal.ThrowExceptionForLastError ();
				return buf.ToString ();
			}
			set {
				Syscall.sethostname (value);
			}
		}

		public static long GetConfigurationValue (SysConf name)
		{
			long r = Syscall.sysconf (name);
			if (r == -1 && Syscall.GetLastError() == Error.EINVAL)
				PosixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		public static string GetConfigurationString (ConfStr name)
		{
			ulong len = Syscall.confstr (name, null, 0);
			if (len == 0)
				return "";
			StringBuilder buf = new StringBuilder ((int) len+1);
			len = Syscall.confstr (name, buf, len);
			return buf.ToString ();
		}

		public static void SetNiceValue (int inc)
		{
			int r = Syscall.nice (inc);
			PosixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static string UserName {
			get {return PosixUser.GetCurrentUserName();}
		}

		public static int CreateSession ()
		{
			return Syscall.setsid ();
		}

		public static void SetProcessGroup ()
		{
			int r = Syscall.setpgrp ();
			PosixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static int GetProcessGroup ()
		{
			return Syscall.getpgrp ();
		}

		public static uint[] GetSupplementaryGroups ()
		{
			int ngroups = Syscall.getgroups (0, new uint[]{});
			if (ngroups == -1)
				PosixMarshal.ThrowExceptionForLastError ();
			uint[] groups = new uint[ngroups];
			int r = Syscall.getgroups (groups);
			PosixMarshal.ThrowExceptionForLastErrorIf (r);
			return groups;
		}

		public static void SetSupplementaryGroups (uint[] list)
		{
			int r = Syscall.setgroups (list);
			PosixMarshal.ThrowExceptionForLastErrorIf (r);
		}
	}
}

// vim: noexpandtab
