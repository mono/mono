//
// System.Xml.XPath.XPathNodeType
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

namespace System.Xml.XPath
{
	public enum XPathNodeType
	{
		Root = 0,
		Element = 1,
		Attribute = 2,
		Namespace = 3,
		Text = 4,
		SignificantWhitespace = 5,
		Whitespace = 6,
		ProcessingInstruction = 7,
		Comment = 8,
		All = 9,
	}
}
