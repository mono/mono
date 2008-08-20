// CS0121: The call is ambiguous between the following methods or properties: `Foo<int,int>.Test(int)' and `Foo<int,int>.Test(int)'
// Line: 23
using System;

public class Foo<T,U>
{
	public void Test (T index)
	{
		Console.WriteLine ("Test 1: {0}", index);
	}

	public void Test (U index)
	{
		Console.WriteLine ("Test 2: {0}", index);
	}
}

class X
{
	static void Main ()
	{
		Foo<int,int> foo = new Foo<int,int> ();
		foo.Test (3);
	}
}
