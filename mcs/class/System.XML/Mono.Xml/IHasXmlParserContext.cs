//
// Mono.Xml.IHasParserContext.cs
//
// Author:
//	Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// (C) 2003 Atsushi Enomoto
//
// This interface is used to support ResolveEntity for custom XmlReader.
//
using System.Xml;

namespace Mono.Xml
{
	public interface IHasXmlParserContext
	{
		XmlParserContext ParserContext { get; }
	}
}
