// CS0555: User-defined operator cannot take an object of the enclosing type and convert to an object of the enclosing type
// Line: 6

struct S
{
	public static implicit operator S (S? s)
	{
		return new S ();
	}
}
