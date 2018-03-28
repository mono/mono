//
// System.NumberFormatter.cs
//
// Author:
//   Kazuki Oikawa (kazuki@panicode.com)
//   Eyal Alaluf (eyala@mainsoft.com)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2008 Mainsoft Co. (http://www.mainsoft.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Globalization;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;

namespace System
{
	internal sealed partial class NumberFormatter
	{
		#region Static Fields

		const int DefaultExpPrecision = 6;
		const int HundredMillion = 100000000;
		const long SeventeenDigitsThreshold = 10000000000000000;
		const ulong ULongDivHundredMillion = UInt64.MaxValue / HundredMillion;
		const ulong ULongModHundredMillion = 1 + UInt64.MaxValue % HundredMillion;

		const int DoubleBitsExponentShift = 52;
		const int DoubleBitsExponentMask = 0x7ff;
		const long DoubleBitsMantissaMask = 0xfffffffffffff;
		const int DecimalBitsScaleMask = 0x1f0000;

		const int SingleDefPrecision = 7;
		const int DoubleDefPrecision = 15;
		const int Int32DefPrecision = 10;
		const int UInt32DefPrecision = 10;
		const int Int64DefPrecision = 19;
		const int UInt64DefPrecision = 20;
		const int DecimalDefPrecision = 100;
		const int TenPowersListLength = 19;

		const double MinRoundtripVal = -1.79769313486231E+308;
		const double MaxRoundtripVal = 1.79769313486231E+308;

		// The below arrays are taken from mono/metatdata/number-formatter.h

		private static readonly unsafe ulong* MantissaBitsTable;
		private static readonly unsafe int* TensExponentTable;
		private static readonly unsafe char* DigitLowerTable;
		private static readonly unsafe char* DigitUpperTable;
		private static readonly unsafe long* TenPowersList;

		// DecHexDigits s a translation table from a decimal number to its
		// digits hexadecimal representation (e.g. DecHexDigits [34] = 0x34).
		private static readonly unsafe int* DecHexDigits;

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		private unsafe static extern void GetFormatterTables (out ulong* MantissaBitsTable, out int* TensExponentTable,
				out char* DigitLowerTable, out char* DigitUpperTable,
				out long* TenPowersList, out int* DecHexDigits);

		unsafe static NumberFormatter()
		{
			GetFormatterTables (out MantissaBitsTable, out TensExponentTable,
				out DigitLowerTable, out DigitUpperTable, out TenPowersList, out DecHexDigits);
		}

		unsafe static long GetTenPowerOf(int i)
		{
			return TenPowersList [i];
		}
		#endregion Static Fields

		#region Fields

		private NumberFormatInfo _nfi;

		//part of the private stringbuffer
		private char[] _cbuf;

		private bool _NaN;
		private bool _infinity;
		private bool _isCustomFormat;
		private bool _specifierIsUpper;
		private bool _positive;
		private char _specifier;
		private int _precision;
		private int _defPrecision;

		private int _digitsLen;
		private int _offset; // Represent the first digit offset.
		private int _decPointPos;

		// The following fields are a hexadeimal representation of the digits.
		// For instance _val = 0x234 represents the digits '2', '3', '4'.
		private uint _val1; // Digits 0 - 7.
		private uint _val2; // Digits 8 - 15.
		private uint _val3; // Digits 16 - 23.
		private uint _val4; // Digits 23 - 31. Only needed for decimals.

		#endregion Fields

		#region Constructor Helpers

		// Translate an unsigned int to hexadecimal digits.
		// i.e. 123456789 is represented by _val1 = 0x23456789 and _val2 = 0x1
		private void InitDecHexDigits (uint value)
		{
			if (value >= HundredMillion) {
				int div1 = (int)(value / HundredMillion);
				value -= HundredMillion * (uint)div1;
				_val2 = FastToDecHex (div1);
			}
			_val1 = ToDecHex ((int)value);
		}

		// Translate an unsigned long to hexadecimal digits.
		private void InitDecHexDigits (ulong value)
		{
			if (value >= HundredMillion) {
				long div1 = (long)(value / HundredMillion);
				value -= HundredMillion * (ulong)div1;
				if (div1 >= HundredMillion) {
					int div2 = (int)(div1 / HundredMillion);
					div1 = div1 - div2 * (long)HundredMillion;
					_val3 = ToDecHex (div2);
				}
				if (div1 != 0)
					_val2 = ToDecHex ((int)(div1));
			}
			if (value != 0)
				_val1 = ToDecHex ((int)value);
		}

		// Translate a decimal integer to hexadecimal digits.
		// The decimal integer is 96 digits and its value is hi * 2^64 + lo.
		// is the lower 64 bits.
		private void InitDecHexDigits (uint hi, ulong lo)
		{
			if (hi == 0) {
				InitDecHexDigits (lo); // Only the lower 64 bits matter.
				return;
			}

			// Compute (hi, lo) = (hi , lo) / HundredMillion.
			uint divhi = hi / HundredMillion;
			ulong remhi = hi - divhi * HundredMillion;
			ulong divlo = lo / HundredMillion;
			ulong remlo = lo - divlo * HundredMillion + remhi * ULongModHundredMillion;
			hi = divhi;
			lo = divlo + remhi * ULongDivHundredMillion;
			divlo = remlo / HundredMillion;
			remlo -= divlo * HundredMillion;
			lo += divlo;
			_val1 = ToDecHex ((int)remlo);

			// Divide hi * 2 ^ 64 + lo by HundredMillion using the fact that
			// hi < HundredMillion.
			divlo = lo / HundredMillion;
			remlo = lo - divlo * HundredMillion;
			lo = divlo;
			if (hi != 0) {
				lo += hi * ULongDivHundredMillion;
				remlo += hi * ULongModHundredMillion;
				divlo = remlo / HundredMillion;
				lo += divlo;
				remlo -= divlo * HundredMillion;
			}
			_val2 = ToDecHex ((int)remlo);

			// Now we are left with 64 bits store in lo.
			if (lo >= HundredMillion) {
				divlo = lo / HundredMillion;
				lo -= divlo * HundredMillion;
				_val4 = ToDecHex ((int)divlo);
			}
			_val3 = ToDecHex ((int)lo);
		}

		// Helper to translate an int in the range 0 .. 9999 to its
		// Hexadecimal digits representation.
		unsafe private static uint FastToDecHex (int val)
		{
			if (val < 100)
				return (uint)DecHexDigits [val];

			// Uses 2^19 (524288) to compute val / 100 for val < 10000.
			int v = (val * 5243) >> 19;
			return (uint)((DecHexDigits [v] << 8) | DecHexDigits [val - v * 100]);
		}

		// Helper to translate an int in the range 0 .. 99999999 to its
		// Hexadecimal digits representation.
		private static uint ToDecHex (int val)
		{
			uint res = 0;
			if (val >= 10000) {
				int v = val / 10000;
				val -= v * 10000;
				res = FastToDecHex (v) << 16;
			}
			return res | FastToDecHex (val);
		}

		// Helper to count number of hexadecimal digits in a number.
		private static int FastDecHexLen (int val)
		{
			if (val < 0x100)
				if (val < 0x10)
					return 1;
				else
					return 2;
			else if (val < 0x1000)
				return 3;
			else
				return 4;
		}

