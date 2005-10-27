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
				return UnixDirectory.GetCurrentDirectory ();
			}
			set {
				UnixDirectory.SetCurrentDirectory (value);
			}
		}

		public static string MachineName {
			get {
				StringBuilder buf = new StringBuilder (8);
				int r = 0;
				Error e = (Error) 0;
				do {
					buf.Capacity *= 2;
					r = Syscall.gethostname (buf);
				} while (r == (-1) && ((e = Syscall.GetLastError()) == Error.EINVAL) || 
						(e == Error.ENAMETOOLONG));
				if (r == (-1))
					UnixMarshal.ThrowExceptionForLastError ();
				return buf.ToString ();
			}
			set {
				Syscall.sethostname (value);
			}
		}

		[CLSCompliant (false)]
		[Obsolete ("Use RealUserId")]
		public static uint User {
			get {return UnixUser.GetCurrentUserId ();}
		}

		[CLSCompliant (false)]
		[Obsolete ("Use RealUserId")]
		public static uint UserId {
			get {return UnixUser.GetCurrentUserId ();}
		}

		public static string UserName {
			get {return UnixUser.GetCurrentUserName();}
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
			get {return UnixUser.GetLogin ();}
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetConfigurationValue (Mono.Unix.Native.SysconfName)")]
		public static long GetConfigurationValue (SysConf name)
		{
			long r = Syscall.sysconf (name);
			if (r == -1 && Syscall.GetLastError() == Error.EINVAL)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		[CLSCompliant (false)]
		public static long GetConfigurationValue (Native.SysconfName name)
		{
			long r = Native.Syscall.sysconf (name);
			if (r == -1 && Native.Stdlib.GetLastError() == Native.Errno.EINVAL)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetConfigurationString (Mono.Unix.Native.ConfstrName)")]
		public static string GetConfigurationString (ConfStr name)
		{
			ulong len = Syscall.confstr (name, null, 0);
			if (len == 0)
				return "";
			StringBuilder buf = new StringBuilder ((int) len+1);
			len = Syscall.confstr (name, buf, len);
			return buf.ToString ();
		}

		[CLSCompliant (false)]
		public static string GetConfigurationString (Native.ConfstrName name)
		{
			ulong len = Native.Syscall.confstr (name, null, 0);
			if (len == 0)
				return "";
			StringBuilder buf = new StringBuilder ((int) len+1);
			len = Native.Syscall.confstr (name, buf, len);
			return buf.ToString ();
		}

		public static void SetNiceValue (int inc)
		{
			int r = Syscall.nice (inc);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static int CreateSession ()
		{
			return Syscall.setsid ();
		}

		public static void SetProcessGroup ()
		{
			int r = Syscall.setpgrp ();
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static int GetProcessGroup ()
		{
			return Syscall.getpgrp ();
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetSupplementaryGroupIds")]
		public static uint[] GetSupplementaryGroups ()
		{
			return GetSupplementaryGroupIds ();
		}

		[CLSCompliant (false)]
		[Obsolete ("The return type of this method will change in the next release")]
		public static uint[] GetSupplementaryGroupIds ()
		{
			int ngroups = Syscall.getgroups (0, new uint[]{});
			if (ngroups == -1)
				UnixMarshal.ThrowExceptionForLastError ();
			uint[] groups = new uint[ngroups];
			int r = Syscall.getgroups (groups);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return groups;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use SetSupplementaryGroupIds(long[])")]
		public static void SetSupplementaryGroups (uint[] list)
		{
			SetSupplementaryGroupIds (list);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use SetSupplementaryGroupIds(long[])")]
		public static void SetSupplementaryGroupIds (uint[] list)
		{
			int r = Syscall.setgroups (list);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static void SetSupplementaryGroupIds (long[] list)
		{
			uint[] _list = new uint [list.Length];
			for (int i = 0; i < _list.Length; ++i)
				_list [i] = Convert.ToUInt32 (list [i]);
			int r = Syscall.setgroups (_list);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static int GetParentProcessId ()
		{
			return Syscall.getppid ();
		}
		
		public static UnixProcess GetParentProcess ()
		{
			return new UnixProcess (GetParentProcessId ());
		}

		public static string[] GetUserShells ()
		{
			ArrayList shells = new ArrayList ();

			lock (Syscall.usershell_lock) {
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
