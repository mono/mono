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
}
