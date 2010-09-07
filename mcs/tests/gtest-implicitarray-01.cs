
// Tests implicitly typed arrays

public class Test
{
	static int Main ()
	{
		string[] array = new [] { "Foo", "Bar", "Baz" };
		foreach (string s in array)
			if (s.Length != 3)
				return 1;

		string[] s1 = new[] { null, "a", default (string) };
		double[] s2 = new[] { 0, 1.0, 2 };
			
		var a1 = new[] { null, "a", default (string) };
		var a2 = new[] { 0, 1.0, 2 }; 
		var a3 = new[] { new Test (), null }; 
		var a4 = new[,] { { 1, 2, 3 }, { 4, 5, 6 } };
		var a5 = new[] { default (object) };
		var a6 = new[] { new [] { 1, 2, 3 }, new [] { 4, 5, 6 } };
		
		const byte b = 100;
		var a7 = new[] { b, 10, b, 999, b };
		
		var a8 = new[] { new Test (), 22,  new object(), string.Empty, null };
		
		return 0;
	}
}
