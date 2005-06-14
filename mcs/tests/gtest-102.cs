using System;

class A<T>
	where T: IComparable
{
}

class B<U,V>
	where U: IComparable
	where V: A<U>
{
}

class Driver
{
	public static void Main ()
	{
		A<int> a_int;
		B<int,A<int>> b_stuff;
	}
}
