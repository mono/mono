using System;

class G<T>
{
}

public class Program
{
	static void Test<T> (G<G<T>> g)
	{
	}

	static void Main ()
	{
		dynamic d = new G<G<int>> ();
		Test (d);
	}
}