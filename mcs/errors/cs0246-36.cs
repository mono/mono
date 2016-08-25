// CS0246: The type or namespace name `Foo' could not be found. Are you missing an assembly reference?
// Line: 8

class Crashy
{
	void Call (System.Action<object> action) { }

	public void DoCrash () => Call (f => f as Foo);
}
