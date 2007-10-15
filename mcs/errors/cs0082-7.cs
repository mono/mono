// CS0082: A member `ErrorClass.get_Blah(int)' is already reserved
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
