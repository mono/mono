// CS8157: Cannot return `r' by reference because it was initialized to a value that cannot be returned by reference
// Line: 11

struct S
{
	int i;

	ref int M ()
	{
		ref int r = ref i;
		return ref r;
	}
}