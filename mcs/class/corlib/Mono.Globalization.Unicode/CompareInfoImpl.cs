//
// Note that this is just a conceptual source and subject to changes.
//

using System;
using System.Text;

using Mono.Globalization.Collation;

namespace System.Globalization
{
	internal class CompareInfo
	{
		CultureInfo culture;
		Collator collator;

		internal CompareInfo (CultureInfo ci)
		{
			culture = ci;
			collator = new RuleBasedCollator (ci);
		}

		public override int Compare (
			string string1, int offset1, int length1,
			string string2, int offset2, int length2,
			CompareOptions options)
		{
			// FIXME: check allowed flags here.
			// FIXME: check array range

			// quick ordinal comparison
			if (options == CompareOptions.Ordinal) {
				int min = length1 < length2 ? length1 : length2;
				for (int i = 0; i < min; i++)
					if (string1 [offset1 + i] != string2 [offset2 + i])
						return ((int) string1 [offset1 + i]) 
							- ((int) string2 [offset2 + i]);
				return (length1 > min) ?
					1 :
					(length2 > min) ? -1 : 0;
			}

			return collator.Compare (string1, offset1, length1,
				string2, offset2, length2, options);
		}

		public override SortKey GetSortKey (
			string source, CompareOptions options)
		{
			// FIXME: check allowed flags here.

			return collator.GetSortKey (source, options);
		}

		public override int IndexOf (string source, char value,
			int startIndex, int count, CompareOptions options)
		{
			return IndexOf (source, value.ToString (this.culture),
				startIndex, count, options);
		}

		public override int IndexOf (string source, string value,
			int startIndex, int count, CompareOptions options)
		{
			return collator.IndexOf (
				source, value, startIndex, count, options);
		}
	}
}
