// CS0121: The call is ambiguous between the following methods or properties: `C.Foo(object, string)' and `C.Foo(int, object)'
// Line: 13

class C
{
	delegate void D (int x, string s);

	static void Foo (object o, string s) { }
	static void Foo (int x, object o) { }

	static void Main ()
	{
		D d = Foo;
	}
}