		private static int DecHexLen (uint val)
		{
			if (val < 0x10000)
				return FastDecHexLen ((int)val);
			return 4 + FastDecHexLen ((int)(val >> 16));
		}

		// Count number of hexadecimal digits stored in _val1 .. _val4.
		private int DecHexLen ()
		{
			if (_val4 != 0)
				return DecHexLen (_val4) + 24;
			else if (_val3 != 0)
				return DecHexLen (_val3) + 16;
			else if (_val2 != 0)
				return DecHexLen (_val2) + 8;
			else if (_val1 != 0)
				return DecHexLen (_val1);
			else
				return 0;
		}

		// Helper to count the 10th scale (number of digits) in a number
		private static int ScaleOrder (long hi)
		{
			for (int i = TenPowersListLength - 1; i >= 0; i--)
				if (hi >= GetTenPowerOf (i))
					return i + 1;
			return 1;
		}

		// Compute the initial precision for rounding a floating number
		// according to the used format.
		int InitialFloatingPrecision ()
		{
			if (_specifier == 'R')
				return _defPrecision + 2;
			if (_precision < _defPrecision)
				return _defPrecision;
			if (_specifier == 'G')
				return Math.Min (_defPrecision + 2, _precision);
			if (_specifier == 'E')
				return Math.Min (_defPrecision + 2, _precision + 1);
			return _defPrecision;
		}

		// Parse the given format and extract the precision in it.
		// Returns -1 for empty formats and -2 to indicate that the format
		// is a custom format.
		private static int ParsePrecision (string format)
		{
			int precision = 0;
			for (int i = 1; i < format.Length; i++) {
				int val = format [i] - '0';
				precision = precision * 10 + val;
				if (val < 0 || val > 9 || precision > 99)
					return -2;
			}
			return precision;
		}

		#endregion Constructor Helpers

		#region Constructors

		// Parse the given format and initialize the following fields:
		//   _isCustomFormat, _specifierIsUpper, _specifier & _precision.
		NumberFormatter (Thread current)
		{
			_cbuf = EmptyArray<char>.Value;
			if (current == null)
				return;
			CurrentCulture = current.CurrentCulture;
		}

		private void Init (string format)
		{
			_val1 = _val2 = _val3 = _val4 = 0;
			_offset = 0;
			_NaN = _infinity = false;
			_isCustomFormat = false;
			_specifierIsUpper = true;
			_precision = -1;

			if (format == null || format.Length == 0) {
				_specifier = 'G';
				return;
			}

			char specifier = format [0];
			if (specifier >= 'a' && specifier <= 'z') {
				specifier = (char)(specifier - 'a' + 'A');
				_specifierIsUpper = false;
			}
			else if (specifier < 'A' || specifier > 'Z') {
				_isCustomFormat = true;
				_specifier = '0';
				return;
			}
			_specifier = specifier;
			if (format.Length > 1) {
				_precision = ParsePrecision (format);
				if (_precision == -2) { // Is it a custom format?
					_isCustomFormat = true;
					_specifier = '0';
					_precision = -1;
				}
			}
		}

		private void InitHex (ulong value)
		{
			switch (_defPrecision) {
				case Int32DefPrecision: value = (uint) value;    break;
			}
			_val1 = (uint)value;
			_val2 = (uint)(value >> 32);
			_decPointPos = _digitsLen = DecHexLen ();
			if (value == 0)
				_decPointPos = 1;
		}

		private void Init (string format, int value, int defPrecision)
		{
			Init (format);
			_defPrecision = defPrecision;
			_positive = value >= 0;

			if (value == 0 || _specifier == 'X') {
				InitHex ((ulong)value);
				return;
			}

			if (value < 0)
				value = -value;
			InitDecHexDigits ((uint)value);
			_decPointPos = _digitsLen = DecHexLen ();
		}

		private void Init (string format, uint value, int defPrecision)
		{
			Init (format);
			_defPrecision = defPrecision;
			_positive = true;

			if (value == 0 || _specifier == 'X') {
				InitHex (value);
				return;
			}

			InitDecHexDigits (value);
			_decPointPos = _digitsLen = DecHexLen ();
		}

		private void Init (string format, long value)
		{
			Init (format);
			_defPrecision = Int64DefPrecision;
			_positive = value >= 0;

			if (value == 0 || _specifier == 'X') {
				InitHex ((ulong)value);
				return;
			}

			if (value < 0)
				value = -value;
			InitDecHexDigits ((ulong)value);
			_decPointPos = _digitsLen = DecHexLen ();
		}

		private void Init (string format, ulong value)
		{
			Init (format);
			_defPrecision = UInt64DefPrecision;
			_positive = true;

			if (value == 0 || _specifier == 'X') {
				InitHex ((ulong)value);
				return;
			}

			InitDecHexDigits (value);
			_decPointPos = _digitsLen = DecHexLen ();
		}

		unsafe private void Init (string format, double value, int defPrecision)
		{
			Init (format);

			_defPrecision = defPrecision;
			long bits = BitConverter.DoubleToInt64Bits (value);
		   	_positive = bits >= 0;
			bits &= Int64.MaxValue;
			if (bits == 0) {
				_decPointPos = 1;
				_digitsLen = 0;
				_positive = true;
				return;
			}

			int e = (int)(bits >> DoubleBitsExponentShift);
			long m = bits & DoubleBitsMantissaMask;
			if (e == DoubleBitsExponentMask) {
				_NaN = m != 0;
				_infinity = m == 0;
				return;
			}

			int expAdjust = 0;
			if (e == 0) {
				// We need 'm' to be large enough so we won't lose precision.
				e = 1;
				int scale = ScaleOrder (m);
				if (scale < DoubleDefPrecision) {
					expAdjust = scale - DoubleDefPrecision;
					m *= GetTenPowerOf (-expAdjust);
				}
			}
			else {
				m = (m + DoubleBitsMantissaMask + 1) * 10;
				expAdjust = -1;
			}

			// multiply the mantissa by 10 ^ N
			ulong lo = (uint)m;
			ulong hi = (ulong)m >> 32;
			ulong lo2 = MantissaBitsTable [e];
			ulong hi2 = lo2 >> 32;
			lo2 = (uint)lo2;
			ulong mm = hi * lo2 + lo * hi2 + ((lo * lo2) >> 32);
			long res = (long)(hi * hi2 + (mm >> 32));
			while (res < SeventeenDigitsThreshold) {
				mm = (mm & UInt32.MaxValue) * 10;
				res = res * 10 + (long)(mm >> 32);
				expAdjust--;
			}
			if ((mm & 0x80000000) != 0)
				res++;

			int order = DoubleDefPrecision + 2;
			_decPointPos = TensExponentTable [e] + expAdjust + order;

			// Rescale 'res' to the initial precision (15-17 for doubles).
			int initialPrecision = InitialFloatingPrecision ();
			if (order > initialPrecision) {
				long val = GetTenPowerOf (order - initialPrecision);
				res = (res + (val >> 1)) / val;
				order = initialPrecision;
			}
			if (res >= GetTenPowerOf (order)) {
				order++;
				_decPointPos++;
			}

		   	InitDecHexDigits ((ulong)res);
			_offset = CountTrailingZeros ();
			_digitsLen = order - _offset;
		}

