//
// Note that this is just a conceptual source and subject to changes.
//

using System;
using System.Text;

using Mono.Globalization.Collation;

namespace System.Globalization
{
	internal class CompareInfoImpl : CompareInfo
	{
		CultureInfo culture;
		Collator collator;

		internal CompareInfoImpl (CultureInfo ci)
		{
			culture = ci;
			collator = new RuleBasedCollator (ci);
		}

		int CompareOrdinal (string string1, int offset1, int length1,
			string string2, int offset2, int length2)
		{
			int min = length1 < length2 ? length1 : length2;
			for (int i = 0; i < min; i++)
				if (string1 [offset1 + i] != string2 [offset2 + i])
					return ((int) string1 [offset1 + i]) 
						- ((int) string2 [offset2 + i]);
			return (length1 > min) ?
				1 :
				(length2 > min) ? -1 : 0;
		}

		public virtual int Compare (
			string string1, int offset1, int length1,
			string string2, int offset2, int length2,
			CompareOptions options)
		{
			// FIXME: check allowed flags here.
			// FIXME: check array range

			// quick ordinal comparison
			if (options == CompareOptions.Ordinal)
				return CompareOrdinal (string1, offset1,
				length1, string2, offset2, length2);

			return collator.Compare (string1, offset1, length1,
				string2, offset2, length2, options);
		}

		public virtual int Compare (
			string string1, int offset1, int length1,
			string string2, int offset2, int length2,
			CompareOptions options)
		{
			// FIXME: check allowed flags here.
			// FIXME: check array range

			// quick ordinal comparison
			if (options == CompareOptions.Ordinal)
				return CompareOrdinal (string1, offset1, length1,
				string2, offset2, length2, options);

			return collator.Compare (string1, offset1, length1,
				string2, offset2, length2, options);
		}

		public virtual SortKey GetSortKey (
			string source, CompareOptions options)
		{
			// FIXME: check allowed flags here.

			return collator.GetSortKey (source, options);
		}

		public virtual int IndexOf (string source, char value,
			int startIndex, int count, CompareOptions options)
		{
			return collator.IndexOf (source, value,
				startIndex, count, options);
		}

		public virtual int IndexOf (string source, string value,
			int startIndex, int count, CompareOptions options)
		{
			return collator.IndexOf (source, value,
				startIndex, count, options);
		}

		public virtual int LastIndexOf (string source, char value,
			int startIndex, int count, CompareOptions options)
		{
			return collator.LastIndexOf (source, value,
				startIndex, count, options);
		}

		public virtual int LastIndexOf (string source, string value,
			int startIndex, int count, CompareOptions options)
		{
			return collator.LastIndexOf (source, value,
				startIndex, count, options);
		}

		public virtual bool IsPrefix (string source, string prefix,
					     CompareOptions options)
		{
			return collator.IsPrefix (source, suffix, options);
		}

		public virtual bool IsSuffix (string source, string suffix,
					     CompareOptions options)
		{
			return collator.IsSuffix (source, suffix, options);
		}
	}
}
