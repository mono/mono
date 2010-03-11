// CS0460: `C.I.Test<C>()': Cannot specify constraints for overrides and explicit interface implementation methods
// Line: 11

interface I
{
	void Test<T>() where T : new ();
}

class C : I
{
	void I.Test<C>() where C : class
	{
	}
}
