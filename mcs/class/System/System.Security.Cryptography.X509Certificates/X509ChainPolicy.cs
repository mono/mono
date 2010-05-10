//
// System.Security.Cryptography.X509Certificates.X509ChainPolicy class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005-2006 Novell Inc. (http://www.novell.com)
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

#if SECURITY_DEP || MOONLIGHT

namespace System.Security.Cryptography.X509Certificates {

	public sealed class X509ChainPolicy {

		private OidCollection apps;
		private OidCollection cert;
		private X509Certificate2Collection store;
		private X509RevocationFlag rflag;
		private X509RevocationMode mode;
		private TimeSpan timeout;
		private X509VerificationFlags vflags;
		private DateTime vtime;

		// constructors

		public X509ChainPolicy () 
		{
			Reset ();
		}

		// properties

		public OidCollection ApplicationPolicy {
			get { return apps; }
		}

		public OidCollection CertificatePolicy {
			get { return cert; }
		}

		public X509Certificate2Collection ExtraStore {
			get { return store; }
		}

		public X509RevocationFlag RevocationFlag {
			get { return rflag; }
			set {
				if ((value < X509RevocationFlag.EndCertificateOnly) || (value > X509RevocationFlag.ExcludeRoot))
					throw new ArgumentException ("RevocationFlag");
				rflag = value;
			}
		}

		public X509RevocationMode RevocationMode {
			get { return mode; }
			set {
				if ((value < X509RevocationMode.NoCheck) || (value > X509RevocationMode.Offline))
					throw new ArgumentException ("RevocationMode");
				mode = value;
			}
		}

		public TimeSpan UrlRetrievalTimeout {
			get { return timeout; }
			set { timeout = value; }
		}

		public X509VerificationFlags VerificationFlags {
			get { return vflags; }
			set {
				if ((value | X509VerificationFlags.AllFlags) != X509VerificationFlags.AllFlags)
					throw new ArgumentException ("VerificationFlags");
				vflags = value;
			}
		}

		public DateTime VerificationTime {
			get { return vtime; }
			set { vtime = value; }
		}

		// methods

		public void Reset ()
		{
			apps = new OidCollection ();
			cert = new OidCollection ();
			store = new X509Certificate2Collection ();
			rflag = X509RevocationFlag.ExcludeRoot;
			mode = X509RevocationMode.Online;
			timeout = TimeSpan.Zero;
			vflags = X509VerificationFlags.NoFlag;
			vtime = DateTime.Now;
		}
	}
}

#endif
