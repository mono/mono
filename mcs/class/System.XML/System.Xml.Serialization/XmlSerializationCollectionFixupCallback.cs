//
// System.Xml.Serialization.XmlSerializationCollectionFixupCallback.cs: 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.Xml.Serialization {
	
	[Serializable]
	public delegate void XmlSerializationCollectionFixupCallback (object collection, object collectionItems);
}

