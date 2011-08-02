// CS1587: XML comment is not placed on a valid language element
// Line: 13
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
