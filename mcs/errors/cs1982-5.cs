// CS01982: An attribute argument cannot be dynamic expression
// Line: 6

using System;

[A(typeof (Func<dynamic>))]
public class A : Attribute
{
	public A (Type arg)
	{
	}
}
