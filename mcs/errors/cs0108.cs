// cs0108.cs: The new keyword is required on MEMBER because it hides MEMBER2
// Line:
// Compiler options: -warnaserror -warn:1

class Base {
	public void F () {}
}

class Derived : Base {
	void F () {}
}
