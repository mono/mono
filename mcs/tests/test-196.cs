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
	
	public static int Main ()
	{
		const byte b = 255;
		const int i = b << int.MaxValue;
		const int i2 = b << int.MaxValue;
		
		long token = uint.MaxValue;
		const int column_mask = (int)((1 << 32) - 1);
		int r2 = (int) (token & column_mask);
		if (r2 != 0)
			return 1;

		return 0;
	}
}
