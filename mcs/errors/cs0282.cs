// cs0282.cs: struct instance field `S.y' found in different declaration from instance field `S.x'
// Line: 8
// Compiler options: -warn:4 -warnaserror

partial struct S {
	int x;
}
partial struct S {
	int y;
	static void Main () {}
}
