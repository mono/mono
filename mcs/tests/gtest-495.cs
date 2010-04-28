class Repro
{
	class Outer
	{
		public class Inner<T> where T : class
		{
			public static T[] Values;
		}
	}
	static void Main ()
	{
		Outer.Inner<string>.Values = new string[0];
	}
}
