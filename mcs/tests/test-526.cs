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

interface IM1
{
    void Add (int arg);
}

interface IM2 : IM1
{
    int Add (int arg, bool now);
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
    
	void Foo4 (IM2 im2)
	{
		im2.Add (2);
	}

}