// CS1587: XML comment is not placed on a valid language element
// Line: 9
// Compiler options: -doc:dummy.xml -warnaserror -warn:2

using System;

namespace TopNS
{
	[Flags/** here is also incorrect comment */]
	enum Enum2 {
	}
}
