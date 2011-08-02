// CS0198: A static readonly field `X.a' cannot be assigned to (except in a static constructor or a variable initializer)
// Line: 8
class X {
	static readonly int a;

	static void Y ()
	{
		a = 1;
	}
}

	
