// cs0120.cs: `test.method()': An object reference is required for the nonstatic field, method or property
// Line: 11

class test {

	void method ()
	{
	}
       
	public static int Main (string [] args){
		method ();
		return 1;
	}
}
