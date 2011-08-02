// CS0108: `Derived.EE' hides inherited member `Base.EE'. Use the new keyword if hiding was intended
// Line: 12
// Compiler options: -warnaserror -warn:2

class Base {
	public enum EE {
            Item
        };
}

class Derived : Base {
        public int EE;
}
