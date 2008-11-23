// Compiler options: -unsafe

static class BugClass
{
	unsafe delegate void Foo (void* dummy);
	static unsafe void FooImplementation (void* dummy)
	{
	}
	
	static unsafe Foo Bar = new Foo (FooImplementation);
}

class Bug
{
	unsafe int*[] data = new int*[16];
	
	public static void Main ()
	{
	}
}
