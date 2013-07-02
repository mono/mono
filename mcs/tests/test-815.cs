using System.Runtime.CompilerServices;

interface IS
{
	[IndexerName ("Hello")]
	int this[int index] { get; }
}

class D : IS
{
	[IndexerName ("DUUU")]
	public int this[int index] {
		get {
			return 1;
		}
	}
}

static class X
{
	public static int Main ()
	{
		IS a = new D ();
		int r = a[1];

		D d = new D ();
		r = d[2];

		return 0;
	}
}

