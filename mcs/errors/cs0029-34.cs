// CS0029: Cannot implicitly convert type `string' to `MyTypeImplicitOnly?'
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
