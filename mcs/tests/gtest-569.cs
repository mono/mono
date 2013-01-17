using System;

class C
{
	public static void Main ()
	{
		new TreeMap<int> ();
	}
}

public class TreeMap<T>
{
	class Entry<U>
	{
		internal TreeMap<U>.Entry<int> field;
	}
}

