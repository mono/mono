//
// System.Security.Cryptography DSAParameters.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;

namespace System.Security.Cryptography
{

	/// <summary>
	/// DSA Parameters
	/// </summary>
	public struct DSAParameters
	{
		public int Counter;
		
		public byte[] G;
		
		public byte[] J;
		
		public byte[] P;
		
		public byte[] Q;
		
		public byte[] Seed;
		
		[NonSerialized]
		public byte[] X;
		
		public byte[] Y;
		
	} // DSAParameters
	
} // System.Security.Cryptography
