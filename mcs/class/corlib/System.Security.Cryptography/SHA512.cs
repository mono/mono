//
// System.Security.Cryptography SHA512 Class implementation
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//   Sebastien Pouliot (spouliot@motus.com)
//
// Copyright 2001 by Matthew S. Ford.
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//


using System.Security.Cryptography;

namespace System.Security.Cryptography {
	/// <summary>
	/// Common base class for all derived SHA512 implementations.
	/// </summary>
	public abstract class SHA512 : HashAlgorithm {
		/// <summary>
		/// Called from constructor of derived class.
		/// </summary>
		public SHA512 () 
		{
			HashSizeValue = 512;
		}

		/// <summary>
		/// Creates the default derived class.
		/// </summary>
		public static new SHA512 Create () 
		{
			return Create ("System.Security.Cryptography.SHA512");
		}
	
		/// <summary>
		/// Creates a new derived class.
		/// </summary>
		/// <param name="hashName">Specifies which derived class to create</param>
		public static new SHA512 Create (string hashName) 
		{
			return (SHA512) CryptoConfig.CreateFromName (hashName);
		}
	}
}
