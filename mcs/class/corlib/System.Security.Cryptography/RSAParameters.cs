//
// System.Security.Cryptography.RSAParameters.cs
//
// Authors:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//

using System;

namespace System.Security.Cryptography {
	
	[Serializable]
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
