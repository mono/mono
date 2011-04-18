// CS1587: XML comment is not placed on a valid language element
// Line: 8
// Compiler options: -doc:dummy.xml -warnaserror -warn:2

using System;

/// invalid comment on using alias directive.
using Hoge = System.Xml.XmlDocument;

namespace TopNS
{

	enum Foo {
	}
}
