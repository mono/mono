// CS1648: Members of readonly field `C.s' cannot be modified (except in a constructor or a variable initializer)
// Line: 13

struct S {
	public int x;
}

class C {
	readonly S s;

	public void Test ()
        {
		s.x = 42;
	}
}
