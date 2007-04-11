// CS0452: The type `int' must be a reference type in order to use it as type parameter `T' in the generic type or method `TestClass<T>'
// Line: 23
using System;

public class TestClass<T> where T : class
{
	static public T meth()
	{
		return null;
	}

	static public T Value;
}			
	
public class Test
{
	public Test()
	{
	}
		
	static public void Main()
	{
		int i = TestClass<int>.meth();
		Console.WriteLine (i);
	}
}
