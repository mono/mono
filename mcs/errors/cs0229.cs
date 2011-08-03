// CS0229: Ambiguity between `IList.Count' and `ICounter.Count'
// Line: 24

using System;

interface IList 
{
	int Count { set; }
}

interface ICounter 
{
	int Count { set; }
}

interface IListCounter: IList, ICounter
{
}

class Test
{
	static void Foo (IListCounter t)
	{
		t.Count = 9; 
	}
}