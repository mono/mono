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
		private Passwd passwd;

		public UnixUserInfo (string user)
		{
			passwd = new Passwd ();
			Passwd pw;
			int r = Syscall.getpwnam_r (user, passwd, out pw);
			if (r != 0 || pw == null)
				throw new ArgumentException (Locale.GetText ("invalid username"), "user");
		}

		[CLSCompliant (false)]
		public UnixUserInfo (uint user)
		{
			passwd = new Passwd ();
			Passwd pw;
			int r = Syscall.getpwuid_r (user, passwd, out pw);
			if (r != 0 || pw == null)
				throw new ArgumentException (Locale.GetText ("invalid user id"), "user");
		}

		public UnixUserInfo (long user)
		{
			passwd = new Passwd ();
			Passwd pw;
			int r = Syscall.getpwuid_r (Convert.ToUInt32 (user), passwd, out pw);
			if (r != 0 || pw == null)
				throw new ArgumentException (Locale.GetText ("invalid user id"), "user");
		}

		[Obsolete ("Use UnixUserInfo (Mono.Unix.Native.Passwd)")]
		public UnixUserInfo (Passwd passwd)
		{
			this.passwd = passwd;
		}

		public UnixUserInfo (Native.Passwd passwd)
		{
			this.passwd = new Passwd ();
			this.passwd.pw_name   = passwd.pw_name;
			this.passwd.pw_passwd = passwd.pw_passwd;
			this.passwd.pw_uid    = passwd.pw_uid;
			this.passwd.pw_gid    = passwd.pw_gid;
			this.passwd.pw_gecos  = passwd.pw_gecos;
			this.passwd.pw_dir    = passwd.pw_dir;
			this.passwd.pw_shell  = passwd.pw_shell;
		}

		public string UserName {
			get {return passwd.pw_name;}
		}

		public string Password {
			get {return passwd.pw_passwd;}
		}

		[CLSCompliant (false)]
		[Obsolete ("The type of this property will change to Int64 in the next release")]
		public uint UserId {
			get {return passwd.pw_uid;}
		}

		public UnixGroupInfo Group {
			get {return new UnixGroupInfo (passwd.pw_gid);}
		}

		[CLSCompliant (false)]
		[Obsolete ("The type of this property will change to Int64 in the next release")]
		public uint GroupId {
			get {return passwd.pw_gid;}
		}

		public string GroupName {
			get {return UnixGroup.GetName (passwd.pw_gid);}
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
			return Syscall.getuid ();
		}

		// I would hope that this is the same as GetCurrentUserName, but it is a
		// different syscall, so who knows.
		public static string GetLoginName ()
		{
			StringBuilder buf = new StringBuilder (4);
			int r;
			do {
				buf.Capacity *= 2;
				r = Syscall.getlogin_r (buf, (ulong) buf.Capacity);
			} while (r == (-1) && Syscall.GetLastError() == Error.ERANGE);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return buf.ToString ();
		}

		[Obsolete ("The return type of this method will change to Mono.Unix.Native.Passwd in the next release")]
		public Passwd ToPasswd ()
		{
			return passwd;
		}

		public static UnixUserInfo[] GetLocalUsers ()
		{
			ArrayList entries = new ArrayList ();
			lock (Syscall.pwd_lock) {
				if (Native.Syscall.setpwent () != 0) {
					UnixMarshal.ThrowExceptionForLastError ();
				}
				try {
					Passwd p;
					while ((p = Syscall.getpwent()) != null)
						entries.Add (new UnixUserInfo (p));
					if (Syscall.GetLastError () != (Error) 0)
						UnixMarshal.ThrowExceptionForLastError ();
				}
				finally {
					Syscall.endpwent ();
				}
			}
			return (UnixUserInfo[]) entries.ToArray (typeof(UnixUserInfo));
		}
	}
}

// vim: noexpandtab
