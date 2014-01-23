
// Tests collection initialization

using System;
using System.Collections;
using System.Collections.Generic;

public class Test
{
	class Wrap
	{
		ArrayList numbers = new ArrayList ();
		public int Id;
		public Wrap Next;
		
		public Wrap ()
		{
			Next = new Wrap (100);
		}
		
		public Wrap (int i)
		{
			Next = this;
		}
		
		public ArrayList Numbers { 
			get { 
				return numbers;
			}
		}
	}
	
	static void TestList (List<int> list, int expectedCount)
	{
		if (list.Count != expectedCount)
			throw new ApplicationException (expectedCount.ToString ());
		
		foreach (int i in list)
			Console.WriteLine (i);
	}
	
	public static int Main ()
	{
		ArrayList collection = new ArrayList { "Foo", null, 1 };
		if (collection.Count != 3)
			return 1;
			
		if ((string)collection [0] != "Foo")
			return 2;
			
		if ((int)collection [2] != 1)
			return 3;
					
		List<string> generic_collection = new List<string> { "Hello", "World" };
		foreach (string s in generic_collection)
			if (s.Length != 5)
				return 4;
		
		List<Wrap> a = null;
		a = new List<Wrap> () {		
			new Wrap (0) {
				Id = 0,
				Numbers = { 5, 10 },
				Next = { Id = 55 }
			},
			new Wrap {
				Id = 1,
				Numbers = { collection, }
			},
			new Wrap {
				Id = 2,
				Numbers = { },
			},
			null
		};
		
		if (a.Count != 4)
			return 5;
		
		if ((int)a [0].Numbers [1] != 10)
			return 6;
		
		new List<int> { 1, 2, 3, 4 };
		TestList (new List<int> { 1, 2, 3, 4 }, 4);
		
		new List<int> { };
		
		Console.WriteLine ("OK");
		return 0;
	}
}

