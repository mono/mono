// cs0122-6.cs: `Test.Foo.IBar' is inaccessible due to its protection level
// Line: 11

public class Test
{
	public class Foo
	{
		protected interface IBar {}
	}
	
	private class Bar : Foo.IBar
	{
	}

	public static void Main () {}
}

