// This code must be compilable without any warning
// Compiler options: -warnaserror -warn:4

class BaseClass {
        public int Location = 3;
}

class Derived : BaseClass {
	public new int Location {
            get {
                return 9;
            }
        }
        
        public static void Main () { }
}