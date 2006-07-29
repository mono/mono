using System;

interface IList 
{
	int Count ();
}

interface ICounter 
{
	int Count { set; }
}

interface IListCounter: IList, ICounter
{
}

interface IA
{
	int Value ();
}

interface IB : IA
{
	new int Value { get; }
}

interface IC : IB
{
	new int Value { get; }
}

interface IBB : IList, ICounter
{
}

interface ICC : IBB
{
}

class Test
{
	static void Main ()
	{
	}
	
	static void Foo (IListCounter t)
	{
		t.Count ();
	}
	
	void Foo2 (IC b)
	{
		int i = b.Value;
	}
	
	void Foo3 (ICC c)
	{
		c.Count ();
	}

}