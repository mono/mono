// CS0121: The call is ambiguous between the following methods or properties: `IList.Count()' and `ICounter.Count()'
// Line: 29

using System;

interface IList 
{
	int Count ();
}

interface ICounter 
{
	int Count ();
}

interface ICollection
{
	int Count { set; }
}

interface IListCounter: IList, ICounter, ICollection
{
}

class Test
{
	static void Foo (IListCounter t)
	{
		t.Count ();
	}
}
