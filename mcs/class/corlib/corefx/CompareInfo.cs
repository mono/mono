//
// CompareInfo.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2018  Microsoft Corporation
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

// Disable unreachable code warnings in this entire file.
#pragma warning disable 162

namespace System.Globalization
{
	partial class CompareInfo
	{
		void InitSort (CultureInfo culture)
		{
			_sortName = culture.SortName;
		}

		unsafe static int CompareStringOrdinalIgnoreCase (char* pString1, int length1, char* pString2, int length2)
		{
			var ti = CultureInfo.InvariantCulture.TextInfo;

			int index = 0;
			while (index < length1 && index < length2 && ti.ToUpper (*pString1) == ti.ToUpper (*pString2)) {
				++index;
				++pString1;
				++pString2;
			}

			if (index >= length1) {
				if (index >= length2)
					return 0;

				return -1;
			}

			if (index >= length2)
				return 1;

			return ti.ToUpper (*pString1) - ti.ToUpper (*pString2);
		}

		internal static int IndexOfOrdinalCore (string source, string value, int startIndex, int count, bool ignoreCase)
		{
			if (!ignoreCase)
				return source.IndexOfUnchecked (value, startIndex, count);

			return source.IndexOfUncheckedIgnoreCase (value, startIndex, count);
		}

		internal static unsafe int LastIndexOfOrdinalCore (string source, string value, int startIndex, int count, bool ignoreCase)
		{
			if (!ignoreCase)
				return source.LastIndexOfUnchecked (value, startIndex, count);

			return source.LastIndexOfUncheckedIgnoreCase (value, startIndex, count);
		}

		int LastIndexOfCore (string source, string target, int startIndex, int count, CompareOptions options)
		{
			return internal_index_switch (source, startIndex, count, target, options, false);
		}

		unsafe int IndexOfCore (string source, string target, int startIndex, int count, CompareOptions options, int* matchLengthPtr)
		{
			if (matchLengthPtr != null)
				throw new NotImplementedException ();

			return internal_index_switch (source, startIndex, count, target, options, true);
		}

		unsafe int IndexOfCore (ReadOnlySpan<char> source, ReadOnlySpan<char> target, CompareOptions options, int* matchLengthPtr)
		{
			// TODO: optimize

			var s = new string (source);
			var t = new string (target);

			return IndexOfCore (s, t, 0, s.Length, options, matchLengthPtr);
		}

		int IndexOfOrdinalCore (ReadOnlySpan<char> source, ReadOnlySpan<char> value, bool ignoreCase)
		{
			// TODO: optimize

			var s = new string (source);
			var v = new string (value);

			if (!ignoreCase)
				return s.IndexOfUnchecked (v, 0, s.Length);

			return s.IndexOfUncheckedIgnoreCase (v, 0, s.Length);
		}

		int CompareString (ReadOnlySpan<char> string1, string string2, CompareOptions options)
		{
			// TODO: optimize
			
			var s1 = new string (string1);

			return internal_compare_switch (s1, 0, s1.Length, string2, 0, string2.Length, options);
		}

		int CompareString (ReadOnlySpan<char> string1, ReadOnlySpan<char> string2, CompareOptions options)
		{
			// TODO: optimize

			var s1 = new string (string1);
			var s2 = new string (string2);

			return internal_compare_switch (s1, 0, s1.Length, new string (s2), 0, s2.Length, options);
		}

		unsafe static bool IsSortable (char *text, int length)
		{
			return Mono.Globalization.Unicode.MSCompatUnicodeTable.IsSortable (new string (text, 0, length));
		}

		SortKey CreateSortKey (String source, CompareOptions options)
		{
			if (source == null)
				throw new ArgumentNullException (nameof (source));

			if ((options & ValidSortkeyCtorMaskOffFlags) != 0)
				throw new ArgumentException (SR.Argument_InvalidFlag, nameof (options));

			return CreateSortKeyCore (source, options);
		}

		bool StartsWith (string source, string prefix, CompareOptions options)
		{
			if (UseManagedCollation)
				return GetCollator ().IsPrefix (source, prefix, options);

			if (source.Length < prefix.Length)
				return false;

			return Compare (source, 0, prefix.Length, prefix, 0, prefix.Length, options) == 0;
		}

		bool StartsWith (ReadOnlySpan<char> source, ReadOnlySpan<char> prefix, CompareOptions options)
		{
			// TODO: optimize
			return StartsWith (new string (source), new string (prefix), options);
		}

		bool EndsWith (string source, string suffix, CompareOptions options)
		{
			if (UseManagedCollation)
				return GetCollator ().IsSuffix (source, suffix, options);

			if (source.Length < suffix.Length)
				return false;

			return Compare (source, source.Length - suffix.Length, suffix.Length, suffix, 0, suffix.Length, options) == 0;
		}

		bool EndsWith (ReadOnlySpan<char> source, ReadOnlySpan<char> suffix, CompareOptions options)
		{
			// TODO: optimize
			return EndsWith (new string (source), new string (suffix), options);
		}

		internal int GetHashCodeOfStringCore (string source, CompareOptions options)
		{
			return GetSortKey (source, options).GetHashCode ();
		}

		SortVersion GetSortVersion ()
		{
			throw new NotImplementedException ();
		}
	}
}