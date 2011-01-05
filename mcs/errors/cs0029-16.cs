// CS0029: Cannot implicitly convert type `string' to `int'
// Line: 28


delegate string funcs (string s);
delegate int funci (int i);

class X
{
	static void Foo (funci fi)
	{
	}
	
	static void Foo (funcs fs)
	{
	}

	static void Main ()
	{
		Foo (x => {
			int a = "a";
			return 2;
		});
	}
}