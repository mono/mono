using System;

class Program
{
	class C
	{
	}

	void Foo<T> () where T : C
	{
	}

	public static int Main ()
	{
		return 0;
	}
}
