// CS0120: An object reference is required to access non-static member `X.method()'
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
