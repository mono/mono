//
// System.Security.Cryptography MD5 Class implementation
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
	/// Common base class for all derived MD5 implementations.
	/// </summary>
	public abstract class MD5 : HashAlgorithm {
		/// <summary>
		/// Called from constructor of derived class.
		/// </summary>
		// Why is it protected when others abstract hash classes are public ?
		protected MD5 () 
		{
			HashSizeValue = 128;
		}
	
		/// <summary>
		/// Creates the default derived class.
		/// </summary>
		public static new MD5 Create () 
		{
			return Create ("System.Security.Cryptography.MD5");
		}

		/// <summary>
		/// Creates a new derived implementation.
		/// </summary>
		/// <param name="hashName">Specifies which derived class to create</param>
		public static new MD5 Create (string hashName) 
		{
			return (MD5) CryptoConfig.CreateFromName (hashName);
		}
	}
}

