//
// CompareInfo.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2015 Xamarin Inc (http://www.xamarin.com)
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


using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mono.Globalization.Unicode;
using System.Threading;

namespace System.Globalization
{
	interface ISimpleCollator
	{
		SortKey GetSortKey (string source, CompareOptions options);

		int Compare (string s1, string s2);

		int Compare (string s1, int idx1, int len1, string s2, int idx2, int len2, CompareOptions options);

		bool IsPrefix (string src, string target, CompareOptions opt);

		bool IsSuffix (string src, string target, CompareOptions opt);

		int IndexOf (string s, string target, int start, int length, CompareOptions opt);

		int IndexOf (string s, char target, int start, int length, CompareOptions opt);

		int LastIndexOf (string s, string target, CompareOptions opt);

		int LastIndexOf (string s, string target, int start, int length, CompareOptions opt);

		int LastIndexOf (string s, char target, CompareOptions opt);

		int LastIndexOf (string s, char target, int start, int length, CompareOptions opt);
	}

	partial class CompareInfo
	{
		[NonSerialized]
		ISimpleCollator collator;

		// Maps culture IDs to SimpleCollator objects
		static Dictionary<string, ISimpleCollator> collators;
		static bool managedCollation;
		static bool managedCollationChecked;

#if WASM
		const bool UseManagedCollation = false;
#else
		static bool UseManagedCollation {
			get {
				if (!managedCollationChecked) {
					managedCollation = Environment.internalGetEnvironmentVariable ("MONO_DISABLE_MANAGED_COLLATION") != "yes" && MSCompatUnicodeTable.IsReady;
					managedCollationChecked = true;
				}

				return managedCollation;
			}
		}
#endif

		ISimpleCollator GetCollator ()
		{
#if WASM
			return null;
#else
			if (collator != null)
				return collator;

			if (collators == null) {
				Interlocked.CompareExchange (ref collators, new Dictionary<string, ISimpleCollator> (StringComparer.Ordinal), null);
			}

			lock (collators) {
				if (!collators.TryGetValue (_sortName, out collator)) {
					collator = new SimpleCollator (CultureInfo.GetCultureInfo (m_name));
					collators [_sortName] = collator;
				}
			}

			return collator;
#endif
		}

		SortKey CreateSortKeyCore (string source, CompareOptions options)
		{
			if (UseManagedCollation)
				return GetCollator ().GetSortKey (source, options);
			return new SortKey (culture, source, options);
		}

		int internal_index_switch (string s1, int sindex, int count, string s2, CompareOptions opt, bool first)
		{
			// TODO: should not be needed,  why is there specialization for OrdinalIgnore and not for Ordinal
			if (opt == CompareOptions.Ordinal)
				return first ? s1.IndexOfUnchecked (s2, sindex, count) : s1.LastIndexOfUnchecked (s2, sindex, count);
			
			return UseManagedCollation ?
				internal_index_managed (s1, sindex, count, s2, opt, first) :
				internal_index (s1, sindex, count, s2, first);
		}

		int internal_compare_switch (string str1, int offset1, int length1, string str2, int offset2, int length2, CompareOptions options)
		{
			return UseManagedCollation ?
				internal_compare_managed (str1, offset1, length1,
				str2, offset2, length2, options) :
				internal_compare (str1, offset1, length1,
				str2, offset2, length2, options);
		}

		int internal_compare_managed (string str1, int offset1, int length1, string str2, int offset2, int length2, CompareOptions options)
		{
			return GetCollator ().Compare (str1, offset1, length1,
				str2, offset2, length2, options);
		}

		int internal_index_managed (string s, int sindex, int count, char c, CompareOptions opt, bool first)
		{
			return first ?
				GetCollator ().IndexOf (s, c, sindex, count, opt) :
				GetCollator ().LastIndexOf (s, c, sindex, count, opt);
		}

		int internal_index_managed (string s1, int sindex, int count, string s2, CompareOptions opt, bool first)
		{
			return first ?
				GetCollator ().IndexOf (s1, s2, sindex, count, opt) :
				GetCollator ().LastIndexOf (s1, s2, sindex, count, opt);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static unsafe extern int internal_compare_icall (char* str1, int length1,
			char* str2, int length2, CompareOptions options);

		private static unsafe int internal_compare (string str1, int offset1,
			int length1, string str2, int offset2, int length2, CompareOptions options)
		{
			fixed (char* fixed_str1 = str1,
				     fixed_str2 = str2)
				return internal_compare_icall (fixed_str1 + offset1, length1,
					fixed_str2 + offset2, length2, options);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static unsafe extern int internal_index_icall (char *source, int sindex,
			int count, char *value, int value_length, bool first);

		private static unsafe int internal_index (string source, int sindex,
			int count, string value, bool first)
		{
			fixed (char* fixed_source = source,
				     fixed_value = value)
				return internal_index_icall (fixed_source, sindex, count,
					fixed_value, value?.Length ?? 0, first);
		}
	}
}
