// CS0201: Only assignment, call, increment, decrement, await, and new object expressions can be used as a statement
// Line: 13

class C<T>
{
	static T Test ()
	{
		return default (T);
	}
	
	public static void Main ()
	{
		Test ().Foo;
	}
}
