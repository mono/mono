//
// Mono.Directory.LDAP.LDAPMessage
//
// Author:
//    Chris Toshok (toshok@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//
// Just enough (for now) LDAP support to get System.DirectoryServices
// working.

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
using System.Runtime.InteropServices;

namespace Mono.Directory.LDAP 
{
	public class LDAPMessage {

	  	internal LDAPMessage(LDAP ld, IntPtr ldm) {
			this.ld = ld;
			this.ldm = ldm;
		}

		public LDAPMessage FirstMessage () {
			IntPtr nm = ldap_first_message (ld.NativeLDAP ,ldm);

			if (nm == IntPtr.Zero)
				return null;
			else
				return new LDAPMessage (ld, nm);
		}

		public LDAPMessage NextMessage () {
			IntPtr nm = ldap_next_message (ld.NativeLDAP ,ldm);

			if (nm == IntPtr.Zero)
				return null;
			else
				return new LDAPMessage (ld, nm);
		}

		public int CountMessages () {
			return ldap_count_messages (ld.NativeLDAP, ldm);
		}

		public LDAPMessage FirstEntry() {
			IntPtr nm = ldap_first_entry (ld.NativeLDAP ,ldm);

			if (nm == IntPtr.Zero)
				return null;
			else
				return new LDAPMessage (ld, nm);
		}

		public LDAPMessage NextEntry() {
			IntPtr nm = ldap_next_entry (ld.NativeLDAP ,ldm);

			if (nm == IntPtr.Zero)
				return null;
			else
				return new LDAPMessage (ld, nm);
		}

		public int CountEntries() {
			return ldap_count_entries (ld.NativeLDAP, ldm);
		}

		public string DN {
			get { return ldap_get_dn (ld.NativeLDAP, ldm); }
		}

		[MonoTODO]
		public string[] GetValues (string target) {
		  throw new NotImplementedException ();
		  /*
			string[] ldap_values;

			Console.WriteLine ("calling ldap_get_values ({0})", target);

			ldap_values = ldap_get_values (ld.NativeLDAP, ldm, target);

			if (ldap_values != null) {
			  string[] rv;
			  int i;

			  rv = new string[ldap_values.Length - 1];
			  for (i = 0; i < ldap_values.Length - 1; i ++)
			    rv[i] = ldap_values[i];

			  return rv;
			}
			else {
			  return null;
			}
		  */
		}

		[DllImport("ldap")]
		extern static string ldap_get_dn (IntPtr ld, IntPtr ldm);

		[DllImport("ldap")]
		extern static IntPtr ldap_first_message (IntPtr ld, IntPtr ldm);

		[DllImport("ldap")]
		extern static IntPtr ldap_next_message (IntPtr ld, IntPtr ldm);

		[DllImport("ldap")]
		extern static int ldap_count_messages (IntPtr ld, IntPtr ldm);

		[DllImport("ldap")]
		extern static IntPtr ldap_first_entry (IntPtr ld, IntPtr ldm);

		[DllImport("ldap")]
		extern static IntPtr ldap_next_entry (IntPtr ld, IntPtr ldm);

		[DllImport("ldap")]
		extern static int ldap_count_entries (IntPtr ld, IntPtr ldm);

		[DllImport("ldap")]
		extern static string ldap_first_attribute (IntPtr ld, IntPtr ldm, out IntPtr ber);

		[DllImport("ldap")]
		extern static string ldap_next_attribute (IntPtr ld, IntPtr ldm, IntPtr ber);

		/*
		[DllImport("ldapglue")]
		extern static void ldapsharp_get_values (IntPtr ld, IntPtr ldm, string target,
							 out string[] values, out int count);
		*/
		IntPtr ldm;
		LDAP   ld;
	}
}
