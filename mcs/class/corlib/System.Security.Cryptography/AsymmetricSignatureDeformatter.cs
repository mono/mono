//
// System.Security.Cryptography AsymmetricSignatureDeformatter Class implementation
//
// Authors:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;
using System.Security;

namespace System.Security.Cryptography {
	
	/// <summary>
	/// Abstract base class for all asymmetric signature deformatter.
	/// Available derived classes:
	/// DSASignatureDeformatter, RSAPKCS1SignatureDeformatter
	/// </summary>
	public abstract class AsymmetricSignatureDeformatter {
		
		/// <summary>
		/// constructor, no idea why it is here (abstract class)  :-)
		/// just for compatibility with MS
		/// </summary>
		public AsymmetricSignatureDeformatter () {}
		
		/// <summary>
		/// Sets the hash algorithm used for verifying a signature
		/// </summary>
		public abstract void SetHashAlgorithm (string strName);		
		
		/// <summary>
		/// set the keypair
		/// </summary>
		public abstract void SetKey (AsymmetricAlgorithm key);
		
		/// <summary>
		/// Verifies the given Signature
		/// </summary>
		public abstract bool VerifySignature (byte[] rgbHash, byte[] rgbSignature);

		/// <summary>
		/// Verifies the given Signature with the given hash algorithm
		/// </summary>
		public virtual bool VerifySignature (HashAlgorithm hash, byte[] rgbSignature) 
		{
			if (hash == null)
				throw new ArgumentNullException ("hash");
			return VerifySignature (hash.Hash, rgbSignature);
		}
		
	} // AsymmetricSignatureDeformatter
	
} // System.Security.Cryptography

