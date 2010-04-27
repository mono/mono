// CS1546: Property or event `Test.ITopic.Title' is not supported by the C# language
// Line: 9
// Compiler options: -r:CS1546-lib.dll

using Test;

class C {		
	public C (ITopic it) {
		string i = it.Title (2, 3);
	}
}
