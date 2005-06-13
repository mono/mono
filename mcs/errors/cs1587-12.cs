// cs1587.cs: XML comment is placed on an invalid language element which can not accept it.
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
