// cs0198: can not assign to static readonly variable outside static constructor
// Line: 8
class X {
	static readonly int a;

	static void Y ()
	{
		a = 1;
	}
}

	
