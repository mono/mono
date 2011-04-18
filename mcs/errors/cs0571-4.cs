// CS0571: `MainClass.this[int, bool, string].get': cannot explicitly call operator or accessor
// Line: 15

using System.Runtime.CompilerServices;

class MainClass {
	[IndexerName ("AA")]
	int this [int Value, bool Value2, string Value3] {
		get {
			return 1;
		}
	}
		
	public MainClass () {
		int i = get_AA (2, false, "aaa");
	}
}
