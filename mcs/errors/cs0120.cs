// cs0120: Ie, a static method invoking a non-static method
// `An object reference is required for the nonstatic field, method or property test.method()
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
