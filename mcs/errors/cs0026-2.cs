// cs0026-2.cs: Keyword `this' is not valid in a static property, static method, or static field initializer
// Line: 4
class X {
	static object o = this;

	static int Main ()
	{
		return 1;
	}
}
