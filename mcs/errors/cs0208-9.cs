// CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type `Foo'
// Line: 7
// Compiler options: -t:library -unsafe

public unsafe struct Foo
{
        public Foo *foo;
	string x;
}


