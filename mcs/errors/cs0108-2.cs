// cs0108-2.cs: The new keyword is required on 'Derived.Test(bool)' because it hides 'BaseInterface.Test(bool)'
// Line: 9
// Compiler options: -warnaserror -warn:1 -t:library

interface BaseInterface {
	void Test (bool arg);
}

interface Derived : BaseInterface {
	void Test (bool arg);
}
