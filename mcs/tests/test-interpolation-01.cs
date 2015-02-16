using System;

public class Test
{
	public static int Main ()
	{
		string s;
		int res = 5;

		s = $"Result {res}";
		if (s != "Result 5")
			return 1;

		s = $"Result {   res   }  ";
		if (s != "Result 5  ")
			return 2;

		s = $"Result { res, 7 }";
		if (s != "Result       5")
			return 3;

		s = $"";
		if (s != "")
			return 4;

		s = $"Result { res } { res }++";
		if (s != "Result 5 5++")
			return 5;

		s = $"Result {{ res }} { res }";
		if (s != "Result { res } 5")
			return 6;

		s = $"Result { res /* foo */ }";
		if (s != "Result 5")
			return 7;

		s = $"{{0}}";
		if (s != "{0}")
			return 8;

		s = $"{300:X}";
		if (s != "12C")
			return 9;

		s = $"{200:{{X+Y}}}";
		if (s != "{{X+Y}")
			return 10;

		s = $"{ $"{ res }" }";
		if (s != "5")
			return 11;

		s = $" \u004d ";
		if (s != " M ")
			return 12;

		byte b = 3;
		s = $"b = {(int)b}";
		if (s != "b = 3")
			return 13;

		Console.WriteLine ("ok");
		return 0;
	}
}