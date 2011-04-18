// CS1587: XML comment is not placed on a valid language element
// Line: 10
// Compiler options: -doc:dummy.xml -warnaserror -warn:2

using System;

namespace TopNS
{
	enum Enum3 {
		Foo /** invalid comment between enum identifier and comma */,
		Bar
	}
}
