// cs1593: Using different kind of signatures for delegate invocation.
// Line: 11
//
// The delegate has an explicit signature with 0 arguments, so it 
// can not be assigned to a delegate with one argument.
//
delegate void D (int x);

class X {
	static void Main ()
	{
		D d2 = delegate () {};
	}
}
