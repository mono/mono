// CS0109: The member `Derived.this[string]' does not hide an inherited member. The new keyword is not required
// Line: 10
// Compiler options: -warnaserror -warn:4

class Base {
	public bool this [int arg] { set {} }
}

class Derived : Base {
	public new bool this [string arg] { set {} }
}
