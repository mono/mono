//
// X509BasicConstraintsExtension.cs - System.Security.Cryptography.X509BasicConstraintsExtension
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.X509Certificates {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class X509BasicConstraintsExtension : X509Extension {

		public X509BasicConstraintsExtension () {}

		public bool CertificateAuthority {
			get { return false; }
		}

		public bool HasPathLengthConstraint {
			get { return false; }
		}

		public int PathLengthConstraint {
			get { return 0; }
		}
	}
}

#endif