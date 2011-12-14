// CS0687: The namespace alias qualifier `::' cannot be used to invoke a method. Consider using `.' instead
// Line: 9

using foo = System.Console;

class X {
	static void Main ()
	{
		foo::WriteLine ("hello");
	}
}
