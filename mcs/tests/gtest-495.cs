class Repro
{
	class Outer
	{
		public class Inner<T> where T : class
		{
			public static T[] Values;
		}
	}
	public static void Main ()
	{
		Outer.Inner<string>.Values = new string[0];
	}
}
