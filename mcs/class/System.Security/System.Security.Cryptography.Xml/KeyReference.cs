//
// KeyReference.cs - KeyReference implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-ReferenceList
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

#if NET_1_2

using System.Xml;

namespace System.Security.Cryptography.Xml {
	public sealed class KeyReference : EncryptedReference {

		#region Constructors
	
		public KeyReference ()
			: base ()
		{
			ReferenceType = XmlEncryption.ElementNames.KeyReference;
		}
	
		public KeyReference (string uri)
			: base (uri)
		{
			ReferenceType = XmlEncryption.ElementNames.KeyReference;
		}
	
		public KeyReference (string uri, TransformChain tc)
			: base (uri, tc)
		{
			ReferenceType = XmlEncryption.ElementNames.KeyReference;
		}
	
		#endregion // Constructors
	}
}

#endif
