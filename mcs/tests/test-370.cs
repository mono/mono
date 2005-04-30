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

		public static void Main()
		{
			int result = ParseType("foo");
		}
	}
}
