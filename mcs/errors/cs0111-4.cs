// cs0111.cs: Class 'ErrorClass' already defines a member called 'get_Blah' with the same parameter types
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
