// cs0467-3.cs: Ambiguity between method `ICounter.Count()' and non-method `ICollection.Count'. Using method `ICounter.Count()'
// Line: 34
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

interface IListCounterNew : IListCounter
{
}

class Test
{
	static void Foo (IListCounterNew t)
	{
		t.Count ();
	}
}