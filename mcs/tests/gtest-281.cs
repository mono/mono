using System;
using System.Collections.Generic;

public class List {
        internal void AddRange<T>(ICollection<T> otherList) {
        }
}

public class Tests
{
	public static void Main ()
	{
		object[] args = new object [0];
		List l = new List ();
		l.AddRange (args);
	}
}
