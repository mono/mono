
class Demo {
	static int Main ()
	{
		if (0b1 != 1)
			return 1;
		var hex1 = 0x123ul;
		var bin1  = 0b100100011ul;
		var bin11 = 0b100100011lu;
		if (hex1 != bin1)
			return 2;
		if (hex1 != bin11)
			return 3;
		if (hex1.GetType () != bin1.GetType ())
			return 4;
		if (hex1.GetType () != bin11.GetType ())
			return 5;

		var hex2 = 0x7FFFFFFF;
		var bin2 = 0b1111111111111111111111111111111;

		if (hex2 != bin2)
			return 6;
		if (hex2.GetType () != bin2.GetType ())
			return 7;

		var hex3 = 0xFFFFFFFF;
		var bin3 = 0b11111111111111111111111111111111;
		if (hex3 != bin3)
			return 8;
		if (hex3.GetType () != bin3.GetType ())
			return 9;

		var hex4 = 0xFFFFFFFFu;
		var bin4 = 0b11111111111111111111111111111111u;
		if (hex4 != bin4)
			return 10;
		if (hex4.GetType () != bin4.GetType ())
			return 11;

		var hex5 = 0x7FFFFFFFFFFFFFFF;
		var bin5 = 0b111111111111111111111111111111111111111111111111111111111111111;
		if (hex5 != bin5)
			return 12;
		if (hex5.GetType () != bin5.GetType ())
			return 13;

		var hex6 = 0xFFFFFFFFFFFFFFFF;
		var bin6 = 0b1111111111111111111111111111111111111111111111111111111111111111;
		if (hex6 != bin6)
			return 14;
		if (hex6.GetType () != bin6.GetType ())
			return 15;

		return 0;
	}
}