// CS0120: An object reference is required to access non-static member `X.Y(System.Text.StringBuilder)'
// Line: 8

using System.Text;

class X {
	static void Main () {
		X.Y(null);	
	}
	
	void Y(StringBuilder someParam) {
	}
}
