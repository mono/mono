//
// System.Security.Cryptography AsymmetricKeyExchangeDeformatter Class implementation
//
// Authors:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;

namespace System.Security.Cryptography {
	
	public abstract class AsymmetricKeyExchangeDeformatter {
		
		public AsymmetricKeyExchangeDeformatter () 
		{
		}
		
		public abstract string Parameters {
			get;
			set;
		}
		
		public abstract byte[] DecryptKeyExchange (byte[] rgb);
		
		public abstract void SetKey (AsymmetricAlgorithm key);
	}
}

