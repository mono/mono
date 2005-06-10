//
// System.Security.Cryptography.RSAParameters.cs
//
// Authors:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
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
	
	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public struct RSAParameters {
		[NonSerialized]
		public byte[] P;
		[NonSerialized]
		public byte[] Q;
		[NonSerialized]
		public byte[] D;
		[NonSerialized]
		public byte[] DP;
		[NonSerialized]
		public byte[] DQ;
		[NonSerialized]
		public byte[] InverseQ;

		public byte[] Modulus;
		public byte[] Exponent;
	}
}
