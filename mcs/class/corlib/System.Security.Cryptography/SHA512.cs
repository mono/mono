//
// System.Security.Cryptography SHA512 Class implementation
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//   Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright 2001 by Matthew S. Ford.
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

namespace System.Security.Cryptography {

	public abstract class SHA512 : HashAlgorithm {

		public SHA512 () 
		{
			HashSizeValue = 512;
		}

		public static new SHA512 Create () 
		{
			return Create ("System.Security.Cryptography.SHA512");
		}
	
		public static new SHA512 Create (string hashName) 
		{
			return (SHA512) CryptoConfig.CreateFromName (hashName);
		}
	}
}
