// CS1501: No overload for method `Call' takes `0' arguments
// Line: 8

class A<T> where T : CB, IA
{
	void Foo (T t)
	{
		t.Call ();
	}
}

class CB : CA
{
}

class CA
{
	public void Call (int arg)
	{
	}
}

interface IA
{
	void Call (bool arg, int arg2);
}
