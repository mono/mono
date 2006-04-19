// cs0191.cs: A readonly field `X.a' cannot be assigned to (except in a constructor or a variable initializer)
// Line: 8
class X {
	readonly int a;

	void Y ()
	{
		a = 1;
	}
}

	
