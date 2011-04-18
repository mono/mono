// CS0122: `Test.Foo.Bar' is inaccessible due to its protection level
// Line: 11

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

