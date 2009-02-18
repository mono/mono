using System;
using System.Collections.Generic;

class Test
{
	public static int Counter = 0;
	
	public struct Nested : IDisposable
	{
		public int Current { get { return 1; } }
		public bool MoveNext ()
		{
			return false;
		}
		
		public void Reset ()
		{
		}

		void IDisposable.Dispose()
		{
			Counter++;
		}

		public void Dispose()
		{
			throw new ApplicationException ("error");
		}
	}

	public Nested GetEnumerator ()
	{
		return new Nested ();
	}
}

public static class Program
{
	public static int Main ()
	{
		Test t = new Test ();
		
		foreach (int i in t) {
		}
		
		if (Test.Counter != 1)
			return 1;
		
		return 0;
	}
}
