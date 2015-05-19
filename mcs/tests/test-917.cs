public class Foo
{
	public static class Nested
	{
		class bar
		{
			public static int value;
		}
		
		public static void Main ()
		{
			{
				bool bar = false;
			}
			
			var i = bar.value;
		} 
	}
}

