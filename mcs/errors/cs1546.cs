// cs1546.cs: Property `Title' is not supported by the C# language. Try to call the accessor method `Test.ITopic.get_Title(int, int)' directly
// Line: 9
// Compiler options: -r:CS1546-lib.dll

using Test;

class C {		
	public C (ITopic it) {
		string i = it.Title (2, 3);
	}
}
