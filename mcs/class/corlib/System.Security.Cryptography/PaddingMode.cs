//
// System.Security.Cryptography PaddingMode enumeration
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// Copyright 2001 by Matthew S. Ford.
//


namespace System.Security.Cryptography {
	
	/// <summary>
	/// How to pad the message processed by block ciphers when they don't come out to the being the size of the block.
	/// </summary>
	[Serializable]
	public enum PaddingMode {
		None = 0x1,
		PKCS7, // Each byte contains the value of the number of padding bytes.
		Zeros  // Append zeros to the message.
	}
}
	
