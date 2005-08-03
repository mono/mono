// cs0431.cs: `foo' cannot be used with '::' since it denotes a type
// Line: 13

using foo = A;

class A {
	public class B { }
}

class X {
	static void Main ()
	{
		foo::B b = new A.B ();
	}
}
