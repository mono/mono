// Compiler options: -doc:dummy.xml -warnaserror -warn:2
using System;

namespace TopNS
{
	enum Enum3 {
		Foo /** invalid comment between enum identifier and comma */,
		Bar
	}
}
