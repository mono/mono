// Compiler options: -doc:dummy.xml -warnaserror -warn:2
using System;

namespace TopNS
{
	class Foo
	{
		string this [string bar] {
			get { /** incorrect */ return ""; }
			set { }
		}
	}
}
