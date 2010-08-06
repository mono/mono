// CS0106: The modifier `sealed' is not valid for this item
// Line: 6

struct S
{
	public sealed override int GetHashCode ()
	{
		return 1;
	}
}