		private void Init (string format, decimal value)
		{
			Init (format);
			_defPrecision = DecimalDefPrecision;

			int[] bits = decimal.GetBits (value);
			int scale = (bits [3] & DecimalBitsScaleMask) >> 16;
			_positive = bits [3] >= 0;
			if (bits [0] == 0 && bits [1] == 0 && bits [2] == 0) {
				_decPointPos = -scale;
				_positive = true;
				_digitsLen = 0;
				return;
			}

		   	InitDecHexDigits ((uint)bits [2], ((ulong)bits [1] << 32) | (uint)bits [0]);
			_digitsLen = DecHexLen ();
			_decPointPos = _digitsLen - scale;
			if (_precision != -1 || _specifier != 'G') {
				_offset = CountTrailingZeros ();
				_digitsLen -= _offset;
			}
		}

		#endregion Constructors

		#region Inner String Buffer

		//_cbuf moved to before other fields to improve layout
		private int _ind;

		private void ResetCharBuf (int size)
		{
			_ind = 0;
			if (_cbuf.Length < size)
				_cbuf = new char [size];
		}

		private void Resize (int len)
		{
			Array.Resize (ref _cbuf, len);
		}

		private void Append (char c)
		{
			if (_ind == _cbuf.Length)
				Resize (_ind + 10);
			_cbuf [_ind++] = c;
		}

		private void Append (char c, int cnt)
		{
			if (_ind + cnt > _cbuf.Length)
				Resize (_ind + cnt + 10);
			while (cnt-- > 0)
				_cbuf [_ind++] = c;
		}

		private void Append (string s)
		{
			int slen = s.Length;
			if (_ind + slen > _cbuf.Length)
				Resize (_ind + slen + 10);
			for (int i = 0; i < slen; i++)
				_cbuf [_ind++] = s [i];
		}

		#endregion Inner String Buffer

		#region Helper properties

		private NumberFormatInfo GetNumberFormatInstance (IFormatProvider fp)
		{
			if (_nfi != null && fp == null)
				return _nfi;
			return NumberFormatInfo.GetInstance (fp);
		}

		CultureInfo CurrentCulture {
			set {
				if (value != null && value.IsReadOnly)
					_nfi = value.NumberFormat;
				else
					_nfi = null;
			}
		}

		private int IntegerDigits {
			get { return _decPointPos > 0 ? _decPointPos : 1; }
		}

		private int DecimalDigits {
			get { return _digitsLen > _decPointPos ? _digitsLen - _decPointPos : 0; }
		}

		private bool IsFloatingSource {
			get { return _defPrecision == DoubleDefPrecision || _defPrecision == SingleDefPrecision; }
		}

		private bool IsZero {
			get { return _digitsLen == 0; }
		}

		private bool IsZeroInteger {
			get { return _digitsLen == 0 || _decPointPos <= 0; }
		}

		#endregion Helper properties

		#region Round

		private void RoundPos (int pos)
		{
			RoundBits (_digitsLen - pos);
		}

		private bool RoundDecimal (int decimals)
		{
			return RoundBits (_digitsLen - _decPointPos - decimals);
		}

		private bool RoundBits (int shift)
		{
			if (shift <= 0)
				return false;

			if (shift > _digitsLen) {
				_digitsLen = 0;
				_decPointPos = 1;
				_val1 = _val2 = _val3 = _val4 = 0;
				_positive = true;
				return false;
			}
			shift += _offset;
			_digitsLen += _offset;
			while (shift > 8) {
				_val1 = _val2;
				_val2 = _val3;
				_val3 = _val4;
				_val4 = 0;
				_digitsLen -= 8;
				shift -= 8;
			}
			shift = (shift - 1) << 2;
			uint v = _val1 >> shift;
			uint rem16 = v & 0xf;
			_val1 = (v ^ rem16) << shift;
			bool res = false;
			if (rem16 >= 0x5) {
				_val1 |= 0x99999999 >> (28 - shift);
				AddOneToDecHex ();
				int newlen = DecHexLen ();
				res = newlen != _digitsLen;
				_decPointPos = _decPointPos + newlen - _digitsLen;
				_digitsLen = newlen;
			}
			RemoveTrailingZeros ();
			return res;
		}

		private void RemoveTrailingZeros ()
		{
			_offset = CountTrailingZeros ();
			_digitsLen -= _offset;
			if (_digitsLen == 0) {
				_offset = 0;
				_decPointPos = 1;
				_positive = true;
			}
		}

		private void AddOneToDecHex ()
		{
			if (_val1 == 0x99999999) {
				_val1 = 0;
				if (_val2 == 0x99999999) {
					_val2 = 0;
					if (_val3 == 0x99999999) {
						_val3 = 0;
						_val4 = AddOneToDecHex (_val4);
					}
					else
						_val3 = AddOneToDecHex (_val3);
				}
				else
					_val2 = AddOneToDecHex (_val2);
			}
			else
				_val1 = AddOneToDecHex (_val1);
		}

		// Assume val != 0x99999999
		private static uint AddOneToDecHex (uint val)
		{
			if ((val & 0xffff) == 0x9999)
				if ((val & 0xffffff) == 0x999999)
					if ((val & 0xfffffff) == 0x9999999)
						return val + 0x06666667;
					else
						return val + 0x00666667;
				else if ((val & 0xfffff) == 0x99999)
					return val + 0x00066667;
				else
					return val + 0x00006667;
			else if ((val & 0xff) == 0x99)
				if ((val & 0xfff) == 0x999)
					return val + 0x00000667;
				else
					return val + 0x00000067;
			else if ((val & 0xf) == 0x9)
				return val + 0x00000007;
			else
				return val + 1;
		}

		private int CountTrailingZeros ()
		{
			if (_val1 != 0)
				return CountTrailingZeros (_val1);
			if (_val2 != 0)
				return CountTrailingZeros (_val2) + 8;
			if (_val3 != 0)
				return CountTrailingZeros (_val3) + 16;
			if (_val4 != 0)
				return CountTrailingZeros (_val4) + 24;
			return _digitsLen;
		}

		private static int CountTrailingZeros (uint val)
		{
			if ((val & 0xffff) == 0)
				if ((val & 0xffffff) == 0)
					if ((val & 0xfffffff) == 0)
						return 7;
					else
						return 6;
				else if ((val & 0xfffff) == 0)
					return 5;
				else
					return 4;
			else if ((val & 0xff) == 0)
				if ((val & 0xfff) == 0)
					return 3;
				else
					return 2;
			else if ((val & 0xf) == 0)
				return 1;
			else
				return 0;
		}

		#endregion Round

		#region public number formatting methods

		[ThreadStatic]
		static NumberFormatter threadNumberFormatter;

		[ThreadStatic]
		static NumberFormatter userFormatProvider;

		private static NumberFormatter GetInstance (IFormatProvider fp)
		{
			if (fp != null) {
				if (userFormatProvider == null) {
					Interlocked.CompareExchange (ref userFormatProvider, new NumberFormatter (null), null);
				}

				return userFormatProvider;
			}

			NumberFormatter res = threadNumberFormatter;
			threadNumberFormatter = null;
			if (res == null)
				return new NumberFormatter (Thread.CurrentThread);
			res.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			return res;
		}

		private void Release()
		{
			if (this != userFormatProvider)
				threadNumberFormatter = this;
		}

