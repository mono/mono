// Compiler options: -doc:dummy.xml -warnaserror -warn:2
using System;

namespace TopNS
{
/// invalid comment on using directive inside namespace.
using System.Xml;

	enum Foo {
	}
}
