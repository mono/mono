// cs0111-4.cs: `ErrorClass.get_Blah(int)' is already defined. Rename this member or use different parameter types
// Line: 8

using System.Runtime.CompilerServices;
class ErrorClass {
	[IndexerName ("Blah")]
	public int this [int a] {
            get { return 1; }
	}
        
        public void get_Blah (int b) {}
	
        public static void Main ()
        {
        }
}
