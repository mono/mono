//
// DHParameters.cs: Defines a structure that holds the parameters of the Diffie-Hellman algorithm
//
// Author:
//	Pieter Philippaerts (Pieter@mentalis.org)
//
// (C) 2003 The Mentalis.org Team (http://www.mentalis.org/)
//

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

using System;

namespace Mono.Security.Cryptography {
	/// <summary>
	/// Represents the parameters of the Diffie-Hellman algorithm.
	/// </summary>
	[Serializable]
	public struct DHParameters {
		/// <summary>
		/// Represents the public <b>P</b> parameter of the Diffie-Hellman algorithm.
		/// </summary>
		public byte[] P;
		/// <summary>
		/// Represents the public <b>G</b> parameter of the Diffie-Hellman algorithm.
		/// </summary>
		public byte[] G;
		/// <summary>
		/// Represents the private <b>X</b> parameter of the Diffie-Hellman algorithm.
		/// </summary>
		[NonSerialized]
		public byte[] X;
	}
}