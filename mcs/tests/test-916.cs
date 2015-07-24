public class Foo
{
	public static class Nested
	{
		static int bar ()
		{
			return 0;
		}
		
		public static void Main ()
		{
			var i = bar ();
			{
				bool bar = false;
			}
		} 
	}
}

