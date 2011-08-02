// CS1580: Invalid type for parameter `1' in XML comment cref attribute `Foo(x)'
// Line: 7
// Compiler options: -doc:dummy.xml -warnaserror -warn:1

using System;
/// <seealso cref="Foo(x)"/>
public class Test
{
	int Foo ()
	{
		return 0;
	}
}
