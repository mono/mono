//
// IXmlSchemaInfo.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell, Inc.
//

#if NET_2_0
namespace System.Xml.Schema
{
	public interface IXmlSchemaInfo
	{
		bool IsDefault { get; }

		bool IsNil { get; }

		XmlSchemaSimpleType MemberType { get; }

		XmlSchemaAttribute SchemaAttribute { get; }

		XmlSchemaElement SchemaElement { get; }

		XmlSchemaType SchemaType { get; }

		XmlSchemaValidity Validity { get; }
	}
}
#endif
