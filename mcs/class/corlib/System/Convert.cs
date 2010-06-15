//
// System.Convert.cs
//
// Author:
//   Derek Holden (dholden@draper.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//
// System.Convert class. This was written word for word off the 
// Library specification for System.Convert in the ECMA TC39 TG2 
// and TG3 working documents. The first page of which has a table
// for all legal conversion scenerios.
//
// This header and the one above it can be formatted however, just trying
// to keep it consistent w/ the existing mcs headers.
//
// This Convert class could be written another way, with each type 
// implementing IConvertible and defining their own conversion functions,
// and this class just calling the type's implementation. Or, they can 
// be defined here and the implementing type can use these functions when 
// defining their IConvertible interface. Byte's ToBoolean() calls 
// Convert.ToBoolean(byte), or Convert.ToBoolean(byte) calls 
// byte.ToBoolean(). The first case is what is done here.
//
// See http://lists.ximian.com/archives/public/mono-list/2001-July/000525.html
//
// There are also conversion functions that are not defined in
// the ECMA draft, such as there is no bool ToBoolean(DateTime value), 
// and placing that somewhere won't compile w/ this Convert since the
// function doesn't exist. However calling that when using Microsoft's
// System.Convert doesn't produce any compiler errors, it just throws
// an InvalidCastException at runtime.
//
// Whenever a decimal, double, or single is converted to an integer
// based type, it is even rounded. This uses Math.Round which only 
// has Round(decimal) and Round(double), so in the Convert from 
// single cases the value is passed to Math as a double. This 
// may not be completely necessary.
//
// The .NET Framework SDK lists DBNull as a member of this class
// as 'public static readonly object DBNull;'. 
//
// It should also be decided if all the cast return values should be
// returned as unchecked or not.
//
// All the XML function comments were auto generated which is why they
// sound someone redundant.
//
// TYPE | BOOL BYTE CHAR DT DEC DBL I16 I32 I64 SBYT SNGL STR UI16 UI32 UI64
// -----+--------------------------------------------------------------------
// BOOL |   X    X           X   X   X   X   X    X    X   X    X    X    X
// BYTE |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// CHAR |        X    X              X   X   X    X        X    X    X    X
// DT   |                 X                                X
// DEC  |   X    X           X   X   X   X   X    X    X   X    X    X    X
// DBL  |   X    X           X   X   X   X   X    X    X   X    X    X    X
// I16  |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// I32  |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// I64  |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// SBYT |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// SNGL |   X    X           X   X   X   X   X    X    X   X    X    X    X
// STR  |   X    X    X   X  X   X   X   X   X    X    X   X    X    X    X
// UI16 |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// UI32 |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// UI64 |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System {
  
