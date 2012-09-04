using System.Collections.Generic;

namespace Test
{
	public static class Program
	{
		public static void Main ()
		{
			foreach (var a in Test ())
				Test ();
		}

		public static IEnumerable<object> Test ()
		{
			lock (new object ()) {
				yield return null;
				yield break;
			}
		}
	}
}