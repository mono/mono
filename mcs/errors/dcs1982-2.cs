// CS01982: An attribute argument cannot be dynamic expression
// Line: 6

using System;

[A((dynamic) null)]
public class A : Attribute
{
	public A (Type arg)
	{
	}
}
