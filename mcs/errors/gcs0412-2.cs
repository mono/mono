// CS0412: The type parameter name `T' is the same as local variable or parameter name
// Line: 8

using System;

interface I
{
	T Foo<T>(IComparable T);
}
