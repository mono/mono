

using System;
using System.Collections.Generic;
using System.Linq;

class OrderByTests
{
	class Data
	{
		public int ID { get; set; }
		public string Name { get; set; }
		
		public override string ToString ()
		{
			return ID + " " + Name;
		}
	}
	
	public static int Main ()
	{
		int[] int_array = new int [] { 0, 1 };
		string[] string_array = new string[] { "f", "a", "z", "aa" };
		
		IEnumerable<int> e;
		
		e = from int i in int_array orderby i select i;
		
		List<int> l = new List<int> (e);
		if (l [0] != 0)
			return 1;
			
		if (l [1] != 1)
			return 2;

		e = from int i in int_array orderby i ascending select i;
		
		l = new List<int> (e);
		if (l [0] != 0)
			return 100;
			
		if (l [1] != 1)
			return 101;
		
		e = from i in int_array orderby i descending select i + 1;
		l = new List<int> (e);
		if (l [0] != 2)
			return 3;
			
		if (l [1] != 1)
			return 4;

		IEnumerable<string> s;
		s = from i in string_array orderby i select i;
		
		List<string> ls = new List<string> (s);
		
		if (ls [0] != "a")
			return 5;
			
		if (ls [1] != "aa")
			return 6;
				
		if (ls [2] != "f")
			return 7;
		
		if (ls [3] != "z")
			return 7;		
		
		s = from i in string_array orderby i.Length select i;
		
		// Multiple orderby
		Data[] data = new Data[] {
			new Data { ID = 10, Name = "bcd" },
			new Data { ID = 20, Name = "Abcd" },
			new Data { ID = 20, Name = "Ab" },
			new Data { ID = 10, Name = "Zyx" }
		};
		
		var de = from i in data orderby i.ID ascending, i.Name descending select i;
		
		List<Data> ld = new List<Data> (de);
		if (ld [0].Name != "Zyx")
			return 10;
		
		var de2 = from i in data orderby i.ID descending, i.Name ascending select i;
		ld = new List<Data> (de2);
		if (ld [0].Name != "Ab")
			return 11;
		
		var de3 = from i in data  
			where i.ID == 10
			orderby i.ID descending, i.Name ascending select i;
		ld = new List<Data> (de3);
		if (ld [0].Name != "bcd")
			return 12;
		
		var de4 = from i in data
			where i.ID == 20
			orderby i.Name group i by i.Name;
		
		var group_order = new List<IGrouping<string, Data>> (de4);
		ld = new List<Data>(group_order [0]);

		if (ld [0].Name != "Ab")
			return 13;
		
		ld = new List<Data>(group_order [1]);
		if (ld [0].Name != "Abcd")
			return 14;		
		
		Console.WriteLine ("OK");
		return 0;
	}
}
