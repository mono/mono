//
// System.Security.Cryptography AsymmetricKeyExchangeDeformatter Class implementation
//
// Authors:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;
using System.Security;

namespace System.Security.Cryptography {
	
	/// <summary>
	/// Abstract base class for all asymmetric key exchange deformatter.
	/// Available derived classes:
	/// RSAOAEPKeyExchangeDeformatter, RSAPKCS1KeyExchangeDeformatter
	/// </summary>
	public abstract class AsymmetricKeyExchangeDeformatter {
		
		/// <summary>
		/// constructor, no idea why it is here (abstract class)  :-)
		/// just for compatibility with MS
		/// </summary>
		public AsymmetricKeyExchangeDeformatter() {
		}
		
		/// <summary>
		/// XML string containing the parameters of an asymmetric key exchange operation
		/// </summary>
		public abstract string Parameters {get; set;}
		
		/// <summary>
		/// get secret data
		/// </summary>
		public abstract byte[] DecryptKeyExchange(byte[] rgb);
		
		/// <summary>
		/// set the private key
		/// </summary>
		public abstract void SetKey(AsymmetricAlgorithm key);
		
	} // AsymmetricKeyExchangeDeformatter
	
} // System.Security.Cryptography

