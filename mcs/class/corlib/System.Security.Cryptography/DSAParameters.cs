//
// System.Security.Cryptography DSAParameters.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;

namespace System.Security.Cryptography {

	[Serializable]
	public struct DSAParameters {

		public int Counter;
		
		public byte[] G;
		
		public byte[] J;
		
		public byte[] P;
		
		public byte[] Q;
		
		public byte[] Seed;
		
		[NonSerialized]
		public byte[] X;
		
		public byte[] Y;
	}
}
