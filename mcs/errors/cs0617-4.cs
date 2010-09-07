// CS0617: `Foo' is not a valid named attribute argument. Named attribute arguments must be fields which are not readonly, static, const or read-write properties which are public and not static
// Line: 11

using System;

public sealed class FooAttribute : Attribute
{
	internal int Foo;
}

[Foo (Foo = 1)]
public class Tests
{
}
