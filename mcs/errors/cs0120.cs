// CS0120: An object reference is required to access non-static member `test.method()'
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
