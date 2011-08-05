// Compiler options: -r:test-824-lib.dll
using System;

class Context : IZZZ
{
	public void Foo (IBBB command)
	{
	}

	public void Foo (IAAA query)
	{
		throw new System.NotImplementedException ();
	}
}

class Test : IAAA, IBBB
{
	static void Main ()
	{
		Test cmd = new Test ();
		IZZZ context = new Context ();
		context.Foo (cmd);
	}
}
