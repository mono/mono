// Compiler options: -doc:dummy.xml -warnaserror -warn:2
using System;

namespace TopNS
{
	[Flags/** here is also incorrect comment */]
	enum Enum2 {
	}
}
