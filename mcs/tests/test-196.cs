//
// Tests related to constants and binary operators (bug 39018)
//

class X {
	void Bug1 () {
		uint a = 1, b = 2;
		long l = (b & (0x1 << 31));
	}

	void Bug2 () {
		uint a = 1, b = 2;
		const int l = 1;
		const int r = 31;
		
		long ll = (b & (l << r));
	}
	
	static void Main ()
	{
		return 0;
	}
}
