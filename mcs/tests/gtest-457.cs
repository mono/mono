using System;

class Program
{
	class C
	{
	}

	void Foo<T> () where T : C
	{
	}

	static int Main ()
	{
		return 0;
	}
}
