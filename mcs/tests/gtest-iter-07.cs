using System;
using System.Collections.Generic;

public class Test
{
	public static int Main ()
	{
		MySystem mySystem = new MySystem ();
		return 0;
	}

	public static void TestFunction (IEnumerable<string> items)
	{
		List<string> newList;
		Console.WriteLine ("1");
		newList = new List<string> (items);
		Console.WriteLine ("2");
		newList = new List<string> (items);
	}
}

public class MySystem
{
	private List<string> _items = new List<string> ();

	public MySystem ()
	{
		_items.Add ("a");
	}

	public IEnumerable<string> Items
	{
		get
		{
			foreach (string i in _items) {
				Console.WriteLine (i);
				yield return i;
			}
		}
	}
}
