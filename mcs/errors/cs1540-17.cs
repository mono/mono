// CS1540: Cannot access protected member `A.Foo()' via a qualifier of type `D2'. The qualifier must be of type `D' or derived from it
// Line: 8
// Compiler options: -r:CS1540-17-lib.dll

class D : B
{
	public void Test ()
	{
		C.Get().Foo ();
	}
}

class D2 : B
{
}

class B : A
{
}

class C
{
	public static D2 Get ()
	{
		return new D2 ();
	}
}
