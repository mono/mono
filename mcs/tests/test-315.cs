using System;

public interface IDirectedEnumerable
{
	IDirectedEnumerable Backwards();
}

public interface IDirectedCollectionValue : IDirectedEnumerable
{
	new IDirectedCollectionValue Backwards();
}

public class GuardedCollectionValue : IDirectedCollectionValue
{
	IDirectedEnumerable IDirectedEnumerable.Backwards ()
	{
		return this;
	}

	public IDirectedCollectionValue Backwards ()
	{
		return this;
	}
}

public interface ISequenced : IDirectedCollectionValue
{
}

public class GuardedSequenced
{
	ISequenced sequenced;

	public IDirectedCollectionValue Test ()
	{
		return sequenced.Backwards();
	}
}

class X
{
	public static void Main ()
	{ }
}
