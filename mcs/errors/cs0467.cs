// CS0467: Ambiguity between method `IMethod.Count()' and invocable non-method `IList.Count'. Using method group
// Line: 27
// Compiler options: -warn:2 -warnaserror

using System;

delegate void D (int i);

interface IList 
{
	D Count { get; }
}

interface IMethod
{
	int Count ();
}

interface IListCounter: IList, IMethod
{
}

class Test
{
	static void Foo (IListCounter t)
	{
		t.Count ();
	}
}
