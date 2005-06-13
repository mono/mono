// cs0122-5.cs: `Test.Foo.Bar' is inaccessible due to its protection level

public class Test
{
	public class Foo
	{
		private class Bar {}
	}
	
	private class Bar : Foo.Bar
	{
	}

	public static void Main () {}
}

