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
