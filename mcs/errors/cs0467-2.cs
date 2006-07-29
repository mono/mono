// cs0467-2.cs: Ambiguity between method `ICounter.Count()' and non-method `ICollection.Count'. Using method `ICounter.Count()'
// Line: 30
// Compiler options: -warnaserror -warn:2

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