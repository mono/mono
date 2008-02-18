

using System;
using System.Collections.Generic;
using System.Linq;

class Data
{
	public int Key;
	public string Value;
}

class Join
{
	public static int Main ()
	{
		Data[] d1 = new Data[] { new Data () { Key = 1, Value = "First" } };
		Data[] d2 = new Data[] { 
			new Data () { Key = 1, Value = "Second" },
			new Data () { Key = 1, Value = "Third" }
		};

		
		var e = from a in d1
			join b in d2 on a.Key equals b.Key
			select new { Result = a.Value + b.Value };

		var res = e.ToList ();
		if (res.Count != 2)
			return 1;
		
		if (res [0].Result != "FirstSecond")
			return 2;
			
		if (res [1].Result != "FirstThird")
			return 3;
			
		e = from Data a in d1
			join b in d2 on a.Key equals b.Key
			where b.Value == "Second"
			select new { Result = a.Value + b.Value };
			
		res = e.ToList ();
		if (res.Count != 1)
			return 4;
		
		if (res [0].Result != "FirstSecond")
			return 5;		
			
		// Explicitly typed
		e = from Data a in d1
            join Data b in d2 on a.Key equals b.Key
            select new { Result = a.Value + b.Value };
		
		res = e.ToList ();
		if (res.Count != 2)
			return 10;
		
		if (res [0].Result != "FirstSecond")
			return 11;
			
		if (res [1].Result != "FirstThird")
			return 12;
		
		var e2 = from Data a in d1
			join b in d2 on a.Key equals b.Key
			group b by a.Key;

		var res2 = e2.ToList ();
		if (res2.Count != 1)
			return 20;
		
		if (res2 [0].Key != 1)
			return 21;
			
		Console.WriteLine ("OK");
		return 0;
	}
}

