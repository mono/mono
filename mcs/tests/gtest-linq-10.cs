

using System;
using System.Collections.Generic;
using System.Linq;

class DataA
{
	public int Key;
	public string Text;
}

class DataB
{
	public int Key;
	public string Value;	
}

class GroupJoin
{
	public static int Main ()
	{
		DataA[] d1 = new DataA[] { new DataA () { Key = 1, Text = "Foo" }};
		DataB[] d2 = new DataB[] { new DataB () { Key = 2, Value = "Second" }};
		
		var e = from a in d1
			join b in d2 on a.Key equals b.Key into ab
			from x in ab.DefaultIfEmpty ()
			select new { a = x == default (DataB) ? "<empty>" : x.Value, b = a.Text };

		var res = e.ToList ();
		if (res.Count != 1)
			return 1;
		
		if (res [0].a != "<empty>")
			return 2;
			
		if (res [0].b != "Foo")
			return 3;
			
		// Explicitly typed
		e = from a in d1
			join DataB b in d2 on a.Key equals b.Key into ab
			from x in ab.DefaultIfEmpty ()
			select new { a = x == default (DataB) ? "<empty>" : x.Value, b = a.Text };
			
		foreach (var o in e)
			Console.WriteLine (o);
			
		res = e.ToList ();
		if (res.Count != 1)
			return 10;
		
		if (res [0].a != "<empty>")
			return 11;
			
		if (res [0].b != "Foo")
			return 12;
			
		var e2 = from a in d1
			join a in d2 on a.Key equals a.Key into ab
			select a;
		
		Console.WriteLine ("OK");
		return 0;
	}
}

