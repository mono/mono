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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mono.Globalization.Unicode;
using System.Threading;

namespace System.Globalization
{
	partial class CompareInfo
	{
		[NonSerialized]
		SimpleCollator collator;

		// Maps culture IDs to SimpleCollator objects
		static Dictionary<string, SimpleCollator> collators;
		static bool managedCollation;
		static bool managedCollationChecked;

		static bool UseManagedCollation {
			get {
				if (!managedCollationChecked) {
					managedCollation = Environment.internalGetEnvironmentVariable ("MONO_DISABLE_MANAGED_COLLATION") != "yes" && MSCompatUnicodeTable.IsReady;
					managedCollationChecked = true;
				}

				return managedCollation;
			}
		}

		SimpleCollator GetCollator ()
		{
			if (collator != null)
				return collator;

			if (collators == null) {
				Interlocked.CompareExchange (ref collators, new Dictionary<string, SimpleCollator> (StringComparer.Ordinal), null);
			}

			lock (collators) {
				if (!collators.TryGetValue (m_sortName, out collator)) {
					collator = new SimpleCollator (CultureInfo.GetCultureInfo (m_name));
					collators [m_sortName] = collator;
				}
			}

			return collator;
		}

		SortKey CreateSortKeyCore (string source, CompareOptions options)
		{
			if (UseManagedCollation)
				return GetCollator ().GetSortKey (source, options);
			SortKey key=new SortKey (culture, source, options);

			/* Need to do the icall here instead of in the
			 * SortKey constructor, as we need access to
			 * this instance's collator.
			 */
			assign_sortkey (key, source, options);
			
			return(key);        	
		}

		int internal_index_switch (string s, int sindex, int count, char c, CompareOptions opt, bool first)
		{
			if (opt == CompareOptions.Ordinal && first)
				return s.IndexOfUnchecked (c, sindex, count);

			return UseManagedCollation ?
				internal_index_managed (s, sindex, count, c, opt, first) :
				internal_index (s, sindex, count, c, opt, first);
		}	

		int internal_index_switch (string s1, int sindex, int count, string s2, CompareOptions opt, bool first)
		{
			if (opt == CompareOptions.Ordinal && first)
				return s1.IndexOfUnchecked (s2, sindex, count);
			
			return UseManagedCollation ?
				internal_index_managed (s1, sindex, count, s2, opt, first) :
				internal_index (s1, sindex, count, s2, opt, first);
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
		private extern void assign_sortkey (object key, string source,
							CompareOptions options);		

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern int internal_compare (string str1, int offset1,
							 int length1, string str2,
							 int offset2, int length2,
							 CompareOptions options);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern int internal_index (string source, int sindex,
						   int count, char value,
						   CompareOptions options,
						   bool first);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern int internal_index (string source, int sindex,
						   int count, string value,
						   CompareOptions options,
						   bool first);
	}
}