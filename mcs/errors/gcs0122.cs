// CS0122: `Foo.Print(this string)' is inaccessible due to its protection level
// Line: 19


using System;

static class Foo
{
	static void Print (this string s)
	{
	}
}

static class Program
{
	static void Main(string[] args)
	{
		string s = "Hello, world";
		Foo.Print(s);
	}
}