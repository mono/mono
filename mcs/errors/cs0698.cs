// cs0698.cs: A generic type cannot derive from `System.Attribute' because it is an attribute class
// Line: 6

using System;

class Stack<T> : Attribute
{ }

class X
{
	static void Main ()
	{ }
}
