//
// System.Security.Cryptography MD5 Class implementation
//
// Authors:
//	Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright 2001 by Matthew S. Ford.
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

namespace System.Security.Cryptography {

	public abstract class MD5 : HashAlgorithm {

		// Why is it protected when others abstract hash classes are public ?
		protected MD5 () 
		{
			HashSizeValue = 128;
		}
	
		public static new MD5 Create () 
		{
			return Create ("System.Security.Cryptography.MD5");
		}

		public static new MD5 Create (string hashName) 
		{
			return (MD5) CryptoConfig.CreateFromName (hashName);
		}
	}
}

