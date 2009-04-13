using System;

namespace Test
{
	public class Application
	{
		private static int ParseType(string inType)
		{
			switch (inType)
			{
				case "headless":
					return 0;

				case "headed":
				default:
					return 1;

				case "wedge":
					return 2;

				case "hi":
					return 3;

				case "she":
					return 4;

				case "sha2":
					return 5;
			}
		}

		public static int Main()
		{
			int result1 = ParseType("foo");
			Console.WriteLine (result1);
			if (result1 != 1)
				return 1;

			int result2 = ParseType("headed");
			Console.WriteLine (result2);
			if (result1 != result2)
				return 2;
			
			return 0;
		}
	}
}
