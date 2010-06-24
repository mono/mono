// CS0278: `IListCounter' contains ambiguous implementation of `enumerable' pattern. Method `IList.GetEnumerator()' is ambiguous with method `ICounter.GetEnumerator()'
// Line: 26
// Compiler options: -warnaserror -warn:2

using System;
using System.Collections;

interface IList 
{
	IEnumerator GetEnumerator ();
}

interface ICounter 
{
	IEnumerator GetEnumerator ();
}

interface IListCounter: IList, ICounter
{
}

class Test
{
	static void Foo (IListCounter t)
	{
		foreach (var e in t)
		{
		}
	}
}
