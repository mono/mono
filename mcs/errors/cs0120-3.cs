// cs0120: `An object reference is required for the nonstatic field, method or property test.method()
// Line: 11

class X {

	void method ()
	{
	}
       
	public static int Main (string [] args){
		X.method ();
		return 1;
	}
}
