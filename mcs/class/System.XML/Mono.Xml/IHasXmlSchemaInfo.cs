//
// Mono.Xml.IHasXmlSchemaInfo.cs
//
// Author:
//	Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// (C) 2003 Atsushi Enomoto
//
// This interface is used to support XmlSchema type information.
//
using System.Xml;

namespace Mono.Xml
{
	public interface IHasXmlSchemaInfo
	{
		object SchemaType { get; }
	}
}
