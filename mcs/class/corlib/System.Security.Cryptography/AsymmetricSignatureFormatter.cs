//
// System.Security.Cryptography AsymmetricSignatureFormatter Class implementation
//
// Authors:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;

namespace System.Security.Cryptography {
	
	public abstract class AsymmetricSignatureFormatter {
		
		public AsymmetricSignatureFormatter () 
		{
		}
		
		public abstract void SetHashAlgorithm (string strName);		
		
		public abstract void SetKey (AsymmetricAlgorithm key);
		
		public abstract byte[] CreateSignature (byte[] rgbHash);

		public virtual byte[] CreateSignature (HashAlgorithm hash) 
		{
			if (hash == null)
				throw new ArgumentNullException ("hash");

			SetHashAlgorithm (hash.ToString ());
			return CreateSignature (hash.Hash);
		}
	}
}

