struct C<T>
{
	public static implicit operator C<T> (T value)
	{
		return default (C<T>);
	}
}

class C
{
	public static void Main ()
	{
		C<bool?> p = true;
		C<int?> p2 = (int?)null;
	}
}
