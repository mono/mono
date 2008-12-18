namespace Test
{
	public class Bar<T, U>
	{
		public void DoSomething<V> () where V : U
		{
		}
	}

	public class Baz
	{
		public void GetInTroubleHere ()
		{
			var bar = new Bar<string, string> ();
			bar.DoSomething<string> ();
		}
		
		public static void Main ()
		{
		}
	}
}

