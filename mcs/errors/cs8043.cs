// CS8043: `S.S(long)': Structs with primary constructor cannot specify default constructor initializer
// Line: 6

struct S (int x)
{
	public S (long x)
		: this ()
	{
	}
}
