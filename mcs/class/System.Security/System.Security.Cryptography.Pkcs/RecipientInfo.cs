//
// RecipientInfo.cs - System.Security.Cryptography.Pkcs.RecipientInfo
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.Pkcs {

	public abstract class RecipientInfo {

		private RecipientInfoType _type;

		// constructors

		protected RecipientInfo () {}


		// documented as protected at http://longhorn.msdn.microsoft.com
		// but not present in the 1.2 beta SDK
		internal RecipientInfo (RecipientInfoType recipInfoType) 
		{
			_type = recipInfoType;
		}

		// properties

		public abstract byte[] EncryptedKey { get; }

		public abstract AlgorithmIdentifier KeyEncryptionAlgorithm { get; }

		public abstract SubjectIdentifier RecipientIdentifier { get; }

		public RecipientInfoType Type {
			get { return _type; }
		}

		public abstract int Version { get; }
	}
}

#endif