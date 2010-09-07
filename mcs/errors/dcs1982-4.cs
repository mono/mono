// CS01982: An attribute argument cannot be dynamic expression
// Line: 6

using System;

[A(new dynamic [] { 8 })]
public class A : Attribute
{
	public A (object arg)
	{
	}
}
