using System;
using System.Collections.Generic;

public class A<T>
{
	public class B
	{
		private List<Action<T[]>> l = new List<Action<T[]>>();

		protected void W<R>(string s, Func<T, R> f)
		{
			Action<T[]> w = delegate(T[] d)
			{
				R[] r = new R[d.Length];
				for (int i = 0; i < d.Length; i++)
					r[i] = f(d[i]);
			};
			l.Add(w);
		}
	}
}

public class B
{
	public static void Main ()
	{
	}
}