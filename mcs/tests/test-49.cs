//
// This tests excercises a number of switch things:
//
// Code to test for multiple-labels is different that
// code for a single label.
//
// Code for switching on strings is different from the integer
// code.
//
// nulls (for strings) need to be handled specially since ceq will
// throw an exception if there.
//
// null as a case statement needs to be caught specially
// 
using System;

class X {
	enum A {
		a = 23333,
	}
	public const string One = "one";

	static int s (byte b)
	{
		switch (b){
			case 0: return 255-0;
			case 1: return 255-1;
			case 2: return 255-2;
			case 3: return 255-3;
			case 4: return 255-4;
			case 5: return 255-5;
			case 6: return 255-6;
			case 7: return 255-7;
			case 8: return 255-8;
			case 9: return 255-9;
			case 10: return 255-10;
			case 11: return 255-11;
			case 12: return 255-12;
			case 13: return 255-13;
			case 14: return 255-14;
			case 15: return 255-15;
			case 16: return 255-16;
			case 17: return 255-17;
			case 18: return 255-18;
			case 19: return 255-19;
			case 20: return 255-20;
			case 21: return 255-21;
			case 22: return 255-22;
			case 23: return 255-23;
			case 24: return 255-24;
			case 25: return 255-25;
			case 26: return 255-26;
			case 27: return 255-27;
			case 28: return 255-28;
			case 29: return 255-29;
			case 30: return 255-30;
			case 31: return 255-31;
			case 32: return 255-32;
			case 33: return 255-33;
			case 34: return 255-34;
			case 35: return 255-35;
			case 36: return 255-36;
			case 37: return 255-37;
			case 38: return 255-38;
			case 39: return 255-39;
			case 40: return 255-40;
			case 41: return 255-41;
			case 42: return 255-42;
			case 43: return 255-43;
			case 44: return 255-44;
			case 45: return 255-45;
			case 46: return 255-46;
			case 47: return 255-47;
			case 48: return 255-48;
			case 49: return 255-49;
			case 50: return 255-50;
			case 51: return 255-51;
			case 52: return 255-52;
			case 53: return 255-53;
			case 54: return 255-54;
			case 55: return 255-55;
			case 56: return 255-56;
			case 57: return 255-57;
			case 58: return 255-58;
			case 59: return 255-59;
			case 60: return 255-60;
			case 61: return 255-61;
			case 62: return 255-62;
			case 63: return 255-63;
			case 64: return 255-64;
			case 65: return 255-65;
			case 66: return 255-66;
			case 67: return 255-67;
			case 68: return 255-68;
			case 69: return 255-69;
			case 70: return 255-70;
			case 71: return 255-71;
			case 72: return 255-72;
			case 73: return 255-73;
			case 74: return 255-74;
			case 75: return 255-75;
			case 76: return 255-76;
			case 77: return 255-77;
			case 78: return 255-78;
			case 79: return 255-79;
			case 80: return 255-80;
			case 81: return 255-81;
			case 82: return 255-82;
			case 83: return 255-83;
			case 84: return 255-84;
			case 85: return 255-85;
			case 86: return 255-86;
			case 87: return 255-87;
			case 88: return 255-88;
			case 89: return 255-89;
			case 90: return 255-90;
			case 91: return 255-91;
			case 92: return 255-92;
			case 93: return 255-93;
			case 94: return 255-94;
			case 95: return 255-95;
			case 96: return 255-96;
			case 97: return 255-97;
			case 98: return 255-98;
			case 99: return 255-99;
			case 100: return 255-100;
			case 101: return 255-101;
			case 102: return 255-102;
			case 103: return 255-103;
			case 104: return 255-104;
			case 105: return 255-105;
			case 106: return 255-106;
			case 107: return 255-107;
			case 108: return 255-108;
			case 109: return 255-109;
			case 110: return 255-110;
			case 111: return 255-111;
			case 112: return 255-112;
			case 113: return 255-113;
			case 114: return 255-114;
			case 115: return 255-115;
			case 116: return 255-116;
			case 117: return 255-117;
			case 118: return 255-118;
			case 119: return 255-119;
			case 120: return 255-120;
			case 121: return 255-121;
			case 122: return 255-122;
			case 123: return 255-123;
			case 124: return 255-124;
			case 125: return 255-125;
			case 126: return 255-126;
			case 127: return 255-127;
			case 128: return 255-128;
			case 129: return 255-129;
			case 130: return 255-130;
			case 131: return 255-131;
			case 132: return 255-132;
			case 133: return 255-133;
			case 134: return 255-134;
			case 135: return 255-135;
			case 136: return 255-136;
			case 137: return 255-137;
			case 138: return 255-138;
			case 139: return 255-139;
			case 140: return 255-140;
			case 141: return 255-141;
			case 142: return 255-142;
			case 143: return 255-143;
			case 144: return 255-144;
			case 145: return 255-145;
			case 146: return 255-146;
			case 147: return 255-147;
			case 148: return 255-148;
			case 149: return 255-149;
			case 150: return 255-150;
			case 151: return 255-151;
			case 152: return 255-152;
			case 153: return 255-153;
			case 154: return 255-154;
			case 155: return 255-155;
			case 156: return 255-156;
			case 157: return 255-157;
			case 158: return 255-158;
			case 159: return 255-159;
			case 160: return 255-160;
			case 161: return 255-161;
			case 162: return 255-162;
			case 163: return 255-163;
			case 164: return 255-164;
			case 165: return 255-165;
			case 166: return 255-166;
			case 167: return 255-167;
			case 168: return 255-168;
			case 169: return 255-169;
			case 170: return 255-170;
			case 171: return 255-171;
			case 172: return 255-172;
			case 173: return 255-173;
			case 174: return 255-174;
			case 175: return 255-175;
			case 176: return 255-176;
			case 177: return 255-177;
			case 178: return 255-178;
			case 179: return 255-179;
			case 180: return 255-180;
			case 181: return 255-181;
			case 182: return 255-182;
			case 183: return 255-183;
			case 184: return 255-184;
			case 185: return 255-185;
			case 186: return 255-186;
			case 187: return 255-187;
			case 188: return 255-188;
			case 189: return 255-189;
			case 190: return 255-190;
			case 191: return 255-191;
			case 192: return 255-192;
			case 193: return 255-193;
			case 194: return 255-194;
			case 195: return 255-195;
			case 196: return 255-196;
			case 197: return 255-197;
			case 198: return 255-198;
			case 199: return 255-199;
			case 200: return 255-200;
			case 201: return 255-201;
			case 202: return 255-202;
			case 203: return 255-203;
			case 204: return 255-204;
			case 205: return 255-205;
			case 206: return 255-206;
			case 207: return 255-207;
			case 208: return 255-208;
			case 209: return 255-209;
			case 210: return 255-210;
			case 211: return 255-211;
			case 212: return 255-212;
			case 213: return 255-213;
			case 214: return 255-214;
			case 215: return 255-215;
			case 216: return 255-216;
			case 217: return 255-217;
			case 218: return 255-218;
			case 219: return 255-219;
			case 220: return 255-220;
			case 221: return 255-221;
			case 222: return 255-222;
			case 223: return 255-223;
			case 224: return 255-224;
			case 225: return 255-225;
			case 226: return 255-226;
			case 227: return 255-227;
			case 228: return 255-228;
			case 229: return 255-229;
			case 230: return 255-230;
			case 231: return 255-231;
			case 232: return 255-232;
			case 233: return 255-233;
			case 234: return 255-234;
			case 235: return 255-235;
			case 236: return 255-236;
			case 237: return 255-237;
			case 238: return 255-238;
			case 239: return 255-239;
			case 240: return 255-240;
			case 241: return 255-241;
			case 242: return 255-242;
			case 243: return 255-243;
			case 244: return 255-244;
			case 245: return 255-245;
			case 246: return 255-246;
			case 247: return 255-247;
			case 248: return 255-248;
			case 249: return 255-249;
			case 250: return 255-250;
			case 251: return 255-251;
			case 252: return 255-252;
			case 253: return 255-253;
			case 254: return 255-254;
			case 255: return 255-255;
		}

		return -1;
	}

