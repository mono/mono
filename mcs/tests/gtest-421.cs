using System;

public class OneOff
{
	public static int Main ()
	{
		double[] darray = { 1.0, 2.0, 3.0 };
		double[] clone = OneOff.Clone (darray);
		Console.WriteLine (clone.Length);
		return clone.Length == 3 ? 0 : 1;
	}

	private static T[] Clone<T> (T[] o)
	{
		if (o == null)
			return null;
		Type t = typeof (T);
		if (t.IsValueType)
			return (T[]) o.Clone ();
		else if (t.IsArray && (t.GetElementType ().IsValueType || t.GetElementType () == typeof (string))) {
			T[] copy = new T[o.Length];
			for (int i = 0; i < o.Length; i++)
				copy[i] = (T) (o[i] as Array).Clone ();
			return copy;
		} else
			throw new ArgumentException ("oops");
	}
}
