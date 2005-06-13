// cs0120-3.cs: An object reference is required for the nonstatic field, method, or property `X.method()'
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
