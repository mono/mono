// cs0108.cs: The new keyword is required on 'Derived.EE' because it hides inherited member
// Line: 11

class Base {
	public enum EE {
            Item
        };
}

class Derived : Base {
        public int EE;
}
