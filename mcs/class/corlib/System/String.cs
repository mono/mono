// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
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

using System;

namespace System {
	
	public sealed class String : IComparable, IClonable, IConvertable, IEnumerable {
		public static string Empty = "";
		private char c_str[];
		private int length;

		// Constructors
		unsafe public String (char *value)
		{
			int i;

			if (value == null) {
				this.length = 0;
			} else {
				for (i = 0; *(value + i) != '\0'; i++);
				this.length = i;
			}

			this.c_str = new char [this.length + 1];
			for (i = 0; i < this.length; i++)
				this.c_str[i] = *(value + i);
			this.c_str[i] = '\0';
		}

		public String (char[] value)
		{
			int i, len = 0;

			if (value != null)
				for (this.len = 0; value[len] != '\0'; len++);

			this.length = len;
			this.c_str = new char [len + 1];
			for (i = 0; i < len; i++)
				this.c_str[i] = value[i];
			this.c_str[i] = '\0';
		}

		unsafe public String (sbyte *value)
		{
			// FIXME: consider unicode?
			int i;

			if (value == null) {
				this.length = 0;
			} else {
				for (i = 0; *(value + i) != '\0'; i++);
				this.length = i;
			}

			this.c_str = new char [this.length + 1];
			for (i = 0; i < this.length; i++)
				this.c_str[i] = *(value + i);
			this.c_str[i] = '\0';
		}

