// CS0698: A generic type cannot derive from `X' because it is an attribute class
// Line: 6

using System;

class Stack<T> : X
{ }

class X : Attribute
{
}
