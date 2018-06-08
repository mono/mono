//
// System.String.cs
//
// Authors:
//   Patrik Torstensson
//   Jeffrey Stedfast (fejj@ximian.com)
//   Dan Lewis (dihlewis@yahoo.co.uk)
//   Sebastien Pouliot  <sebastien@ximian.com>
//   Marek Safar (marek.safar@seznam.cz)
//   Andreas Nahr (Classdevelopment@A-SoftTech.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell (http://www.novell.com)
// Copyright (c) 2012 Xamarin, Inc (http://www.xamarin.com)
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
//

using System.Runtime.CompilerServices;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics.Private;

namespace System
{
	partial class String
	{
		[NonSerialized]
		int _stringLength;
		[NonSerialized]
		char _firstChar;

		public static readonly String Empty;

		internal unsafe int IndexOfUnchecked (string value, int startIndex, int count)
		{
			int valueLen = value.Length;
			if (count < valueLen)
				return -1;

			if (valueLen == 0)
				return startIndex;

			fixed (char* thisptr = &_firstChar, valueptr = value) {
				char* ap = thisptr + startIndex;
				char* thisEnd = ap + count - valueLen + 1;
				while (ap != thisEnd) {
					if (*ap == *valueptr) {
						for (int i = 1; i < valueLen; i++) {
							if (ap[i] != valueptr[i])
								goto NextVal;
						}
						return (int)(ap - thisptr);
					}
					NextVal:
					ap++;
				}
			}
			return -1;
		}

		internal unsafe int IndexOfUncheckedIgnoreCase (string value, int startIndex, int count)
		{
			int valueLen = value.Length;
			if (count < valueLen)
				return -1;

			if (valueLen == 0)
				return startIndex;

			var ti = CultureInfo.InvariantCulture.TextInfo;

			fixed (char* thisptr = &_firstChar, valueptr = value) {
				char* ap = thisptr + startIndex;
				char* thisEnd = ap + count - valueLen + 1;
				char valueUpper = ti.ToUpper (*valueptr);
				while (ap != thisEnd) {
					if (ti.ToUpper (*ap) == valueUpper) {
						for (int i = 1; i < valueLen; i++) {
							if (ti.ToUpper (ap[i]) != ti.ToUpper (valueptr [i]))
								goto NextVal;
						}
						return (int)(ap - thisptr);
					}
					NextVal:
					ap++;
				}
			}
			return -1;
		}

		internal unsafe int LastIndexOfUnchecked (string value, int startIndex, int count)
		{
			int valueLen = value.Length;
			if (count < valueLen)
				return -1;

			if (valueLen == 0)
				return startIndex;

			fixed (char* thisptr = &_firstChar, valueptr = value) {
				char* ap = thisptr + startIndex;

				char* thisEnd = ap - count + valueLen - 1;
				char* valueEnd = valueptr + valueLen - 1;

				while (ap != thisEnd) {
					if (*ap == *valueEnd) {
						char* apEnd = ap;
						while (valueptr != valueEnd) {
							valueEnd--;
							ap--;
							if (*ap != *valueEnd) {
								valueEnd = valueptr + valueLen - 1;
								ap = apEnd;
								goto NextVal;
							}
						}

						return (int)(ap - thisptr);
					}
				NextVal:
					ap--;
				}
			}

			return -1;
		}

		internal unsafe int LastIndexOfUncheckedIgnoreCase (string value, int startIndex, int count)
		{
			int valueLen = value.Length;
			if (count < valueLen)
				return -1;

			if (valueLen == 0)
				return startIndex;

			var ti = CultureInfo.InvariantCulture.TextInfo;

			fixed (char* thisptr = &_firstChar, valueptr = value) {
				char* ap = thisptr + startIndex;

				char* thisEnd = ap - count + valueLen - 1;
				char* valueEnd = valueptr + valueLen - 1;

				var valueEndUpper = ti.ToUpper (*valueEnd);

				while (ap != thisEnd) {
					if (ti.ToUpper (*ap) == valueEndUpper) {
						char* apEnd = ap;
						while (valueptr != valueEnd) {
							valueEnd--;
							ap--;
							if (ti.ToUpper (*ap) != ti.ToUpper (*valueEnd)) {
								valueEnd = valueptr + valueLen - 1;
								ap = apEnd;
								goto NextVal;
							}
						}

						return (int)(ap - thisptr);
					}
				NextVal:
					ap--;
				}
			}

			return -1;
		}

