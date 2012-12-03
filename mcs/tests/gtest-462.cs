using System;

class Program
{
	public static int Main ()
	{
		Tester<int> t = new Tester<int> ();
		int r = t.Get (333);
		Console.WriteLine (r);
		if (r != 333)
			return 1;

		r = t.Get (222.12);
		Console.WriteLine (r);
		if (r != 0)
			return 2;

		return 0;
	}

	class Tester<T> where T : struct, IConvertible
	{
		public T Get (object data)
		{
			var val = data;
			if (val is T)
				return (T) val;
			return default (T);
		}
	}
}
