// Compiler options: -doc:dummy.xml -warnaserror -warn:2
using System;

namespace TopNS
{
	class Foo
	{
		Foo (string foo)
		{
			/** incorrect doccomment*/
		}
	}
}
