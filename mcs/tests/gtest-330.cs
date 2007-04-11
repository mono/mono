using System;
using System.Collections;
using System.Collections.Generic;

public class BaseCollection<T> : IEnumerable<T>
{
	protected List<T> items = new List<T> ();

	IEnumerator<T> IEnumerable<T>.GetEnumerator ()
	{
		return items.GetEnumerator ();
	}

        IEnumerator IEnumerable.GetEnumerator ()
        {
		return items.GetEnumerator ();
        }
}
 
public class BaseIntList<T> : BaseCollection<T>
{
}

public class IntList : BaseIntList<int>
{
}

class X
{
        public static void Main ()
        {
		IntList list = new IntList ();
		foreach (int i in list) {
			Console.WriteLine (i);
		}
        }
}
