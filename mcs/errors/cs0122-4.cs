// CS0122: `FooAttribute.Foo' is inaccessible due to its protection level
// Line: 11
// This is bug #55970

using System;

public sealed class FooAttribute : Attribute {
	int Foo;
}

[Foo (Foo = 1)]
public class Tests {
	public static void Main () {
	}
}