//
// System.Security.Cryptography DeriveBytes.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;

namespace System.Security.Cryptography
{

	/// <summary>
	/// Abstract base class for all classes that derive byte information from an integer
	/// </summary>
	public abstract class DeriveBytes
	{
	
		protected DeriveBytes() {}
		
		public abstract byte[] GetBytes(int cb);

		public abstract void Reset();
		
	} // DeriveBytes
	
} // System.Security.Cryptography
