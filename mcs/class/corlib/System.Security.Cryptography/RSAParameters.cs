//
// System.Security.Cryptography.RSAParameters.cs
//
// Authors:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//
// Stubbed.
//

using System;

namespace System.Security.Cryptography {
	
	[MonoTODO]
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
		[NonSerialized]

		public byte[] Modulus;
		public byte[] Exponent;
	}
}