		public String (char c, int count)
		{
			int i;

			this.length = count;
			this.c_str = new char [count + 1];
			for (i = 0; i < count; i++)
				this.c_str[i] = c;
			this.c_str[i] = '\0';
		}

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
			this.c_str[i] = '\0';
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
			this.c_str[i] = '\0';
		}

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
				this.c_str[i] = *(value + startIndex + i);
			this.c_str[i] = '\0';
		}

		unsafe public String (sbyte *value, int startIndex, in length, Encoding enc)
		{
			// FIXME: implement me
		}

		protected ~String ()
		{
			delete this.c_str;
		}

		protected string MemberwiseClone ()
		{
			// FIXME: implement me
		}

		// Properties
		public int Length {
			get {
				return this.length;
			}
		}

		// FIXME: is this correct syntax??
		public char Chars (int index) {
			get {
				if (index > this.length)
					throw new ArgumentOutOfRangeException ();

				return this.c_str[index];
			}
		}

		// Methods
		public object Clone ()
		{
			// FIXME: implement me
		}

		public static int Compare (string strA, string strB)
		{
			int i;

			/* Does this remind anyone of the nautilus string.h wrappers? :-) */
			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;
			} else if (strB == null)
				return 1;

			for (i = 0; strA[i] != strB[i] && strA[i] != '\0'; i++);

			return ((int) (strA[i] - strB[i]));
		}

		public static int Compare (string strA, string strB, bool ignoreCase)
		{
			int i;

			if (!ignoreCase)
				return Compare (strA, strB);

			/*
			 * And here I thought Eazel developers were on crack...
			 * if a string is null it should throw an exception damnit!
			 */
			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;
			} else if (strB == null)
				return 1;

			for (i = 0; strA[i] != '\0' && strB[i] != '\0'; i++) {
				char cA, cB;

				cA = strA[i] >= 'A' && strA[i] <= 'Z' ? strA[i] + 33 : strA[i];
				cB = strB[i] >= 'A' && strB[i] <= 'Z' ? strB[i] + 33 : strB[i];

				if (cA != cB)
					break;
			}

			return ((int) (strA[i] - strB[i]));
		}

		public static int Compare (string strA, string strB, bool ignoreCase, CultureInfo culture)
		{
			// FIXME: implement me
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

			for (i = 0; i < length; i++) {
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

			for (i = 0; i < length; i++) {
				int iA = indexA + i;
				int iB = indexB + i;
				char cA, cB;

				cA = strA[iA] >= 'A' && strA[iA] <= 'Z' ? strA[iA] + 33 : strA[iA];
				cB = strB[iB] >= 'A' && strB[iB] <= 'Z' ? strB[iB] + 33 : strB[iB];

				if (cA != cB)
					break;
			}

			return ((int) (strA[indexA + i] - strB[indexB + i]));
		}

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
		}

		private static char toLowerOrdinal (char value)
		{
			// FIXME: implement me
			return value;
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

			for (i = 0; strA[i] != '\0'; i++) {
				char cA, cB;

				cA = toLowerOrdinal (strA[i]);
				cB = toLowerOrdinal (strB[i]);

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

			/* Nooooooooo!! */
			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;
			} else if (strB == null)
				return 1;

			for (i = 0; i < length; i++) {
				char cA, cB;

				cA = toLowerOrdinal (strA[indexA + i]);
				cB = toLowerOrdinal (strB[indexB + i]);

				if (cA != cB)
					break;
			}

			return ((int) (strA[indexA + i] - strB[indexB + i]));
		}

		public int CompareTo (object obj)
		{
			return Compare (this, obj == null ? null : obj.ToString);
		}

		public int CompareTo (string str)
		{
			return Compare (this, str);
		}

		public static string Concat (object arg)
		{
			return Concat (this, arg ? arg.ToString () : this.Empty);
		}

		public static string Concat (params object[] args)
		{
			// FIXME: I guess I don't have to `delete' strings and it's elements?
			string strings[], str;
			int len, i, j, k;

			if (args == null)
				throw new ArgumentNullException ();

			strings = new string [args.Length];
			len = 0;
			for (i = 0; i < args.Length; i++) {
				/* use Empty for each null argument */
				if (args[i] == null)
					strings[i] = this.Empty;
				else
					strings[i] = args[i].ToString ();
				len += strings[i].Length;
			}

			if (len == 0)
				return this.Empty;

			str = new string [len + 1];
			i = 0;
			for (j = 0; j < args.Length; j++)
				for (k = 0; k < strings[j].Length; k++)
					str[i++] = strings[j][k];
			str[i] = '\0';

			return str;
		}

		public static string Concat (params string[] values)
		{
			int len, i, j, k;
			string str;

			if (values == null)
				throw new ArgumentNullException ();

			len = 0;
			for (i = 0; i < values.Length; i++)
				len += values[i] ? values[i].Length : 0;

			if (len == 0)
				return this.Empty;

			str = new string [len + 1];
			i = 0;
			for (j = 0; j < values.Length; j++) {
				if (values[j] == null)
					continue;

				for (k = 0; k < values[j].Length; k++)
					str[i++] = values[j][k];
			}
			str[i] = '\0';

			return str;
		}

		public static string Concat (object arg0, object arg1)
		{
			string str0 = arg0 ? arg0.ToString () : this.Empty;
			string str1 = arg1 ? arg1.ToString () : this.Empty;

			return Concat (str0, str1);
		}

		public static string Concat (string str0, string str1)
		{
			string concat;
			int i, j, len;

			if (str0 == null)
				str0 = this.Empty;
			if (str1 == null)
				str1 == this.Empty;

			len = str0.Length + str1.Length;
			if (len == 0)
				return this.Empty;

			concat = new string [len + 1];
			for (i = 0; i < str0.Length; i++)
				concat[i] = str0[i];
			for (j = 0 ; j < str1.Length; j++)
				concat[i + j] = str1[j];
			concat[len] = '\0';

			return concat;
		}

		public static string Concat (object arg0, object arg1, object arg2)
		{
			string str0 = arg0 ? arg0.ToString () : this.Empty;
			string str1 = arg1 ? arg1.ToString () : this.Empty;
			string str2 = arg2 ? arg2.ToString () : this.Empty;

			return Concat (str0, str1, str2);
		}

		public static string Concat (string str0, string str1, string str2)
		{
			string concat;
			int i, j, k, len;

			if (str0 == null)
				str0 = this.Empty;
			if (str1 == null)
				str1 = this.Empty;
			if (str2 == null)
				str2 = this.Empty;

			len = str0.Length + str1.Length + str2.Length;
			if (len == 0)
				return this.Empty;
			
			concat = new string [len + 1];
			for (i = 0; i < str0.Length; i++)
				concat[i] = str0[i];
			for (j = 0; j < str1.Length; j++)
				concat[i + j] = str1[j];
			for (k = 0; k < str2.Length; k++)
				concat[i + j + k] = str2[k];
			concat[len] = '\0';

			return concat;
		}

		public static string Concat (string str0, string str1, string str2, string str3)
		{
			string concat;
			int i, j, k, l, len;

			if (str0 == null)
				str0 = this.Empty;
			if (str1 == null)
				str1 = this.Empty;
			if (str2 == null)
				str2 = this.Empty;
			if (str3 == null)
				str3 = this.Empty;

			len = str0.Length + str1.Length + str2.Length + str3.Length;
			if (len == 0)
				return this.Empty;

			concat = new string [len + 1];
			for (i = 0; i < str0.Length; i++)
				concat[i] = str0[i];
			for (j = 0; j < str1.Length; j++)
				concat[i + j] = str1[j];
			for (k = 0; k < str2.Length; k++)
				concat[i + j + k] = str2[k];
			for (l = 0; l < str3.Length; l++)
				concat[i + j + k + l] = str3[l];
			concat[len] = '\0';

			return concat;
		}

		public static string Copy (string str)
		{
			if (str == null)
				throw new ArgumentNullException ();

			return new String (str);
		}

		public void CopyTo (int sourceIndex, char[] destination, int destinationIndex, int count)
		{
			// FIXME: should I null-terminate?
			int i, len;

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

			start = this.length - value.Length;
			if (start < 0)
				return false;

			for (i = start; i < this.length && endswith; i++)
				endswith = this.c_str[i] == value[i - start];

			return endswith;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is String))
				return false;

			return this == (String) obj;
		}

		public new bool Equals (string value)
		{
			return this == value;
		}

		public static new bool Equals (string a, string b)
		{
			return a == b;
		}

		public static string Format (string format, object arg0)
		{
			// FIXME: implement me
		}

		public static string Format (string format, params object[] args)
		{
			// FIXME: implement me
		}

		public static string Format (IFormatProvider provider, string format, params object[] args)
		{
			// FIXME: implement me
		}

		public static string Format (string format, object arg0, object arg1)
		{
			// FIXME: implement me
		}

		public static string Format (string format, object arg0, object arg1, object arg2)
		{
			// FIXME: implement me
		}

		public CharEnumerator GetEnumerator ()
		{
			// FIXME: implement me
		}

		public override int GetHashCode ()
		{
			// FIXME: implement me
		}

		public Type GetType ()
		{
			// FIXME: implement me
		}

		public TypeCode GetTypeCode ()
		{
			// FIXME: implement me
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
			// FIXME: Use a modified Boyer-Moore algorithm to work with unicode?
			int i;

			if (value == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || count < 0 || startIndex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			for (i = startIndex; i - startIndex < count; i++) {
				if (this.c_str[i] == value[0]) {
					bool equal = true;
					int j, offset;

					offset = i - startIndex;
					for (j = 1; equal && value[j] != '\0' && offset + j < count; j++)
						equal = this.c_str[i + j] == value[j];

					if (equal)
						return i;
				}
			}

			return -1;
		}

		public int IndexOfAny (char[] values)
		{
			return IndexOfAny (values, 0, this.length);
		}

		public int IndexOfAny (char[] values, int startIndex)
		{
			return IndexOfAny (values, startIndex, this.length);
		}

		public int IndexOfAny (char[] values, int startIndex, int count)
		{
			int i, valuelen;

			if (values == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || count < 0 || startIndex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			for (valuelen = 0; values[valuelen] != '\0'; valuelen++);

			if (valuelen == 0)
				return -1;

			for (i = startIndex, i < startIndex + count, i++) {
				for (int j = 0; j < vlen; j++) {
					if (this.c_str[i] == values[j])
						return i;
				}
			}

			return -1;
		}

		public string Insert (int startIndex, string value)
		{
			string str;
			int i, j;

			if (value == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || startIndex > this.length)
				throw new ArgumentOutOfRangeException ();

			str = new string [value.Length + this.length + 1];
			for (i = 0; i < startIndex; i++)
				str[i] = this.c_str[i];
			for (j = 0; j < value.Length; j++)
				str[i + j] = value[j];
			for ( ; i < this.length; i++)
				str[i + j] = this.c_str[i];
			str[i + j] = '\0';

			return str;
		}

		public static string Intern (string str)
		{
			if (str == null)
				throw new ArgumentNullException ();
			// FIXME: implement me
		}

		public static string IsInterned (string str)
		{
			if (str == null)
				throw new ArgumentNullException ();
			// FIXME: implement me
		}

		public static string Join (string separator, string[] value)
		{
			// FIXME: msdn doesn't specify what happens when separator is null
			//        or what to do if value[0] is null or if even value is a
			//        NULL terminated array (I'm just assuming it is)
			// NOTE: this does not call Join (string, string[], int, int)
			//       because to do so would mean counting the # of elements twice
			string str;
			int len, i, j;

			if (separator == null || value == null)
				throw new ArgumentNullException ();

			if (value[0] == null)
				return this.Empty;

			len = value[0].Length;
			for (i = 1; value[i] != null; i++)
				len += separator.Length + value[i].Length;

			str = new string [len + 1];
			for (i = 0; i < value[0].Length; i++)
				str[i] = value[0][i];
			for (j = 1; value[j] != null; j++) {
				int k;

				for (k = 0; k < separator.Length; k++)
					str[i++] = separator[k];
				for (k = 0; k < value[j].Length; k++)
					str[i++] = value[j][k];
			}
			str[i] = '\0';

			return str;
		}

		public static string Join (string separator, string[] value, int startIndex, int count)
		{
			// FIXME: msdn doesn't specify what happens when separator is null
			//        or what to do if value[0] is null
			// FIXME: does value.Length give me the length of the array?
			int len, i, j, elements, used;
			string str;

			if (separator == null || value == null)
				throw new ArgumentNullException ();

			len = 0;
			used = 0;
			elements = 0;
			for (i = 0; value[i] != null; i++, elements++) {
				if (i == startIndex) {
					len = value[i].Length;
					used = 1;
				} else if (i > startIndex && used < count) {
					len += separator.Length + value[i].Length;
					used++;
				}
			}

			if (startIndex + count > elements)
				throw new ArgumentOutOfRangeException ();

			// We have no elements to join?
			if (i == 0)
				return this.Empty;

			str = new string [len + 1];
			for (i = 0; i < value[startIndex].Length; i++)
				str[i] = value[startIndex][i];

			used = 0;
			for (j = startIndex + 1; used < count; j++, used++) {
				int k;

				for (k = 0; k < separator.Length; k++)
					str[i++] = separator[k];
				for (k = 0; k < value[j].Length; k++)
					str[i++] = value[j][k];
			}
			str[i] = '\0';

			return str;
		}

		public int LastIndexOf (char value)
		{
			int i;

			for (i = this.length; i >= 0; i--) {
				if (this.c_str[i] == value)
					return i;
			}

			return -1;
		}

		public int LastIndexOf (string value)
		{
			return LastIndexOf (value, this.length, this.length);
		}

		public int LastIndexOf (char value, int startIndex)
		{
			int i;

			if (startIndex < 0 || startIndex > this.length)
				throw new ArgumentOutOfRangeException ();

			for (i = startIndex; i >= 0; i--) {
				if (this.c_str[i] == value)
					return i;
			}

			return -1;
		}

		public int LastIndexOf (string value, int startIndex)
		{
			return LastIndexOf (value, startIndex, this.length);
		}

		public int LastIndexOf (char value, int startIndex, int count)
		{
			int i;

			if (startIndex < 0 || count < 0)
				throw new ArgumentOutOfRangeException ();

			if (startIndex > this.length || startIndex - count < 0)
				throw new ArgumentOutOfRangeException ();

			for (i = startIndex; i >= startIndex - count; i--) {
				if (this.c_str[i] == value)
					return i;
			}

			return -1;
		}

		public int LastIndexOf (string value, int startIndex, int count)
		{
			// FIXME: currently I'm using startIndex as the 0-position in the comparison,
			//        but maybe it's the end-position in MS's implementation?
			//        msdn is unclear on this point. I think this is correct though.
			int i, len;

			if (value == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || startIndex > this.length)
				throw new ArgumentOutOfRangeException ();

			if (count < 0 || startIndex - count < 0)
				throw new ArgumentOutOfRangeException ();

			if (value == this.Empty)
				return startIndex;

			if (startIndex + value.Length > this.length) {
				/* just a little optimization */
				int start;

				start = this.length - value.Length;
				count -= startIndex - start;
				startIndex = start;
			}

			// FIXME: use a reversed-unicode-safe-Boyer-Moore?
			len = value.Length;
			for (i = startIndex; i >= startIndex - count; i--) {
				if (this.c_str[i + len] == value[len]) {
					bool equal = true;
					int j;

					for (j = len - 1; equal && j >= 0; j--)
						equal = this.c_str[i + j] == value[j];

					if (equal)
						return i;
				}
			}

			return -1;
		}

		public int LastIndexOfAny (char[] values)
		{
			// FIXME: implement me
		}

		public int LastIndexOfAny (char[] values, int startIndex)
		{
			// FIXME: implement me
		}

		public int LastIndexOfAny (char[] values, int startIndex, int count)
		{
			// FIXME: implement me
		}

		public string PadLeft (int totalWidth)
		{
			return PadLeft (totalWidth, ' ');
		}

		public string PadLeft (int totalWidth, char padChar)
		{
			string str;
			int i, j;

			if (totalWidth < 0)
				throw new ArgumentException ();

			str = new string [totalWidth > this.length ? totalWidth : this.length + 1];
			for (i = 0; i < totalWidth - this.length; i++)
				str[i] = padChar;

			for (j = 0; j < this.length; i++, j++)
				str[i] = this.c_str[j];

			str[i] = '\0';

			return str;
		}

		public string PadRight (int totalWidth)
		{
			return PadRight (totalWidth, ' ');
		}

		public string PadRight (int totalWidth, char padChar)
		{
			string str;
			int i;

			if (totalWidth < 0)
				throw new ArgumentException ();

			str = new string [totalWidth > this.length ? totalWidth : this.length + 1];
			for (i = 0; i < this.length; i++)
				str[i] = this.c_str[i];

			for ( ; j < str.Length; i++)
				str[i] = padChar;

			str[i] = '\0';

			return str;
		}

		public string Remove (int startIndex, int count)
		{
			string str;
			int i, j, len;

			if (startIndex < 0 || count < 0 || startIndex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			len = this.length - count;
			if (len == 0)
				return this.Empty;

			str = new string [len + 1];
			for (i = 0; i < startIndex; i++)
				str[i] = this.c_str[i];
			for (j = i + count; j < this.length; j++)
				str[i++] = this.c_str[j];
			str[i] = '\0';

			return str;
		}

		public string Replace (char oldChar, char newChar)
		{
			string str;
			int i;

			str = new string [this.length + 1];
			for (i = 0; i < this.length; i++) {
				if (this.c_str[i] == oldChar)
					str[i] = newChar;
				else
					str[i] = this.c_str[i];
			}
			str[i] = '\0';

			return str;
		}

		public string Replace (string oldValue, string newValue)
		{
			// FIXME: msdn doesn't specify what to do if either args is null
			int index, len, i, j;
			string str;

			if (oldValue == null || newValue == null)
				throw new ArgumentNullException ();

			// Use IndexOf in case I later rewrite it to use Boyer-Moore
			index = IndexOf (oldValue, 0);
			if (index == -1) {
				// This is the easy one ;-)
				return Substring (0, this.length);
			}

			len = this.length - oldValue.Length + newValue.Length;
			if (len == 0)
				return this.Empty;

			str = new string [len + 1];
			for (i = 0; i < index; i++)
				str[i] = this.c_str[i];
			for (j = 0; j < newValue.Length; j++)
				str[i++] = newValue[j];
			for (j = index + oldValue.Length; j < this.length; j++)
				str[i++] = this.c_str[j];
			str[i] = '\0';

			return str;
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

			// FIXME: implement me
		}

		public string[] Split (params char[] separator, int maxCount)
		{
			// FIXME: implement me
		}

		public bool StartsWith (string value)
		{
			bool startswith = true;
			int i;

			if (value == null)
				throw new ArgumentNullException ();

			if (value.Length > this.length)
				return false;

			for (i = 0; i < value.Length && startswith; i++)
				startswith = startswith && value[i] == this.c_str[i];

			return startswith;
		}

		public string Substring (int startIndex)
		{
			string str;
			int i, len;

			if (startIndex < 0 || startIndex > this.length)
				throw new ArgumentOutOfRangeException ();

			len = this.length - startIndex;
			if (len == 0)
				return this.Empty;

			str = new string [len + 1];
			for (i = startIndex; i < this.length; i++)
				str[i - startIndex] = this.c_str[i];
			str[i] = '\0';

			return str;
		}

		public string Substring (int startIndex, int length)
		{
			string str;
			int i;

			if (startIndex < 0 || length < 0 || startIndex + length > this.length)
				throw new ArgumentOutOfRangeException ();

			if (length == 0)
				return this.Empty;

			str = new string [length + 1];
			for (i = startIndex; i < startIndex + length; i++)
				str[i - startIndex] = this.c_str[i];
			str[i] = '\0';

			return str;
		}

		public char[] ToCharArray ()
		{
			return ToCharArray (0, this.length);
		}

		public char[] ToCharArray (int startIndex, int length)
		{
			char [] chars;
			int i, j;

			if (startIndex < 0 || length < 0 || startIndex + length > this.length)
				throw new ArgumentOutOfRangeException ();

			chars = new char [length + 1];
			for (i = startIndex, i < length; i++)
				chars[i - startIndex] = this.c_str[i];

			chars[length] = '\0';

			return chars;
		}

		public string ToLower ()
		{
			string str;
			int i;

			str = new string [this.length + 1];
			for (i = 0; i < this.length; i++) {
				char c = this.c_str[i];

				str[i] = c >= 'A' && c <= 'Z' ? c + 33 : c;
			}

			str[i] = '\0';

			return str;
		}

		public string ToLower (CultureInfo culture)
		{
			// FIXME: implement me
		}

		public override string ToString ()
		{
			return Substring (0, this.length);
		}

		public string ToString (IFormatProvider format)
		{
			// FIXME: implement me
		}

		public string ToUpper ()
		{
			string str;
			int i;

			str = new string [this.length + 1];
			for (i = 0; i < this.length; i++) {
				char c = this.c_str[i];

				str[i] = c >= 'a' && c <= 'z' ? c - 33 : c;
			}
			str[i] = '\0';

			return str;
		}

		public string ToUpper (CultureInfo culture)
		{
			// FIXME: implement me
		}

		private bool is_lwsp (char c)
		{
			/* this comes from the msdn docs for String.Trim() */
			if ((c >= '0x9' && c <= '0xD') || c == '0x20' || c == '0xA0' ||
			    (c >= '0x2000' && c <= '0x200B') || c == '0x3000' || c == '0xFEFF')
				return true;
			else
				return false;
		}

		public string Trim ()
		{
			return Trim (null);
		}

		public string Trim (params char[] trimChars)
		{
			// FIXME: this implementation seems lame to me...
			int begin, end, i;
			bool matches;

			matches = true;
			for (begin = 0; matches && begin < this.length; begin++) {
				if (trimChars != null) {
					matches = false;
					for (i = 0; !matches && i < trimChars.Length; i++)
						matches = this.c_str[begin] == trimChars[i];
				} else {
					matches = is_lwsp (this.c_str[begin]);
				}
			}

			matches = true;
			for (end = this.length; end > begin; end--) {
				if (trimChars != null) {
					matches = false;
					for (i = 0; !matches && i < trimChars.Length; i++)
						matches = this.c_str[end] == trimChars[i];
				} else {
					matches = is_lwsp (this.c_str[end]);
				}
			}

			if (begin == end)
				return this.Empty;

			return Substring (begin, end - begin);
		}

		public string TrimEnd (params char[] trimChars)
		{
			bool matches = true;
			int end, i;

			for (end = this.length; end > 0; end--) {
				if (trimChars != null) {
					matches = false;
					for (i = 0; !matches && i < trimChars.Length; i++)
						matches = this.c_str[end] == trimChars[i];
				} else {
					matches = is_lwsp (this.c_str[end]);
				}
			}

			if (end == 0)
				return this.Empty;

			return Substring (0, end);
		}

		public string TrimStart (params char[] trimChars)
		{
			bool matches = true;
			int begin, i;

			for (begin = 0; matches && begin < this.length; begin++) {
				if (trimChars != null) {
					matches = false;
					for (i = 0; !matches && i < trimChars.Length; i++)
						matches = this.c_str[begin] == trimChars[i];
				} else {
					matches = is_lwsp (this.c_str[begin]);
				}
			}

			if (begin == this.length)
				return this.Empty;

			return Substring (begin, this.length - begin);
		}
	}
}
