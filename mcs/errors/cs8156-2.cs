// CS8156: An expression cannot be used in this context because it may not be returned by reference
// Line: 8

class X
{
	int Prop {
		get {
			return 1;
		}
	}

	ref int Test ()
	{
		return ref Prop;
	}
}