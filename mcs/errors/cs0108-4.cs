// cs0108.cs: The new keyword is required on 'Derived.Prop(int)' because it hides inherited member
// Line: 13

class Base {
	public void Prop (int a) {}
}

class Derived : Base {
	public int Prop {
            get {
                return 0;
            }
        }
}
