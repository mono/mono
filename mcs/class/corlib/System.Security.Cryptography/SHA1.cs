//
// System.Security.Cryptography.SHA1 Class implementation
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

	public abstract class SHA1 : HashAlgorithm {

		protected SHA1 ()
		{
			HashSizeValue = 160;
		}
	
		public static new SHA1 Create () 
		{
			return Create ("System.Security.Cryptography.SHA1");
		}

		public static new SHA1 Create (string hashName) 
		{
			return (SHA1) CryptoConfig.CreateFromName (hashName);
		}
	}
}
