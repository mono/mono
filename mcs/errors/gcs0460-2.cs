// CS0460: `C.Test<T>()': Cannot specify constraints for overrides and explicit interface implementation methods
// Line: 11

abstract class A
{
	protected abstract int Test<T>() where T : class;
}

class C : A
{
	protected override int Test<T>() where T : new()
	{
	}
}
