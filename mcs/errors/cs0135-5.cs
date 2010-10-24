// CS0135: `bar' conflicts with a declaration in a child block
// Line: 13

public class Foo
{
	public static class Nested
	{
		static int bar ()
		{
			return 0;
		}
		
		public static void Bar ()
		{
			var i = bar ();
			{
				bool bar = false;
			}
		} 
	}
}

