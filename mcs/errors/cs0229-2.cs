// CS0229: Ambiguity between `IList.Test' and `ICounter.Test'
// Line: 26

using System;

delegate void Foo ();

interface IList 
{
	event Foo Test;
}

interface ICounter 
{
	event Foo Test;
}

interface IListCounter: IList, ICounter
{
}

class Test
{
	static void Foo (IListCounter t)
	{
		t.Test += null;
	}
}