// CS0122: `M.Test<S.P>(I<S.P>)' is inaccessible due to its protection level
// Line: 27

interface I<T>
{
}

struct S
{
	class P
	{
	}
	
	public class C : I<P>
	{
	}
}

class M
{
	static void Test<T>(I<T> iface)
	{
	}

	static void Test()
	{
		Test (new S.C ());
	}
}
