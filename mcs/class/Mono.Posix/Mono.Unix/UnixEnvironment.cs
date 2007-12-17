//
// Mono.Unix/UnixEnvironment.cs
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
using Mono.Unix;

namespace Mono.Unix {

	public sealed /* static */ class UnixEnvironment
	{
		private UnixEnvironment () {}

		public static string CurrentDirectory {
			get {
				return UnixDirectoryInfo.GetCurrentDirectory ();
			}
			set {
				UnixDirectoryInfo.SetCurrentDirectory (value);
			}
		}

		public static string MachineName {
			get {
				Native.Utsname buf;
				if (Native.Syscall.uname (out buf) != 0)
					throw UnixMarshal.CreateExceptionForLastError ();
				return buf.nodename;
			}
			set {
				int r = Native.Syscall.sethostname (value);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);
			}
		}

		public static string UserName {
			get {return UnixUserInfo.GetRealUser ().UserName;}
		}

		public static UnixGroupInfo RealGroup {
			get {return new UnixGroupInfo (RealGroupId);}
			// set can't be done as setgid(2) modifies effective gid as well
		}

		public static long RealGroupId {
			get {return Native.Syscall.getgid ();}
			// set can't be done as setgid(2) modifies effective gid as well
		}

		public static UnixUserInfo RealUser {
			get {return new UnixUserInfo (RealUserId);}
			// set can't be done as setuid(2) modifies effective uid as well
		}

		public static long RealUserId {
			get {return Native.Syscall.getuid ();}
			// set can't be done as setuid(2) modifies effective uid as well
		}

		public static UnixGroupInfo EffectiveGroup {
			get {return new UnixGroupInfo (EffectiveGroupId);}
			set {EffectiveGroupId = value.GroupId;}
		}

		public static long EffectiveGroupId {
			get {return Native.Syscall.getegid ();}
			set {Native.Syscall.setegid (Convert.ToUInt32 (value));}
		}

		public static UnixUserInfo EffectiveUser {
			get {return new UnixUserInfo (EffectiveUserId);}
			set {EffectiveUserId = value.UserId;}
		}

		public static long EffectiveUserId {
			get {return Native.Syscall.geteuid ();}
			set {Native.Syscall.seteuid (Convert.ToUInt32 (value));}
		}

		public static string Login {
			get {return UnixUserInfo.GetRealUser ().UserName;}
		}

		[CLSCompliant (false)]
		public static long GetConfigurationValue (Native.SysconfName name)
		{
			long r = Native.Syscall.sysconf (name);
			if (r == -1 && Native.Stdlib.GetLastError() != (Native.Errno) 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		[CLSCompliant (false)]
		public static string GetConfigurationString (Native.ConfstrName name)
		{
			ulong len = Native.Syscall.confstr (name, null, 0);
			if (len == unchecked ((ulong) (-1)))
				UnixMarshal.ThrowExceptionForLastError ();
			if (len == 0)
				return "";
			StringBuilder buf = new StringBuilder ((int) len+1);
			len = Native.Syscall.confstr (name, buf, len);
			if (len == unchecked ((ulong) (-1)))
				UnixMarshal.ThrowExceptionForLastError ();
			return buf.ToString ();
		}

		public static void SetNiceValue (int inc)
		{
			int r = Native.Syscall.nice (inc);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static int CreateSession ()
		{
			int s = Native.Syscall.setsid ();
			UnixMarshal.ThrowExceptionForLastErrorIf (s);
			return s;
		}

		public static void SetProcessGroup ()
		{
			int r = Native.Syscall.setpgrp ();
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static int GetProcessGroup ()
		{
			return Native.Syscall.getpgrp ();
		}

		public static UnixGroupInfo[] GetSupplementaryGroups ()
		{
			uint[] ids = _GetSupplementaryGroupIds ();
			UnixGroupInfo[] groups = new UnixGroupInfo [ids.Length];
			for (int i = 0; i < groups.Length; ++i)
				groups [i] = new UnixGroupInfo (ids [i]);
			return groups;
		}

		private static uint[] _GetSupplementaryGroupIds ()
		{
			int ngroups = Native.Syscall.getgroups (0, new uint[]{});
			if (ngroups == -1)
				UnixMarshal.ThrowExceptionForLastError ();
			uint[] groups = new uint[ngroups];
			int r = Native.Syscall.getgroups (groups);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return groups;
		}

		public static void SetSupplementaryGroups (UnixGroupInfo[] groups)
		{
			uint[] list = new uint [groups.Length];
			for (int i = 0; i < list.Length; ++i) {
				list [i] = Convert.ToUInt32 (groups [i].GroupId);
			}
			int r = Native.Syscall.setgroups (list);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static long[] GetSupplementaryGroupIds ()
		{
			uint[] _groups = _GetSupplementaryGroupIds ();
			long[] groups = new long [_groups.Length];
			for (int i = 0; i < groups.Length; ++i)
				groups [i] = _groups [i];
			return groups;
		}

		public static void SetSupplementaryGroupIds (long[] list)
		{
			uint[] _list = new uint [list.Length];
			for (int i = 0; i < _list.Length; ++i)
				_list [i] = Convert.ToUInt32 (list [i]);
			int r = Native.Syscall.setgroups (_list);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static int GetParentProcessId ()
		{
			return Native.Syscall.getppid ();
		}
		
		public static UnixProcess GetParentProcess ()
		{
			return new UnixProcess (GetParentProcessId ());
		}

		public static string[] GetUserShells ()
		{
			ArrayList shells = new ArrayList ();

			lock (Native.Syscall.usershell_lock) {
				try {
					if (Native.Syscall.setusershell () != 0)
						UnixMarshal.ThrowExceptionForLastError ();
					string shell;
					while ((shell = Native.Syscall.getusershell ()) != null)
						shells.Add (shell);
				}
				finally {
					Native.Syscall.endusershell ();
				}
			}

			return (string[]) shells.ToArray (typeof(string));
		}
	}
}

// vim: noexpandtab
