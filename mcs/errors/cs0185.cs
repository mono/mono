// cs0185.cs: object is not a reference type (as expected by lock)
// Line:

class X {
	static void Main ()
	{
		lock (5) {
		}
	}
}
      
