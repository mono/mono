//
// X509ChainPolicy.cs - System.Security.Cryptography.X509Certificates.X509ChainPolicy
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

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

#if NET_2_0

using System;
using System.Security.Cryptography;

namespace System.Security.Cryptography.X509Certificates {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class X509ChainPolicy {

		private OidCollection _apps;
		private OidCollection _cert;
		private X509CertificateExCollection _store;
		private X509RevocationFlag _rflag;
		private X509RevocationMode _mode;
		private TimeSpan _timeout;
		private X509VerificationFlags _vflags;
		private DateTime _vtime;

		// constructors

		// only accessible from X509Chain
		internal X509ChainPolicy () 
		{
			Reset ();
		}

		// properties

		public OidCollection ApplicationPolicy {
			get { return _apps; }
		}

		public OidCollection CertificatePolicy {
			get { return _cert; }
		}

		public X509CertificateExCollection ExtraStore {
			get { return _store; }
		}

		public X509RevocationFlag RevocationFlag {
			get { return _rflag; }
			set { _rflag = value; }
		}

		public X509RevocationMode RevocationMode {
			get { return _mode; }
			set { _mode = value; }
		}

		public TimeSpan UrlRetrievalTimeout {
			get { return _timeout; }
			set { _timeout = value; }
		}

		public X509VerificationFlags VerificationFlags {
			get { return _vflags; }
			set { _vflags = value; }
		}

		public DateTime VerificationTime {
			get { return _vtime; }
			set { _vtime = value; }
		}

		// methods

		public void Reset ()
		{
			_apps = new OidCollection ();
			_cert = new OidCollection ();
			_store = new X509CertificateExCollection ();
			_rflag = X509RevocationFlag.ExcludeRoot;
			_mode = X509RevocationMode.Online;
			_timeout = new TimeSpan (0);
			_vflags = X509VerificationFlags.NoFlag;
			_vtime = DateTime.Now;
		}
	}
}

#endif