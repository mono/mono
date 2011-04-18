// CS0122: `A.x' is inaccessible due to its protection level
// Line: 16

public class A
{
	protected bool x = true;
	
	public A()
	{}
}

public class B
{
	public static void Main(string[] args)
	{
		if (new A().x)
		{
			System.Console.WriteLine("this should not compile");
		}
	}
}
