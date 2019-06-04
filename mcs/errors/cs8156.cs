// CS8156: An expression cannot be used in this context because it may not be returned by reference
// Line: 8

class Test
{
	ref int Foo ()
	{
		return ref 2;
	}
}