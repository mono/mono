//
// DataReference.cs - DataReference implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-ReferenceList
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

#if NET_1_2

using System.Xml;

namespace System.Security.Cryptography.Xml {
	public sealed class DataReference : EncryptedReference {

		#region Constructors
	
		public DataReference ()
			: base ()
		{
			ReferenceType = XmlEncryption.ElementNames.DataReference;
		}
	
		public DataReference (string uri)
			: base (uri)
		{
			ReferenceType = XmlEncryption.ElementNames.DataReference;
		}
	
		public DataReference (string uri, TransformChain tc)
			: base (uri, tc)
		{
			ReferenceType = XmlEncryption.ElementNames.DataReference;
		}
	
		#endregion // Constructors
	}
}

#endif
