// CS0412: The type parameter name `T' is the same as `method parameter'
// Line: 8

using System;

interface I
{
	T Foo<T>(IComparable T);
}