// Compiler options: -doc:dummy.xml -warnaserror -warn:1
using System;

/// <see cref="Test.Foo(int)"/> Test has Foo, but is property that has no args.
public class Test
{
	string Foo {
		get { return null; }
	}
}
