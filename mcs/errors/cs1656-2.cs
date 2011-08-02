// CS1656: Cannot assign to `p' because it is a `fixed variable'
// Line: 10
// Compiler options: -unsafe

unsafe class X {

	static int x = 0;
	static void Main () {
		fixed (int* p = &x) {
		    p = (int*)22;
		}
	}		    
}
	
