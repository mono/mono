//
// Mono.Directory.LDAP.LDAP
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
  	class TimeVal {
		public int tv_sec;
		public int tv_usec;

		public static TimeVal FromTimeSpan (TimeSpan span) {
			TimeVal tv = new TimeVal();
			long nanoseconds;

			/* make sure we're dealing with a positive TimeSpan */
			span = span.Duration();

			nanoseconds = span.Ticks * 100;

			tv.tv_sec = (int)(nanoseconds / 1E+09);
			tv.tv_usec = (int)((nanoseconds % 1E+09) / 1000);

			return tv;
		}
	}

	public enum SearchScope {
		Base = 0x0000,
		OneLevel = 0x0001,
		SubTree = 0x0002
	}

	public class LDAP {

		/* Search Scopes */
		public LDAP (string uri) {
			int rv;
			rv = ldap_initialize (out ld, uri);
			// FIXME throw something here if ldap_initialize returns an error
		}

		public LDAP (string host, int port) {
			ld = ldap_init (host, port);
			// FIXME throw something here if ldap_init fails.
		}

		public int BindSimple (string who, string cred) {
			return ldap_simple_bind_s (ld, who, cred);
		}

		public int StartTLS () {
			// FIXME should expose client/server ctrls
			return ldap_start_tls_s (ld, IntPtr.Zero, IntPtr.Zero);
		}

		public int Search (string      base_entry,
				   SearchScope scope,
				   string      filter,
				   string[]    attrs,
				   bool        attrsonly,
				   TimeSpan    timeOut,
				   int         sizeLimit,
				   out LDAPMessage res) {
		  // FIXME should expose client/server ctrls
		  IntPtr serverctrls = new IntPtr();
		  IntPtr clientctrls = new IntPtr();
		  TimeVal tv = TimeVal.FromTimeSpan (timeOut);
		  IntPtr native_res;
		  int rv;

		  rv = ldap_search_ext_s (ld, base_entry, (int) scope, filter,
					  attrs, attrsonly ? 1 : 0,
					  serverctrls, clientctrls,
					  ref tv, sizeLimit, out native_res);

		  if (native_res != IntPtr.Zero)
		    res = new LDAPMessage (this, native_res);
		  else
		    res = null;

		  return rv;
		}
				   
		public void Unbind () {
			// FIXME should expose client/server ctrls
			ldap_unbind_ext_s (ld, IntPtr.Zero, IntPtr.Zero);
			// FIXME throw something here if ldap_unbind_ext_s returns an error
		}

		public IntPtr NativeLDAP {
			get { return ld; }
		}

		[DllImport("ldap")]
		static extern IntPtr ldap_init(string host, int port);

		[DllImport("ldap")]
		static extern int ldap_initialize(out IntPtr ld, string uri);

		[DllImport("ldap")]
		static extern int ldap_simple_bind_s(IntPtr ld,
						     string who, string cred);

		[DllImport("ldap")]
		static extern int ldap_start_tls_s (IntPtr ld,
						    IntPtr serverctrls,
						    IntPtr clientctrls);

		[DllImport("ldap")]
		static extern int ldap_search_ext_s (IntPtr	ld,
						     string	base_entry,
						     int	scope,
						     string	filter,
						     string[]	attrs,
						     int	attrsonly,
						     IntPtr	serverctrls,
						     IntPtr	clientctrls,
						     ref TimeVal timeout,
						     int	sizelimit,
						     out IntPtr	res);

		[DllImport("ldap")]
		static extern int ldap_unbind_ext_s (IntPtr	ld,
						     IntPtr	serverctrls,
						     IntPtr	clientctrls);

		IntPtr ld;
	}
}
