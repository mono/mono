//
// X509IssuerSerial.cs - X509IssuerSerial implementation for XML Encryption
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

#if NET_1_2

namespace System.Security.Cryptography.Xml {
	public struct X509IssuerSerial {
		public string IssuerName;
		public string SerialNumber;
	}
}

#endif
