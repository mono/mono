// CS0655: `a' is not a valid named attribute argument because it is not a valid attribute parameter type
// Line: 11

using System;

class TestAttribute : Attribute
{
	public int[,] a;
}

[Test (a = null)]
class C
{
}