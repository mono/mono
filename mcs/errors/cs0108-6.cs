// cs0108.cs: The new keyword is required on 'Derived.Prop' because it hides inherited member
// Line: 14
// Compiler options: -warnaserror -warn:2

class Base {
	public int Prop {
            get {
                return 0;
            }
        }    
}

class Derived : Base {
	public bool Prop = false;
}
