//
// System.Security.Cryptography PaddingMode enumeration
//
// Authors:
//	Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright 2001 by Matthew S. Ford.
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

namespace System.Security.Cryptography {
	
	[Serializable]
	public enum PaddingMode {
		None = 0x1,
		PKCS7,
		Zeros,
#if NET_2_0
		ANSIX923,
		ISO10126
#endif
	}
}
	
