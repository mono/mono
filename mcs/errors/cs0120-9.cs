// cs0120-9.cs: `X.Y(System.Text.StringBuilder)': An object reference is required for the nonstatic field, method or property
// Line: 8

using System.Text;

class X {
	static void Main () {
		X.Y(null);	
	}
	
	void Y(StringBuilder someParam) {
	}
}
