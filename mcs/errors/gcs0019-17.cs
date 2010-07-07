// CS0019: Operator `==' cannot be applied to operands of type `T' and `U'
// Line: 12

interface I
{
}

class Program
{
	bool Test<T, U> (T t, U u) where T : I where U : I
	{
		return t == u;
	}
}
