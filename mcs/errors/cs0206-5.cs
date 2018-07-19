// CS0206: A property, indexer or dynamic member access may not be passed as `ref' or `out' parameter
// Line: 10

class X
{
	static int P { get; set; }

	static void Main ()
	{
		ref int rl = ref P;
	}
}