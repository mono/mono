//
// System.String.cs
//
// Authors:
//   Patrik Torstensson
//   Jeffrey Stedfast (fejj@ximian.com)
//   Dan Lewis (dihlewis@yahoo.co.uk)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell (http://www.novell.com)
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

using System;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System
{
	[Serializable]
	public sealed class String : IConvertible, ICloneable, IEnumerable,
#if NET_2_0
		IComparable, IComparable<String>
#else
		IComparable
#endif
	{
		[NonSerialized] private int length;
		[NonSerialized] private char start_char;

		private const int COMPARE_CASE = 0;
		private const int COMPARE_INCASE = 1;
		private const int COMPARE_ORDINAL = 2;

		public static readonly String Empty = "";

		public static unsafe bool Equals (string a, string b)
		{
			if ((a as object) == (b as object))
				return true;

			if (a == null || b == null)
				return false;

			int len = a.length;

			if (len != b.length)
				return false;

			if (len == 0)
				return true;

			fixed (char * s1 = &a.start_char, s2 = &b.start_char) {
				// it must be one char, because 0 len is done above
				if (len < 2)
					return *s1 == *s2;

				// check by twos
				int * sint1 = (int *) s1, sint2 = (int *) s2;
				int n2 = len >> 1;
				do {
					if (*sint1++ != *sint2++)
						return false;
				} while (--n2 != 0);

				// nothing left
				if ((len & 1) == 0)
					return true;

				// check the last one
				return *(char *) sint1 == *(char *) sint2;
			}
		}

		public static bool operator == (String a, String b)
		{
			return Equals (a, b);
		}

		public static bool operator != (String a, String b)
		{
			return !Equals (a, b);
		}

		public override bool Equals (Object obj)
		{
			return Equals (this, obj as String);
		}

		public bool Equals (String value)
		{
			return Equals (this, value);
		}

		[IndexerName ("Chars")]
		public extern char this [int index] {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public Object Clone ()
		{
			return this;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.String;
		}

		public void CopyTo (int sourceIndex, char[] destination, int destinationIndex, int count)
		{
			// LAMESPEC: should I null-terminate?
			if (destination == null)
				throw new ArgumentNullException ("destination");

			if (sourceIndex < 0 || destinationIndex < 0 || count < 0)
				throw new ArgumentOutOfRangeException (); 

			// re-ordered to avoid possible integer overflow
			if (sourceIndex > Length - count)
				throw new ArgumentOutOfRangeException ("sourceIndex + count > Length");
			// re-ordered to avoid possible integer overflow
			if (destinationIndex > destination.Length - count)
				throw new ArgumentOutOfRangeException ("destinationIndex + count > destination.Length");

			InternalCopyTo (sourceIndex, destination, destinationIndex, count);
		}

		public char[] ToCharArray ()
		{
			return ToCharArray (0, length);
		}

		public char[] ToCharArray (int startIndex, int length)
		{
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException ("startIndex", "< 0"); 
			if (length < 0)
				throw new ArgumentOutOfRangeException ("length", "< 0"); 
			// re-ordered to avoid possible integer overflow
			if (startIndex > this.length - length)
				throw new ArgumentOutOfRangeException ("startIndex + length > this.length"); 

			char[] tmp = new char [length];

			InternalCopyTo (startIndex, tmp, 0, length);

			return tmp;
		}

		public String [] Split (params char [] separator)
		{
			return Split (separator, Int32.MaxValue);
		}

		public String[] Split (char[] separator, int count)
		{
			if (separator == null || separator.Length == 0)
				separator = WhiteChars;

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			if (count == 0) 
				return new String[0];

			if (count == 1) 
				return new String[1] { ToString() };

			return InternalSplit (separator, count);
		}

		public String Substring (int startIndex)
		{
			if (startIndex < 0 || startIndex > this.length)
				throw new ArgumentOutOfRangeException ("startIndex");

			string tmp = InternalAllocateStr (this.length - startIndex);
			InternalStrcpy (tmp, 0, this, startIndex, length - startIndex);

			return tmp;
		}

		public String Substring (int startIndex, int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException ("length", "< 0");
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException ("startIndex", "< 0");
			// re-ordered to avoid possible integer overflow
			if (startIndex > this.length - length)
				throw new ArgumentOutOfRangeException ("startIndex + length > this.length");

			if (length == 0)
				return String.Empty;

			string tmp = InternalAllocateStr (length);
			InternalStrcpy (tmp, 0, this, startIndex, length);

			return tmp;
		}	

		private static readonly char[] WhiteChars = { (char) 0x9, (char) 0xA, (char) 0xB, (char) 0xC, (char) 0xD,
			(char) 0x20, (char) 0xA0, (char) 0x2000, (char) 0x2001, (char) 0x2002, (char) 0x2003, (char) 0x2004,
			(char) 0x2005, (char) 0x2006, (char) 0x2007, (char) 0x2008, (char) 0x2009, (char) 0x200A, (char) 0x200B,
			(char) 0x3000, (char) 0xFEFF };

		public String Trim (params char[] trimChars)
		{
			if (trimChars == null || trimChars.Length == 0)
				trimChars = WhiteChars;

			return InternalTrim (trimChars, 0);
		}

		public String TrimStart (params char[] trimChars)
		{
			if (trimChars == null || trimChars.Length == 0)
				trimChars = WhiteChars;

			return InternalTrim (trimChars, 1);
		}

		public String TrimEnd (params char[] trimChars)
		{
			if (trimChars == null || trimChars.Length == 0)
				trimChars = WhiteChars;

			return InternalTrim (trimChars, 2);
		}

		public static int Compare (String strA, String strB)
		{
			return Compare (strA, strB, false, CultureInfo.CurrentCulture);
		}

		public static int Compare (String strA, String strB, bool ignoreCase)
		{
			return Compare (strA, strB, ignoreCase, CultureInfo.CurrentCulture);
		}

		public static int Compare (String strA, String strB, bool ignoreCase, CultureInfo culture)
		{
			if (culture == null)
				throw new ArgumentNullException ("culture");

			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;

			}
			else if (strB == null) {
				return 1;
			}

			CompareOptions compopts;

			if (ignoreCase)
				compopts = CompareOptions.IgnoreCase;
			else
				compopts = CompareOptions.None;

			return culture.CompareInfo.Compare (strA, strB, compopts);
		}

		public static int Compare (String strA, int indexA, String strB, int indexB, int length)
		{
			return Compare (strA, indexA, strB, indexB, length, false, CultureInfo.CurrentCulture);
		}

		public static int Compare (String strA, int indexA, String strB, int indexB, int length, bool ignoreCase)
		{
			return Compare (strA, indexA, strB, indexB, length, ignoreCase, CultureInfo.CurrentCulture);
		}
		
		public static int Compare (String strA, int indexA, String strB, int indexB, int length, bool ignoreCase, CultureInfo culture)
		{
			if (culture == null)
				throw new ArgumentNullException ("culture");

			if ((indexA > strA.Length) || (indexB > strB.Length) || (indexA < 0) || (indexB < 0) || (length < 0))
				throw new ArgumentOutOfRangeException ();

			if (length == 0)
				return 0;
			
			if (strA == null) {
				if (strB == null) {
					return 0;
				} else {
					return -1;
				}
			}
			else if (strB == null) {
				return 1;
			}

			CompareOptions compopts;

			if (ignoreCase)
				compopts = CompareOptions.IgnoreCase;
			else
				compopts = CompareOptions.None;

			/* Need to cap the requested length to the
			 * length of the string, because
			 * CompareInfo.Compare will insist that length
			 * <= (string.Length - offset)
			 */
			int len1 = length;
			int len2 = length;
			
			if (length > (strA.Length - indexA)) {
				len1 = strA.Length - indexA;
			}

			if (length > (strB.Length - indexB)) {
				len2 = strB.Length - indexB;
			}

			return culture.CompareInfo.Compare (strA, indexA, len1, strB, indexB, len2, compopts);
		}

		public int CompareTo (Object value)
		{
			if (value == null)
				return 1;

			if (!(value is String))
				throw new ArgumentException ();

			return String.Compare (this, (String) value, false);
		}

		public int CompareTo (String strB)
		{
			if (strB == null)
				return 1;

			return Compare (this, strB, false);
		}

		public static int CompareOrdinal (String strA, String strB)
		{
			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;
			}
			else if (strB == null) {
				return 1;
			}

			/* Invariant, because that is cheaper to
			 * instantiate (and chances are it already has
			 * been.)
			 */
			return CultureInfo.InvariantCulture.CompareInfo.Compare (strA, strB, CompareOptions.Ordinal);
		}

		public static int CompareOrdinal (String strA, int indexA, String strB, int indexB, int length)
		{
			if ((indexA > strA.Length) || (indexB > strB.Length) || (indexA < 0) || (indexB < 0) || (length < 0))
				throw new ArgumentOutOfRangeException ();

			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;
			}
			else if (strB == null) {
				return 1;
			}

			/* Need to cap the requested length to the
			 * length of the string, because
			 * CompareInfo.Compare will insist that length
			 * <= (string.Length - offset)
			 */
			int len1 = length;
			int len2 = length;

			if (length > (strA.Length - indexA)) {
				len1 = strA.Length - indexA;
			}

			if (length > (strB.Length - indexB)) {
				len2 = strB.Length - indexB;
			}

			return CultureInfo.InvariantCulture.CompareInfo.Compare (strA, indexA, len1, strB, indexB, len2, CompareOptions.Ordinal);
		}

		public bool EndsWith (String value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if (value.Length == 0)
				return true;

			if (value.length > this.length)
				return false;

			return (0 == Compare (this, length - value.length, value, 0, value.length));
		}

		public int IndexOfAny (char [] anyOf)
		{
			if (anyOf == null)
				throw new ArgumentNullException ("anyOf");

			return InternalIndexOfAny (anyOf, 0, this.length);
		}

		public int IndexOfAny (char [] anyOf, int startIndex)
		{
			if (anyOf == null)
				throw new ArgumentNullException ("anyOf");
			if (startIndex < 0 || startIndex >= this.length)
				throw new ArgumentOutOfRangeException ("sourceIndex");

			return InternalIndexOfAny (anyOf, startIndex, this.length - startIndex);
		}

		public int IndexOfAny (char [] anyOf, int startIndex, int count)
		{
			if (anyOf == null)
				throw new ArgumentNullException ("anyOf");
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException ("startIndex", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// re-ordered to avoid possible integer overflow
			if (startIndex > this.length - count)
				throw new ArgumentOutOfRangeException ("startIndex + count > this.length");

			return InternalIndexOfAny (anyOf, startIndex, count);
		}

		public int IndexOf (char value)
		{
			return IndexOf (value, 0, this.length);
		}

		public int IndexOf (String value)
		{
			return IndexOf (value, 0, this.length);
		}

		public int IndexOf (char value, int startIndex)
		{
			return IndexOf (value, startIndex, this.length - startIndex);
		}

		public int IndexOf (String value, int startIndex)
		{
			return IndexOf (value, startIndex, this.length - startIndex);
		}

		/* This method is culture-insensitive */
		public int IndexOf (char value, int startIndex, int count)
		{
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException ("startIndex", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// re-ordered to avoid possible integer overflow
			if (startIndex > this.length - count)
				throw new ArgumentOutOfRangeException ("startIndex + count > this.length");

			if ((startIndex == 0 && this.length == 0) || (startIndex == this.length) || (count == 0))
				return -1;

			for (int pos = startIndex; pos < startIndex + count; pos++) {
				if (this[pos] == value)
					return(pos);
			}
			return -1;
		}

		/* But this one is culture-sensitive */
		public int IndexOf (String value, int startIndex, int count)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException ("startIndex", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// re-ordered to avoid possible integer overflow
			if (startIndex > this.length - count)
				throw new ArgumentOutOfRangeException ("startIndex + count > this.length");

			if (value.length == 0)
				return startIndex;

			if (startIndex == 0 && this.length == 0)
				return -1;

			if (count == 0)
				return -1;

			return CultureInfo.CurrentCulture.CompareInfo.IndexOf (this, value, startIndex, count);
		}

		public int LastIndexOfAny (char [] anyOf)
		{
			if (anyOf == null)
				throw new ArgumentNullException ("anyOf");

			return InternalLastIndexOfAny (anyOf, this.length - 1, this.length);
		}

		public int LastIndexOfAny (char [] anyOf, int startIndex)
		{
			if (anyOf == null) 
				throw new ArgumentNullException ("anyOf");

			if (startIndex < 0 || startIndex > this.length)
				throw new ArgumentOutOfRangeException ();

			if (this.length == 0)
				return -1;

			return InternalLastIndexOfAny (anyOf, startIndex, startIndex + 1);
		}

		public int LastIndexOfAny (char [] anyOf, int startIndex, int count)
		{
			if (anyOf == null) 
				throw new ArgumentNullException ("anyOf");

			if ((startIndex < 0) || (startIndex > this.Length))
				throw new ArgumentOutOfRangeException ("startIndex", "< 0 || > this.Length");
			if ((count < 0) || (count > this.Length))
				throw new ArgumentOutOfRangeException ("count", "< 0 || > this.Length");
			if (startIndex - count + 1 < 0)
				throw new ArgumentOutOfRangeException ("startIndex - count + 1 < 0");

			if (this.length == 0)
				return -1;

			return InternalLastIndexOfAny (anyOf, startIndex, count);
		}

		public int LastIndexOf (char value)
		{
			if (this.length == 0)
				return -1;
			else
				return LastIndexOf (value, this.length - 1, this.length);
		}

		public int LastIndexOf (String value)
		{
			if (this.length == 0)
				/* This overload does additional checking */
				return LastIndexOf (value, 0, 0);
			else
				return LastIndexOf (value, this.length - 1, this.length);
		}

		public int LastIndexOf (char value, int startIndex)
		{
			return LastIndexOf (value, startIndex, startIndex + 1);
		}

		public int LastIndexOf (String value, int startIndex)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			int max = startIndex;
			if (max < this.Length)
				max++;
			return LastIndexOf (value, startIndex, max);
		}

		/* This method is culture-insensitive */
		public int LastIndexOf (char value, int startIndex, int count)
		{
			if (startIndex == 0 && this.length == 0)
				return -1;

			// >= for char (> for string)
			if ((startIndex < 0) || (startIndex >= this.Length))
				throw new ArgumentOutOfRangeException ("startIndex", "< 0 || >= this.Length");
			if ((count < 0) || (count > this.Length))
				throw new ArgumentOutOfRangeException ("count", "< 0 || > this.Length");
			if (startIndex - count + 1 < 0)
				throw new ArgumentOutOfRangeException ("startIndex - count + 1 < 0");

			for(int pos = startIndex; pos > startIndex - count; pos--) {
				if (this [pos] == value)
					return pos;
			}
			return -1;
		}

		/* But this one is culture-sensitive */
		public int LastIndexOf (String value, int startIndex, int count)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			// -1 > startIndex > for string (0 > startIndex >= for char)
			if ((startIndex < -1) || (startIndex > this.Length))
				throw new ArgumentOutOfRangeException ("startIndex", "< 0 || > this.Length");
			if ((count < 0) || (count > this.Length))
				throw new ArgumentOutOfRangeException ("count", "< 0 || > this.Length");
			if (startIndex - count + 1 < 0)
				throw new ArgumentOutOfRangeException ("startIndex - count + 1 < 0");

			if (value.Length == 0)
				return 0;

			if (startIndex == 0 && this.length == 0)
				return -1;

			// This check is needed to match undocumented MS behaviour
			if (this.length == 0 && value.length > 0)
				return -1;

			if (value.length > startIndex)
				return -1;

			if (count == 0)
				return -1;

			if (startIndex == this.Length)
				startIndex--;
			return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf (this, value, startIndex, count);
		}

		public String PadLeft (int totalWidth)
		{
			return PadLeft (totalWidth, ' ');
		}

		public String PadLeft (int totalWidth, char paddingChar)
		{
			if (totalWidth < 0)
				throw new ArgumentOutOfRangeException ("totalWidth", "< 0");

			if (totalWidth < this.length)
				return String.Copy (this);

			return InternalPad (totalWidth, paddingChar, false);
		}

		public String PadRight (int totalWidth)
		{
			return PadRight (totalWidth, ' ');
		}

		public String PadRight (int totalWidth, char paddingChar)
		{
			if (totalWidth < 0)
				throw new ArgumentOutOfRangeException ("totalWidth", "< 0");

			if (totalWidth < this.length)
				return String.Copy (this);

			return InternalPad (totalWidth, paddingChar, true);
		}

		public bool StartsWith (String value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			
			if (value.Length == 0)
				return true;

			if (this.length < value.length)
				return false;

			return (0 == Compare (this, 0, value, 0 , value.length));
		}

		/* This method is culture insensitive */
		public String Replace (char oldChar, char newChar)
		{
			return InternalReplace (oldChar, newChar);
		}

		/* This method is culture sensitive */
		public String Replace (String oldValue, String newValue)
		{
			if (oldValue == null)
				throw new ArgumentNullException ("oldValue");

			if (oldValue.Length == 0)
				throw new ArgumentException ("oldValue is the empty string.");

			if (this.Length == 0)
				return this;
			
			if (newValue == null)
				newValue = String.Empty;

			return InternalReplace (oldValue, newValue, CultureInfo.CurrentCulture.CompareInfo);
		}

		public String Remove (int startIndex, int count)
		{
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException ("startIndex", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// re-ordered to avoid possible integer overflow
			if (startIndex > this.length - count)
				throw new ArgumentOutOfRangeException ("startIndex + count > this.length");

			return InternalRemove (startIndex, count);
		}

		public String ToLower ()
		{
			// CurrentCulture can never be invariant or null
			return InternalToLower (CultureInfo.CurrentCulture);
		}

		public String ToLower (CultureInfo culture)
		{
			if (culture == null)
				throw new ArgumentNullException ("culture");

			if (culture.LCID == 0x007F) { // Invariant
				return ToLowerInvariant ();
			}
			return InternalToLower (culture);
		}

		internal unsafe String ToLowerInvariant ()
		{
			string tmp = InternalAllocateStr (length);
			fixed (char* source = &start_char, dest = tmp) {

				char* destPtr = (char*)dest;
				char* sourcePtr = (char*)source;

				for (int n = 0; n < length; n++) {
					*destPtr = Char.ToLowerInvariant (*sourcePtr);
					sourcePtr++;
					destPtr++;
				}
			}
			return tmp;
		}

		public String ToUpper ()
		{
			// CurrentCulture can never be invariant or null
			return InternalToUpper (CultureInfo.CurrentCulture);
		}

		public String ToUpper (CultureInfo culture)
		{
			if (culture == null)
				throw new ArgumentNullException ("culture");

			if (culture.LCID == 0x007F) { // Invariant
				return ToUpperInvariant ();
			}
			return InternalToUpper (culture);
		}

		internal unsafe String ToUpperInvariant ()
		{
			string tmp = InternalAllocateStr (length);
			fixed (char* source = &start_char, dest = tmp) {

				char* destPtr = (char*)dest;
				char* sourcePtr = (char*)source;

				for (int n = 0; n < length; n++) {
					*destPtr = Char.ToUpperInvariant (*sourcePtr);
					sourcePtr++;
					destPtr++;
				}
			}
			return tmp;
		}

		public override String ToString ()
		{
			return this;
		}

		public String ToString (IFormatProvider provider)
		{
			return this;
		}

		public String Trim ()
		{
			return Trim (null);
		}

		public static String Format (String format, Object arg0)
		{
			return Format (null, format, new Object[] {arg0});
		}

		public static String Format (String format, Object arg0, Object arg1)
		{
			return Format (null, format, new Object[] {arg0, arg1});
		}

		public static String Format (String format, Object arg0, Object arg1, Object arg2)
		{
			return Format (null, format, new Object[] {arg0, arg1, arg2});
		}

		public static string Format (string format, params object[] args)
		{
			return Format (null, format, args);
		}
	
		public static string Format (IFormatProvider provider, string format, params object[] args)
		{
			StringBuilder b = new StringBuilder ();
			FormatHelper (b, provider, format, args);
			return b.ToString ();
		}
		
		internal static void FormatHelper (StringBuilder result, IFormatProvider provider, string format, params object[] args)
		{
			if (format == null || args == null)
				throw new ArgumentNullException ();

			int ptr = 0;
			int start = ptr;
			while (ptr < format.length) {
				char c = format[ptr ++];

				if (c == '{') {
					result.Append (format, start, ptr - start - 1);

					// check for escaped open bracket

					if (format[ptr] == '{') {
						start = ptr ++;
						continue;
					}

					// parse specifier
				
					int n, width;
					bool left_align;
					string arg_format;

					ParseFormatSpecifier (format, ref ptr, out n, out width, out left_align, out arg_format);
					if (n >= args.Length)
						throw new FormatException ("Index (zero based) must be greater than or equal to zero and less than the size of the argument list.");

					// format argument

					object arg = args[n];

					string str;
					if (arg == null)
						str = "";
					else if (arg is IFormattable)
						str = ((IFormattable)arg).ToString (arg_format, provider);
					else
						str = arg.ToString ();

					// pad formatted string and append to result

					if (width > str.length) {
						string pad = new String (' ', width - str.length);

						if (left_align) {
							result.Append (str);
							result.Append (pad);
						}
						else {
							result.Append (pad);
							result.Append (str);
						}
					}
					else
						result.Append (str);

					start = ptr;
				}
				else if (c == '}' && ptr < format.length && format[ptr] == '}') {
					result.Append (format, start, ptr - start - 1);
					start = ptr ++;
				}
				else if (c == '}') {
					throw new FormatException ("Input string was not in a correct format.");
				}
			}

			if (start < format.length)
				result.Append (format.Substring (start));
		}

		public static String Copy (String str)
		{
			if (str == null)
				throw new ArgumentNullException ("str");

			int length = str.length;

			String tmp = InternalAllocateStr (length);
			InternalStrcpy (tmp, 0, str);
			return tmp;
		}

		public static String Concat (Object obj)
		{
			if (obj == null)
				return String.Empty;

			return obj.ToString ();
		}

		public static String Concat (Object obj1, Object obj2)
		{
			string s1, s2;

			s1 = (obj1 != null) ? obj1.ToString () : null;
			s2 = (obj2 != null) ? obj2.ToString () : null;
			
			if (s1 == null) {
				if (s2 == null)
					return String.Empty;
				else
					return s2;
			} else if (s2 == null)
				return s1;

			String tmp = InternalAllocateStr (s1.Length + s2.Length);
			InternalStrcpy (tmp, 0, s1);
			InternalStrcpy (tmp, s1.length, s2);

			return tmp;
		}

		public static String Concat (Object obj1, Object obj2, Object obj3)
		{
			string s1, s2, s3;
			if (obj1 == null)
				s1 = String.Empty;
			else
				s1 = obj1.ToString ();

			if (obj2 == null)
				s2 = String.Empty;
			else
				s2 = obj2.ToString ();

			if (obj3 == null)
				s3 = String.Empty;
			else
				s3 = obj3.ToString ();

			return Concat (s1, s2, s3);
		}

#if ! BOOTSTRAP_WITH_OLDLIB
		[CLSCompliant(false)]
		public static String Concat (Object obj1, Object obj2, Object obj3,
					     Object obj4, __arglist)
		{
			string s1, s2, s3, s4;

			if (obj1 == null)
				s1 = String.Empty;
			else
				s1 = obj1.ToString ();

			if (obj2 == null)
				s2 = String.Empty;
			else
				s2 = obj2.ToString ();

			if (obj3 == null)
				s3 = String.Empty;
			else
				s3 = obj3.ToString ();

			ArgIterator iter = new ArgIterator (__arglist);
			int argCount = iter.GetRemainingCount();

			StringBuilder sb = new StringBuilder ();
			if (obj4 != null)
				sb.Append (obj4.ToString ());

			for (int i = 0; i < argCount; i++) {
				TypedReference typedRef = iter.GetNextArg ();
				sb.Append (TypedReference.ToObject (typedRef));
			}

			s4 = sb.ToString ();

			return Concat (s1, s2, s3, s4);			
		}
#endif

		public static String Concat (String s1, String s2)
		{
			if (s1 == null) {
				if (s2 == null)
					return String.Empty;
				return s2;
			}

			if (s2 == null)
				return s1; 

			String tmp = InternalAllocateStr (s1.length + s2.length);

			InternalStrcpy (tmp, 0, s1);
			InternalStrcpy (tmp, s1.length, s2);

			return tmp;
		}

		public static String Concat (String s1, String s2, String s3)
		{
			if (s1 == null){
				if (s2 == null){
					if (s3 == null)
						return String.Empty;
					return s3;
				} else {
					if (s3 == null)
						return s2;
				}
				s1 = String.Empty;
			} else {
				if (s2 == null){
					if (s3 == null)
						return s1;
					else
						s2 = String.Empty;
				} else {
					if (s3 == null)
						s3 = String.Empty;
				}
			}

			//return InternalConcat (s1, s2, s3);
			String tmp = InternalAllocateStr (s1.length + s2.length + s3.length);

			InternalStrcpy (tmp, 0, s1);
			InternalStrcpy (tmp, s1.length, s2);
			InternalStrcpy (tmp, s1.length + s2.length, s3);

			return tmp;
		}

		public static String Concat (String s1, String s2, String s3, String s4)
		{
			if (s1 == null && s2 == null && s3 == null && s4 == null)
				return String.Empty;

			if (s1 == null)
				s1 = String.Empty;
			if (s2 == null)
				s2 = String.Empty;
			if (s3 == null)
				s3 = String.Empty;
			if (s4 == null)
				s4 = String.Empty;

			String tmp = InternalAllocateStr (s1.length + s2.length + s3.length + s4.length);

			InternalStrcpy (tmp, 0, s1);
			InternalStrcpy (tmp, s1.length, s2);
			InternalStrcpy (tmp, s1.length + s2.length, s3);
			InternalStrcpy (tmp, s1.length + s2.length + s3.length, s4);

			return tmp;
		}

		public static String Concat (params Object[] args)
		{
			if (args == null)
				throw new ArgumentNullException ("args");

			int i = args.Length;
			if (i == 0)
				return String.Empty;

			string [] strings = new string [i];
			i = 0;
			int len = 0;
			foreach (object arg in args) {
				if (arg == null) {
					strings[i] = String.Empty;
				} else {
					strings[i] = arg.ToString ();
					len += strings[i].length;
				}
				i++;
			}

			if (len == 0)
				return String.Empty;

			return InternalJoin (String.Empty, strings, 0, strings.Length);
		}

		public static String Concat (params String[] values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");

			return InternalJoin (String.Empty, values, 0, values.Length);
		}

		public String Insert (int startIndex, String value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if (startIndex < 0 || startIndex > this.length)
				throw new ArgumentOutOfRangeException ();

			return InternalInsert (startIndex, value);
		}


		public static string Intern (string str)
		{
			if (str == null)
				throw new ArgumentNullException ("str");

			return InternalIntern (str);
		}

		public static string IsInterned (string str)
		{
			if (str == null)
				throw new ArgumentNullException ("str");

			return InternalIsInterned (str);
		}
	
		public static string Join (string separator, string [] value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			return Join (separator, value, 0, value.Length);
		}

		public static string Join (string separator, string[] value, int startIndex, int count)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException ("startIndex", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// re-ordered to avoid possible integer overflow
			if (startIndex > value.Length - count)
				throw new ArgumentOutOfRangeException ("startIndex + count > value.length");

			if (startIndex == value.Length)
				return String.Empty;
			if (separator == null)
				separator = String.Empty;

			return InternalJoin (separator, value, startIndex, count);
		}

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return Convert.ToBoolean (this, provider);
		}

		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return Convert.ToByte (this, provider);
		}

		char IConvertible.ToChar (IFormatProvider provider)
		{
			return Convert.ToChar (this, provider);
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			return Convert.ToDateTime (this, provider);
		}

		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return Convert.ToDecimal (this, provider);
		}

		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return Convert.ToDouble (this, provider);
		}

		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return Convert.ToInt16 (this, provider);
		}

		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return Convert.ToInt32 (this, provider);
		}

		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return Convert.ToInt64 (this, provider);
		}
	
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return Convert.ToSByte (this, provider);
		}

		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return Convert.ToSingle (this, provider);
		}

		string IConvertible.ToString (IFormatProvider format)
		{
			return this;
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return Convert.ToType (this, conversionType,  provider);
		}

		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return Convert.ToUInt16 (this, provider);
		}

		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return Convert.ToUInt32 (this, provider);
		}

		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return Convert.ToUInt64 (this, provider);
		}

		TypeCode IConvertible.GetTypeCode ()
		{
			return TypeCode.String;
		}

		public int Length {
			get {
				return length;
			}
		}

		public CharEnumerator GetEnumerator ()
		{
			return new CharEnumerator (this);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new CharEnumerator (this);
		}

		private static void ParseFormatSpecifier (string str, ref int ptr, out int n, out int width,
		                                          out bool left_align, out string format)
		{
			// parses format specifier of form:
			//   N,[\ +[-]M][:F]}
			//
			// where:

			try {
				// N = argument number (non-negative integer)

				n = ParseDecimal (str, ref ptr);
				if (n < 0)
					throw new FormatException ("Input string was not in a correct format.");

				// M = width (non-negative integer)

				if (str[ptr] == ',') {
					// White space between ',' and number or sign.
					int start = ++ptr;
					while (Char.IsWhiteSpace (str [ptr]))
						++ptr;

					format = str.Substring (start, ptr - start);

					left_align = (str [ptr] == '-');
					if (left_align)
						++ ptr;

					width = ParseDecimal (str, ref ptr);
					if (width < 0)
						throw new FormatException ("Input string was not in a correct format.");
				}
				else {
					width = 0;
					left_align = false;
					format = "";
				}

				// F = argument format (string)

				if (str[ptr] == ':') {
					int start = ++ ptr;
					while (str[ptr] != '}')
						++ ptr;

					format += str.Substring (start, ptr - start);
				}
				else
					format = null;

				if (str[ptr ++] != '}')
					throw new FormatException ("Input string was not in a correct format.");
			}
			catch (IndexOutOfRangeException) {
				throw new FormatException ("Input string was not in a correct format.");
			}
		}

		private static int ParseDecimal (string str, ref int ptr)
		{
			int p = ptr;
			int n = 0;
			while (true) {
				char c = str[p];
				if (c < '0' || '9' < c)
					break;

				n = n * 10 + c - '0';
				++ p;
			}

			if (p == ptr)
				return -1;

			ptr = p;
			return n;
		}

		internal unsafe void InternalSetChar (int idx, char val)
		{
			if ((uint) idx >= (uint) Length)
				throw new ArgumentOutOfRangeException ("idx");

			fixed (char * pStr = &start_char) 
			{
				pStr [idx] = val;
			}
		}

		internal unsafe void InternalSetLength (int newLength)
		{
			if (newLength > length)
				throw new ArgumentOutOfRangeException ("newLength", "newLength as to be <= length");

			length = newLength;

			// zero terminate, we can pass string objects directly via pinvoke
			fixed (char * pStr = &start_char) {
				pStr [length] = '\0';
			}
		}

		public unsafe override int GetHashCode ()
		{
			fixed (char * c = this) {
				char * cc = c;
				char * end = cc + length - 1;
				int h = 0;
				for (;cc < end; cc += 2) {
					h = (h << 5) - h + *cc;
					h = (h << 5) - h + cc [1];
				}
				++end;
				if (cc < end)
					h = (h << 5) - h + *cc;
				return h;
			}
		}

		[CLSCompliant (false), MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe public extern String (char *value);

		[CLSCompliant (false), MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe public extern String (char *value, int startIndex, int length);

		[CLSCompliant (false), MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe public extern String (sbyte *value);

		[CLSCompliant (false), MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe public extern String (sbyte *value, int startIndex, int length);

		[CLSCompliant (false), MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe public extern String (sbyte *value, int startIndex, int length, Encoding enc);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern String (char [] val, int startIndex, int length);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern String (char [] val);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern String (char c, int count);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string InternalJoin (string separator, string[] value, int sIndex, int count);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern String InternalInsert (int sourceIndex, String value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern String InternalReplace (char oldChar, char newChar);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern String InternalReplace (String oldValue, string newValue, CompareInfo comp);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern String InternalRemove (int sIndex, int count);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void InternalCopyTo (int sIndex, char[] dest, int destIndex, int count);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern String[] InternalSplit (char[] separator, int count);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern String InternalTrim (char[] chars, int typ);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern int InternalIndexOfAny (char [] arr, int sIndex, int count);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern int InternalLastIndexOfAny (char [] anyOf, int sIndex, int count);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern String InternalPad (int width, char chr, bool right);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern String InternalToLower (CultureInfo culture);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern String InternalToUpper (CultureInfo culture);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static String InternalAllocateStr (int length);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void InternalStrcpy (String dest, int destPos, String src);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void InternalStrcpy (String dest, int destPos, char[] chars);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void InternalStrcpy (String dest, int destPos, String src, int sPos, int count);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void InternalStrcpy (String dest, int destPos, char[] chars, int sPos, int count);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string InternalIntern (string str);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string InternalIsInterned (string str);
	}
}
