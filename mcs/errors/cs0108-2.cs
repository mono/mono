// cs0108-2.cs: The new keyword is required on 'Derived.Test(bool)' because it hides 'BaseInterface.Test(bool)'
// Line: 9

interface BaseInterface {
	void Test (bool arg);
}

interface Derived : BaseInterface {
	void Test (bool arg);
}