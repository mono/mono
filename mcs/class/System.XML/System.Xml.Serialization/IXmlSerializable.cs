//
// System.Xml.Serialization.IXmlSerializable.cs
//
// Author: 
//    Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml.Schema;

namespace System.Xml.Serialization {
	public interface IXmlSerializable {

		XmlSchema GetSchema ();
		void ReadXml (XmlReader reader);
		void WriteXml (XmlWriter writer);
	}
}
