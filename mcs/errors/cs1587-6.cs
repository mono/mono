// Compiler options: -doc:dummy.xml -warnaserror -warn:2
using System;

/// invalid comment on using alias directive.
using Hoge = System.Xml.XmlDocument;

namespace TopNS
{

	enum Foo {
	}
}
