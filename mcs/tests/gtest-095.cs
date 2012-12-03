using System;

public interface IDirectedEnumerable<T>
{
	IDirectedEnumerable<T> Backwards();
}

public interface IDirectedCollectionValue<T> : IDirectedEnumerable<T>
{
	new IDirectedCollectionValue<T> Backwards();
}

public class GuardedCollectionValue<T> : IDirectedCollectionValue<T>
{
	IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards ()
	{
		return this;
	}

	public IDirectedCollectionValue<T> Backwards ()
	{
		return this;
	}
}

public interface ISequenced<T> : IDirectedCollectionValue<T>
{
}

public class GuardedSequenced<T>
{
	ISequenced<T> sequenced;

	public IDirectedCollectionValue<T> Test ()
	{
		return sequenced.Backwards();
	}
}

class X
{
	public static void Main ()
	{ }
}
