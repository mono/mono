//
// X509IssuerSerial.cs - X509IssuerSerial implementation for XML Encryption
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2004 Novell Inc. (http://www.novell.com)
//

namespace System.Security.Cryptography.Xml {

#if NET_2_0
	public
#else
	// structure was undocumented (but present) before Fx 2.0
	internal
#endif
	struct X509IssuerSerial {
		public string IssuerName;
		public string SerialNumber;

		public X509IssuerSerial (string issuer, string serial) 
		{
			IssuerName = issuer;
			SerialNumber = serial;
		}
	}
}

