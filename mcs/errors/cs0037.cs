//cs0037.cs: Can not convert null to struct because its a value type
// Line: 10
struct S {
	int a;
}

class X {
	static void Main ()
	{
		S s = (S) null;
	}
}
