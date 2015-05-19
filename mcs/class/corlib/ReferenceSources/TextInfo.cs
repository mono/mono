using System.Runtime.CompilerServices;

namespace System.Globalization
{
	partial class TextInfo
	{
		unsafe static ushort *to_lower_data_low;
		unsafe static ushort *to_lower_data_high;
		unsafe static ushort *to_upper_data_low;
		unsafe static ushort *to_upper_data_high;

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		unsafe static extern void GetDataTablePointersLite (out ushort *to_lower_data_low, out ushort *to_lower_data_high, out ushort *to_upper_data_low, out ushort *to_upper_data_high);

		static readonly object cookie = new object ();

		unsafe static void ReadDataTable ()
		{
			if (to_lower_data_low == null) {
				lock (cookie) {
					if (to_lower_data_low != null)
						return;

					GetDataTablePointersLite (out to_lower_data_low, out to_lower_data_high, out to_upper_data_low, out to_upper_data_high);
				}
			}
		}

		unsafe string ToUpperInternal (string str)
		{
			if (str.Length == 0)
				return String.Empty;

			string tmp = String.FastAllocateString (str.Length);
			fixed (char* source = str, dest = tmp) {

				char* destPtr = (char*)dest;
				char* sourcePtr = (char*)source;

				for (int n = 0; n < str.Length; n++) {
					*destPtr = ToUpper (*sourcePtr);
					sourcePtr++;
					destPtr++;
				}
			}
			return tmp;
		}

		unsafe string ToLowerInternal (string str)
		{
			if (str.Length == 0)
				return String.Empty;

			string tmp = String.FastAllocateString (str.Length);
			fixed (char* source = str, dest = tmp) {

				char* destPtr = (char*)dest;
				char* sourcePtr = (char*)source;

				for (int n = 0; n < str.Length; n++) {
					*destPtr = ToLower (*sourcePtr);
					sourcePtr++;
					destPtr++;
				}
			}
			return tmp;
		}

		char ToUpperInternal (char c)
		{
			switch (c) {
			case '\u0069': // Latin lowercase i
				if (!IsAsciiCasingSameAsInvariant)
					return '\u0130'; // dotted capital I
				break;
			case '\u0131': // dotless i
				return '\u0049'; // I

			case '\u01c5': // see ToLower()
				return '\u01c4';
			case '\u01c8': // see ToLower()
				return '\u01c7';
			case '\u01cb': // see ToLower()
				return '\u01ca';
			case '\u01f2': // see ToLower()
				return '\u01f1';
			case '\u0390': // GREEK SMALL LETTER IOTA WITH DIALYTIKA AND TONOS
				return '\u03aa'; // it is not in ICU
			case '\u03b0': // GREEK SMALL LETTER UPSILON WITH DIALYTIKA AND TONOS
				return '\u03ab'; // it is not in ICU
			case '\u03d0': // GREEK BETA
				return '\u0392';
			case '\u03d1': // GREEK THETA
				return '\u0398';
			case '\u03d5': // GREEK PHI
				return '\u03a6';
			case '\u03d6': // GREEK PI
				return '\u03a0';
			case '\u03f0': // GREEK KAPPA
				return '\u039a';
			case '\u03f1': // GREEK RHO
				return '\u03a1';
			// am not sure why miscellaneous GREEK symbols are 
			// not handled here.
			}

			return ToUpperInvariant (c);
		}		

		char ToLowerInternal (char c)
		{
			switch (c) {
			case '\u0049': // Latin uppercase I
				if (!IsAsciiCasingSameAsInvariant)
					return '\u0131'; // I becomes dotless i
				break;
			case '\u0130': // I-dotted
				return '\u0069'; // i

			case '\u01c5': // LATIN CAPITAL LETTER D WITH SMALL LETTER Z WITH CARON
				return '\u01c6';
			// \u01c7 -> \u01c9 (LJ) : invariant
			case '\u01c8': // LATIN CAPITAL LETTER L WITH SMALL LETTER J
				return '\u01c9';
			// \u01ca -> \u01cc (NJ) : invariant
			case '\u01cb': // LATIN CAPITAL LETTER N WITH SMALL LETTER J
				return '\u01cc';
			// WITH CARON : invariant
			// WITH DIAERESIS AND * : invariant

			case '\u01f2': // LATIN CAPITAL LETTER D WITH SMALL LETTER Z
				return '\u01f3';
			case '\u03d2':  // ? it is not in ICU
				return '\u03c5';
			case '\u03d3':  // ? it is not in ICU
				return '\u03cd';
			case '\u03d4':  // ? it is not in ICU
				return '\u03cb';
			}
			return ToLowerInvariant (c);			
		}

		static char ToLowerInvariant (char c)
		{
			ReadDataTable ();

			unsafe {
				if (c <= ((char)0x24cf))
					return (char) to_lower_data_low [c];
				if (c >= ((char)0xff21))
					return (char) to_lower_data_high[c - 0xff21];
			}
			return c;
		}

		static char ToUpperInvariant (char c)
		{
			ReadDataTable ();

			unsafe {
				if (c <= ((char)0x24e9))
					return (char) to_upper_data_low [c];
				if (c >= ((char)0xff21))
					return (char) to_upper_data_high [c - 0xff21];
			}
			return c;
		}		

		static unsafe int InternalCompareStringOrdinalIgnoreCase (String strA, int indexA, String strB, int indexB, int lenA, int lenB)
		{
			if (strA == null) {
				return strB == null ? 0 : -1;
			}
			if (strB == null) {
				return 1;
			}
			int lengthA = Math.Min (lenA, strA.Length - indexA);
			int lengthB = Math.Min (lenB, strB.Length - indexB);

			if (lengthA == lengthB && Object.ReferenceEquals (strA, strB))
				return 0;

			fixed (char* aptr = strA, bptr = strB) {
				char* ap = aptr + indexA;
				char* end = ap + Math.Min (lengthA, lengthB);
				char* bp = bptr + indexB;
				while (ap < end) {
					if (*ap != *bp) {
						char c1 = Char.ToUpperInvariant (*ap);
						char c2 = Char.ToUpperInvariant (*bp);
						if (c1 != c2)
							return c1 - c2;
					}
					ap++;
					bp++;
				}
				return lengthA - lengthB;
			}
		}
	}
}