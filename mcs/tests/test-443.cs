// Compiler options: -r:test-442-lib.dll

using Test;

class C {		
	public C (ITopic it) {
		string i = it.get_Title (2, 3);
		it.set_Title (1, 2, false, "bb");
	}
	
	public static void Main () {}
}
