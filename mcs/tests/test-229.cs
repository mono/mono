using System;
using System.Collections;
using System.Collections.Specialized;

public class List : IEnumerable {

	int pos = 0;
	int [] items;
	
	public List (int i) 
	{
		items = new int [i];
	}
	
	public void Add (int value) 
	{
		items [pos ++] = value;
	}
	
	public MyEnumerator GetEnumerator ()
	{
		return new MyEnumerator(this);
	}
	
	IEnumerator IEnumerable.GetEnumerator ()
	{
		return GetEnumerator ();
	}
	
	public struct MyEnumerator : IEnumerator {
		
		List l;
		int p;
		
		public MyEnumerator (List l) 
		{
			this.l = l;
			p = -1;
		}
		
		public object Current {
			get {
				return l.items [p];
			}
		}
		
		public bool MoveNext() 
		{
			return ++p < l.pos;
		}

		public void Reset() 
		{
			p = 0;
		}
	}
}

public class UberList : List {
	public UberList (int i) : base (i)
	{
	}
	
	public static int Main(string[] args)
	{
		return One () && Two () && Three () ? 0 : 1;

	}
	
	static bool One ()
	{
		List l = new List (1);
		l.Add (1);
		
		foreach (int i in l)
			if (i == 1)
				return true;
		return false;
	}
	
	static bool Two ()
	{
		List l = new UberList (1);
		l.Add (1);
		
		foreach (int i in l)
			if (i == 1)
				return true;
		return false;
	}
	
	static bool Three ()
	{
		UberList l = new UberList (1);
		l.Add (1);
		
		foreach (int i in l)
			if (i == 1)
				return true;
		return false;
	}
}
