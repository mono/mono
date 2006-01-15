//
// Mono.Unix/UnixUserInfo.cs
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

	public sealed class UnixUserInfo
	{
		private Native.Passwd passwd;

		public UnixUserInfo (string user)
		{
			passwd = new Native.Passwd ();
			Native.Passwd pw;
			int r = Native.Syscall.getpwnam_r (user, passwd, out pw);
			if (r != 0 || pw == null)
				throw new ArgumentException (Locale.GetText ("invalid username"), "user");
		}

		[CLSCompliant (false)]
		public UnixUserInfo (uint user)
		{
			passwd = new Native.Passwd ();
			Native.Passwd pw;
			int r = Native.Syscall.getpwuid_r (user, passwd, out pw);
			if (r != 0 || pw == null)
				throw new ArgumentException (Locale.GetText ("invalid user id"), "user");
		}

		public UnixUserInfo (long user)
		{
			passwd = new Native.Passwd ();
			Native.Passwd pw;
			int r = Native.Syscall.getpwuid_r (Convert.ToUInt32 (user), passwd, out pw);
			if (r != 0 || pw == null)
				throw new ArgumentException (Locale.GetText ("invalid user id"), "user");
		}

		public UnixUserInfo (Native.Passwd passwd)
		{
			this.passwd = CopyPasswd (passwd);
		}

		private static Native.Passwd CopyPasswd (Native.Passwd pw)
		{
			Native.Passwd p = new Native.Passwd ();

			p.pw_name   = pw.pw_name;
			p.pw_passwd = pw.pw_passwd;
			p.pw_uid    = pw.pw_uid;
			p.pw_gid    = pw.pw_gid;
			p.pw_gecos  = pw.pw_gecos;
			p.pw_dir    = pw.pw_dir;
			p.pw_shell  = pw.pw_shell;

			return p;
		}

		public string UserName {
			get {return passwd.pw_name;}
		}

		public string Password {
			get {return passwd.pw_passwd;}
		}

		public long UserId {
			get {return passwd.pw_uid;}
		}

		public UnixGroupInfo Group {
			get {return new UnixGroupInfo (passwd.pw_gid);}
		}

		public long GroupId {
			get {return passwd.pw_gid;}
		}

		public string GroupName {
			get {return Group.GroupName;}
		}

		public string RealName {
			get {return passwd.pw_gecos;}
		}

		public string HomeDirectory {
			get {return passwd.pw_dir;}
		}

		public string ShellProgram {
			get {return passwd.pw_shell;}
		}

		public override int GetHashCode ()
		{
			return passwd.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType())
				return false;
			return passwd.Equals (((UnixUserInfo) obj).passwd);
		}

		public override string ToString ()
		{
			return passwd.ToString ();
		}

		public static UnixUserInfo GetRealUser ()
		{
			return new UnixUserInfo (GetRealUserId ());
		}

		public static long GetRealUserId ()
		{
			return Native.Syscall.getuid ();
		}

		// I would hope that this is the same as GetCurrentUserName, but it is a
		// different syscall, so who knows.
		public static string GetLoginName ()
		{
			StringBuilder buf = new StringBuilder (4);
			int r;
			do {
				buf.Capacity *= 2;
				r = Native.Syscall.getlogin_r (buf, (ulong) buf.Capacity);
			} while (r == (-1) && Native.Stdlib.GetLastError() == Native.Errno.ERANGE);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return buf.ToString ();
		}

		public Native.Passwd ToPasswd ()
		{
			return CopyPasswd (passwd);
		}

		public static UnixUserInfo[] GetLocalUsers ()
		{
			ArrayList entries = new ArrayList ();
			lock (Native.Syscall.pwd_lock) {
				if (Native.Syscall.setpwent () != 0) {
					UnixMarshal.ThrowExceptionForLastError ();
				}
				try {
					Native.Passwd p;
					while ((p = Native.Syscall.getpwent()) != null)
						entries.Add (new UnixUserInfo (p));
					if (Native.Syscall.GetLastError () != (Native.Errno) 0)
						UnixMarshal.ThrowExceptionForLastError ();
				}
				finally {
					Native.Syscall.endpwent ();
				}
			}
			return (UnixUserInfo[]) entries.ToArray (typeof(UnixUserInfo));
		}
	}
}

// vim: noexpandtab
