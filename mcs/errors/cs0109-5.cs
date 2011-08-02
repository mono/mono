// CS0109: The member `Derived.Test()' does not hide an inherited member. The new keyword is not required
// Line: 10
// Compiler options: -warnaserror -warn:4

class Base {
	void Test (bool arg) {}
}

class Derived : Base {
	new void Test () {}
}