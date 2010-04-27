using System;
using System.Collections;
using System.Collections.Generic;

class Program
{
	static int Main ()
	{
		Int32Collection src = new Int32Collection ();
		Int32Collection dest = new Int32Collection ();

		src.Add (5);
		src.Add (7);
		dest.Add (4);

		ReplaceContentsWith<Int32Collection> (src, dest);

		if (dest.Count != 2)
			return 1;
		if (dest[0] != 5)
			return 2;
		if (dest[1] != 7)
			return 3;

		return 0;
	}

	private static void ReplaceContentsWith<T> (T src, T dst)
		where T : Int32Collection
	{
		dst.Clear ();
		foreach (int value in src)
			dst.Add (value);
	}
}

class Int32Collection : IEnumerable
{
	List<int> list = new List<int> ();

	public int Count
	{
		get { return list.Count; }
	}

	public int this[int index]
	{
		get { return (int) list[index]; }
		set { list[index] = value; }
	}

	public void Add (int value)
	{
		list.Add (value);
	}

	public void Clear ()
	{
		list.Clear ();
	}

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return list.GetEnumerator ();
	}
}