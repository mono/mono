public class Foo
{
	public static class Nested
	{
		public static int Bar {
			get {
				// bar should be referring to Foo.Nested.bar here
				return bar.value;
			}
		} 

		static class bar {
			// The desired
			public const int value = 3;
		}
	}

	// The undesired
	int bar; 

	public static void Main ()
	{
	}
}
