//
// System.Security.Cryptography DSASignatureDeformatter.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;

namespace System.Security.Cryptography
{

	/// <summary>
	/// DSA Signature Deformatter
	/// </summary>
	public class DSASignatureDeformatter : AsymmetricSignatureDeformatter
	{
		
		public DSASignatureDeformatter()
		{
			// TODO: implement
		}
		
		public DSASignatureDeformatter(AsymmetricAlgorithm key)
		{
			// TODO: implement
		}
		
		public override void SetHashAlgorithm(string strName)
		{
			throw new CryptographicException("This method is not used");
		}
		
		public override void SetKey(AsymmetricAlgorithm key)
		{
			// TODO: implement
		}
		
		public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature)
		{
			// TODO: implement
			return false;
		}
		
	} // DSASignatureDeformatter
	
} // System.Security.Cryptography
