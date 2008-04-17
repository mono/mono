// CS0687: A namespace alias qualifier `foo::WriteLine' did not resolve to a namespace or a type
// Line: 9

using foo = System.Console;

class X {
	static void Main ()
	{
		foo::WriteLine ("hello");
	}
}
