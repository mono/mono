using System;
using System.Collections.Generic;

internal class C<TFirst>
{
	internal struct VSlot<T>
	{
		public readonly T Value;

		public VSlot (T value)
		{
			Value = value;
		}
	}

	internal IEnumerable<V> GetEnumerable<V> (IEnumerable<VSlot<V>> input)
	{
		foreach (var v in input)
			yield return v.Value;
	}
}

class C
{
	public static int Main ()
	{
		var c = new C<long> ();
		string value = null;
		foreach (var v in c.GetEnumerable (new[] { new C<long>.VSlot<string> ("foo") })) {
			value = v;
		}

		if (value != "foo")
			return 1;

		return 0;
	}
}
