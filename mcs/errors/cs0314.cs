// CS0314: The type `U' cannot be used as type parameter `T' in the generic type or method `A<T>'. There is no boxing or type parameter conversion from `U' to `System.IComparable'
// Line: 13

using System;

class A<T>
	where T: IComparable
{
}

class B<U,V>
	where V: A<U>
{
}

class Driver
{
	public static void Main ()
	{
	}
}
