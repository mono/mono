//
// System.Security.Cryptography AsymmetricKeyExchangeFormatter Class implementation
//
// Authors:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;

namespace System.Security.Cryptography {
	
	public abstract class AsymmetricKeyExchangeFormatter {
		
		public AsymmetricKeyExchangeFormatter ()
		{
		}
		
		public abstract string Parameters {
			get;
		}
		
		public abstract byte[] CreateKeyExchange (byte[] data);

		public abstract byte[] CreateKeyExchange (byte[] data, Type symAlgType);

		public abstract void SetKey (AsymmetricAlgorithm key);
	}
}

