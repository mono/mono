// cs0026: use of this is not allowed in a static field initializer
// 
class X {
	static object o = this;

	static int Main ()
	{
		return 1;
	}
}