	static int test (int v)
	{
		Console.WriteLine ("value: " + v);
		switch (v){
		case 1:
		case 2:
			return 1;
			
		case 3:
			return 3;

		case 4:
			return 5;

		default:
			Console.WriteLine ("default");
			return 6;
		}

		return 7;
	}

	static int tests (string s)
	{
		switch (s){
		case "one":
		case "two":
			return 1;
		case "three":
			return 3;
		case "four":
			return 5;
		case null:
			return 9;
		case "new":
			goto case null;
		default:
			return 6;
		}

		return 1;
	}
	
	static int tests2 (string s)
	{
		switch (s){
		case "one":
			goto case null;
		case "two":
			goto default;
		case null:
		default:
			return 3;
		}
	}

	static int testn (string s)
	{
		switch (s){
		case "one":
			return 1;
			
		default:
			return 0;
		}
		return -1;
	}

	static int testm (string s)
	{
		switch (s){
		case "one":
			return 1;
		}
		return 100;
	}

	static int testo (string s)
	{
		switch (s){
		case "one":
			return 1;
		case null:
			return 100;
		}
		return 2;
	}

	static int testp (string s)
	{
		switch (s){
		case "one":
		case null:
		case "two":
			return 1;
		case "three":
			return 3;
		}
		return 4;
	}

