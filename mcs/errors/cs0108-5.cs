// cs0108.cs: The new keyword is required on 'Derived.Prop' because it hides inherited member
// Line: 13

class Base {
	public bool Prop = false;
}

class Derived : Base {
	public int Prop {
            get {
                return 0;
            }
        }
}