		public static string NumberToString (string format, uint value, IFormatProvider fp)
		{
			NumberFormatter inst = GetInstance (fp);
			inst.Init (format, value, Int32DefPrecision);
			string res = inst.IntegerToString (format, fp);
			inst.Release();
			return res;
		}

		public static string NumberToString (string format, int value, IFormatProvider fp)
		{
			NumberFormatter inst = GetInstance (fp);
			inst.Init (format, value, UInt32DefPrecision);
			string res = inst.IntegerToString (format, fp);
			inst.Release();
			return res;
		}

		public static string NumberToString (string format, ulong value, IFormatProvider fp)
		{
			NumberFormatter inst = GetInstance (fp);
			inst.Init (format, value);
			string res = inst.IntegerToString (format, fp);
			inst.Release();
			return res;
		}

		public static string NumberToString (string format, long value, IFormatProvider fp)
		{
			NumberFormatter inst = GetInstance (fp);
			inst.Init (format, value);
			string res = inst.IntegerToString (format, fp);
			inst.Release();
			return res;
		}

		public static string NumberToString (string format, float value, IFormatProvider fp)
		{
			NumberFormatter inst = GetInstance (fp);
			inst.Init (format, value, SingleDefPrecision);
			NumberFormatInfo nfi = inst.GetNumberFormatInstance (fp);
			string res;
			if (inst._NaN)
				res = nfi.NaNSymbol;
			else if (inst._infinity)
				if (inst._positive)
					res = nfi.PositiveInfinitySymbol;
				else
					res = nfi.NegativeInfinitySymbol;
			else if (inst._specifier == 'R')
				res = inst.FormatRoundtrip (value, nfi);
			else
				res = inst.NumberToString (format, nfi);
			inst.Release();
			return res;
		}

		public static string NumberToString (string format, double value, IFormatProvider fp)
		{
			NumberFormatter inst = GetInstance (fp);
			inst.Init (format, value, DoubleDefPrecision);
			NumberFormatInfo nfi = inst.GetNumberFormatInstance (fp);
			string res;
			if (inst._NaN)
				res = nfi.NaNSymbol;
			else if (inst._infinity)
				if (inst._positive)
					res = nfi.PositiveInfinitySymbol;
				else
					res = nfi.NegativeInfinitySymbol;
			else if (inst._specifier == 'R')
				res = inst.FormatRoundtrip (value, nfi);
			else
				res = inst.NumberToString (format, nfi);
			inst.Release();
			return res;
		}

		public static string NumberToString (string format, decimal value, IFormatProvider fp)
		{
			NumberFormatter inst = GetInstance (fp);
			inst.Init (format, value);
			string res = inst.NumberToString (format, inst.GetNumberFormatInstance (fp));
			inst.Release();
			return res;
		}

		private string IntegerToString (string format, IFormatProvider fp)
		{
			NumberFormatInfo nfi = GetNumberFormatInstance (fp);
			switch (_specifier) {
			case 'C':
				return FormatCurrency (_precision, nfi);
			case 'D':
				return FormatDecimal (_precision, nfi);
			case 'E':
				return FormatExponential (_precision, nfi);
			case 'F':
				return FormatFixedPoint (_precision, nfi);
			case 'G':
				if (_precision <= 0)
					return FormatDecimal (-1, nfi);
				return FormatGeneral (_precision, nfi);
			case 'N':
				return FormatNumber (_precision, nfi);
			case 'P':
				return FormatPercent (_precision, nfi);
			case 'X':
				return FormatHexadecimal (_precision);
			default:
				if (_isCustomFormat)
					return FormatCustom (format, nfi);
				throw new FormatException ("The specified format '" + format + "' is invalid");
			}
		}

		private string NumberToString (string format, NumberFormatInfo nfi)
		{
			switch (_specifier) {
			case 'C':
				return FormatCurrency (_precision, nfi);
			case 'E':
				return FormatExponential (_precision, nfi);
			case 'F':
				return FormatFixedPoint (_precision, nfi);
			case 'G':
				return FormatGeneral (_precision, nfi);
			case 'N':
				return FormatNumber (_precision, nfi);
			case 'P':
				return FormatPercent (_precision, nfi);
			case 'X':
			default:
				if (_isCustomFormat)
					return FormatCustom (format, nfi);
				throw new FormatException ("The specified format '" + format + "' is invalid");
			}
		}

		string FormatCurrency (int precision, NumberFormatInfo nfi)
		{
			precision = (precision >= 0 ? precision : nfi.CurrencyDecimalDigits);
			RoundDecimal (precision);
			ResetCharBuf (IntegerDigits * 2 + precision * 2 + 16);

			if (_positive) {
				switch (nfi.CurrencyPositivePattern) {
				case 0:
					Append (nfi.CurrencySymbol);
					break;
				case 2:
					Append (nfi.CurrencySymbol);
					Append (' ');
					break;
				}
			}
			else {
				switch (nfi.CurrencyNegativePattern) {
				case 0:
					Append ('(');
					Append (nfi.CurrencySymbol);
					break;
				case 1:
					Append (nfi.NegativeSign);
					Append (nfi.CurrencySymbol);
					break;
				case 2:
					Append (nfi.CurrencySymbol);
					Append (nfi.NegativeSign);
					break;
				case 3:
					Append (nfi.CurrencySymbol);
					break;
				case 4:
					Append ('(');
					break;
				case 5:
					Append (nfi.NegativeSign);
					break;
				case 8:
					Append (nfi.NegativeSign);
					break;
				case 9:
					Append (nfi.NegativeSign);
					Append (nfi.CurrencySymbol);
					Append (' ');
					break;
				case 11:
					Append (nfi.CurrencySymbol);
					Append (' ');
					break;
				case 12:
					Append (nfi.CurrencySymbol);
					Append (' ');
					Append (nfi.NegativeSign);
					break;
				case 14:
					Append ('(');
					Append (nfi.CurrencySymbol);
					Append (' ');
					break;
				case 15:
					Append ('(');
					break;
				}
			}

			AppendIntegerStringWithGroupSeparator (nfi.CurrencyGroupSizes, nfi.CurrencyGroupSeparator);

			if (precision > 0) {
				Append (nfi.CurrencyDecimalSeparator);
				AppendDecimalString (precision);
			}

			if (_positive) {
				switch (nfi.CurrencyPositivePattern) {
				case 1:
					Append (nfi.CurrencySymbol);
					break;
				case 3:
					Append (' ');
					Append (nfi.CurrencySymbol);
					break;
				}
			}
			else {
				switch (nfi.CurrencyNegativePattern) {
				case 0:
					Append (')');
					break;
				case 3:
					Append (nfi.NegativeSign);
					break;
				case 4:
					Append (nfi.CurrencySymbol);
					Append (')');
					break;
				case 5:
					Append (nfi.CurrencySymbol);
					break;
				case 6:
					Append (nfi.NegativeSign);
					Append (nfi.CurrencySymbol);
					break;
				case 7:
					Append (nfi.CurrencySymbol);
					Append (nfi.NegativeSign);
					break;
				case 8:
					Append (' ');
					Append (nfi.CurrencySymbol);
					break;
				case 10:
					Append (' ');
					Append (nfi.CurrencySymbol);
					Append (nfi.NegativeSign);
					break;
				case 11:
					Append (nfi.NegativeSign);
					break;
				case 13:
					Append (nfi.NegativeSign);
					Append (' ');
					Append (nfi.CurrencySymbol);
					break;
				case 14:
					Append (')');
					break;
				case 15:
					Append (' ');
					Append (nfi.CurrencySymbol);
					Append (')');
					break;
				}
			}

			return new string (_cbuf, 0, _ind);
		}

