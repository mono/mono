// Compiler options: /t:library
using System;

public class Foo<T>
{
	public int Test (int index)
	{
		Console.WriteLine ("Test 1: {0}", index);
		return 1;
	}

	public int Test (T index)
	{
		Console.WriteLine ("Test 2: {0}", index);
		return 2;
	}
}
