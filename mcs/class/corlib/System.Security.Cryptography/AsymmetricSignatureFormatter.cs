//
// System.Security.Cryptography AsymmetricSignatureFormatter Class implementation
//
// Authors:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;
using System.Security;

namespace System.Security.Cryptography {
	
	/// <summary>
	/// Abstract base class for all asymmetric signature formatter.
	/// Available derived classes:
	/// DSASignatureFormatter, RSAPKCS1SignatureFormatter
	/// </summary>
	public abstract class AsymmetricSignatureFormatter {
		
		/// <summary>
		/// constructor, no idea why it is here (abstract class)  :-)
		/// just for compatibility with MS
		/// </summary>
		public AsymmetricSignatureFormatter () 
		{
		}
		
		/// <summary>
		/// Sets the hash algorithm used for verifying a signature
		/// </summary>
		public abstract void SetHashAlgorithm (string strName);		
		
		/// <summary>
		/// set the private key
		/// </summary>
		public abstract void SetKey (AsymmetricAlgorithm key);
		
		/// <summary>
		/// Create a signature from the given data
		/// </summary>
		public abstract byte[] CreateSignature (byte[] rgbHash);

		/// <summary>
		/// Create a signature from data with the specified hash algorithm
		/// </summary>
		public virtual byte[] CreateSignature (HashAlgorithm hash) 
		{
			if (hash == null)
				throw new ArgumentNullException ();
			SetHashAlgorithm (hash.ToString ());
			return CreateSignature (hash.Hash);
		}
		
	} // AsymmetricSignatureFormatter
	
} // System.Security.Cryptography