	static int test_def (string s)
	{
		switch (s){
		case "one":
			goto default;
		case "two":
			return 1;
		case "three":
			return 2;
		default:
			return 3;
		}
		return 4;
	}

	static int test_coverage (int x)
	{
		switch (x){
		case 0:
			return 1;
		default:
			return 10;
		}
	}

	static int test_goto (int a)
	{
		switch (a){
		case 0:
			goto case 2;
		case 1:
			return 10;
		case 2:
			return 20;
		default:
			return 100;
		}
	}

	static int test_memberaccess (string s)
	{
		switch (s){
		case X.One: 
			return 3;
		default:
			return 4;
		}	
	}

	static int test_string_multiple_targets (string s)
	{
		switch (s){
		case "A":
			return 1;
		case "B":
			return 2;
		case "C":
		case "D":
			return 3;
		}
		return 0;
	}

	enum My : byte {
		A
	}

	static int test_casts (int n)
	{
		switch (n) {
			case (int) (char) (int) My.A: 
				return 1;

			default:
				return 2;
		}
	}

	public enum TestEnum : long {
		a, b, c
	}

	public static int testSwitchEnumLong (TestEnum c)
	{
		
		switch (c) {
		case TestEnum.a:
			return 10;
			
		case TestEnum.b:
			return 20;

		case TestEnum.c:
			goto case TestEnum.b;

		default:
			return 40;
		}
		
	}

	static int test_long_enum_switch ()
	{
		if (testSwitchEnumLong (TestEnum.a) != 10)
			return 1;
		if (testSwitchEnumLong (TestEnum.b) != 20)
			return 2;
		if (testSwitchEnumLong (TestEnum.c) != 20)
			return 3;
		if (testSwitchEnumLong ((TestEnum)5) != 40)
			return 4;
		return 0;
	}

	static int tests_default (string s)
	{
		// tests default in the middle of the switch
		
		switch (s){
		default:
		case "hello":
		case "world":
			return 1;
		case "how":
			return 2;
		}
	}

	// Bug #74655
	static int tests_default_2 (string foo)
	{
		switch (foo) {
		case "Hello":
			break;
		default:
			return 1;
		case "foo":
			return 2;
		case "Monkey":
			break;
		}
		return 3;
	}
	
	static void test_76590 (string s)
	{
       switch (s) {
        case "null":
		case (string)null:
          break;
          
        case "#":
          break;
          
        default:
          break;
        }		
	}

