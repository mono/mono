//
// RIPEMD160.cs: Defines a base class from which all RIPEMD-160 implementations inherit
//
// Author:
//	Pieter Philippaerts (Pieter@mentalis.org)
//
// (C) 2003 The Mentalis.org Team (http://www.mentalis.org/)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography {
	/// <summary>
	/// Represents the abstract class from which all implementations of the <see cref="RIPEMD160"/> hash algorithm inherit.
	/// </summary>
	public abstract class RIPEMD160 : HashAlgorithm {
		/// <summary>
		/// Initializes a new instance of <see cref="RIPEMD160"/>.
		/// </summary>
		protected RIPEMD160 () 
		{
			this.HashSizeValue = 160;
		}

		/// <summary>
		/// Creates an instance of the default implementation of the <see cref="RIPEMD160"/> hash algorithm.
		/// </summary>
		/// <returns>A new instance of the RIPEMD160 hash algorithm.</returns>
		public static new RIPEMD160 Create () 
		{
			return Create ("System.Security.Cryptography.RIPEMD160");
		}

		/// <summary>
		/// Creates an instance of the specified implementation of the <see cref="RIPEMD160"/> hash algorithm.
		/// </summary>
		/// <param name="hashName">The name of the specific implementation of RIPEMD160 to use.</param>
		/// <returns>A new instance of the specified implementation of RIPEMD160.</returns>
		public static new RIPEMD160 Create (string hashName) 
		{
			return (RIPEMD160)CryptoConfig.CreateFromName (hashName);
		}
	}
}

#endif