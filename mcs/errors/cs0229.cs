// cs0229.cs: Ambiguity between `IList.Count' and `ICounter.Count (int)'
// Line: 30
using System;

interface IList {
	int Count { get; set; }
}

interface ICounter {
	void Count (int i);
}

interface IListCounter: IList, ICounter {}


class ListCounter : IListCounter {
	int IList.Count {
		get { Console.WriteLine ("int IList.Count.get"); return 1; }
		set { Console.WriteLine ("int IList.Count.set"); }
	}
	
	void ICounter.Count (int i)
	{
		Console.WriteLine ("int ICounter.Count (int i)");
	}
	
	static void Main ()
	{
		IListCounter t = new ListCounter ();
		t.Count (1); 
	}
}