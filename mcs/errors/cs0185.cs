// cs0185.cs: `int' is not a reference type as required by the lock statement
// Line:

class X {
	static void Main ()
	{
		lock (5) {
		}
	}
}
      
