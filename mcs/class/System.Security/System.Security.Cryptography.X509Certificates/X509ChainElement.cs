//
// X509ChainElement.cs - System.Security.Cryptography.X509Certificates.X509ChainElement
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

	public class X509ChainElement {

		// constructors

		// only accessible from X509Chain.ChainElements
		internal X509ChainElement () {}

		// properties

		[MonoTODO]
		public X509CertificateEx Certificate {
			get { return null; }
		}

		[MonoTODO]
		public X509ChainStatus[] ChainElementStatus {
			get { return null; }
		}

		[MonoTODO]
		public string Information {
			get { return null; }
		}
	}
}

#endif