// CS0108: `Derived.Method()' hides inherited member `Base.Method()'. Use the new keyword if hiding was intended
// Line: 11
// Compiler options: -warnaserror -warn:2

class Base {
	public bool Method () { return false; }
        public void Method (int a) {}
}

class Derived : Base {
        public void Method () {}
}
