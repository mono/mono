// CS0619: `Error.Filename' is obsolete: `NOT'
// Line: 8
// Compiler options: -reference:CS0619-42-lib.dll

class A: Error {
	public A () {
		string s = Filename;
	}
	
	public override string Filename {
		set {
		}
	}
}