		private string FormatDecimal (int precision, NumberFormatInfo nfi)
		{
			if (precision < _digitsLen)
				precision = _digitsLen;
			if (precision == 0)
				return "0";

			ResetCharBuf (precision + 1);
			if (!_positive)
				Append (nfi.NegativeSign);
			AppendDigits (0, precision);

			return new string (_cbuf, 0, _ind);
		}

		unsafe private string FormatHexadecimal (int precision)
		{
			int size = Math.Max (precision, _decPointPos);
			char* digits = _specifierIsUpper ? DigitUpperTable : DigitLowerTable;

			ResetCharBuf (size);
			_ind = size;
			ulong val = _val1 | ((ulong)_val2 << 32);
			while (size > 0) {
				_cbuf [--size] = digits [val & 0xf];
				val >>= 4;
			}
			return new string (_cbuf, 0, _ind);
		}

		string FormatFixedPoint (int precision, NumberFormatInfo nfi)
		{
			if (precision == -1)
				precision = nfi.NumberDecimalDigits;

			RoundDecimal (precision);

			ResetCharBuf (IntegerDigits + precision + 2);

			if (!_positive)
				Append (nfi.NegativeSign);

			AppendIntegerString (IntegerDigits);

			if (precision > 0) {
				Append (nfi.NumberDecimalSeparator);
				AppendDecimalString (precision);
			}

			return new string (_cbuf, 0, _ind);
		}

		private string FormatRoundtrip (double origval, NumberFormatInfo nfi)
		{
			NumberFormatter nfc = GetClone ();
			if (origval >= MinRoundtripVal && origval <= MaxRoundtripVal) {
				string shortRep = FormatGeneral (_defPrecision, nfi);
				if (origval == Double.Parse (shortRep, nfi))
					return shortRep;
			}
			return nfc.FormatGeneral (_defPrecision + 2, nfi);
		}

		private string FormatRoundtrip (float origval, NumberFormatInfo nfi)
		{
			NumberFormatter nfc = GetClone ();
			string shortRep = FormatGeneral (_defPrecision, nfi);
			// Check roundtrip only for "normal" double values.
			if (origval == Single.Parse (shortRep, nfi))
				return shortRep;
			return nfc.FormatGeneral (_defPrecision + 2, nfi);
		}

		private string FormatGeneral (int precision, NumberFormatInfo nfi)
		{
			bool enableExp;
			if (precision == -1) {
				enableExp = IsFloatingSource;
				precision = _defPrecision;
			}
			else {
				enableExp = true;
				if (precision == 0)
					precision = _defPrecision;
				RoundPos (precision);
			}

			int intDigits = _decPointPos;
			int digits = _digitsLen;
			int decDigits = digits - intDigits;

			if ((intDigits > precision || intDigits <= -4) && enableExp)
				return FormatExponential (digits - 1, nfi, 2);

			if (decDigits < 0)
				decDigits = 0;
			if (intDigits < 0)
				intDigits = 0;
			ResetCharBuf (decDigits + intDigits + 3);

			if (!_positive)
				Append (nfi.NegativeSign);

			if (intDigits == 0)
				Append ('0');
			else
				AppendDigits (digits - intDigits, digits);

			if (decDigits > 0) {
				Append (nfi.NumberDecimalSeparator);
				AppendDigits (0, decDigits);
			}

			return new string (_cbuf, 0, _ind);
		}

		string FormatNumber (int precision, NumberFormatInfo nfi)
		{
			precision = (precision >= 0 ? precision : nfi.NumberDecimalDigits);
			ResetCharBuf (IntegerDigits * 3 + precision);
			RoundDecimal (precision);

			if (!_positive) {
				switch (nfi.NumberNegativePattern) {
				case 0:
					Append ('(');
					break;
				case 1:
					Append (nfi.NegativeSign);
					break;
				case 2:
					Append (nfi.NegativeSign);
					Append (' ');
					break;
				}
			}

			AppendIntegerStringWithGroupSeparator (nfi.NumberGroupSizes, nfi.NumberGroupSeparator);

			if (precision > 0) {
				Append (nfi.NumberDecimalSeparator);
				AppendDecimalString (precision);
			}

			if (!_positive) {
				switch (nfi.NumberNegativePattern) {
				case 0:
					Append (')');
					break;
				case 3:
					Append (nfi.NegativeSign);
					break;
				case 4:
					Append (' ');
					Append (nfi.NegativeSign);
					break;
				}
			}

			return new string (_cbuf, 0, _ind);
		}

		string FormatPercent (int precision, NumberFormatInfo nfi)
		{
			precision = (precision >= 0 ? precision : nfi.PercentDecimalDigits);
			Multiply10(2);
			RoundDecimal (precision);
			ResetCharBuf (IntegerDigits * 2 + precision + 16);

			if (_positive) {
				if (nfi.PercentPositivePattern == 2)
					Append (nfi.PercentSymbol);
			}
			else {
				switch (nfi.PercentNegativePattern) {
				case 0:
					Append (nfi.NegativeSign);
					break;
				case 1:
					Append (nfi.NegativeSign);
					break;
				case 2:
					Append (nfi.NegativeSign);
					Append (nfi.PercentSymbol);
					break;
				}
			}

			AppendIntegerStringWithGroupSeparator (nfi.PercentGroupSizes, nfi.PercentGroupSeparator);

			if (precision > 0) {
				Append (nfi.PercentDecimalSeparator);
				AppendDecimalString (precision);
			}

			if (_positive) {
				switch (nfi.PercentPositivePattern) {
				case 0:
					Append (' ');
					Append (nfi.PercentSymbol);
					break;
				case 1:
					Append (nfi.PercentSymbol);
					break;
				}
			}
			else {
				switch (nfi.PercentNegativePattern) {
				case 0:
					Append (' ');
					Append (nfi.PercentSymbol);
					break;
				case 1:
					Append (nfi.PercentSymbol);
					break;
				}
			}

			return new string (_cbuf, 0, _ind);
		}

		 string FormatExponential (int precision, NumberFormatInfo nfi)
		{
			if (precision == -1)
				precision = DefaultExpPrecision;

			RoundPos (precision + 1);
			return FormatExponential (precision, nfi, 3);
		}

		private string FormatExponential (int precision, NumberFormatInfo nfi, int expDigits)
		{
			int decDigits = _decPointPos;
			int digits = _digitsLen;
			int exponent = decDigits - 1;
			decDigits = _decPointPos = 1;

			ResetCharBuf (precision + 8);

			if (!_positive)
				Append (nfi.NegativeSign);

			AppendOneDigit (digits - 1);

			if (precision > 0) {
				Append (nfi.NumberDecimalSeparator);
				AppendDigits (digits - precision - 1, digits - _decPointPos);
			}

			AppendExponent (nfi, exponent, expDigits);

			return new string (_cbuf, 0, _ind);
		}

