// error CS0118: 'A.B' denotes a 'namespace', where a type was expected
// Line: 9
// Compiler options: -r:CS0118-2-lib.dll

using A.B.C;

namespace A.D {
	class Test {
		static public void Main () 
		{
			B c = new B ();
		}
	}
}
