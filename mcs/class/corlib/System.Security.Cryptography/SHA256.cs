//
// System.Security.Cryptography SHA256 Class implementation
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

	public abstract class SHA256 : HashAlgorithm {

		public SHA256 () 
		{
			HashSizeValue = 256;
		}
	
		public static new SHA256 Create () 
		{
			return Create ("System.Security.Cryptography.SHA256");
		}
	
		public static new SHA256 Create (string hashName) 
		{
			return (SHA256) CryptoConfig.CreateFromName (hashName);
		}
	}
}
