//
// X509ChainStatus.cs - System.Security.Cryptography.X509Certificates.X509ChainStatus
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

	public struct X509ChainStatus {

		private X509ChainStatusFlags _status;
		private string _info;

		// properties

		public X509ChainStatusFlags Status {
			get { return _status; }
			set { _status = value; }
		}

		public string StatusInformation {
			get { return _info; }
			set { _info = value; }
		}
	}
}

#endif