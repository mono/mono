//
// Mono.Posix/PosixUserInfo.cs
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

	public sealed class PosixUserInfo
	{
		private Passwd passwd;

		public PosixUserInfo (string user)
		{
			passwd = new Passwd ();
			Passwd pw;
			int r = Syscall.getpwnam_r (user, passwd, out pw);
			if (r != 0 || pw == null)
				throw new ArgumentException (Locale.GetText ("invalid username"), "user");
		}

		public PosixUserInfo (uint user)
		{
			passwd = new Passwd ();
			Passwd pw;
			int r = Syscall.getpwuid_r (user, passwd, out pw);
			if (r != 0 || pw == null)
				throw new ArgumentException (Locale.GetText ("invalid user id"), "user");
		}

		public PosixUserInfo (Passwd passwd)
		{
			this.passwd = passwd;
		}

		public string UserName {
			get {return passwd.pw_name;}
		}

		public string Password {
			get {return passwd.pw_passwd;}
		}

		public uint UserId {
			get {return passwd.pw_uid;}
		}

		public uint GroupId {
			get {return passwd.pw_gid;}
		}

		public string GroupName {
			get {return PosixGroup.GetName (passwd.pw_gid);}
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
			return passwd.Equals (((PosixUserInfo) obj).passwd);
		}

		public override string ToString ()
		{
			return passwd.ToString ();
		}
	}
}

// vim: noexpandtab
