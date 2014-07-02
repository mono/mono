// CS0151: A switch expression of type `X?' cannot be converted to an integral type, bool, char, string, enum or nullable type
// Line: 15

struct X 
{
    public static implicit operator int (X x)
    {
        return 1;
    }

	static void Main ()
	{
		X? x = null;
		switch (x) {
		default:
			break;
		}
	}
}
