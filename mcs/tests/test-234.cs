using System;

class T {

	enum E {
		e0 = 1 << 0,
		e1 = 1 << 1,
		e2 = 1 << 2,
		e3 = 1 << 3,
		e4 = 1 << 4,
		e5 = 1 << 5,
		e6 = 1 << 6,
		e7 = 1 << 7,
		e8 = 1 << 8,
		e9 = 1 << 9,
		e10 = 1 << 10,
		e11 = 1 << 11,
		e12 = 1 << 12,
		e13 = 1 << 13,
		e14 = 1 << 14,
		e15 = 1 << 15,
		e16 = 1 << 16,
		e17 = 1 << 17,
		e18 = 1 << 18,
		e19 = 1 << 19,
		e20 = 1 << 20,
		e21 = 1 << 21,
		e22 = 1 << 22,
		e23 = 1 << 23,
		e24 = 1 << 24,
		e25 = 1 << 25,
		e26 = 1 << 26,
		e27 = 1 << 27,
		e28 = 1 << 28,
		e29 = 1 << 29,
		e30 = 1 << 30,
		e31 = 1 << 31,
	}
	public static void Main ()
	{
		E e = E.e1;
		string s;
		switch (e) {
		case E.e0: s = "case 0"; break;
		case E.e1: s = "case 1"; break;
		case E.e2: s = "case 2"; break;
		case E.e3: s = "case 3"; break;
		case E.e4: s = "case 4"; break;
		case E.e5: s = "case 5"; break;
		case E.e6: s = "case 6"; break;
		case E.e7: s = "case 7"; break;
		case E.e8: s = "case 8"; break;
		case E.e9: s = "case 9"; break;
		case E.e10: s = "case 10"; break;
		case E.e11: s = "case 11"; break;
		case E.e12: s = "case 12"; break;
		case E.e13: s = "case 13"; break;
		case E.e14: s = "case 14"; break;
		case E.e15: s = "case 15"; break;
		case E.e16: s = "case 16"; break;
		case E.e17: s = "case 17"; break;
		case E.e18: s = "case 18"; break;
		case E.e19: s = "case 19"; break;
		case E.e20: s = "case 20"; break;
		case E.e21: s = "case 21"; break;
		case E.e22: s = "case 22"; break;
		case E.e23: s = "case 23"; break;
		case E.e24: s = "case 24"; break;
		case E.e25: s = "case 25"; break;
		case E.e26: s = "case 26"; break;
		case E.e27: s = "case 27"; break;
		case E.e28: s = "case 28"; break;
		case E.e29: s = "case 29"; break;
		case E.e30: s = "case 30"; break;
		case E.e31: s = "case 31"; break;
		}
	}
}