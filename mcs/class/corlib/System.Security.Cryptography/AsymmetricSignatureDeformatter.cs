//
// System.Security.Cryptography AsymmetricSignatureDeformatter Class implementation
//
// Authors:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;

namespace System.Security.Cryptography {
	
	public abstract class AsymmetricSignatureDeformatter {
		
		public AsymmetricSignatureDeformatter ()
		{
		}
		
		public abstract void SetHashAlgorithm (string strName);		
		
		public abstract void SetKey (AsymmetricAlgorithm key);
		
		public abstract bool VerifySignature (byte[] rgbHash, byte[] rgbSignature);

		public virtual bool VerifySignature (HashAlgorithm hash, byte[] rgbSignature) 
		{
			if (hash == null)
				throw new ArgumentNullException ("hash");

			return VerifySignature (hash.Hash, rgbSignature);
		}
	}
}

