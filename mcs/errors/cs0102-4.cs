// cs0102.cs: The class 'ErrorClass' already contains a definition for 'Blah'
// Line: 7

using System.Runtime.CompilerServices;
class ErrorClass {
	[IndexerName ("Blah")]
	public int this [int a] {
            get { return 1; }
	}
        
        public int Blah;
}
