// CS0108: `Derived.Test(bool)' hides inherited member `BaseInterface.Test(bool)'. Use the new keyword if hiding was intended
// Line: 9
// Compiler options: -warnaserror -warn:2 -t:library

interface BaseInterface {
	void Test (bool arg);
}

interface Derived : BaseInterface {
	void Test (bool arg);
}
