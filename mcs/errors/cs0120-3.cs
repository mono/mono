// cs0120-3.cs: `method()': An object reference is required for the nonstatic field, method or property
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
