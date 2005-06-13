// cs1587.cs: XML comment is placed on an invalid language element which can not accept it.
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
