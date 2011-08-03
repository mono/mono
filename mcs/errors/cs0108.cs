// CS0108: `Derived.F()' hides inherited member `Base.F()'. Use the new keyword if hiding was intended
// Line:
// Compiler options: -warnaserror -warn:2

class Base {
	public void F () {}
}

class Derived : Base {
	void F () {}
}
