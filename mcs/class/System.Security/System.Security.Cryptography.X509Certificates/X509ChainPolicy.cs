//
// X509ChainPolicy.cs - System.Security.Cryptography.X509Certificates.X509ChainPolicy
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

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