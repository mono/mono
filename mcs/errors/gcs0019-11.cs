// CS0019: Operator `+' cannot be applied to operands of type `null' and `MyTypeImplicitOnly?'
// Line: 13

struct MyTypeImplicitOnly
{
}

class C
{
	static void Main ()
	{
		MyTypeImplicitOnly? mt = null;
		mt = null + mt;
	}
}
