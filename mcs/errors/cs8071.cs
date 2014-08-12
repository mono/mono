// CS8071: Type arguments are not allowed in the nameof operator
// Line: 16

class G<T>
{
	class N
	{
		public int Foo;
	}
}

class Test
{
	public static void Main ()
	{
		var n = nameof (G<int>.N.Foo);
	}
}