		string FormatCustom (string format, NumberFormatInfo nfi)
		{
			bool p = _positive;
			int offset = 0;
			int length = 0;
			CustomInfo.GetActiveSection (format, ref p, IsZero, ref offset, ref length);
			if (length == 0)
				return _positive ? string.Empty : nfi.NegativeSign;
			_positive = p;

			CustomInfo info = CustomInfo.Parse (format, offset, length, nfi);
#if false
			Console.WriteLine ("Format : {0}",format);
			Console.WriteLine ("DecimalDigits : {0}",info.DecimalDigits);
			Console.WriteLine ("DecimalPointPos : {0}",info.DecimalPointPos);
			Console.WriteLine ("DecimalTailSharpDigits : {0}",info.DecimalTailSharpDigits);
			Console.WriteLine ("IntegerDigits : {0}",info.IntegerDigits);
			Console.WriteLine ("IntegerHeadSharpDigits : {0}",info.IntegerHeadSharpDigits);
			Console.WriteLine ("IntegerHeadPos : {0}",info.IntegerHeadPos);
			Console.WriteLine ("UseExponent : {0}",info.UseExponent);
			Console.WriteLine ("ExponentDigits : {0}",info.ExponentDigits);
			Console.WriteLine ("ExponentTailSharpDigits : {0}",info.ExponentTailSharpDigits);
			Console.WriteLine ("ExponentNegativeSignOnly : {0}",info.ExponentNegativeSignOnly);
			Console.WriteLine ("DividePlaces : {0}",info.DividePlaces);
			Console.WriteLine ("Percents : {0}",info.Percents);
			Console.WriteLine ("Permilles : {0}",info.Permilles);
#endif
			StringBuilder sb_int = new StringBuilder (info.IntegerDigits * 2);
			StringBuilder sb_dec = new StringBuilder (info.DecimalDigits * 2);
			StringBuilder sb_exp = (info.UseExponent ? new StringBuilder (info.ExponentDigits * 2) : null);

			int diff = 0;
			if (info.Percents > 0)
				Multiply10(2 * info.Percents);
			if (info.Permilles > 0)
				Multiply10(3 * info.Permilles);
			if (info.DividePlaces > 0)
				Divide10(info.DividePlaces);

			bool expPositive = true;
			if (info.UseExponent && (info.DecimalDigits > 0 || info.IntegerDigits > 0)) {
				if (!IsZero) {
					RoundPos (info.DecimalDigits + info.IntegerDigits);
					diff -= _decPointPos - info.IntegerDigits;
					_decPointPos = info.IntegerDigits;
				}

				expPositive = diff <= 0;
				AppendNonNegativeNumber (sb_exp, diff < 0 ? -diff : diff);
			}
			else
				RoundDecimal (info.DecimalDigits);

			if (info.IntegerDigits != 0 || !IsZeroInteger)
				AppendIntegerString (IntegerDigits, sb_int);

			AppendDecimalString (DecimalDigits, sb_dec);

			if (info.UseExponent) {
				if (info.DecimalDigits <= 0 && info.IntegerDigits <= 0)
					_positive = true;

				if (sb_int.Length < info.IntegerDigits)
					sb_int.Insert (0, "0", info.IntegerDigits - sb_int.Length);

				while (sb_exp.Length < info.ExponentDigits - info.ExponentTailSharpDigits)
					sb_exp.Insert (0, '0');

				if (expPositive && !info.ExponentNegativeSignOnly)
					sb_exp.Insert (0, nfi.PositiveSign);
				else if (!expPositive)
					sb_exp.Insert (0, nfi.NegativeSign);
			}
			else {
				if (sb_int.Length < info.IntegerDigits - info.IntegerHeadSharpDigits)
					sb_int.Insert (0, "0", info.IntegerDigits - info.IntegerHeadSharpDigits - sb_int.Length);
				if (info.IntegerDigits == info.IntegerHeadSharpDigits && IsZeroOnly (sb_int))
					sb_int.Remove (0, sb_int.Length);
			}

			ZeroTrimEnd (sb_dec, true);
			while (sb_dec.Length < info.DecimalDigits - info.DecimalTailSharpDigits)
				sb_dec.Append ('0');
			if (sb_dec.Length > info.DecimalDigits)
				sb_dec.Remove (info.DecimalDigits, sb_dec.Length - info.DecimalDigits);

			return info.Format (format, offset, length, nfi, _positive, sb_int, sb_dec, sb_exp);
		}
		#endregion public number formatting methods

		#region StringBuilder formatting helpers

		private static void ZeroTrimEnd (StringBuilder sb, bool canEmpty)
		{
			int len = 0;
			for (int i = sb.Length - 1; (canEmpty ? i >= 0 : i > 0); i--) {
				if (sb [i] != '0')
					break;
				len++;
			}

			if (len > 0)
				sb.Remove (sb.Length - len, len);
		}

		private static bool IsZeroOnly (StringBuilder sb)
		{
			for (int i = 0; i < sb.Length; i++)
				if (char.IsDigit (sb [i]) && sb [i] != '0')
					return false;
			return true;
		}

		private static void AppendNonNegativeNumber (StringBuilder sb, int v)
		{
			if (v < 0)
				throw new ArgumentException ();

			int i = ScaleOrder (v) - 1;
			do {
				int n = v / (int)GetTenPowerOf (i);
				sb.Append ((char)('0' | n));
				v -= (int)GetTenPowerOf (i--) * n;
			} while (i >= 0);
		}

		#endregion StringBuilder formatting helpers

		#region Append helpers

		private void AppendIntegerString (int minLength, StringBuilder sb)
		{
			if (_decPointPos <= 0) {
				sb.Append ('0', minLength);
				return;
			}

			if (_decPointPos < minLength)
				sb.Append ('0', minLength - _decPointPos);

			AppendDigits (_digitsLen - _decPointPos, _digitsLen, sb);
		}

		private void AppendIntegerString (int minLength)
		{
			if (_decPointPos <= 0) {
				Append ('0', minLength);
				return;
			}

			if (_decPointPos < minLength)
				Append ('0', minLength - _decPointPos);

			AppendDigits (_digitsLen - _decPointPos, _digitsLen);
		}

		private void AppendDecimalString (int precision, StringBuilder sb)
		{
			AppendDigits (_digitsLen - precision - _decPointPos, _digitsLen - _decPointPos, sb);
		}

		private void AppendDecimalString (int precision)
		{
			AppendDigits (_digitsLen - precision - _decPointPos, _digitsLen - _decPointPos);
		}

		private void AppendIntegerStringWithGroupSeparator (int[] groups, string groupSeparator)
		{
			if (IsZeroInteger) {
				Append ('0');
				return;
			}

			int total = 0;
			int groupIndex = 0;
			for (int i = 0; i < groups.Length; i++) {
				total += groups [i];
				if (total <= _decPointPos)
					groupIndex = i;
				else
					break;
			}

			if (groups.Length > 0 && total > 0) {
				int counter;
				int groupSize = groups [groupIndex];
				int fraction = _decPointPos > total ? _decPointPos - total : 0;
				if (groupSize == 0) {
					while (groupIndex >= 0 && groups [groupIndex] == 0)
						groupIndex--;

					groupSize = fraction > 0 ? fraction : groups [groupIndex];
				}
				if (fraction == 0)
					counter = groupSize;
				else {
					groupIndex += fraction / groupSize;
					counter = fraction % groupSize;
					if (counter == 0)
						counter = groupSize;
					else
						groupIndex++;
				}

				if (total >= _decPointPos) {
					int lastGroupSize = groups [0];
					if (total > lastGroupSize) {
						int lastGroupDiff = -(lastGroupSize - _decPointPos);
						int lastGroupMod;

						if (lastGroupDiff < lastGroupSize)
							counter = lastGroupDiff;
						else if (lastGroupSize > 0 && (lastGroupMod = _decPointPos % lastGroupSize) > 0)
							counter = lastGroupMod;
					}
				}
				
				for (int i = 0; ;) {
					if ((_decPointPos - i) <= counter || counter == 0) {
						AppendDigits (_digitsLen - _decPointPos, _digitsLen - i);
						break;
					}
					AppendDigits (_digitsLen - i - counter, _digitsLen - i);
					i += counter;
					Append (groupSeparator);
					if (--groupIndex < groups.Length && groupIndex >= 0)
						groupSize = groups [groupIndex];
					counter = groupSize;
				}
			}
			else {
				AppendDigits (_digitsLen - _decPointPos, _digitsLen);
			}
		}

