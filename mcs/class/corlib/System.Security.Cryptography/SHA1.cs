//
// System.Security.Cryptography.SHA1 Class implementation
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//   Sebastien Pouliot (spouliot@motus.com)
//
// Copyright 2001 by Matthew S. Ford.
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System.Security.Cryptography;

namespace System.Security.Cryptography 
{
	/// <summary>
	/// Common base class for all derived SHA1 implementations.
	/// </summary>
	public abstract class SHA1 : HashAlgorithm 
	{
		/// <summary>
		/// Called from constructor of derived class.
		/// </summary>
		protected SHA1 ()
		{
			HashSizeValue = 160;
		}
	
		/// <summary>
		/// Creates the default derived class.
		/// </summary>
		public static new SHA1 Create () 
		{
			return Create ("System.Security.Cryptography.SHA1");
		}

	
		/// <summary>
		/// Creates a new derived class.
		/// </summary>
		/// <param name="hashName">Specifies which derived class to create</param>
		public static new SHA1 Create (string hashName) 
		{
			return (SHA1) CryptoConfig.CreateFromName (hashName);
		}
	}
}
