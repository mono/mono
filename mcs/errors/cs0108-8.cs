// cs0108.cs: The new keyword is required on 'Derived.Method()' because it hides inherited member
// Line: 11
// Compiler options: -warnaserror -warn:2

class Base {
	public bool Method () { return false; }
        public void Method (int a) {}
}

class Derived : Base {
        public void Method () {}
}