		// minDigits is in the range 1..3
		private void AppendExponent (NumberFormatInfo nfi, int exponent, int minDigits)
		{
			if (_specifierIsUpper || _specifier == 'R')
				Append ('E');
			else
				Append ('e');

			if (exponent >= 0)
				Append (nfi.PositiveSign);
			else {
				Append (nfi.NegativeSign);
				exponent = -exponent;
			}

			if (exponent == 0)
				Append ('0', minDigits);
			else if (exponent < 10) {
				Append ('0', minDigits - 1);
				Append ((char)('0' | exponent));
			}
			else {
				uint hexDigit = FastToDecHex (exponent);
				if (exponent >= 100 || minDigits == 3)
					Append ((char)('0' | (hexDigit >> 8)));
				Append ((char)('0' | ((hexDigit >> 4) & 0xf)));
				Append ((char)('0' | (hexDigit & 0xf)));
			}
		}

		private void AppendOneDigit (int start)
		{
			if (_ind == _cbuf.Length)
				Resize (_ind + 10);

			start += _offset;
			uint v;
			if (start < 0)
				v = 0;
			else if (start < 8)
				v = _val1;
			else if (start < 16)
				v = _val2;
			else if (start < 24)
				v = _val3;
			else if (start < 32)
				v = _val4;
			else
				v = 0;
			v >>= (start & 0x7) << 2;
			_cbuf [_ind++] = (char)('0' | v & 0xf);
		}

