// Compiler options: -r:test-921-lib.dll

using Reference;

class A
{
	void Foo (IA a)
	{
		IB b = a.Equals;
	}

	public static void Main ()
	{
	}
}