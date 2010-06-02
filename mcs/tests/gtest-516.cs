using System;

interface I<T> : IA<T>
{
}

interface IA<T>
{
	T this [int i] { set; }
}

class B
{
	I<int> i;
	
	void Foo ()
	{
		i [10] = 1;
	}
	
	public static void Main ()
	{
	}
}
