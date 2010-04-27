// Compiler options: -warnaserror -warn:4

using System;

interface IList 
{
	int Count { get; set; }
}

interface ICounter
{
	void Count (int i);
}

interface IEx
{
	void Count (params int[] i);
}

interface IListCounter: IEx, IList, ICounter
{
}

class Test
{
	static void Foo (IListCounter t)
	{
		t.Count (1); 
	}
	
	public static void Main ()
	{
	}
}
