//
// XmlTypeMapMemberAttribute.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Xml.Schema;

namespace System.Xml.Serialization
{
	// XmlTypeMapMemberAttribute
	// A member of a class that must be serialized as an attribute

	internal class XmlTypeMapMemberAttribute: XmlTypeMapMember
	{
		string _attributeName;
		string _dataType;
		string _namespace = "";
		XmlSchemaForm _form;
		XmlTypeMapping _mappedType;

		public XmlTypeMapMemberAttribute()
		{
		}

		public string AttributeName
		{
			get { return _attributeName; }
			set { _attributeName = value; }
		}

		public string Namespace
		{
			get { return _namespace; }
			set { _namespace = value; }
		}

		public string DataTypeNamespace
		{
			get 
			{ 
				if (_mappedType == null) return XmlSchema.Namespace;
				else return _mappedType.Namespace;
			}
		}

		public XmlSchemaForm Form 
		{
			get { return _form; }
			set { _form = value; }
		}

		public XmlTypeMapping MappedType
		{
			get { return _mappedType; }
			set { _mappedType = value; }
		}
	}
}
