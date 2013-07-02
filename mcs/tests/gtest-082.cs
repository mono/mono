using System;

public class Queue<T>
{
	protected class Enumerator
	{
		Queue<T> queue;

		public Enumerator (Queue<T> queue)
		{
			this.queue = queue;
		}
	}
}

class X
{
	public static void Main ()
	{ }
}
