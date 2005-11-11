 // gcs0309.cs: The type `U' must be convertible to `System.IComparable' in order to use it as parameter `T' in the generic type or method `A<T>'
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
