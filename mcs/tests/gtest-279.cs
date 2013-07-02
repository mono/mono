using System;
using System.Collections.Generic;

interface IFoo
{
	void Bar();
	IList<T> Bar<T>();
}

class Foo : IFoo
{
	public void Bar()
	{
		Console.WriteLine("Bar");
	}
	
	public IList<T> Bar<T>()
	{
		Console.WriteLine("Bar<T>");
		return null;
	}
}

class BugReport
{
	public static void Main(string[] args)
	{
		Foo f = new Foo();
		f.Bar();
		f.Bar<int>();
	}
}


