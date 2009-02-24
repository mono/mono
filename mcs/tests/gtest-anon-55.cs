using System;

public sealed class Thing<TFirst> where TFirst : class
{
	public static Thing<TFirst> Create<TSecond> (Func<TFirst, TSecond> fn)
		where TSecond : class
	{
		return new Thing<TFirst> (
			delegate (TFirst item) {
				TSecond foo = item == null ? null : fn (item);
				Console.WriteLine (foo);
			});
	}

	public void SomeAction ()
	{
		_fn (null);
	}

	private Thing (Action<TFirst> fn)
	{
		_fn = fn;
	}

	Action<TFirst> _fn;
}

public static class Program
{
	public static void Main ()
	{
		var foo = Thing<object>.Create (x => x);
		foo.SomeAction ();
	}
}