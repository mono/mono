interface ICollectionValue
{
	int Count { get; }
}

interface ISCGCollection
{
	int Count { get; }
}

interface ICollection : ISCGCollection, ICollectionValue
{
	new int Count { get; }
}

interface ISequenced : ICollection
{
}

class Test : ISequenced
{
	public int Count { get { return 0; } }
}

static class Maine
{
	public static int Main ()
	{
		ISequenced t = new Test ();
		if (t.Count != 0)
			return 1;
		return 0;
	}
}
