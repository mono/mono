//
// System.Security.Cryptography AsymmetricKeyExchangeFormatter Class implementation
//
// Authors:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;
using System.Security;

namespace System.Security.Cryptography {
	
	/// <summary>
	/// Abstract base class for all asymmetric key exchange formatter.
	/// Available derived classes:
	/// RSAOAEPKeyExchangeFormatter, RSAPKCS1KeyExchangeFormatter
	/// </summary>
	public abstract class AsymmetricKeyExchangeFormatter {
		
		/// <summary>
		/// constructor, no idea why it is here (abstract class)  :-)
		/// just for compatibility with MS
		/// </summary>
		public AsymmetricKeyExchangeFormatter() {
		}
		
		/// <summary>
		/// XML string containing the parameters of an asymmetric key exchange operation
		/// </summary>
		public abstract string Parameters {get;}
		
		/// <summary>
		/// create encrypted key exchange data
		/// </summary>
		public abstract byte[] CreateKeyExchange(byte[] data);

		/// <summary>
		/// create encrypted key exchange data
		/// </summary>
		public abstract byte[] CreateKeyExchange(byte[] data, Type symAlgType);

		/// <summary>
		/// set the private key
		/// </summary>
		public abstract void SetKey(AsymmetricAlgorithm key);
		
	} // AsymmetricKeyExchangeFormatter
	
} // System.Security.Cryptography

