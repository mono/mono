// cs0108.cs: The new keyword is required on 'Derived.Prop because it hides inherited member
// Line: 13
// Compiler options: -warnaserror -warn:2

class Base {
	public bool Prop = false;
}

class Derived : Base {
        public void Prop (bool b) {}
}
