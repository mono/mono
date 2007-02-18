// CS0305: Using the generic type `A.B<T>' requires `1' type argument(s)
// Line: 12

class A 
{
	class B<T> 
	{ 
	}
	
	static void Main() 
	{
		B b = new B<A>();
	}
}
