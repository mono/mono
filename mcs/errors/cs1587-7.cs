// Compiler options: -doc:dummy.xml -warnaserror -warn:2
using System;

namespace TopNS
{
/// invalid comment on using alias directive inside namespace.
using Hoge = System.Xml.XmlDocument;

	enum Foo {
	}
}
