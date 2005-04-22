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
