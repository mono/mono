// -*- Mode: C++; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.String.cs
//
// Author:
//   Jeffrey Stedfast (fejj@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

// FIXME: from what I gather from msdn, when a function is to return an empty string
//        we should be returning this.Empty - some methods do this and others don't.

// FIXME: I didn't realise until later that `string' has a .Length method and so
//        I am missing some proper bounds-checking in some methods. Find these
//        instances and throw the ArgumentOutOfBoundsException at the programmer.
//        I like pelting programmers with ArgumentOutOfBoundsException's :-)

// FIXME: The ToLower(), ToUpper(), and Compare(..., bool ignoreCase) methods
//        need to be made unicode aware.

// FIXME: when you have a char carr[], does carr.Length include the terminating null char?

using System;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {

	public sealed class String : IComparable, ICloneable, IConvertible, IEnumerable {
		public static readonly string Empty = "";
		private char[] c_str;
		private int length;

		// Constructors

		internal String (int storage)
		{
			length = storage;
			c_str = new char [storage];
		}
		
		[CLSCompliant(false)]
		unsafe public String (char *value)
		{
			int i;

			// FIXME: can I do value.Length here?
			if (value == null) {
				this.length = 0;
			} else {
				for (i = 0; *(value + i) != '\0'; i++);
				this.length = i;
			}

			this.c_str = new char [this.length + 1];
			for (i = 0; i < this.length; i++)
				this.c_str[i] = *(value + i);
		}

		public String (char[] value)
		{
			int i;

			// FIXME: value.Length includes the terminating null char?
			this.length = value != null ? strlen (value): 0;
			this.c_str = new char [this.length + 1];
			for (i = 0; i < this.length; i++)
				this.c_str[i] = value[i];
		}

		[CLSCompliant(false)]
		unsafe public String (sbyte *value)
		{
			// FIXME: consider unicode?
			int i;

			// FIXME: can I do value.Length here? */
			if (value == null) {
				this.length = 0;
			} else {
				for (i = 0; *(value + i) != '\0'; i++);
				this.length = i;
			}

			this.c_str = new char [this.length + 1];
			for (i = 0; i < this.length; i++)
				this.c_str[i] = (char) *(value + i);
		}

		public String (char c, int count)
		{
			int i;

			this.length = count;
			this.c_str = new char [count + 1];
			for (i = 0; i < count; i++)
				this.c_str[i] = c;
		}

		[CLSCompliant(false)]
		unsafe public String (char *value, int startIndex, int length)
		{
			int i;

			if (value == null && startIndex != 0 && length != 0)
				throw new ArgumentNullException ();

			if (startIndex < 0 || length < 0)
				throw new ArgumentOutOfRangeException ();

			this.length = length;
			this.c_str = new char [length + 1];
			for (i = 0; i < length; i++)
				this.c_str[i] = *(value + startIndex + i);
		}

		public String (char[] value, int startIndex, int length)
		{
			int i;

			if (value == null && startIndex != 0 && length != 0)
				throw new ArgumentNullException ();

			if (startIndex < 0 || length < 0)
				throw new ArgumentOutOfRangeException ();

			this.length = length;
			this.c_str = new char [length + 1];
			for (i = 0; i < length; i++)
				this.c_str[i] = value[startIndex + i];
		}

		[CLSCompliant(false)]
		unsafe public String (sbyte *value, int startIndex, int length)
		{
			// FIXME: consider unicode?
			int i;

			if (value == null && startIndex != 0 && length != 0)
				throw new ArgumentNullException ();

			if (startIndex < 0 || length < 0)
				throw new ArgumentOutOfRangeException ();

			this.length = length;
			this.c_str = new char [length + 1];
			for (i = 0; i < length; i++)
				this.c_str[i] = (char) *(value + startIndex + i);
		}

		[CLSCompliant(false)][MonoTODO]
		unsafe public String (sbyte *value, int startIndex, int length, Encoding enc)
		{
			// FIXME: implement me
		}

		~String ()
		{
			// FIXME: is there anything we need to do here?
			/*base.Finalize ();*/
		}

		// Properties
		public int Length {
			get {
				return this.length;
			}
		}

		// FIXME: is this correct syntax??
		public char this [int index] {
			get {
				if (index >= this.length)
					throw new ArgumentOutOfRangeException ();

				return this.c_str[index];
			}
		}

		// Private helper methods
		private static int strlen (char[] str)
		{
			// FIXME: if str.Length includes terminating null char, then return (str.Length - 1)
			return str.Length;
		}

		[MonoTODO]
		private static char tolowerordinal (char c)
		{
			// FIXME: implement me
			return c;
		}

		private static bool is_lwsp (char c)
		{
			/* this comes from the msdn docs for String.Trim() */
			if ((c >= '\x9' && c <= '\xD') || c == '\x20' || c == '\xA0' ||
			    (c >= '\x2000' && c <= '\x200B') || c == '\x3000' || c == '\xFEFF')
				return true;
			else
				return false;
		}

		private static int BoyerMoore (char[] haystack, string needle, int startIndex, int count)
		{
			/* (hopefully) Unicode-safe Boyer-Moore implementation */
			int[] skiptable = new int[65536];  /* our unicode-safe skip-table */
			int h, n, he, ne, hc, nc, i;

			if (haystack == null || needle == null)
				throw new ArgumentNullException ();

			/* if the search buffer is shorter than the pattern buffer, we can't match */
			if (count < needle.length)
				return -1;

			/* return an instant match if the pattern is 0-length */
			if (needle.length == 0)
				return startIndex;

			/* set a pointer at the end of each string */
			ne = needle.length - 1;      /* position of char before '\0' */
			he = startIndex + count;     /* position of last valid char */

			/* init the skip table with the pattern length */
			nc = needle.length;
			for (i = 0; i < 65536; i++)
				skiptable[i] = nc;

			/* set the skip value for the chars that *do* appear in the
			 * pattern buffer (needle) to the distance from the index to
			 * the end of the pattern buffer. */
			for (nc = 0; nc < ne; nc++)
				skiptable[(int) needle[nc]] = ne - nc;

			h = startIndex;
			while (count >= needle.length) {
				hc = h + needle.length - 1;  /* set the haystack compare pointer */
				nc = ne;                     /* set the needle compare pointer */

				/* work our way backwards until they don't match */
				for (i = 0; nc > 0; nc--, hc--, i++)
					if (needle[nc] != haystack[hc])
						break;

				if (needle[nc] != haystack[hc]) {
					n = skiptable[(int) haystack[hc]] - i;
					h += n;
					count -= n;
				} else
					return h;
			}

			return -1;
		}

		// Methods
		[MonoTODO]
		public object Clone ()
		{
			// FIXME: implement me
			return null;
		}

		public static int Compare (string strA, string strB)
		{
			int min, i;

			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;
			} else if (strB == null)
				return 1;

			min = strA.length < strB.length ? strA.length : strB.length;

			for (i = 0; i < min; i++) {
				if (strA.c_str[i] != strB.c_str[i]) {
					return (int)strA.c_str[i] - (int)strB.c_str[i];
				}
			}
			if (strA.length == strB.length) {
				return 0;
			}
			if (strA.length > min) {
				return 1;
			} else {
				return -1;
			}
		}

		public static int Compare (string strA, string strB, bool ignoreCase)
		{
			int min, i;

			if (!ignoreCase)
				return Compare (strA, strB);

			/*
			 * And here I thought Eazel developers were on crack...
			 * if a string is null it should pelt the programmer with
			 * ArgumentNullExceptions, damnit!
			 */
			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;
			} else if (strB == null)
				return 1;

			min = strA.length < strB.length ? strA.length : strB.length;

			for (i = 0; i < min; i++) {
				if (Char.ToLower (strA.c_str[i]) != Char.ToLower (strB.c_str[i]))
					break;
			}

			return ((int) (strA.c_str[i] - strB.c_str[i]));
		}

		[MonoTODO]
		public static int Compare (string strA, string strB, bool ignoreCase, CultureInfo culture)
		{
			// FIXME: implement me
			return 0;
		}

		public static int Compare (string strA, int indexA, string strB, int indexB, int length)
		{
			int i;

			if (length < 0 || indexA < 0 || indexB < 0)
				throw new ArgumentOutOfRangeException ();

			if (indexA > strA.Length || indexB > strB.Length)
				throw new ArgumentOutOfRangeException ();

			/* And again with the ("" > null) logic... lord have mercy! */
			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;
			} else if (strB == null)
				return 1;

			for (i = 0; i < length - 1; i++) {
				if (strA[indexA + i] != strB[indexB + i])
					break;
			}

			return ((int) (strA[indexA + i] - strB[indexB + i]));
		}

		public static int Compare (string strA, int indexA, string strB, int indexB,
					   int length, bool ignoreCase)
		{
			int i;

			if (!ignoreCase)
				return Compare (strA, indexA, strB, indexB, length);

			if (length < 0 || indexA < 0 || indexB < 0)
				throw new ArgumentOutOfRangeException ();

			if (indexA > strA.Length || indexB > strB.Length)
				throw new ArgumentOutOfRangeException ();

			/* When will the hurting stop!?!? */
			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;
			} else if (strB == null)
				return 1;

			for (i = 0; i < length - 1; i++) {
				if (Char.ToLower (strA[indexA + i]) != Char.ToLower (strB[indexB + i]))
					break;
			}

			return ((int) (strA[indexA + i] - strB[indexB + i]));
		}

		[MonoTODO]
		public static int Compare (string strA, int indexA, string strB, int indexB,
					   int length, bool ignoreCase, CultureInfo culture)
		{
			if (culture == null)
				throw new ArgumentNullException ();

			if (length < 0 || indexA < 0 || indexB < 0)
				throw new ArgumentOutOfRangeException ();

			if (indexA > strA.Length || indexB > strB.Length)
				throw new ArgumentOutOfRangeException ();

			/* I can't take it anymore! */
			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;
			} else if (strB == null)
				return 1;

			// FIXME: implement me
			return 0;
		}

		public static int CompareOrdinal (string strA, string strB)
		{
			int i;

			/* Please God, make it stop! */
			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;
			} else if (strB == null)
				return 1;

			for (i = 0; i < strA.Length && i < strB.Length; i++) {
				char cA, cB;

				cA = tolowerordinal (strA[i]);
				cB = tolowerordinal (strB[i]);

				if (cA != cB)
					break;
			}

			return ((int) (strA[i] - strB[i]));
		}

		public static int CompareOrdinal (string strA, int indexA, string strB, int indexB,
						  int length)
		{
			int i;

			if (length < 0 || indexA < 0 || indexB < 0)
				throw new ArgumentOutOfRangeException ();

			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;
			} else if (strB == null)
				return 1;

			for (i = 0; i < length; i++) {
				char cA, cB;

				cA = tolowerordinal (strA[indexA + i]);
				cB = tolowerordinal (strB[indexB + i]);

				if (cA != cB)
					break;
			}

			return ((int) (strA[indexA + i] - strB[indexB + i]));
		}

		public int CompareTo (object obj)
		{
			return Compare (this, obj == null ? null : obj.ToString ());
		}

		public int CompareTo (string str)
		{
			return Compare (this, str);
		}

		public static string Concat (object arg)
		{
			return arg != null ? arg.ToString () : String.Empty;
		}

		public static string Concat (params object[] args)
		{
			string[] strings;
			char[] str;
			int len, i;

			if (args == null)
				throw new ArgumentNullException ();

			strings = new string [args.Length];
			len = 0;
			i = 0;
			foreach (object arg in args) {
				/* use Empty for each null argument */
				if (arg == null)
					strings[i] = String.Empty;
				else
					strings[i] = arg.ToString ();
				len += strings[i].length;
				i++;
			}

			if (len == 0)
				return String.Empty;

			String res = new String (len);
			str = res.c_str;
			i = 0;
			for (int j = 0; j < strings.Length; j++)
				for (int k = 0; k < strings[j].length; k++)
					str[i++] = strings[j].c_str[k];

			return res;
		}

		public static string Concat (params string[] values)
		{
			int len, i;
			char[] str;

			if (values == null)
				throw new ArgumentNullException ();

			len = 0;
			foreach (string value in values)
				len += value != null ? value.Length : 0;

			if (len == 0)
				return String.Empty;

			String res = new String (len);
			str = res.c_str;
			i = 0;
			foreach (string value in values) {
				if (value == null)
					continue;

				for (int j = 0; j < value.length; j++)
					str[i++] = value.c_str[j];
			}

			return res;
		}

		public static string Concat (object arg0, object arg1)
		{
			string str0 = arg0 != null ? arg0.ToString () : String.Empty;
			string str1 = arg1 != null ? arg1.ToString () : String.Empty;

			return Concat (str0, str1);
		}

		public static string Concat (string str0, string str1)
		{
			char[] concat;
			int i, j, len;

			if (str0 == null)
				str0 = String.Empty;
			if (str1 == null)
				str1 = String.Empty;

			len = str0.length + str1.length;
			if (len == 0)
				return String.Empty;

			String res = new String (len);

			concat = res.c_str;
			for (i = 0; i < str0.length; i++)
				concat[i] = str0.c_str[i];
			for (j = 0 ; j < str1.length; j++)
				concat[i + j] = str1.c_str[j];

			return res;
		}

		public static string Concat (object arg0, object arg1, object arg2)
		{
			string str0 = arg0 != null ? arg0.ToString () : String.Empty;
			string str1 = arg1 != null ? arg1.ToString () : String.Empty;
			string str2 = arg2 != null ? arg2.ToString () : String.Empty;

			return Concat (str0, str1, str2);
		}

		public static string Concat (string str0, string str1, string str2)
		{
			char[] concat;
			int i, j, k, len;

			if (str0 == null)
				str0 = String.Empty;
			if (str1 == null)
				str1 = String.Empty;
			if (str2 == null)
				str2 = String.Empty;

			len = str0.length + str1.length + str2.length;
			if (len == 0)
				return String.Empty;

			String res = new String (len);

			concat = res.c_str;
			for (i = 0; i < str0.length; i++)
				concat[i] = str0.c_str[i];
			for (j = 0; j < str1.length; j++)
				concat[i + j] = str1.c_str[j];
			for (k = 0; k < str2.length; k++)
				concat[i + j + k] = str2.c_str[k];

			return res;
		}

		public static string Concat (string str0, string str1, string str2, string str3)
		{
			char[] concat;
			int i, j, k, l, len;

			if (str0 == null)
				str0 = String.Empty;
			if (str1 == null)
				str1 = String.Empty;
			if (str2 == null)
				str2 = String.Empty;
			if (str3 == null)
				str3 = String.Empty;

			len = str0.length + str1.length + str2.length + str3.length;
			if (len == 0)
				return String.Empty;
			String res = new String (len);

			concat = res.c_str;
			for (i = 0; i < str0.length; i++)
				concat[i] = str0.c_str[i];
			for (j = 0; j < str1.length; j++)
				concat[i + j] = str1.c_str[j];
			for (k = 0; k < str2.length; k++)
				concat[i + j + k] = str2.c_str[k];
			for (l = 0; l < str3.length; l++)
				concat[i + j + k + l] = str3.c_str[l];

			return res;
		}

		public static string Copy (string str)
		{
			// FIXME: how do I *copy* a string if I can only have 1 of each?
			if (str == null)
				throw new ArgumentNullException ();

			return str;
		}

		public void CopyTo (int sourceIndex, char[] destination, int destinationIndex, int count)
		{
			// LAMESPEC: should I null-terminate?
			int i;

			if (destination == null)
				throw new ArgumentNullException ();

			if (sourceIndex < 0 || destinationIndex < 0 || count < 0)
				throw new ArgumentOutOfRangeException ();

			if (sourceIndex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			if (destinationIndex + count > destination.Length)
				throw new ArgumentOutOfRangeException ();

			for (i = 0; i < count; i++)
				destination[destinationIndex + i] = this.c_str[sourceIndex + i];
		}

		public bool EndsWith (string value)
		{
			bool endswith = true;
			int start, i;

			if (value == null)
				throw new ArgumentNullException ();

			start = this.length - value.length;
			if (start < 0)
				return false;

			for (i = start; i < this.length && endswith; i++)
				endswith = this.c_str[i] == value.c_str[i - start];

			return endswith;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is String))
				return false;

			return this == (String) obj;
		}

		public bool Equals (string value)
		{
			return this == value;
		}

		public static bool Equals (string a, string b)
		{
			return a == b;
		}

		[MonoTODO]
		public static string Format (string format, object arg0)
		{
			// FIXME: implement me
			return format+arg0.ToString();
		}

		[MonoTODO]
		public static string Format (string format, params object[] args)
		{
			// FIXME: implement me
			Console.WriteLine (args[0].ToString());
			if (args.Length == 1)
				return format+args[0].ToString();
			if (args.Length == 2)
				return format+args[0].ToString()+args[1].ToString();
			if (args.Length == 3)
				return format+args[0].ToString()+args[1].ToString()+args[2].ToString();
			if (args.Length == 4)
				return format+args[0].ToString()+args[1].ToString()+args[2].ToString()+args[3].ToString();
			Console.WriteLine ("String.Format with args: "+args.Length.ToString());
			return format;
		}
		
		[MonoTODO]
		public static string Format (IFormatProvider provider, string format, params object[] args)
		{
			// FIXME: implement me
			return null;
		}
		
		[MonoTODO]
		public static string Format (string format, object arg0, object arg1)
		{
			// FIXME: implement me
			return format+arg0.ToString()+arg1.ToString();
		}

		[MonoTODO]
		public static string Format (string format, object arg0, object arg1, object arg2)
		{
			// FIXME: implement me
			return format+arg0.ToString()+arg1.ToString()+arg2.ToString();
		}

		//public CharEnumerator GetEnumerator ()
		[MonoTODO]
                public IEnumerator GetEnumerator ()
		{
			// FIXME: implement me
			return null;
		}

		public override int GetHashCode ()
		{
			int h = 0;
			int i;
			for (i = 0; i < length; ++i)
				h = (h << 5) - h + c_str [i];
			return h;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.String;
		}

		public int IndexOf (char value)
		{
			return IndexOf (value, 0, this.length);
		}

		public int IndexOf (string value)
		{
			return IndexOf (value, 0, this.length);
		}

		public int IndexOf (char value, int startIndex)
		{
			return IndexOf (value, startIndex, this.length - startIndex);
		}

		public int IndexOf (string value, int startIndex)
		{
			return IndexOf (value, startIndex, this.length - startIndex);
		}

		public int IndexOf (char value, int startIndex, int count)
		{
			int i;

			if (startIndex < 0 || count < 0 || startIndex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			for (i = startIndex; i - startIndex < count; i++)
				if (this.c_str[i] == value)
					return i;

			return -1;
		}

		public int IndexOf (string value, int startIndex, int count)
		{
			if (value == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || count < 0 || startIndex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			return BoyerMoore (this.c_str, value, startIndex, count);
#if XXX
			int i;
			for (i = startIndex; i - startIndex + value.Length <= count; ) {
				if (this.c_str[i] == value[0]) {
					bool equal = true;
					int j, nexti = 0;

					for (j = 1; equal && j < value.Length; j++) {
						equal = this.c_str[i + j] == value[j];
						if (this.c_str[i + j] == value[0] && nexti == 0)
							nexti = i + j;
					}

					if (equal)
						return i;

					if (nexti != 0)
						i = nexti;
					else
						i += j;
				} else
					i++;
			}

			return -1;
#endif
		}

		public int IndexOfAny (char[] values)
		{
			return IndexOfAny (values, 0, this.length);
		}

		public int IndexOfAny (char[] values, int startIndex)
		{
			return IndexOfAny (values, startIndex, this.length - startIndex);
		}

		public int IndexOfAny (char[] values, int startIndex, int count)
		{
			if (values == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || count < 0 || startIndex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			for (int i = startIndex; i < startIndex + count; i++) {
				for (int j = 0; j < strlen (values); j++) {
					if (this.c_str[i] == values[j])
						return i;
				}
			}

			return -1;
		}

		public string Insert (int startIndex, string value)
		{
			char[] str;
			int i, j;

			if (value == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || startIndex > this.length)
				throw new ArgumentOutOfRangeException ();

			String res = new String (value.length + this.length);

			str = res.c_str;
			for (i = 0; i < startIndex; i++)
				str[i] = this.c_str[i];
			for (j = 0; j < value.length; j++)
				str[i + j] = value.c_str[j];
			for ( ; i < this.length; i++)
				str[i + j] = this.c_str[i];

			return res;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static string Intern (string str);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static string IsInterned (string str);

		public static string Join (string separator, string[] value)
		{
			return Join (separator, value, 0, value.Length);
		}

		public static string Join (string separator, string[] value, int startIndex, int count)
		{
			// LAMESPEC: msdn doesn't specify what happens when separator is null
			int len, i, j, used;
			char[] str;

			if (separator == null || value == null)
				throw new ArgumentNullException ();

			if (startIndex + count > value.Length)
				throw new ArgumentOutOfRangeException ();

			len = 0;
			for (i = startIndex, used = 0; used < count; i++, used++) {
				if (i != startIndex)
					len += separator.length;

				len += value[i].length;
			}

			// We have no elements to join?
			if (i == startIndex)
				return String.Empty;

			String res = new String (len);

			str = res.c_str;
			for (i = 0; i < value[startIndex].length; i++)
				str[i] = value[startIndex][i];

			used = 1;
			for (j = startIndex + 1; used < count; j++, used++) {
				int k;

				for (k = 0; k < separator.length; k++)
					str[i++] = separator.c_str[k];
				for (k = 0; k < value[j].length; k++)
					str[i++] = value[j].c_str[k];
			}

			return res;
		}

		public int LastIndexOf (char value)
		{
			int i = this.length;
			if (i == 0)
				return -1;
			--i;
			for (; i >= 0; i--) {
				if (this.c_str[i] == value)
					return i;
			}

			return -1;
		}

		public int LastIndexOf (string value)
		{
			return LastIndexOf (value, this.length - 1, this.length);
		}

		public int LastIndexOf (char value, int startIndex)
		{
			if (startIndex < 0 || startIndex >= this.length)
				throw new ArgumentOutOfRangeException ();

			for (int i = startIndex; i >= 0; i--) {
				if (this.c_str[i] == value)
					return i;
			}

			return -1;
		}

		public int LastIndexOf (string value, int startIndex)
		{
			return LastIndexOf (value, startIndex, startIndex + 1);
		}

		public int LastIndexOf (char value, int startIndex, int count)
		{
			if (startIndex < 0 || count < 0)
				throw new ArgumentOutOfRangeException ();

			if (startIndex >= this.length || startIndex - count + 1 < 0)
				throw new ArgumentOutOfRangeException ();

			for (int i = startIndex; i > startIndex - count; i--) {
				if (this.c_str[i] == value)
					return i;
			}

			return -1;
		}

		public int LastIndexOf (string value, int startIndex, int count)
		{
			// LAMESPEC: currently I'm using startIndex as the 0-position in the comparison,
			//           but maybe it's the end-position in MS's implementation?
			//           msdn is unclear on this point. I think this is correct though.
			int i, len;

			if (value == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || startIndex >= this.length)
				throw new ArgumentOutOfRangeException ();

			if (count < 0 || startIndex - count + 1 < 0)
				throw new ArgumentOutOfRangeException ();

			if (value == String.Empty)
				return startIndex;

			if (startIndex + value.length > this.length) {
				/* just a little optimization */
				int start;

				start = this.length - value.length;
				count -= startIndex - start;
				startIndex = start;
			}

			// FIXME: use a reversed-unicode-safe-Boyer-Moore?
			len = value.length - 1;
			for (i = startIndex; i > startIndex - count; i--) {
				if (this.c_str[i + len] == value.c_str[len]) {
					bool equal = true;
					int j;

					for (j = len - 1; equal && j >= 0; j--)
						equal = this.c_str[i + j] == value.c_str[j];

					if (equal)
						return i;
				}
			}

			return -1;
		}

		public int LastIndexOfAny (char[] values)
		{
			return LastIndexOfAny (values, this.length - 1, this.length);
		}

		public int LastIndexOfAny (char[] values, int startIndex)
		{
			return LastIndexOfAny (values, startIndex, startIndex + 1);
		}

		public int LastIndexOfAny (char[] values, int startIndex, int count)
		{
			int i;

			if (values == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || count < 0 || startIndex - count + 1 < 0)
				throw new ArgumentOutOfRangeException ();

			for (i = startIndex; i > startIndex - count; i--) {
				for (int j = 0; j < strlen (values); j++) {
					if (this.c_str[i] == values[j])
						return i;
				}
			}

			return -1;
		}

		public string PadLeft (int totalWidth)
		{
			return PadLeft (totalWidth, ' ');
		}

		public string PadLeft (int totalWidth, char padChar)
		{
			char[] str;
			int i, j;

			if (totalWidth < 0)
				throw new ArgumentException ();

			str = new char [totalWidth > this.length ? totalWidth : this.length];
			for (i = 0; i < totalWidth - this.length; i++)
				str[i] = padChar;

			for (j = 0; j < this.length; i++, j++)
				str[i] = this.c_str[j];

			return new String (str);
		}

		public string PadRight (int totalWidth)
		{
			return PadRight (totalWidth, ' ');
		}

		public string PadRight (int totalWidth, char padChar)
		{
			char[] str;
			int i;

			if (totalWidth < 0)
				throw new ArgumentException ();

			str = new char [totalWidth > this.length ? totalWidth : this.length];
			for (i = 0; i < this.length; i++)
				str[i] = this.c_str[i];

			for ( ; i < str.Length; i++)
				str[i] = padChar;

			return new String (str);
		}

		public string Remove (int startIndex, int count)
		{
			char[] str;
			int i, j, len;

			if (startIndex < 0 || count < 0 || startIndex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			len = this.length - count;
			if (len == 0)
				return String.Empty;
			
			String res = new String (len);
			str = res.c_str;
			for (i = 0; i < startIndex; i++)
				str[i] = this.c_str[i];
			for (j = i + count; j < this.length; j++)
				str[i++] = this.c_str[j];

			return res;
		}

		public string Replace (char oldChar, char newChar)
		{
			char[] str;
			int i;

			String res = new String (length);
			str = res.c_str;
			for (i = 0; i < this.length; i++) {
				if (this.c_str[i] == oldChar)
					str[i] = newChar;
				else
					str[i] = this.c_str[i];
			}

			return res;
		}

		public string Replace (string oldValue, string newValue)
		{
			// LAMESPEC: msdn doesn't specify what to do if either args is null
			int index, len, i, j;
			char[] str;

			if (oldValue == null || newValue == null)
				throw new ArgumentNullException ();

			// Use IndexOf in case I later rewrite it to use Boyer-Moore
			index = IndexOf (oldValue, 0);
			if (index == -1) {
				// This is the easy one ;-)
				return Substring (0, this.length);
			}

			len = this.length - oldValue.length + newValue.length;
			if (len == 0)
				return String.Empty;

			String res = new String (len);
			str = res.c_str;
			for (i = 0; i < index; i++)
				str[i] = this.c_str[i];
			for (j = 0; j < newValue.length; j++)
				str[i++] = newValue[j];
			for (j = index + oldValue.length; j < this.length; j++)
				str[i++] = this.c_str[j];

			return res;
		}

		private int splitme (char[] separators, int startIndex)
		{
			/* this is basically a customized IndexOfAny() for the Split() methods */
			for (int i = startIndex; i < this.length; i++) {
				if (separators != null) {
					foreach (char sep in separators) {
						if (this.c_str[i] == sep)
							return i - startIndex;
					}
				} else if (is_lwsp (this.c_str[i])) {
					return i - startIndex;
				}
			}

			return -1;
		}

		public string[] Split (params char[] separator)
		{
			/**
			 * split:
			 * @separator: delimiting chars or null to split on whtspc
			 *
			 * Returns: 1. An array consisting of a single
			 * element (@this) if none of the delimiting
			 * chars appear in @this. 2. An array of
			 * substrings which are delimited by one of
			 * the separator chars. 3. An array of
			 * substrings separated by whitespace if
			 * @separator is null. The Empty string should
			 * be returned wherever 2 delimiting chars are
			 * adjacent.
			 **/
			// FIXME: would using a Queue be better?
			string[] strings;
			ArrayList list;
			int index, len;

			list = new ArrayList ();
			for (index = 0, len = 0; index < this.length; index += len + 1) {
				len = splitme (separator, index);
				len = len > -1 ? len : this.length - index;
				if (len == 0) {
					list.Add (String.Empty);
				} else {
					char[] str;
					int i;

					str = new char [len];
					for (i = 0; i < len; i++)
						str[i] = this.c_str[index + i];

					list.Add (new String (str));
				}
			}

			strings = new string [list.Count];
			if (list.Count == 1) {
				/* special case for an array holding @this */
				strings[0] = this;
			} else {
				for (index = 0; index < list.Count; index++)
					strings[index] = (string) list[index];
			}

			return strings;
		}

		public string[] Split (char[] separator, int maxCount)
		{
			// FIXME: what to do if maxCount <= 0?
			// FIXME: would using Queue be better than ArrayList?
			string[] strings;
			ArrayList list;
			int index, len, used;

			used = 0;
			list = new ArrayList ();
			for (index = 0, len = 0; index < this.length && used < maxCount; index += len + 1) {
				len = splitme (separator, index);
				len = len > -1 ? len : this.length - index;
				if (len == 0) {
					list.Add (String.Empty);
				} else {
					char[] str;
					int i;

					str = new char [len];
					for (i = 0; i < len; i++)
						str[i] = this.c_str[index + i];

					list.Add (new String (str));
				}
				used++;
			}

			/* fit the remaining chunk of the @this into it's own element */
			if (index != this.length) {
				char[] str;
				int i;

				str = new char [this.length - index];
				for (i = index; i < this.length; i++)
					str[i - index] = this.c_str[i];

				list.Add (new String (str));
			}

			strings = new string [list.Count];
			if (list.Count == 1) {
				/* special case for an array holding @this */
				strings[0] = this;
			} else {
				for (index = 0; index < list.Count; index++)
					strings[index] = (string) list[index];
			}

			return strings;
		}

		public bool StartsWith (string value)
		{
			bool startswith = true;
			int i;

			if (value == null)
				throw new ArgumentNullException ();

			if (value.length > this.length)
				return false;

			for (i = 0; i < value.length && startswith; i++)
				startswith = startswith && value.c_str[i] == this.c_str[i];

			return startswith;
		}

		public string Substring (int startIndex)
		{
			char[] str;
			int i, len;

			if (startIndex < 0 || startIndex > this.length)
				throw new ArgumentOutOfRangeException ();

			len = this.length - startIndex;
			if (len == 0)
				return String.Empty;
			String res = new String (len);
			str = res.c_str;
			for (i = startIndex; i < this.length; i++)
				str[i - startIndex] = this.c_str[i];

			return res;
		}

		public string Substring (int startIndex, int length)
		{
			char[] str;
			int i;

			if (startIndex < 0 || length < 0 || startIndex + length > this.length)
				throw new ArgumentOutOfRangeException ();

			if (length == 0)
				return String.Empty;
			
			String res = new String (length);
			str = res.c_str;
			for (i = startIndex; i < startIndex + length; i++)
				str[i - startIndex] = this.c_str[i];

			return res;
		}

		[MonoTODO]
		public bool ToBoolean (IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public byte ToByte (IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public char ToChar (IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		public char[] ToCharArray ()
		{
			return ToCharArray (0, this.length);
		}

		public char[] ToCharArray (int startIndex, int length)
		{
			char[] chars;
			int i;

			if (startIndex < 0 || length < 0 || startIndex + length > this.length)
				throw new ArgumentOutOfRangeException ();

			chars = new char [length];
			for (i = startIndex; i < length; i++)
				chars[i - startIndex] = this.c_str[i];

			return chars;
		}

		[MonoTODO]
		public DateTime ToDateTime (IFormatProvider provider)
		{
			// FIXME: implement me
			// return new DateTime (0);
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public decimal ToDecimal (IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public double ToDouble (IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public short ToInt16 (IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int ToInt32 (IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long ToInt64 (IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		public string ToLower ()
		{
			char[] str;
			int i;

			String res = new String (length);
			str = res.c_str;
			for (i = 0; i < this.length; i++)
				str[i] = Char.ToLower (this.c_str[i]);

			return res;
		}

		[MonoTODO]
		public string ToLower (CultureInfo culture)
		{
			// FIXME: implement me
			throw new NotImplementedException ();

		}

		[CLSCompliant(false)][MonoTODO]
		public sbyte ToSByte (IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public float ToSingle (IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return this;
		}

		[MonoTODO]
		public string ToString (IFormatProvider format)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object ToType (Type conversionType, IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		[CLSCompliant(false)][MonoTODO]
		public ushort ToUInt16 (IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		[CLSCompliant(false)][MonoTODO]
		public uint ToUInt32 (IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		[CLSCompliant(false)][MonoTODO]
		public ulong ToUInt64 (IFormatProvider provider)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		public string ToUpper ()
		{
			char[] str;
			int i;

			String res = new String (length);
			str = res.c_str;
			for (i = 0; i < this.length; i++)
				str[i] = Char.ToUpper (this.c_str[i]);

			return res;
		}

		[MonoTODO]
		public string ToUpper (CultureInfo culture)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		public string Trim ()
		{
			return Trim (null);
		}

		public string Trim (params char[] trimChars)
		{
			int begin, end;
			bool matches;

			matches = true;
			for (begin = 0; matches && begin < this.length; begin++) {
				if (trimChars != null) {
					matches = false;
					foreach (char c in trimChars) {
						matches = this.c_str[begin] == c;
						if (matches)
							break;
					}
				} else {
					matches = is_lwsp (this.c_str[begin]);
				}
			}

			matches = true;
			for (end = this.length - 1; matches && end > begin; end--) {
				if (trimChars != null) {
					matches = false;
					foreach (char c in trimChars) {
						matches = this.c_str[end] == c;
						if (matches)
							break;
					}
				} else {
					matches = is_lwsp (this.c_str[end]);
				}
			}

			if (begin >= end)
				return String.Empty;

			return Substring (begin, end - begin);
		}

		public string TrimEnd (params char[] trimChars)
		{
			bool matches = true;
			int end;

			for (end = this.length; end > 0; end--) {
				if (trimChars != null) {
					matches = false;
					foreach (char c in trimChars) {
						matches = this.c_str[end] == c;
						if (matches)
							break;
					}
				} else {
					matches = is_lwsp (this.c_str[end]);
				}
			}

			if (end == 0)
				return String.Empty;

			return Substring (0, end);
		}

		public string TrimStart (params char[] trimChars)
		{
			bool matches = true;
			int begin;

			for (begin = 0; matches && begin < this.length; begin++) {
				if (trimChars != null) {
					matches = false;
					foreach (char c in trimChars) {
						matches = this.c_str[begin] == c;
						if (matches)
							break;
					}
				} else {
					matches = is_lwsp (this.c_str[begin]);
				}
			}

			if (begin == this.length)
				return String.Empty;

			return Substring (begin, this.length - begin);
		}

		// Operators
		public static bool operator ==(string a, string b)
		{
			if ((object)a == null) {
				if ((object)b == null)
					return true;
				return false;
			}
			if ((object)b == null)
				return false;
	
			if (a.length != b.length)
				return false;

			int l = a.length;
			for (int i = 0; i < l; i++)
				if (a.c_str[i] != b.c_str[i])
					return false;

			return true;
		}

		public static bool operator !=(string a, string b)
		{
			return !(a == b);
		}
	}
}
