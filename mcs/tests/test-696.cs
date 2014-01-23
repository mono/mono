struct S {
	public static implicit operator S (C c) { S s; return s; }
	public static void f (S s) { }
}

class C {
	public static void Main () { S.f (null); }
}
