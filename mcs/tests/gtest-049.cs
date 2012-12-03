// Important test: compare this to gcs0408-*.cs; these are the allowed cases.

class X<T>
{
	void A (T t)
	{ }

	void A (T[] t)
	{ }

	void A (T[,] t)
	{ }

	void A (T[][] t)
	{ }

	void B (T[] t)
	{ }

	void B (int t)
	{ }

	void C (T[] t)
	{ }

	void C (T[,] t)
	{ }

	void C (int[,,] t)
	{ }

	void D (int x, T y)
	{ }

	void D (T x, long y)
	{ }
}

class Foo
{
	public static void Main ()
	{ }
}
