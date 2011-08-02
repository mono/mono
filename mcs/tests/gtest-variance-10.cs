using System;

public interface I<out T>
{
	int Count{ get; }
}

class Foo {}

public class Test : I<string>, I<Foo>
{
	int I<string>.Count
	{
		get { return 1; }
	}
	
	int I<Foo>.Count
	{
		get { return 2; }
	}
}

public static class Program
{
	static int Main ()
	{
		var col = new Test();

		var test = (I<object>)(object) col;
		if (test.Count != 1)
			return 1;
		
		return 0;
	}
}
