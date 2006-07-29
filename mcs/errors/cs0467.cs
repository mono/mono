// cs0467.cs: Ambiguity between method `ICounter.Count(int)' and non-method `IList.Count'. Using method `ICounter.Count(int)'
// Line: 30
// Compiler options: -warnaserror -warn:2

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
}