//	[CLSCompliant(false)]
	public static class Convert {

		// Fields
		public static readonly object DBNull = System.DBNull.Value;
	
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static byte [] InternalFromBase64String (string str, bool allowWhitespaceOnly);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static byte [] InternalFromBase64CharArray (char [] arr, int offset, int length);

		public static byte[] FromBase64CharArray (char[] inArray, int offset, int length)
		{
			if (inArray == null)
				throw new ArgumentNullException ("inArray");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset < 0");
			if (length < 0)
				throw new ArgumentOutOfRangeException ("length < 0");
			// avoid integer overflow
			if (offset > inArray.Length - length)
				throw new ArgumentOutOfRangeException ("offset + length > array.Length");

			return InternalFromBase64CharArray (inArray, offset, length);
		}

		public static byte[] FromBase64String (string s)
		{
			if (s == null)
				throw new ArgumentNullException ("s");

			if (s.Length == 0) {
				return new byte[0];
			}

			return InternalFromBase64String (s, true);
		}

		public static TypeCode GetTypeCode (object value)
		{
			if (value == null)
				return TypeCode.Empty;
			else 
				return Type.GetTypeCode (value.GetType ());
		}

		public static bool IsDBNull (object value)
		{
			if (value is DBNull)
				return true;
			else
				return false;
		}
		
		public static int ToBase64CharArray (byte[] inArray, int offsetIn, int length, 
						    char[] outArray, int offsetOut)
		{
			if (inArray == null)
				throw new ArgumentNullException ("inArray");
			if (outArray == null)
				throw new ArgumentNullException ("outArray");
			if (offsetIn < 0 || length < 0 || offsetOut < 0)
				throw new ArgumentOutOfRangeException ("offsetIn, length, offsetOut < 0");
			// avoid integer overflow
			if (offsetIn > inArray.Length - length)
				throw new ArgumentOutOfRangeException ("offsetIn + length > array.Length");

			// note: normally ToBase64Transform doesn't support multiple block processing
			byte[] outArr = ToBase64Transform.InternalTransformFinalBlock (inArray, offsetIn, length);
			
			char[] cOutArr = new ASCIIEncoding ().GetChars (outArr);
			
			// avoid integer overflow
			if (offsetOut > outArray.Length - cOutArr.Length)
				throw new ArgumentOutOfRangeException ("offsetOut + cOutArr.Length > outArray.Length");
			
			Array.Copy (cOutArr, 0, outArray, offsetOut, cOutArr.Length);
			
			return cOutArr.Length;
		}
		
		public static string ToBase64String (byte[] inArray)
		{
			if (inArray == null)
				throw new ArgumentNullException ("inArray");

			return ToBase64String (inArray, 0, inArray.Length);
		}
		
		public static string ToBase64String (byte[] inArray, int offset, int length)
		{
			if (inArray == null)
				throw new ArgumentNullException ("inArray");
			if (offset < 0 || length < 0)
				throw new ArgumentOutOfRangeException ("offset < 0 || length < 0");
			// avoid integer overflow
			if (offset > inArray.Length - length)
				throw new ArgumentOutOfRangeException ("offset + length > array.Length");
			
			// note: normally ToBase64Transform doesn't support multiple block processing
			byte[] outArr = ToBase64Transform.InternalTransformFinalBlock (inArray, offset, length);
			
			return (new ASCIIEncoding ().GetString (outArr));
		}

		[ComVisible (false)]
		public static string ToBase64String (byte[] inArray, Base64FormattingOptions options)
		{
			if (inArray == null)
				throw new ArgumentNullException ("inArray");
			return ToBase64String (inArray, 0, inArray.Length, options);
		}

		[ComVisible (false)]
		public static string ToBase64String (byte[] inArray, int offset, int length, Base64FormattingOptions options)
		{
			if (inArray == null)
				throw new ArgumentNullException ("inArray");
			if (offset < 0 || length < 0)
				throw new ArgumentOutOfRangeException ("offset < 0 || length < 0");
			// avoid integer overflow
			if (offset > inArray.Length - length)
				throw new ArgumentOutOfRangeException ("offset + length > array.Length");

			if (length == 0)
				return String.Empty;

			if (options == Base64FormattingOptions.InsertLineBreaks)
				return ToBase64StringBuilderWithLine (inArray, offset, length).ToString ();
			else
				return Encoding.ASCII.GetString (ToBase64Transform.InternalTransformFinalBlock (inArray, offset, length));
		}

		[ComVisible (false)]
		public static int ToBase64CharArray (byte[] inArray, int offsetIn, int length, 
						    char[] outArray, int offsetOut, Base64FormattingOptions options)
		{
			if (inArray == null)
				throw new ArgumentNullException ("inArray");
			if (outArray == null)
				throw new ArgumentNullException ("outArray");
			if (offsetIn < 0 || length < 0 || offsetOut < 0)
				throw new ArgumentOutOfRangeException ("offsetIn, length, offsetOut < 0");
			// avoid integer overflow
			if (offsetIn > inArray.Length - length)
				throw new ArgumentOutOfRangeException ("offsetIn + length > array.Length");

			if (length == 0)
				return 0;

			// note: normally ToBase64Transform doesn't support multiple block processing
			if (options == Base64FormattingOptions.InsertLineBreaks) {
				StringBuilder sb = ToBase64StringBuilderWithLine (inArray, offsetIn, length);
				sb.CopyTo (0, outArray, offsetOut, sb.Length);
				return sb.Length;
			} else {
				byte[] outArr = ToBase64Transform.InternalTransformFinalBlock (inArray, offsetIn, length);
			
				char[] cOutArr = Encoding.ASCII.GetChars (outArr);
			
				// avoid integer overflow
				if (offsetOut > outArray.Length - cOutArr.Length)
					throw new ArgumentOutOfRangeException ("offsetOut + cOutArr.Length > outArray.Length");
			
				Array.Copy (cOutArr, 0, outArray, offsetOut, cOutArr.Length);
				return cOutArr.Length;
			}
		}

		private const int MaxBytesPerLine = 57;

		static StringBuilder ToBase64StringBuilderWithLine (byte [] inArray, int offset, int length)
		{
			StringBuilder sb = new StringBuilder ();

			int remainder;
			int full = Math.DivRem (length, MaxBytesPerLine, out remainder);
			for (int i = 0; i < full; i ++) {
				byte[] data = ToBase64Transform.InternalTransformFinalBlock (inArray, offset, MaxBytesPerLine);
				sb.AppendLine (Encoding.ASCII.GetString (data));
				offset += MaxBytesPerLine;
			}
			// we never complete (i.e. the last line) with a new line
			if (remainder == 0) {
				int nll = Environment.NewLine.Length;
				sb.Remove (sb.Length - nll, nll);
			} else {
				byte[] data = ToBase64Transform.InternalTransformFinalBlock (inArray, offset, remainder);
				sb.Append (Encoding.ASCII.GetString (data));
			}
			return sb;
		}
		
		// ========== Boolean Conversions ========== //
	
		public static bool ToBoolean (bool value) 
		{ 
			return value; 
		}

		public static bool ToBoolean (byte value) 
		{ 
			return (value != 0); 
		}
 
		public static bool ToBoolean (char value)
		{
			throw new InvalidCastException (Locale.GetText ("Can't convert char to bool"));
		}
		
		public static bool ToBoolean (DateTime value)
		{
			throw new InvalidCastException (Locale.GetText ("Can't convert date to bool"));
		}
		
		public static bool ToBoolean (decimal value) 
		{ 
			return (value != 0M); 
		}

		public static bool ToBoolean (double value) 
		{ 
			return (value != 0); 
		}

		public static bool ToBoolean (float value) 
		{ 
			return (value != 0f); 
		}

		public static bool ToBoolean (int value) 
		{ 
			return (value != 0); 
		}

		public static bool ToBoolean (long value) 
		{ 
			return (value != 0); 
		}

		[CLSCompliant (false)]
		public static bool ToBoolean (sbyte value) 
		{ 
			return (value != 0); 
		}
	
		public static bool ToBoolean (short value) 
		{ 
			return (value != 0); 
		}

		public static bool ToBoolean (string value) 
		{
			if (value == null)
				return false; // LAMESPEC: Spec says throw ArgumentNullException
			return Boolean.Parse (value);
		}

		public static bool ToBoolean (string value, IFormatProvider provider)
		{
			if (value == null)
				return false; // LAMESPEC: Spec says throw ArgumentNullException
			return Boolean.Parse (value); // provider is ignored.
		}

		[CLSCompliant (false)]
		public static bool ToBoolean (uint value) 
		{ 
			return (value != 0);
		}

		[CLSCompliant (false)]
		public static bool ToBoolean (ulong value) 
		{ 
			return (value != 0); 
		}

		[CLSCompliant (false)]
		public static bool ToBoolean (ushort value) 
		{ 
			//if (value == null)
			//	return false;
			return (value != 0); 
		}

		public static bool ToBoolean (object value)
		{
			if (value == null)
				return false;
			return ToBoolean (value, null);
		}

		public static bool ToBoolean (object value, IFormatProvider provider)
		{
			if (value == null)
				return false;
			return ((IConvertible) value).ToBoolean (provider);
		}

		// ========== Byte Conversions ========== //
	
		public static byte ToByte (bool value) 
		{ 
			return (byte)(value ? 1 : 0); 
		}
	
		public static byte ToByte (byte value) 
		{ 
			return value; 
		}

		public static byte ToByte (char value) 
		{ 
			return checked ((byte) value);
		}

		public static byte ToByte (DateTime value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}
	
		public static byte ToByte (decimal value)
		{ 
			// Returned Even-Rounded
			return checked ((byte) Math.Round (value));
		}
	
		public static byte ToByte (double value) 
		{ 
			// Returned Even-Rounded
			return checked ((byte) Math.Round (value));
		}

		public static byte ToByte (float value) 
		{ 
			// Returned Even-Rounded, pass it as a double, could have this
			// method just call Convert.ToByte ( (double)value)
			return checked ((byte) Math.Round (value));
		}

		public static byte ToByte (int value) 
		{ 
			return checked ((byte) value); 
		}

		public static byte ToByte (long value) 
		{ 
			return checked ((byte) value);
		}

		[CLSCompliant (false)]
		public static byte ToByte (sbyte value) 
		{ 
			return checked ((byte) value);
		}
	
		public static byte ToByte (short value) 
		{ 
			return checked ((byte) value);
		}

		public static byte ToByte (string value) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return Byte.Parse (value);
		}

		public static byte ToByte (string value, IFormatProvider provider) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return Byte.Parse (value, provider);
		}

		public static byte ToByte (string value, int fromBase)
		{
			int retVal = ConvertFromBase (value, fromBase, true);

			return checked ((byte) retVal);
		}

		[CLSCompliant (false)]
		public static byte ToByte (uint value) 
		{ 
			return checked ((byte) value);
		}

		[CLSCompliant (false)]
		public static byte ToByte (ulong value) 
		{ 
			return checked ((byte) value);
		}

		[CLSCompliant (false)]
		public static byte ToByte (ushort value) 
		{ 
			return checked ((byte) value);
		}

		public static byte ToByte (object value)
		{
			if (value == null)
				return 0;
			return ToByte (value, null);
		}

		public static byte ToByte (object value, IFormatProvider provider)
		{
			if (value == null)
				return 0;
			return ((IConvertible) value).ToByte (provider);
		}

		// ========== Char Conversions ========== //

		public static char ToChar (bool value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}
		
		public static char ToChar (byte value) 
		{ 
			return (char)value;
		}

		public static char ToChar (char value) 
		{ 
			return value;
		}

		public static char ToChar (DateTime value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static char ToChar (decimal value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static char ToChar (double value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}
		
		public static char ToChar (int value) 
		{ 
			return checked ((char) value);
		}

		public static char ToChar (long value) 
		{ 
			return checked ((char) value);
		}

		public static char ToChar (float value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		[CLSCompliant (false)]
		public static char ToChar (sbyte value) 
		{ 
			return checked ((char) value);
		}
	
		public static char ToChar (short value) 
		{ 
			return checked ((char) value);
		}

		public static char ToChar (string value) 
		{
			return Char.Parse (value);
		}

		public static char ToChar (string value, IFormatProvider provider)
		{
			return Char.Parse (value); // provider is ignored.
		}

		[CLSCompliant (false)]
		public static char ToChar (uint value) 
		{ 
			return checked ((char) value);
		}

		[CLSCompliant (false)]
		public static char ToChar (ulong value) 
		{ 
			return checked ((char) value);
		}

		[CLSCompliant (false)]
		public static char ToChar (ushort value) 
		{ 
			return (char)value; 
		}

		public static char ToChar (object value)
		{
			if (value == null)
				return '\0';
			return ToChar (value, null);
		}

		public static char ToChar (object value, IFormatProvider provider)
		{
			if (value == null)
				return '\0';
			return ((IConvertible) value).ToChar (provider);
		}

		// ========== DateTime Conversions ========== //
	
		public static DateTime ToDateTime (string value) 
		{ 
			if (value == null)
				return DateTime.MinValue; // LAMESPEC: Spec says throw ArgumentNullException
			return DateTime.Parse (value);
		}
	
		public static DateTime ToDateTime (string value, IFormatProvider provider) 
		{
			if (value == null)
				return DateTime.MinValue; // LAMESPEC: Spec says throw ArgumentNullException
			return DateTime.Parse (value, provider);
		}

		public static DateTime ToDateTime (bool value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static DateTime ToDateTime (byte value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static DateTime ToDateTime (char value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static DateTime ToDateTime (DateTime value)
		{
			return value;
		}

		public static DateTime ToDateTime (decimal value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static DateTime ToDateTime (double value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static DateTime ToDateTime (short value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static DateTime ToDateTime (int value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static DateTime ToDateTime (long value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static DateTime ToDateTime (float value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static DateTime ToDateTime (object value)
		{
			if (value == null)
				return DateTime.MinValue;
			return ToDateTime (value, null);
		}

		public static DateTime ToDateTime (object value, IFormatProvider provider)
		{
			if (value == null)
				return DateTime.MinValue;
			return ((IConvertible) value).ToDateTime (provider);
		}

		[CLSCompliant (false)]
		public static DateTime ToDateTime (sbyte value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}
		[CLSCompliant (false)]
		public static DateTime ToDateTime (ushort value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		[CLSCompliant (false)]
		public static DateTime ToDateTime (uint value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		[CLSCompliant (false)]
		public static DateTime ToDateTime (ulong value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		// ========== Decimal Conversions ========== //
	
		public static decimal ToDecimal (bool value) 
		{ 
			return value ? 1 : 0; 
		}
	
		public static decimal ToDecimal (byte value) 
		{ 
			return (decimal)value; 
		}

		public static decimal ToDecimal (char value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static decimal ToDecimal (DateTime value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}
				
		public static decimal ToDecimal (decimal value) 
		{ 
			return value; 
		}

		public static decimal ToDecimal (double value) 
		{ 
			return (decimal) value; 
		}

		public static decimal ToDecimal (float value) 
		{
			return (decimal) value;
		}

		public static decimal ToDecimal (int value) 
		{ 
			return (decimal)value; 
		}
	
		public static decimal ToDecimal (long value) 
		{ 
			return (decimal)value; 
		}

		[CLSCompliant (false)]
		public static decimal ToDecimal (sbyte value) 
		{ 
			return (decimal)value; 
		}
	
		public static decimal ToDecimal (short value) 
		{ 
			return (decimal)value; 
		}

		public static decimal ToDecimal (string value) 
		{
			if (value == null)
				return new Decimal (0); // LAMESPEC: Spec says throw ArgumentNullException
			return Decimal.Parse (value);
		}

		public static decimal ToDecimal (string value, IFormatProvider provider) 
		{
			if (value == null)
				return new Decimal (0); // LAMESPEC: Spec says throw ArgumentNullException
			return Decimal.Parse (value, provider);
		}

		[CLSCompliant (false)]
		public static decimal ToDecimal (uint value) 
		{ 
			return (decimal)value; 
		}

		[CLSCompliant (false)]
		public static decimal ToDecimal (ulong value) 
		{ 
			return (decimal)value; 
		}

		[CLSCompliant (false)]
		public static decimal ToDecimal (ushort value) 
		{ 
			return (decimal)value; 
		}

		public static decimal ToDecimal (object value)
		{
			if (value == null)
				return new Decimal (0);
			return ToDecimal (value, null);
		}

		public static decimal ToDecimal (object value, IFormatProvider provider)
		{
			if (value == null)
				return new Decimal (0);
			return ((IConvertible) value).ToDecimal (provider);
		}
						 

		// ========== Double Conversions ========== //
	
		public static double ToDouble (bool value) 
		{ 
			return value ? 1 : 0; 
		}
	
		public static double ToDouble (byte value) 
		{ 
			return (double) value;
		}

		public static double ToDouble (char value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static double ToDouble (DateTime value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}
	
		public static double ToDouble (decimal value) 
		{ 
			return (double)value; 
		}

		public static double ToDouble (double value) 
		{ 
			return value; 
		}

		public static double ToDouble (float value) 
		{ 
			return (double) value;
		}

		public static double ToDouble (int value) 
		{ 
			return (double)value; 
		}
	
		public static double ToDouble (long value) 
		{ 
			return (double)value; 
		}

		[CLSCompliant (false)]
		public static double ToDouble (sbyte value) 
		{ 
			return (double)value; 
		}
	
		public static double ToDouble (short value) 
		{ 
			return (double)value; 
		}

		public static double ToDouble (string value) 
		{
			if (value == null)
				return 0.0; // LAMESPEC: Spec says throw ArgumentNullException
			return Double.Parse (value);
		}

		public static double ToDouble (string value, IFormatProvider provider) 
		{
			if (value == null)
				return 0.0; // LAMESPEC: Spec says throw ArgumentNullException
			return Double.Parse (value, provider);
		}

		[CLSCompliant (false)]
		public static double ToDouble (uint value) 
		{ 
			return (double)value; 
		}

		[CLSCompliant (false)]
		public static double ToDouble (ulong value) 
		{ 
			return (double)value; 
		}

		[CLSCompliant (false)]
		public static double ToDouble (ushort value) 
		{ 
			return (double)value; 
		}

		public static double ToDouble (object value)
		{
			if (value == null)
				return 0.0;
			return ToDouble (value, null);
		}

		public static double ToDouble (object value, IFormatProvider provider)
		{
			if (value == null)
				return 0.0;
			return ((IConvertible) value).ToDouble (provider);
		}

		// ========== Int16 Conversions ========== //

		public static short ToInt16 (bool value) 
		{ 
			return (short)(value ? 1 : 0); 
		}
	
		public static short ToInt16 (byte value) 
		{ 
			return (short)value; 
		}

		public static short ToInt16 (char value) 
		{
			return checked ((short) value);
		}

		public static short ToInt16 (DateTime value) 
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}
	
		public static short ToInt16 (decimal value) 
		{ 
			// Returned Even-Rounded
			return checked ((short) Math.Round (value));
		}

		public static short ToInt16 (double value) 
		{ 
			// Returned Even-Rounded
			return checked ((short) Math.Round (value));
		}
 
		public static short ToInt16 (float value) 
		{ 
			// Returned Even-Rounded, use Math.Round pass as a double.
			return checked ((short) Math.Round (value));
		}

		public static short ToInt16 (int value) 
		{ 
			return checked ((short) value);
		}
	
		public static short ToInt16 (long value) 
		{ 
			return checked ((short) value);
		}

		[CLSCompliant (false)]
		public static short ToInt16 (sbyte value) 
		{ 
			return value; 
		}
	
		public static short ToInt16 (short value) 
		{ 
			return value; 
		}

		public static short ToInt16 (string value) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return Int16.Parse (value);
		}

		public static short ToInt16 (string value, IFormatProvider provider) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return Int16.Parse (value, provider);
		}

		public static short ToInt16 (string value, int fromBase)
		{
			int result = ConvertFromBase (value, fromBase, false);
			if (fromBase != 10) {
				if (result > ushort.MaxValue) {
					throw new OverflowException ("Value was either too large or too small for an Int16.");
				}

				// note: no sign are available to detect negatives
				if (result > Int16.MaxValue) {
					// return negative 2's complement
					return Convert.ToInt16 (-(65536 - result));
				}
			}
			return Convert.ToInt16 (result);
		}

		[CLSCompliant (false)]
		public static short ToInt16 (uint value) 
		{ 
			return checked ((short) value);
		}

		[CLSCompliant (false)]
		public static short ToInt16 (ulong value) 
		{ 
			return checked ((short) value);
		}

		[CLSCompliant (false)]
		public static short ToInt16 (ushort value) 
		{ 
			return checked ((short) value);
		}

		public static short ToInt16 (object value)
		{
			if (value == null)
				return 0;
			return ToInt16 (value, null);
		}

		public static short ToInt16 (object value, IFormatProvider provider)
		{
			if (value == null)
				return 0;
			return ((IConvertible) value).ToInt16 (provider);
		}
	
		// ========== Int32 Conversions ========== //

		public static int ToInt32 (bool value) 
		{ 
			return value ? 1 : 0; 
		}
	
		public static int ToInt32 (byte value) 
		{ 
			return (int)value; 
		}

		public static int ToInt32 (char value) 
		{ 
			return (int)value; 
		}

		public static int ToInt32 (DateTime value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}
	
		public static int ToInt32 (decimal value) 
		{ 
			// Returned Even-Rounded
			return checked ((int) Math.Round (value));
		}

		public static int ToInt32 (double value) 
		{ 
			// Returned Even-Rounded
			checked {
				return (int)(Math.Round (value));
			}
		}
 
		public static int ToInt32 (float value) 
		{ 
			if (value > Int32.MaxValue || value < Int32.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int32.MaxValue or less than Int32.MinValue"));
	  
			// Returned Even-Rounded, pass as a double, could just call
			// Convert.ToInt32 ( (double)value);
			checked {
				return (int)(Math.Round ( (double)value));
			}
		}

		public static int ToInt32 (int value) 
		{ 
			return value; 
		}
	
		public static int ToInt32 (long value) 
		{ 
			return checked ((int) value);
		}

		[CLSCompliant (false)]
		public static int ToInt32 (sbyte value) 
		{ 
			return (int)value; 
		}
	
		public static int ToInt32 (short value) 
		{ 
			return (int)value; 
		}

		public static int ToInt32 (string value) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return Int32.Parse (value);
		}

		public static int ToInt32 (string value, IFormatProvider provider) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return Int32.Parse (value, provider);
		}

		
		public static int ToInt32 (string value, int fromBase)
		{
			return ConvertFromBase (value, fromBase, false);
		}
		
		[CLSCompliant (false)]
		public static int ToInt32 (uint value) 
		{ 
			return checked ((int) value);
		}

		[CLSCompliant (false)]
		public static int ToInt32 (ulong value) 
		{ 
			return checked ((int) value);
		}

		[CLSCompliant (false)]
		public static int ToInt32 (ushort value) 
		{ 
			return (int)value; 
		}

		public static int ToInt32 (object value)
		{
			if (value == null)
				return 0;
			return ToInt32 (value, null);
		}

		public static int ToInt32 (object value, IFormatProvider provider)
		{
			if (value == null)
				return 0;
			return ((IConvertible) value).ToInt32 (provider);
		}

		// ========== Int64 Conversions ========== //

		public static long ToInt64 (bool value) 
		{ 
			return value ? 1 : 0; 
		}
	
		public static long ToInt64 (byte value) 
		{ 
			return (long)(ulong)value; 
		}

		public static long ToInt64 (char value) 
		{ 
			return (long)value; 
		}

		public static long ToInt64 (DateTime value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}
	
		public static long ToInt64 (decimal value) 
		{ 
			// Returned Even-Rounded
			return checked ((long) Math.Round (value));
		}

		public static long ToInt64 (double value) 
		{ 
			// Returned Even-Rounded
			return checked ((long) Math.Round (value));
		}
 
		public static long ToInt64 (float value) 
		{ 
			// Returned Even-Rounded, pass to Math as a double, could
			// just call Convert.ToInt64 ( (double)value);
			return checked ((long) Math.Round (value));
		}

		public static long ToInt64 (int value) 
		{ 
			return (long)value; 
		}
	
		public static long ToInt64 (long value) 
		{ 
			return value; 
		}

		[CLSCompliant (false)]
		public static long ToInt64 (sbyte value) 
		{ 
			return (long)value; 
		}
	
		public static long ToInt64 (short value) 
		{ 
			return (long)value; 
		}

		public static long ToInt64 (string value) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return Int64.Parse (value);
		}

		public static long ToInt64 (string value, IFormatProvider provider) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return Int64.Parse (value, provider);
		}

		public static long ToInt64 (string value, int fromBase)
		{
			return ConvertFromBase64 (value, fromBase, false);
		}

		[CLSCompliant (false)]
		public static long ToInt64 (uint value) 
		{ 
			return (long)(ulong)value; 
		}

		[CLSCompliant (false)]
		public static long ToInt64 (ulong value) 
		{ 
			return checked ((long) value);
		}

		[CLSCompliant (false)]
		public static long ToInt64 (ushort value) 
		{ 
			return (long)(ulong)value; 
		}

		public static long ToInt64 (object value)
		{
			if (value == null)
				return 0;
			return ToInt64 (value, null);
		}

		public static long ToInt64 (object value, IFormatProvider provider)
		{
			if (value == null)
				return 0;
			return ((IConvertible) value).ToInt64 (provider);
		}
		
		// ========== SByte Conversions ========== //

		[CLSCompliant (false)]
		public static sbyte ToSByte (bool value) 
		{ 
			return (sbyte)(value ? 1 : 0); 
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (byte value) 
		{ 
			return checked ((sbyte) value);
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (char value) 
		{ 
			return checked ((sbyte) value);
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (DateTime value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}
		
		[CLSCompliant (false)]	
		public static sbyte ToSByte (decimal value) 
		{ 
			// Returned Even-Rounded
			return checked ((sbyte) Math.Round (value));
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (double value) 
		{ 
			// Returned Even-Rounded
			return checked ((sbyte) Math.Round (value));
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (float value) 
		{ 
			// Returned Even-Rounded, pass as double to Math
			return checked ((sbyte) Math.Round (value));
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (int value) 
		{ 
			return checked ((sbyte) value);
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (long value) 
		{ 
			return checked ((sbyte) value);
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (sbyte value) 
		{ 
			return value;
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (short value) 
		{ 
			return checked ((sbyte) value);
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (string value) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return SByte.Parse (value);
		}
		
		[CLSCompliant (false)]
		public static sbyte ToSByte (string value, IFormatProvider provider) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return SByte.Parse (value, provider);
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (string value, int fromBase)
		{
			int result = ConvertFromBase (value, fromBase, false);
			if (fromBase != 10) {
				// note: no sign are available to detect negatives
				if (result > SByte.MaxValue) {
					// return negative 2's complement
					return Convert.ToSByte (-(256 - result));
				}
			}
			return Convert.ToSByte (result);
		}
		
		[CLSCompliant (false)]
		public static sbyte ToSByte (uint value) 
		{ 
			return checked ((sbyte) value);
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (ulong value) 
		{ 
			return checked ((sbyte) value);
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (ushort value) 
		{ 
			return checked ((sbyte) value);
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (object value)
		{
			if (value == null)
				return 0;
			return ToSByte (value, null);
		}

		[CLSCompliant (false)]
		public static sbyte ToSByte (object value, IFormatProvider provider)
		{
			if (value == null)
				return 0;
			return ((IConvertible) value).ToSByte (provider);
		}

		// ========== Single Conversions ========== //
	
		public static float ToSingle (bool value) 
		{ 
			return value ? 1 : 0; 
		}
	
		public static float ToSingle (byte value) 
		{ 
			return (float)value; 
		}

		public static float ToSingle (Char value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		public static float ToSingle (DateTime value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}
	
		public static float ToSingle (decimal value) 
		{ 
			return (float)value; 
		}

		public static float ToSingle (double value) 
		{ 
			return (float)value; 
		}
	
		public static float ToSingle (float value) 
		{ 
			return value; 
		}

		public static float ToSingle (int value) 
		{ 
			return (float)value; 
		}
	
		public static float ToSingle (long value) 
		{ 
			return (float)value; 
		}

		[CLSCompliant (false)]
		public static float ToSingle (sbyte value) 
		{ 
			return (float)value; 
		}
	
		public static float ToSingle (short value) 
		{ 
			return (float)value; 
		}

		public static float ToSingle (string value) 
		{
			if (value == null)
				return 0.0f; // LAMESPEC: Spec says throw ArgumentNullException
			return Single.Parse (value);
		}

		public static float ToSingle (string value, IFormatProvider provider) 
		{
			if (value == null)
				return 0.0f; // LAMESPEC: Spec says throw ArgumentNullException
			return Single.Parse (value, provider);
		}	       

		[CLSCompliant (false)]
		public static float ToSingle (uint value) 
		{ 
			return (float)value; 
		}

		[CLSCompliant (false)]
		public static float ToSingle (ulong value) 
		{ 
			return (float)value; 
		}

		[CLSCompliant (false)]
		public static float ToSingle (ushort value) 
		{ 
			return (float)value; 
		}

		public static float ToSingle (object value)
		{
			if (value == null)
				return 0.0f;
			return ToSingle (value, null);
		}

//		[CLSCompliant (false)]
		public static float ToSingle (object value, IFormatProvider provider)
		{
			if (value == null)
				return 0.0f;
			return ((IConvertible) value).ToSingle (provider);
		}

		// ========== String Conversions ========== //
	
		public static string ToString (bool value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (bool value, IFormatProvider provider)
		{
			return value.ToString (); // the same as ToString (bool).
		}
	
		public static string ToString (byte value) 
		{ 
			return value.ToString (); 
		}
	
		public static string ToString (byte value, IFormatProvider provider) 
		{
			return value.ToString (provider); 
		}

		public static string ToString (byte value, int toBase)
		{
			if (value == 0)
				return "0";
			if (toBase == 10)
				return value.ToString ();
			
			byte[] val = BitConverter.GetBytes (value);

			switch (toBase) {
			case 2:
				return ConvertToBase2 (val);
			case 8:
				return ConvertToBase8 (val);
			case 16:
				return ConvertToBase16 (val);
			default:
				throw new ArgumentException (Locale.GetText ("toBase is not valid."));
			}
		}

		public static string ToString (char value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (char value, IFormatProvider provider)
		{
			return value.ToString (); // the same as ToString (char)
		}

		public static string ToString (DateTime value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (DateTime value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}

		public static string ToString (decimal value) 
		{
			return value.ToString ();
		}

		public static string ToString (decimal value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}
	
		public static string ToString (double value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (double value, IFormatProvider provider) 
		{ 
			return value.ToString (provider);
		}
	
		public static string ToString (float value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (float value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}

		public static string ToString (int value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (int value, int toBase)
		{
			if (value == 0)
				return "0";
			if (toBase == 10)
				return value.ToString ();
			
			byte[] val = BitConverter.GetBytes (value);

			switch (toBase) {
			case 2:
				return ConvertToBase2 (val);
			case 8:
				return ConvertToBase8 (val);
			case 16:
				return ConvertToBase16 (val);
			default:
				throw new ArgumentException (Locale.GetText ("toBase is not valid."));
			}
		}

		public static string ToString (int value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}
	
		public static string ToString (long value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (long value, int toBase)
		{
			if (value == 0)
				return "0";
			if (toBase == 10)
				return value.ToString ();
			
			byte[] val = BitConverter.GetBytes (value);

			switch (toBase) {
			case 2:
				return ConvertToBase2 (val);
			case 8:
				return ConvertToBase8 (val);
			case 16:
				return ConvertToBase16 (val);
			default:
				throw new ArgumentException (Locale.GetText ("toBase is not valid."));
			}
		}

		public static string ToString (long value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}

		public static string ToString (object value)
		{
			return ToString (value, null);
		}		

		public static string ToString (object value, IFormatProvider provider)
		{
			if (value is IConvertible)
				return ((IConvertible) value).ToString (provider);
			else if (value != null)
				return value.ToString ();
			return String.Empty;
		}				

		[CLSCompliant (false)]
		public static string ToString (sbyte value) 
		{ 
			return value.ToString (); 
		}

		[CLSCompliant (false)]				
		public static string ToString (sbyte value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}
	
		public static string ToString (short value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (short value, int toBase)
		{
			if (value == 0)
				return "0";
			if (toBase == 10)
				return value.ToString ();
			
			byte[] val = BitConverter.GetBytes (value);

			switch (toBase) {
			case 2:
				return ConvertToBase2 (val);
			case 8:
				return ConvertToBase8 (val);
			case 16:
				return ConvertToBase16 (val);
			default:
				throw new ArgumentException (Locale.GetText ("toBase is not valid."));
			}
		}

		public static string ToString (short value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}

		public static string ToString (string value) 
		{
			return value;
		}

		public static string ToString (string value, IFormatProvider provider)
		{
			return value; // provider is ignored.
		}

		[CLSCompliant (false)]
		public static string ToString (uint value) 
		{ 
			return value.ToString (); 
		}

		[CLSCompliant (false)]
		public static string ToString (uint value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}

		[CLSCompliant (false)]
		public static string ToString (ulong value) 
		{ 
			return value.ToString (); 
		}

		[CLSCompliant (false)]
		public static string ToString (ulong value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}

		[CLSCompliant (false)]
		public static string ToString (ushort value) 
		{ 
			return value.ToString (); 
		}

		[CLSCompliant (false)]
		public static string ToString (ushort value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}
		
		// ========== UInt16 Conversions ========== //

		[CLSCompliant (false)]
		public static ushort ToUInt16 (bool value) 
		{ 
			return (ushort)(value ? 1 : 0); 
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (byte value) 
		{ 
			return (ushort)value; 
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (char value) 
		{ 
			return (ushort)value; 
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (DateTime value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (decimal value) 
		{ 
			// Returned Even-Rounded
			return checked ((ushort) Math.Round (value));
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (double value) 
		{ 
			// Returned Even-Rounded
			return checked ((ushort) Math.Round (value));
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (float value) 
		{ 
			// Returned Even-Rounded, pass as double to Math
			return checked ((ushort) Math.Round (value));
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (int value) 
		{ 
			return checked ((ushort) value);
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (long value) 
		{ 
			return checked ((ushort) value);
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (sbyte value) 
		{ 
			return checked ((ushort) value);
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (short value) 
		{ 
			return checked ((ushort) value);
		}
		
		[CLSCompliant (false)]
		public static ushort ToUInt16 (string value) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return UInt16.Parse (value);
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (string value, IFormatProvider provider) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return UInt16.Parse (value, provider);
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (string value, int fromBase) 
		{
			return ToUInt16 (ConvertFromBase (value, fromBase, true));
		} 

		[CLSCompliant (false)]
		public static ushort ToUInt16 (uint value) 
		{ 
			return checked ((ushort) value);
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (ulong value) 
		{ 
			return checked ((ushort) value);
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (ushort value) 
		{ 
			return value; 
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (object value)
		{
			if (value == null)
				return 0;
			return ToUInt16 (value, null);
		}

		[CLSCompliant (false)]
		public static ushort ToUInt16 (object value, IFormatProvider provider)
		{
			if (value == null)
				return 0;
			return ((IConvertible) value).ToUInt16 (provider);
		}

		// ========== UInt32 Conversions ========== //

		[CLSCompliant (false)]
		public static uint ToUInt32 (bool value) 
		{ 
			return (uint)(value ? 1 : 0); 
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (byte value) 
		{ 
			return (uint)value; 
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (char value) 
		{ 
			return (uint)value; 
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (DateTime value)
		{
			throw new InvalidCastException ("This conversion is not supported.");
		}
		
		[CLSCompliant (false)]
		public static uint ToUInt32 (decimal value) 
		{ 
			// Returned Even-Rounded
			return checked ((uint) Math.Round (value));
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (double value) 
		{ 
			// Returned Even-Rounded
			return checked ((uint) Math.Round (value));
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (float value) 
		{ 
			// Returned Even-Rounded, pass as double to Math
			return checked ((uint) Math.Round (value));
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (int value) 
		{ 
			return checked ((uint) value);
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (long value) 
		{ 
			return checked ((uint) value);
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (sbyte value) 
		{ 
			return checked ((uint) value);
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (short value) 
		{ 
			return checked ((uint) value);
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (string value) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return UInt32.Parse (value);
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (string value, IFormatProvider provider) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return UInt32.Parse (value, provider);
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (string value, int fromBase)
		{
			return (uint) ConvertFromBase (value, fromBase, true);
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (uint value) 
		{ 
			return value; 
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (ulong value) 
		{ 
			return checked ((uint) value);
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (ushort value) 
		{ 
			return (uint)value; 
		}

		[CLSCompliant (false)]
		public static uint ToUInt32 (object value)
		{
			if (value == null)
				return 0;
			return ToUInt32 (value, null);
		}		

		[CLSCompliant (false)]
		public static uint ToUInt32 (object value, IFormatProvider provider)
		{
			if (value == null)
				return 0;
			return ((IConvertible) value).ToUInt32 (provider);
		}		
		

		// ========== UInt64 Conversions ========== //

		[CLSCompliant (false)]
		public static ulong ToUInt64 (bool value) 
		{ 
			return (ulong)(value ? 1 : 0); 
		}

		[CLSCompliant (false)]
		public static ulong ToUInt64 (byte value) 
		{ 
			return (ulong)value; 
		}

		[CLSCompliant (false)]
		public static ulong ToUInt64 (char value) 
		{ 
			return (ulong)value; 
		}

		[CLSCompliant (false)]
		public static ulong ToUInt64 (DateTime value)
		{
			throw new InvalidCastException ("The conversion is not supported.");
		}

		[CLSCompliant (false)]
		public static ulong ToUInt64 (decimal value) 
		{ 
			// Returned Even-Rounded
			return checked ((ulong) Math.Round (value));
		}

		[CLSCompliant (false)]
		public static ulong ToUInt64 (double value) 
		{ 
			// Returned Even-Rounded
			return checked ((ulong) Math.Round (value));
		}
		
		[CLSCompliant (false)] 
		public static ulong ToUInt64 (float value) 
		{ 
			// Returned Even-Rounded, pass as a double to Math
			return checked ((ulong) Math.Round (value));
		}
		
		[CLSCompliant (false)]
		public static ulong ToUInt64 (int value) 
		{ 
			return checked ((ulong) value);
		}
		
		[CLSCompliant (false)]
		public static ulong ToUInt64 (long value) 
		{ 
			return checked ((ulong) value);
		}

		[CLSCompliant (false)]
		public static ulong ToUInt64 (sbyte value) 
		{ 
			return checked ((ulong) value);
		}
		
		[CLSCompliant (false)]	
		public static ulong ToUInt64 (short value) 
		{ 
			return checked ((ulong) value);
		}

		[CLSCompliant (false)]
		public static ulong ToUInt64 (string value) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return UInt64.Parse (value);
		}

		[CLSCompliant (false)]
		public static ulong ToUInt64 (string value, IFormatProvider provider) 
		{
			if (value == null)
				return 0; // LAMESPEC: Spec says throw ArgumentNullException
			return UInt64.Parse (value, provider);
		}

		[CLSCompliant (false)]
		public static ulong ToUInt64 (string value, int fromBase)
		{
			return (ulong) ConvertFromBase64 (value, fromBase, true);
		}

		[CLSCompliant (false)]
		public static ulong ToUInt64 (uint value) 
		{ 
			return (ulong)value; 
		}

		[CLSCompliant (false)]
		public static ulong ToUInt64 (ulong value) 
		{ 
			return value; 
		}
		
		[CLSCompliant (false)]
		public static ulong ToUInt64 (ushort value) 
		{ 
			return (ulong)value; 
		}

		[CLSCompliant (false)]
		public static ulong ToUInt64 (object value)
		{
			if (value == null)
				return 0;
			return ToUInt64 (value, null);
		}		

		[CLSCompliant (false)]
		public static ulong ToUInt64 (object value, IFormatProvider provider)
		{
			if (value == null)
				return 0;
			return ((IConvertible) value).ToUInt64 (provider);
		}		
		

		// ========== Conversion / Helper Functions ========== //

		public static object ChangeType (object value, Type conversionType)
		{
			if ((value != null) && (conversionType == null))
				throw new ArgumentNullException ("conversionType");
			CultureInfo ci = CultureInfo.CurrentCulture;
			IFormatProvider provider;
			if (conversionType == typeof(DateTime)) {
				provider = ci.DateTimeFormat;
			}
			else {
				provider = ci.NumberFormat;
			}
			return ToType (value, conversionType, provider, true);
		}
		
		public static object ChangeType (object value, TypeCode typeCode)
		{
			CultureInfo ci = CultureInfo.CurrentCulture;
			Type conversionType = conversionTable [(int) typeCode];
			IFormatProvider provider;
			if (conversionType == typeof(DateTime)) {
				provider = ci.DateTimeFormat;
			}
			else {
				provider = ci.NumberFormat;
			}
			return ToType (value, conversionType, provider, true);
		}

		public static object ChangeType (object value, Type conversionType, IFormatProvider provider)
		{
			if ((value != null) && (conversionType == null))
				throw new ArgumentNullException ("conversionType");
			return ToType (value, conversionType, provider, true);
		}
		
		public static object ChangeType (object value, TypeCode typeCode, IFormatProvider provider)
		{
			Type conversionType = conversionTable [(int)typeCode];
			return ToType (value, conversionType, provider, true);
		}

		private static bool NotValidBase (int value)
		{
			if ((value == 2) || (value == 8) ||
			   (value == 10) || (value == 16))
				return false;
			
			return true;
		}

		private static int ConvertFromBase (string value, int fromBase, bool unsigned)
		{
			if (NotValidBase (fromBase))
				throw new ArgumentException ("fromBase is not valid.");
			if (value == null)
				return 0;

			int chars = 0;
			int result = 0;
			int digitValue;

			int i=0; 
			int len = value.Length;
			bool negative = false;

			// special processing for some bases
			switch (fromBase) {
				case 10:
					if (value.Substring (i, 1) == "-") {
						if (unsigned) {
							throw new OverflowException (
								Locale.GetText ("The string was being parsed as"
								+ " an unsigned number and could not have a"
								+ " negative sign."));
						}
						negative = true;
						i++;
					}
					break;
				case 16:
					if (value.Substring (i, 1) == "-") {
						throw new ArgumentException ("String cannot contain a "
							+ "minus sign if the base is not 10.");
					}
					if (len >= i + 2) {
						// 0x00 or 0X00
						if ((value[i] == '0') && ((value [i+1] == 'x') || (value [i+1] == 'X'))) {
							i+=2;
						}
					}
					break;
				default:
					if (value.Substring (i, 1) == "-") {
						throw new ArgumentException ("String cannot contain a "
							+ "minus sign if the base is not 10.");
					}
					break;
			}

			if (len == i) {
				throw new FormatException ("Could not find any parsable digits.");
			}

			if (value[i] == '+') {
				i++;
			}

			while (i < len) {
				char c = value[i++];
				if (Char.IsNumber (c)) {
					digitValue = c - '0';
				} else if (Char.IsLetter (c)) {
					digitValue = Char.ToLowerInvariant (c) - 'a' + 10;
				} else {
					if (chars > 0) {
						throw new FormatException ("Additional unparsable "
							+ "characters are at the end of the string.");
					} else {
						throw new FormatException ("Could not find any parsable"
							+ " digits.");
					}
				}

				if (digitValue >= fromBase) {
					if (chars > 0) {
						throw new FormatException ("Additional unparsable "
							+ "characters are at the end of the string.");
					} else {
						throw new FormatException ("Could not find any parsable"
							+ " digits.");
					}
				}

				result = (fromBase) * result + digitValue;
				chars ++;
			}

			if (chars == 0)
				throw new FormatException ("Could not find any parsable digits.");

			if (negative)
				return -result;
			else
				return result;
		}

		// note: this has nothing to do with base64 encoding (just base and Int64)
		private static long ConvertFromBase64 (string value, int fromBase, bool unsigned)
		{
			if (NotValidBase (fromBase))
				throw new ArgumentException ("fromBase is not valid.");
			if (value == null)
				return 0;

			int chars = 0;
			int digitValue = -1;
			long result = 0;
			bool negative = false;

			int i = 0;
			int len = value.Length;

			// special processing for some bases
			switch (fromBase) {
				case 10:
					if (value.Substring(i, 1) == "-") {
						if (unsigned) {
							throw new OverflowException (
								Locale.GetText ("The string was being parsed as"
								+ " an unsigned number and could not have a"
								+ " negative sign."));
						}
						negative = true;
						i++;
					}
					break;
				case 16:
					if (value.Substring (i, 1) == "-") {
						throw new ArgumentException ("String cannot contain a "
							+ "minus sign if the base is not 10.");
					}
					if (len >= i + 2) {
						// 0x00 or 0X00
						if ((value[i] == '0') && ((value[i + 1] == 'x') || (value[i + 1] == 'X'))) {
							i += 2;
						}
					}
					break;
				default:
					if (value.Substring (i, 1) == "-") {
						throw new ArgumentException ("String cannot contain a "
							+ "minus sign if the base is not 10.");
					}
					break;
			}

			if (len == i) {
				throw new FormatException ("Could not find any parsable digits.");
			}

			if (value[i] == '+') {
				i++;
			}

			while (i < len) {
				char c = value[i++];
				if (Char.IsNumber (c)) {
					digitValue = c - '0';
				} else if (Char.IsLetter (c)) {
					digitValue = Char.ToLowerInvariant (c) - 'a' + 10;
				} else {
					if (chars > 0) {
						throw new FormatException ("Additional unparsable "
							+ "characters are at the end of the string.");
					} else {
						throw new FormatException ("Could not find any parsable"
							+ " digits.");
					}
				}

				if (digitValue >= fromBase) {
					if (chars > 0) {
						throw new FormatException ("Additional unparsable "
							+ "characters are at the end of the string.");
					} else {
						throw new FormatException ("Could not find any parsable"
							+ " digits.");
					}
				}

				result = (fromBase * result + digitValue);
				chars ++;
			}

			if (chars == 0)
				throw new FormatException ("Could not find any parsable digits.");

			if (negative)
				return -1 * result;
			else
				return result;
		}

		private static void EndianSwap (ref byte[] value)
		{
			byte[] buf = new byte[value.Length];
			for (int i = 0; i < value.Length; i++)
				buf[i] = value[value.Length-1-i];
			value = buf;
		}

		private static string ConvertToBase2 (byte[] value)
		{
			if (!BitConverter.IsLittleEndian)
				EndianSwap (ref value);
			StringBuilder sb = new StringBuilder ();
			for (int i = value.Length - 1; i >= 0; i--) {
				byte b = value [i];
				for (int j = 0; j < 8; j++) {
					if ((b & 0x80) == 0x80) {
						sb.Append ('1');
					}
					else {
						if (sb.Length > 0)
							sb.Append ('0');
					}
					b <<= 1;
				}
			}
			return sb.ToString ();
		}

		private static string ConvertToBase8 (byte[] value)
		{
			ulong l = 0;
			switch (value.Length) {
			case 1:
				l = (ulong) value [0];
				break;
			case 2:
				l = (ulong) BitConverter.ToUInt16 (value, 0);
				break;
			case 4:
				l = (ulong) BitConverter.ToUInt32 (value, 0);
				break;
			case 8:
				l = BitConverter.ToUInt64 (value, 0);
				break;
			default:
				throw new ArgumentException ("value");
			}

			StringBuilder sb = new StringBuilder ();
			for (int i = 21; i >= 0; i--) {
				// 3 bits at the time
				char val = (char) ((l >> i * 3) & 0x7);
				if ((val != 0) || (sb.Length > 0)) {
					val += '0';
					sb.Append (val);
				}
			}
			return sb.ToString ();
		}

		private static string ConvertToBase16 (byte[] value)
		{
			if (!BitConverter.IsLittleEndian)
				EndianSwap (ref value);
			StringBuilder sb = new StringBuilder ();
			for (int i = value.Length - 1; i >= 0; i--) {
				char high = (char)((value[i] >> 4) & 0x0f);
				if ((high != 0) || (sb.Length > 0)) {
					if (high < 10) 
						high += '0';
					else {
						high -= (char) 10;
						high += 'a';
					}
					sb.Append (high);
				}

				char low = (char)(value[i] & 0x0f);
				if ((low != 0) || (sb.Length > 0)) {
					if (low < 10)
						low += '0';
					else {
						low -= (char) 10;
						low += 'a';
					}
					sb.Append (low);
				}
			}
			return sb.ToString ();
		}

		// Lookup table for the conversion ToType method. Order
		// is important! Used by ToType for comparing the target
		// type, and uses hardcoded array indexes.
		private static readonly Type[] conversionTable = {
			// Valid ICovnertible Types
			null,		    //	0 empty
			typeof (object),   //  1 TypeCode.Object
			typeof (DBNull),   //  2 TypeCode.DBNull
			typeof (Boolean),  //  3 TypeCode.Boolean
			typeof (Char),	   //  4 TypeCode.Char
			typeof (SByte),	   //  5 TypeCode.SByte
			typeof (Byte),	   //  6 TypeCode.Byte
			typeof (Int16),	   //  7 TypeCode.Int16
			typeof (UInt16),   //  8 TypeCode.UInt16
			typeof (Int32),	   //  9 TypeCode.Int32
			typeof (UInt32),   // 10 TypeCode.UInt32
			typeof (Int64),	   // 11 TypeCode.Int64
			typeof (UInt64),   // 12 TypeCode.UInt64
			typeof (Single),   // 13 TypeCode.Single
			typeof (Double),   // 14 TypeCode.Double
			typeof (Decimal),  // 15 TypeCode.Decimal
			typeof (DateTime), // 16 TypeCode.DateTime
			null,		    // 17 null.
			typeof (String),   // 18 TypeCode.String
		};

		// Function to convert an object to another type and return
		// it as an object. In place for the core data types to use
		// when implementing IConvertible. Uses hardcoded indexes in 
		// the conversionTypes array, so if modify carefully.
	
		//
		// The `try_target_to_type' boolean indicates if the code
		// should try to call the IConvertible.ToType method if everything
		// else fails.
		//
		// This should be true for invocations from Convert.cs, and
		// false from the mscorlib types that implement IConvertible that 
		// all into this internal function.
		//
		// This was added to keep the fix for #481687 working and to avoid
		// the regression that the simple fix introduced (485377)
		internal static object ToType (object value, Type conversionType, IFormatProvider provider, bool try_target_to_type) 
		{
			if (value == null) {
				if ((conversionType != null) && conversionType.IsValueType){
					throw new InvalidCastException ("Null object can not be converted to a value type.");
				} else
					return null;
			}

			if (conversionType == null)
				throw new InvalidCastException ("Cannot cast to destination type.");

			if (value.GetType () == conversionType)
				return value;
			
			if (value is IConvertible) {
				IConvertible convertValue = (IConvertible) value;

				if (conversionType == conversionTable[0]) // 0 Empty
					throw new ArgumentNullException ();
				
				else if (conversionType == conversionTable[1]) // 1 TypeCode.Object
					return (object) value;
					
				else if (conversionType == conversionTable[2]) // 2 TypeCode.DBNull
					throw new InvalidCastException (
						"Cannot cast to DBNull, it's not IConvertible");
		  
				else if (conversionType == conversionTable[3]) // 3 TypeCode.Boolean
					return (object) convertValue.ToBoolean (provider);
					
				else if (conversionType == conversionTable[4]) // 4 TypeCode.Char
					return (object) convertValue.ToChar (provider);
		  
				else if (conversionType == conversionTable[5]) // 5 TypeCode.SByte
					return (object) convertValue.ToSByte (provider);

				else if (conversionType == conversionTable[6]) // 6 TypeCode.Byte
					return (object) convertValue.ToByte (provider);
				
				else if (conversionType == conversionTable[7]) // 7 TypeCode.Int16
					return (object) convertValue.ToInt16 (provider);
					
				else if (conversionType == conversionTable[8]) // 8 TypeCode.UInt16
					return (object) convertValue.ToUInt16 (provider);
		  
				else if (conversionType == conversionTable[9]) // 9 TypeCode.Int32
					return (object) convertValue.ToInt32 (provider);
			
				else if (conversionType == conversionTable[10]) // 10 TypeCode.UInt32
					return (object) convertValue.ToUInt32 (provider);
		  
				else if (conversionType == conversionTable[11]) // 11 TypeCode.Int64
					return (object) convertValue.ToInt64 (provider);
		  
				else if (conversionType == conversionTable[12]) // 12 TypeCode.UInt64
					return (object) convertValue.ToUInt64 (provider);
		  
				else if (conversionType == conversionTable[13]) // 13 TypeCode.Single
					return (object) convertValue.ToSingle (provider);
		  
				else if (conversionType == conversionTable[14]) // 14 TypeCode.Double
					return (object) convertValue.ToDouble (provider);

				else if (conversionType == conversionTable[15]) // 15 TypeCode.Decimal
					return (object) convertValue.ToDecimal (provider);

				else if (conversionType == conversionTable[16]) // 16 TypeCode.DateTime
					return (object) convertValue.ToDateTime (provider);
				
				else if (conversionType == conversionTable[18]) // 18 TypeCode.String
					return (object) convertValue.ToString (provider);
				else {
					if (try_target_to_type)
						return convertValue.ToType (conversionType, provider);
				}
			} 
			// Not in the conversion table
			throw new InvalidCastException ((Locale.GetText (
								 "Value is not a convertible object: " + value.GetType().ToString() + " to " + conversionType.FullName)));
		}
	}
}