	static void test_77964()
	{
		char c = 'c';
		switch (c)
		{
		case 'A':
			break;

		case 'a': 
			goto case 65;
		}
	}
	
	static bool bug_78860()
	{
		string model = "TSP100";

		System.Console.WriteLine("switch on '{0}'", model);

		switch(model) {
			case "wibble":
			case null:
				return false;
			case "TSP100":
				return true;
		}
		return false;
	}
	
	static void test_1597 ()
	{
		var a = "";
		switch (a) {
		}
	}
	
	public static int Main ()
	{
		byte b;

		for (b = 0; b < 255; b++){
			if (s (b) != 255 - b){
				Console.WriteLine ("Failed with: " + b + " [" + s (b) + "]");
				return 1;
			}
		}

		Console.WriteLine ("Batch 2");
		if (test (1) != 1)
			return 1;
		if (test (2) != 1)
			return 2;
		if (test (3) != 3)
			return 3;
		if (test (4) != 5)
			return 4;
		if (test (100) != 6)
			return 5;

		if (tests ("one") != 1)
			return 6;
		if (tests ("two") != 1)
			return 7;
		if (tests ("three") != 3)
			return 8;
		if (tests ("four") != 5)
			return 9;
		if (tests (null) != 9)
			return 10;
		if (tests ("blah") != 6)
			return 11;
		if (tests ("new") != 9)
			return 110;


		if (testn ("one") != 1)
			return 12;
		if (testn ("hello") != 0)
			return 13;
		if (testn (null) != 0)
			return 14;

		if (testm ("one") != 1)
			return 15;
		if (testm ("two") != 100)
			return 16;
		if (testm (null) != 100)
			return 17;

		if (testo ("one") != 1)
			return 18;
		if (testo ("two") != 2)
			return 19;
		if (testo (null) != 100)
			return 20;

		if (testp ("one") != 1)
			return 21;
		if (testp (null) != 1)
			return 22;
		if (testp ("two") != 1)
			return 23;
		if (testp ("three") != 3)
			return 24;
		if (testp ("blah") != 4)
			return 25;

		if (test_def ("one") != 3)
			return 26;
		if (test_def ("two") != 1)
			return 27;
		if (test_def ("three") != 2)
			return 28;
		if (test_def (null) != 3)
			return 29;

		if (test_coverage (100) != 10)
			return 30;

		if (test_goto (0) != 20)
			return 31;
		if (test_goto (1) != 10)
			return 32;
		if (test_goto (2) != 20)
			return 33;
		if (test_goto (200) != 100)
			return 34;
	
		if (test_memberaccess ("one") != 3)
			return 35;

		if (test_string_multiple_targets ("A") != 1)
			return 36;
		if (test_string_multiple_targets ("B") != 2)
			return 37;
		if (test_string_multiple_targets ("C") != 3)
			return 38;
		if (test_string_multiple_targets ("D") != 3)
			return 39;
		if (test_string_multiple_targets ("E") != 0)
			return 40;
	      
		if (test_casts (0) != 1)
			return 41;
 
		if (test_long_enum_switch () != 0)
			return 42;

		if (tests_default (null) != 1)
			return 43;
		if (tests_default ("hello") != 1)
			return 44;
		if (tests_default ("world") != 1)
			return 45;
		if (tests_default ("how") != 2)
			return 46;

		if (tests_default_2 ("Test") != 1)
			return 47;
		if (tests_default_2 ("foo") != 2)
			return 48;
		if (tests_default_2 ("Hello") != 3)
			return 49;
		if (tests_default_2 ("Monkey") != 3)
			return 50;
		
		if (!bug_78860 ())
			return 60;
		
		if (tests2 ("a") != 3)
			return 71;
		if (tests2 ("one") != 3)
			return 72;
		if (tests2 ("two") != 3)
			return 73;

		test_1597 ();
		
		Console.WriteLine ("All tests pass");
		return 0;
	}
}
