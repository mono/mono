//
// RIPEMD160.cs: Defines a base class from which all RIPEMD-160 implementations inherit
//
// Author:
//	Pieter Philippaerts (Pieter@mentalis.org)
//
// (C) 2003 The Mentalis.org Team (http://www.mentalis.org/)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Runtime.InteropServices;

namespace System.Security.Cryptography {
	/// <summary>
	/// Represents the abstract class from which all implementations of the <see cref="RIPEMD160"/> hash algorithm inherit.
	/// </summary>
	[ComVisible (true)]
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
#if FULL_AOT_RUNTIME
			return new System.Security.Cryptography.RIPEMD160Managed ();
#else
			return Create ("System.Security.Cryptography.RIPEMD160");
#endif
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
