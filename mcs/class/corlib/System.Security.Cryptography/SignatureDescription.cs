//
// System.Security.Cryptography SignatureDescription Class implementation
//
// Authors:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//
// LAMESPEC: documentation of this class is completely missing in the sdk doc
// TODO: Implement AsymmetricSignatureFormatter & AsymmetricSignatureDeformatter methods

using System;
using System.Security;

namespace System.Security.Cryptography {
	
	/// <summary>
	/// LAMESPEC: no sdk doc available for this class by the time of beta 2
	/// </summary>
	public class SignatureDescription {
		private string _DeformatterAlgorithm;
		private string _DigestAlgorithm;		
		private string _FormatterAlgorithm;		
		private string _KeyAlgorithm;		
		
		/// <summary>
		/// LAMESPEC: no idea what param el should do??
		/// </summary>
		public SignatureDescription (SecurityElement el) {
			if (el == null)
				throw new CryptographicException();
		}
		

		/// <summary>
		/// LAMESPEC: what to do if setting null values?
		/// </summary>
		public string DeformatterAlgorithm {
			get {
				return _DeformatterAlgorithm;
			}
			set {
				_DeformatterAlgorithm = value;
			}
		}

		/// <summary>
		/// LAMESPEC: what to do if setting null values?
		/// </summary>
		public string DigestAlgorithm {
			get {
				return _DigestAlgorithm;
			}
			set {
				_DigestAlgorithm = value;
			}
		}

		/// <summary>
		/// LAMESPEC: what to do if setting null values?
		/// </summary>
		public string FormatterAlgorithm {
			get {
				return _FormatterAlgorithm;
			}
			set {
				_FormatterAlgorithm = value;
			}
		}

		/// <summary>
		/// LAMESPEC: what to do if setting null values?
		/// </summary>
		public string KeyAlgorithm {
			get {
				return _KeyAlgorithm;
			}
			set {
				_KeyAlgorithm = value;
			}
		}

		public virtual AsymmetricSignatureDeformatter CreateDeformatter(AsymmetricAlgorithm key) 
		{
			// TODO: Implement
			return null;
		}
		
		/// <summary>
		/// Create the hash algorithm assigned with this object
		/// </summary>
		public virtual HashAlgorithm CreateDigest()
		{
			return HashAlgorithm.Create(_DigestAlgorithm);
		}
		
		public virtual AsymmetricSignatureFormatter CreateFormatter(AsymmetricAlgorithm key)
		{
			// TODO: Implement
			return null;
		}
		
	} // SignatureDescription
	
} // System.Security.Cryptography

