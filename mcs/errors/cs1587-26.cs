// Compiler options: -doc:dummy.xml -warnaserror -warn:2
using System;

namespace TopNS
{
	class Foo
	{
		public /** invalid comment in field decl */ int field;
	}
}
