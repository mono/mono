interface I<T>
{
}

interface IB<T> : I<string>
{
	
}

struct S
{
	class P
	{
	}
	
	public class C : IB<P>
	{
	}
}

class M
{
	static void Test<T> (I<T> iface)
	{
	}

	static void Test<T> (IB<T> iface)
	{
	}

	static void Main ()
	{
		Test (new S.C ());
	}
}
