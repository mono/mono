// CS0837: The `as' operator cannot be applied to a lambda expression or anonymous method
// Line: 15


class X
{
	delegate void D ();
	
	static void Test (D d)
	{
	}
	
	static void Main ()
	{
		Test ((() => { }) as D);
	}
}
