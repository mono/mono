//
// System.Xml.Serialization.SchemaTypes
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

namespace System.Xml.Serialization
{
	internal enum SchemaTypes {
		NotSet = 0,
		Primitive,
		Enum,
		Array,
		Class,
		XmlSerializable,
		XmlNode,
		Void
	}
}

