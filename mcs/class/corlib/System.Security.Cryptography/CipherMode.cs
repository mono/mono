//
// System.Security.Cryptography CipherMode enumeration
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// Copyright 2001 by Matthew S. Ford.
// (C) 2004 Novell (http://www.novell.com)
//

namespace System.Security.Cryptography {

	[Serializable]
	public enum CipherMode {
		CBC = 0x1, // Cipher Block Chaining
		ECB, // Electronic Codebook
		OFB, // Output Feedback
		CFB, // Cipher Feedback
		CTS, // Cipher Text Stealing
	}
}

