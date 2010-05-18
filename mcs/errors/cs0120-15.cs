// CS0120: An object reference is required to access non-static member `MainClass.Test'
// Line: 20

public class Test
{
	public void Foo ()
	{
	}
}

public class MainClass
{
	public Test Test
	{
		get;
		set;
	}

	public static void Main (string[] args)
	{
		Test.Foo ();
	}
}
