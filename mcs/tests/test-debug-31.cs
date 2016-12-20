using System;

namespace cp534534
{
	class MainClass
	{
		public static void Main ()
		{
			var array = new string[] { "a", "b", "c" };
			
			foreach (var item in array)
			{
				Console.WriteLine (item);
			}

			foreach (var item1 in array) {
				Console.WriteLine (item1);
			}

			foreach (var item2 in array) {
				Console.WriteLine (item2);
			}
		}
	}
}