		internal bool StartsWithOrdinalUnchecked (String value)
		{
			if (this.Length < value.Length || _firstChar != value._firstChar)
				return false;

			return value.Length == 1 ? true : StartsWithOrdinalHelper (this, value);
		}

		// copied from external/corert/src/System.Private.CoreLib/src/System/String.Comparison.cs
		private static unsafe bool StartsWithOrdinalHelper(String str, String startsWith)
		{
			Debug.Assert(str != null);
			Debug.Assert(startsWith != null);
			Debug.Assert(str.Length >= startsWith.Length);

			int length = startsWith.Length;

			fixed (char* ap = &str._firstChar) fixed (char* bp = &startsWith._firstChar)
			{
				char* a = ap;
				char* b = bp;

#if BIT64
				// Single int read aligns pointers for the following long reads
				// No length check needed as this method is called when length >= 2
				Debug.Assert(length >= 2);
				if (*(int*)a != *(int*)b) goto ReturnFalse;
				length -= 2; a += 2; b += 2;

				while (length >= 12)
				{
					if (*(long*)a != *(long*)b) goto ReturnFalse;
					if (*(long*)(a + 4) != *(long*)(b + 4)) goto ReturnFalse;
					if (*(long*)(a + 8) != *(long*)(b + 8)) goto ReturnFalse;
					length -= 12; a += 12; b += 12;
				}
#else
				while (length >= 10)
				{
					if (*(int*)a != *(int*)b) goto ReturnFalse;
					if (*(int*)(a + 2) != *(int*)(b + 2)) goto ReturnFalse;
					if (*(int*)(a + 4) != *(int*)(b + 4)) goto ReturnFalse;
					if (*(int*)(a + 6) != *(int*)(b + 6)) goto ReturnFalse;
					if (*(int*)(a + 8) != *(int*)(b + 8)) goto ReturnFalse;
					length -= 10; a += 10; b += 10;
				}
#endif

				while (length >= 2)
				{
					if (*(int*)a != *(int*)b) goto ReturnFalse;
					length -= 2; a += 2; b += 2;
				}

				// PERF: This depends on the fact that the String objects are always zero terminated 
				// and that the terminating zero is not included in the length. For even string sizes
				// this compare can include the zero terminator. Bitwise OR avoids a branch.
				return length == 0 | *a == *b;

			ReturnFalse:
				return false;
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static String FastAllocateString (int length);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string InternalIsInterned (string str);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string InternalIntern (string str);

		static unsafe int FastCompareStringHelper (uint* strAChars, int countA, uint* strBChars, int countB)
		{
			// CoreRT implementation has alignment issues
			char* ap = (char*) strAChars;
			char* bp = (char*) strBChars;
			char* end = ap + Math.Min (countA, countB);
			while (ap < end) {
				if (*ap != *bp)
					return (int)*ap - (int)*bp;
				ap++;
				bp++;
			}
			return countA - countB;
		}

		internal static unsafe void CharCopy (char *dest, char *src, int count) {
			// Same rules as for memcpy, but with the premise that 
			// chars can only be aligned to even addresses if their
			// enclosing types are correctly aligned
			if ((((int)(byte*)dest | (int)(byte*)src) & 3) != 0) {
				if (((int)(byte*)dest & 2) != 0 && ((int)(byte*)src & 2) != 0 && count > 0) {
					((short*)dest) [0] = ((short*)src) [0];
					dest++;
					src++;
					count--;
				}
				if ((((int)(byte*)dest | (int)(byte*)src) & 2) != 0) {
					Buffer.memcpy2 ((byte*)dest, (byte*)src, count * 2);
					return;
				}
			}
			Buffer.memcpy4 ((byte*)dest, (byte*)src, count * 2);
		}

		#region Runtime method-to-ir dependencies

		/* helpers used by the runtime as well as above or eslewhere in corlib */
		static unsafe void memset (byte *dest, int val, int len)
		{
			if (len < 8) {
				while (len != 0) {
					*dest = (byte)val;
					++dest;
					--len;
				}
				return;
			}
			if (val != 0) {
				val = val | (val << 8);
				val = val | (val << 16);
			}
			// align to 4
			int rest = (int)dest & 3;
			if (rest != 0) {
				rest = 4 - rest;
				len -= rest;
				do {
					*dest = (byte)val;
					++dest;
					--rest;
				} while (rest != 0);
			}
			while (len >= 16) {
				((int*)dest) [0] = val;
				((int*)dest) [1] = val;
				((int*)dest) [2] = val;
				((int*)dest) [3] = val;
				dest += 16;
				len -= 16;
			}
			while (len >= 4) {
				((int*)dest) [0] = val;
				dest += 4;
				len -= 4;
			}
			// tail bytes
			while (len > 0) {
				*dest = (byte)val;
				dest++;
				len--;
			}
		}

		static unsafe void memcpy (byte *dest, byte *src, int size)
		{
			Buffer.Memcpy (dest, src, size);
		}

		/* Used by the runtime */
		internal static unsafe void bzero (byte *dest, int len) {
			memset (dest, 0, len);
		}

		internal static unsafe void bzero_aligned_1 (byte *dest, int len) {
			((byte*)dest) [0] = 0;
		}

		internal static unsafe void bzero_aligned_2 (byte *dest, int len) {
			((short*)dest) [0] = 0;
		}

		internal static unsafe void bzero_aligned_4 (byte *dest, int len) {
			((int*)dest) [0] = 0;
		}

		internal static unsafe void bzero_aligned_8 (byte *dest, int len) {
			((long*)dest) [0] = 0;
		}

		internal static unsafe void memcpy_aligned_1 (byte *dest, byte *src, int size) {
			((byte*)dest) [0] = ((byte*)src) [0];
		}

		internal static unsafe void memcpy_aligned_2 (byte *dest, byte *src, int size) {
			((short*)dest) [0] = ((short*)src) [0];
		}

		internal static unsafe void memcpy_aligned_4 (byte *dest, byte *src, int size) {
			((int*)dest) [0] = ((int*)src) [0];
		}

		internal static unsafe void memcpy_aligned_8 (byte *dest, byte *src, int size) {
			((long*)dest) [0] = ((long*)src) [0];
		}

		#endregion

		// Certain constructors are redirected to CreateString methods with
		// matching argument list. The this pointer should not be used.

		unsafe String CreateString (sbyte* value)
		{
			return Ctor (value);
		}

		unsafe String CreateString (sbyte* value, int startIndex, int length)
		{
			return Ctor (value, startIndex, length);
		}

		unsafe string CreateString (char* value)
		{
			return Ctor (value);
		}

		unsafe string CreateString (char* value, int startIndex, int length)
		{
			return Ctor (value, startIndex, length);
		}

		string CreateString (char [] val, int startIndex, int length)
		{
			return Ctor (val, startIndex, length);
		}

		string CreateString (char [] val)
		{
			return Ctor (val);
		}

		string CreateString (char c, int count)
		{
			return Ctor (c, count);
		}

		unsafe String CreateString (sbyte* value, int startIndex, int length, Encoding enc)
		{
			return Ctor (value, startIndex, length, enc);
		}

		String CreateString (ReadOnlySpan<char> value)
		{
			return Ctor (value);
		}

		[IndexerName ("Chars")]
		public char this [int index] {
			[IntrinsicAttribute]
			get {
				if ((uint)index >= _stringLength)
					ThrowHelper.ThrowIndexOutOfRangeException ();

				return Unsafe.Add (ref _firstChar, index);
			}
		}

		public static String Intern (String str)
		{
			if (str == null) {
				throw new ArgumentNullException ("str");
			}

			return InternalIntern (str);
		}

		public static String IsInterned (String str)
		{
			if (str == null)
				throw new ArgumentNullException ("str");

			return InternalIsInterned (str);
		}

		int LegacyStringGetHashCode ()
		{
			int hash1 = 5381;
			int hash2 = hash1;

			unsafe {
				fixed (char *src = this) {
					int c;
					char *s = src;
					while ((c = s[0]) != 0) {
						hash1 = ((hash1 << 5) + hash1) ^ c;
						c = s [1];
						if (c == 0)
							break;
						hash2 = ((hash2 << 5) + hash2) ^ c;
						s += 2;
					}
				}
			}

			return hash1 + (hash2 * 1566083941);
		}
	}
}