// CS0135: `bar' conflicts with a declaration in a child block
// Line: 13

public class Foo
{
	public static class Nested
	{
		class bar
		{
			public static int value;
		}
		
		public static void Bar ()
		{
			{
				bool bar = false;
			}
			
			var i = bar.value;
		} 
	}
}

