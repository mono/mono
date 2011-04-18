// CS0108: `Derived.Prop(bool)' hides inherited member `Base.Prop'. Use the new keyword if hiding was intended
// Line: 13
// Compiler options: -warnaserror -warn:2

class Base {
	public bool Prop = false;
}

class Derived : Base {
        public void Prop (bool b) {}
}