		private void AppendDigits (int start, int end)
		{
			if (start >= end)
				return;

			int i = _ind + (end - start);
			if (i > _cbuf.Length)
				Resize (i + 10);
			_ind = i;

			end += _offset;
			start += _offset;

			for (int next = start + 8 - (start & 0x7); ; start = next, next += 8) {
				uint v;
				if (next == 8)
					v = _val1;
				else if (next == 16)
					v = _val2;
				else if (next == 24)
					v = _val3;
				else if (next == 32)
					v = _val4;
				else
					v = 0;
				v >>= (start & 0x7) << 2;
				if (next > end)
					next = end;

				_cbuf [--i] = (char)('0' | v & 0xf);
				switch (next - start) {
				case 8:
					_cbuf [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 7;
				case 7:
					_cbuf [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 6;
				case 6:
					_cbuf [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 5;
				case 5:
					_cbuf [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 4;
				case 4:
					_cbuf [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 3;
				case 3:
					_cbuf [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 2;
				case 2:
					_cbuf [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 1;
				case 1:
					if (next == end)
						return;
					continue;
				}
			}
		}

		private void AppendDigits (int start, int end, StringBuilder sb)
		{
			if (start >= end)
				return;

			int i = sb.Length + (end - start);
			sb.Length = i;

			end += _offset;
			start += _offset;

			for (int next = start + 8 - (start & 0x7); ; start = next, next += 8) {
				uint v;
				if (next == 8)
					v = _val1;
				else if (next == 16)
					v = _val2;
				else if (next == 24)
					v = _val3;
				else if (next == 32)
					v = _val4;
				else
					v = 0;
				v >>= (start & 0x7) << 2;
				if (next > end)
					next = end;
				sb [--i] = (char)('0' | v & 0xf);
				switch (next - start) {
				case 8:
					sb [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 7;
				case 7:
					sb [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 6;
				case 6:
					sb [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 5;
				case 5:
					sb [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 4;
				case 4:
					sb [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 3;
				case 3:
					sb [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 2;
				case 2:
					sb [--i] = (char)('0' | (v >>= 4) & 0xf);
					goto case 1;
				case 1:
					if (next == end)
						return;
					continue;
				}
			}
		}

		#endregion Append helpers

		#region others

		private void Multiply10(int count)
		{
			if (count <= 0 || _digitsLen == 0)
				return;

			_decPointPos += count;
		}

		private void Divide10(int count)
		{
			if (count <= 0 || _digitsLen == 0)
				return;

			_decPointPos -= count;
		}

		private NumberFormatter GetClone ()
		{
			return (NumberFormatter)this.MemberwiseClone ();
		}

		#endregion others

		#region custom

		private class CustomInfo
		{
			public bool UseGroup = false;
			public int DecimalDigits = 0;
			public int DecimalPointPos = -1;
			public int DecimalTailSharpDigits = 0;
			public int IntegerDigits = 0;
			public int IntegerHeadSharpDigits = 0;
			public int IntegerHeadPos = 0;
			public bool UseExponent = false;
			public int ExponentDigits = 0;
			public int ExponentTailSharpDigits = 0;
			public bool ExponentNegativeSignOnly = true;
			public int DividePlaces = 0;
			public int Percents = 0;
			public int Permilles = 0;

			public static void GetActiveSection (string format, ref bool positive, bool zero, ref int offset, ref int length)
			{
				int[] lens = new int [3];
				int index = 0;
				int lastPos = 0;
				bool quoted = false;

				for (int i = 0; i < format.Length; i++) {
					char c = format [i];

					if (c == '\"' || c == '\'') {
						if (i == 0 || format [i - 1] != '\\')
							quoted = !quoted;

						continue;
					}

					if (c == ';' && !quoted && (i == 0 || format [i - 1] != '\\')) {
						lens [index++] = i - lastPos;
						lastPos = i + 1;
						if (index == 3)
							break;
					}
				}

				if (index == 0) {
					offset = 0;
					length = format.Length;
					return;
				}
				if (index == 1) {
					if (positive || zero) {
						offset = 0;
						length = lens [0];
						return;
					}
					if (lens [0] + 1 < format.Length) {
						positive = true;
						offset = lens [0] + 1;
						length = format.Length - offset;
						return;
					}
					else {
						offset = 0;
						length = lens [0];
						return;
					}
				}
				if (zero) {
					if (index == 2) {
						if (format.Length - lastPos == 0) {
							offset = 0;
							length = lens [0];
						} else {
							offset = lens [0] + lens [1] + 2;
							length = format.Length - offset;
						}
						return;
					}

					if (lens [2] == 0) {
						offset = 0;
						length = lens [0];
					} else {
						offset = lens [0] + lens [1] + 2;
						length = lens [2];
					}

					return;

				}
				if (positive) {
					offset = 0;
					length = lens [0];
					return;
				}
				if (lens [1] > 0) {
					positive = true;
					offset = lens [0] + 1;
					length = lens [1];
					return;
				}
				offset = 0;
				length = lens [0];
			}

			public static CustomInfo Parse (string format, int offset, int length, NumberFormatInfo nfi)
			{
				char literal = '\0';
				bool integerArea = true;
				bool decimalArea = false;
				bool exponentArea = false;
				bool sharpContinues = true;

				CustomInfo info = new CustomInfo ();
				int groupSeparatorCounter = 0;

				for (int i = offset; i - offset < length; i++) {
					char c = format [i];

					if (c == literal && c != '\0') {
						literal = '\0';
						continue;
					}
					if (literal != '\0')
						continue;

					if (exponentArea && (c != '\0' && c != '0' && c != '#')) {
						exponentArea = false;
						integerArea = (info.DecimalPointPos < 0);
						decimalArea = !integerArea;
						i--;
						continue;
					}

					switch (c) {
					case '\\':
						i++;
						continue;
					case '\'':
					case '\"':
						if (c == '\"' || c == '\'') {
							literal = c;
						}
						continue;
					case '#':
						if (sharpContinues && integerArea)
							info.IntegerHeadSharpDigits++;
						else if (decimalArea)
							info.DecimalTailSharpDigits++;
						else if (exponentArea)
							info.ExponentTailSharpDigits++;

						goto case '0';
					case '0':
						if (c != '#') {
							sharpContinues = false;
							if (decimalArea)
								info.DecimalTailSharpDigits = 0;
							else if (exponentArea)
								info.ExponentTailSharpDigits = 0;
						}
						if (info.IntegerHeadPos == -1)
							info.IntegerHeadPos = i;

						if (integerArea) {
							info.IntegerDigits++;
							if (groupSeparatorCounter > 0)
								info.UseGroup = true;
							groupSeparatorCounter = 0;
						}
						else if (decimalArea)
							info.DecimalDigits++;
						else if (exponentArea)
							info.ExponentDigits++;
						break;
					case 'e':
					case 'E':
						if (info.UseExponent)
							break;

						info.UseExponent = true;
						integerArea = false;
						decimalArea = false;
						exponentArea = true;
						if (i + 1 - offset < length) {
							char nc = format [i + 1];
							if (nc == '+')
								info.ExponentNegativeSignOnly = false;
							if (nc == '+' || nc == '-')
								i++;
							else if (nc != '0' && nc != '#') {
								info.UseExponent = false;
								if (info.DecimalPointPos < 0)
									integerArea = true;
							}
						}

						break;
					case '.':
						integerArea = false;
						decimalArea = true;
						exponentArea = false;
						if (info.DecimalPointPos == -1)
							info.DecimalPointPos = i;
						break;
					case '%':
						info.Percents++;
						break;
					case '\u2030':
						info.Permilles++;
						break;
					case ',':
						if (integerArea && info.IntegerDigits > 0)
							groupSeparatorCounter++;
						break;
					default:
						break;
					}
				}

				if (info.ExponentDigits == 0)
					info.UseExponent = false;
				else
					info.IntegerHeadSharpDigits = 0;

				if (info.DecimalDigits == 0)
					info.DecimalPointPos = -1;

				info.DividePlaces += groupSeparatorCounter * 3;

				return info;
			}

			public string Format (string format, int offset, int length, NumberFormatInfo nfi, bool positive, StringBuilder sb_int, StringBuilder sb_dec, StringBuilder sb_exp)
			{
				StringBuilder sb = new StringBuilder ();
				char literal = '\0';
				bool integerArea = true;
				bool decimalArea = false;
				int intSharpCounter = 0;
				int sb_int_index = 0;
				int sb_dec_index = 0;

				int[] groups = nfi.NumberGroupSizes;
				string groupSeparator = nfi.NumberGroupSeparator;
				int intLen = 0, total = 0, groupIndex = 0, counter = 0, groupSize = 0;
				if (UseGroup && groups.Length > 0) {
					intLen = sb_int.Length;
					for (int i = 0; i < groups.Length; i++) {
						total += groups [i];
						if (total <= intLen)
							groupIndex = i;
					}
					groupSize = groups [groupIndex];
					int fraction = intLen > total ? intLen - total : 0;
					if (groupSize == 0) {
						while (groupIndex >= 0 && groups [groupIndex] == 0)
							groupIndex--;

						groupSize = fraction > 0 ? fraction : groups [groupIndex];
					}
					if (fraction == 0)
						counter = groupSize;
					else {
						groupIndex += fraction / groupSize;
						counter = fraction % groupSize;
						if (counter == 0)
							counter = groupSize;
						else
							groupIndex++;
					}
				}
				else
					UseGroup = false;

				for (int i = offset; i - offset < length; i++) {
					char c = format [i];

					if (c == literal && c != '\0') {
						literal = '\0';
						continue;
					}
					if (literal != '\0') {
						sb.Append (c);
						continue;
					}

					switch (c) {
					case '\\':
						i++;
						if (i - offset < length)
							sb.Append (format [i]);
						continue;
					case '\'':
					case '\"':
						if (c == '\"' || c == '\'')
							literal = c;
						continue;
					case '#':
						goto case '0';
					case '0':
						if (integerArea) {
							intSharpCounter++;
							if (IntegerDigits - intSharpCounter < sb_int.Length + sb_int_index || c == '0')
								while (IntegerDigits - intSharpCounter + sb_int_index < sb_int.Length) {
									sb.Append (sb_int [sb_int_index++]);
									if (UseGroup && --intLen > 0 && --counter == 0) {
										sb.Append (groupSeparator);
										if (--groupIndex < groups.Length && groupIndex >= 0)
											groupSize = groups [groupIndex];
										counter = groupSize;
									}
								}
							break;
						}
						else if (decimalArea) {
							if (sb_dec_index < sb_dec.Length)
								sb.Append (sb_dec [sb_dec_index++]);
							break;
						}

						sb.Append (c);
						break;
					case 'e':
					case 'E':
						if (sb_exp == null || !UseExponent) {
							sb.Append (c);
							break;
						}

						bool flag1 = true;
						bool flag2 = false;

						int q;
						for (q = i + 1; q - offset < length; q++) {
							if (format [q] == '0') {
								flag2 = true;
								continue;
							}
							if (q == i + 1 && (format [q] == '+' || format [q] == '-'))
								continue;
							if (!flag2)
								flag1 = false;
							break;
						}

						if (flag1) {
							i = q - 1;
							integerArea = (DecimalPointPos < 0);
							decimalArea = !integerArea;

							sb.Append (c);
							sb.Append (sb_exp);
							sb_exp = null;
						}
						else
							sb.Append (c);

						break;
					case '.':
						if (DecimalPointPos == i) {
							if (DecimalDigits > 0) {
								while (sb_int_index < sb_int.Length)
									sb.Append (sb_int [sb_int_index++]);
							}
							if (sb_dec.Length > 0)
								sb.Append (nfi.NumberDecimalSeparator);
						}
						integerArea = false;
						decimalArea = true;
						break;
					case ',':
						break;
					case '%':
						sb.Append (nfi.PercentSymbol);
						break;
					case '\u2030':
						sb.Append (nfi.PerMilleSymbol);
						break;
					default:
						sb.Append (c);
						break;
					}
				}

				if (!positive)
					sb.Insert (0, nfi.NegativeSign);

				return sb.ToString ();
			}
		}

		#endregion
	}
}
