// CS1750: Optional parameter expression of type `null' cannot be converted to parameter type `A'
// Line: 8

using System;

class GenericClass<T>
{
	public GenericClass (GenericClass<T> g = null, A a = null)
	{
	}
}

class DerivedClass<T> : GenericClass<T>
{
	public DerivedClass (GenericClass<T> g) : base(g)
	{
	}
}

public struct A
{
	public int Field;
}
