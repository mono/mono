// CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type `Foo'
// Line: 11
// Compiler options: -unsafe

public unsafe partial struct Foo
{
}

public unsafe partial struct Foo
{
	public Foo *foo;
	string x;
}


