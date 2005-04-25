//
// Note that this is just a conceptual source and subject to changes.
//

using System;
using System.Globalization;
using System.Text;


namespace Mono.Globalization.Unicode
{
	internal class CollatorComparer
	{
		struct CompareResult
		{
			public int Result;
			public int Advance1;
			public int Advance2;

			public CompareResult (int result, int advance1, int advance2)
			{
				Result = result;
				Advance1 = advance1;
				Advance2 = advance2;
			}
		}

		public readonly Collator Collator;
		public readonly CompareOptions Options;

		private char [] nfdBuffer = new char [4];

		public CollatorComparer (
			Collator c, CompareOptions opts)
		{
			Collator = c;
			Options = opts;

		}

		// Below are normalization matters:
		// - IgnoreCase -> use culture dependent ToLower()
		// - IgnoreKanaType -> ToKanaTypeInsensitive().
		// - IgnoreNonSpace -> IsIgnorableNonSpacing().
		// - IgnoreSymbols -> IsIgnorableSymbol()
		// - IgnoreWidth -> ToWidthInsensitive()
		//
		// It is independently considered, maybe as a tailored
		// collation element table.
		// - StringSort
		//
		// Even with CompareOptions.None both of Japanese katakana
		// with voice mark"included" or "separate", while half-width
		// katakana are distinguished unless IgnoreWidth is specified.
		// So maybe canonical normalization is done.
		public bool IsIgnorable (int i)
		{
			if (MSCompatUnicodeTable.IsIgnorable (i))
				return true;
			if ((Options & CompareOptions.IgnoreWidth) != 0)
				i = MSCompatUnicodeTable.ToWidthInsensitive (i);
			if ((Options & CompareOptions.IgnoreKanaType) != 0)
				i = MSCompatUnicodeTable.ToKanaTypeInsensitive (i);
			if ((Options & CompareOptions.IgnoreCase) != 0 && i <= char.MaxValue)
				i = collator.Culture.ToLower ((char) i);
			if ((Options & CompareOptions.IgnoreSymbols) != 0
				&& MSCompatUnicodeTable.IsIgnorableSymbol (i))
				return true;
			if ((Options & CompareOptions.IgnoreNonSpace) != 0
				&& MSCompatUnicodeTable.IsIgnorableNonSpacing (i))
				return true;
			return false;
		}

		public int Compare (string s1, int start1, int len1,
			string s2, int start2, int len2)
		{
			if (s1.Length < start1 + len1 ||
				s2.Length < start2 + len2)
				throw new ArgumentOutOfRangeException ("start and length must not be out of range of the string.");

			int min = len1 < len2 ? len1 : len2;
			int start = 0;

			while (true) {
				int stop = -1;
				for (int i = start; i < min; i++) {
					if (s1 [start1 + i] != s2 [start2 + i]) {
						stop = i;
						break;
					}
				}
				if (stop == -1)
					break;

				// go back to where NFD-normalization should start.
				for (int i = stop; i >= start1; i--) {
					if (!CharUnicodeInfo.IsUnsafe (s1 [i])) {
						start1 = i;
						break;
					}
				}
				for (int i = stop; i >= start2; i--) {
					if (!CharUnicodeInfo.IsUnsafe (s2 [i])) {
						start2 = i;
						break;
					}
				}

				CompareResult ret = ComparePrimary (s1, start1, len1, s2, start2, len2);
				if (ret.Result == 0 && Strength >= 2)
					ret = CompareSecondary (s1, start1, len1, s2, start2, len2);
				if (ret.Result == 0 && Strength >= 3)
					ret = CompareThirtiary (s1, start1, len1, s2, start2, len2);
				if (ret.Result == 0 && Strength >= 4)
					ret = CompareQuaternary (s1, start1, len1, s2, start2, len2);
				if (ret.Result != 0)
					return ret.Result;
				start1 += ret.Advance1;
				start2 += ret.Advance2;
			}

			if (len1 == len2)
				return 0;
			return len2 > len1 ? 1 : -1;
		}

		public SortKey GetSortKey (string source)
		{
			string n = Norm.Normalize (source, 1);

			throw new NotImplementedException ();
		}

		#region Normalization

		//
		// It handles UAX#15 Normalization, and optionally removal
		// of ignored characters specified by CompareOptions.
		//
		private string Normalize (string source)
		{
			for (int i = 0; i < nfdBuffer.Length; i++)
				nfdBuffer [i] = 0;

			StringBuilder sb = null;
			int start = 0;
			for (int i = 0; i < source.Length; i++) {
/* This step is removed in the latest UCA v4.1.0.
				// handle Logical Order Exception
				char c = source [i];
				if ('\u0x0E40' <= c && c <= '\u0x0E44'
					|| '\u0x0EC0' <= c && c <= '\u0x0EC4')
					ProcessLogicalException (ref sb,
						source, i, ref start);
*/
				// handle decomposition
				if (Norm.IsNoNfd (source, i))
					ProcessNfd (ref sb, source, i, ref start);
			}
			if (sb != null)
				sb.Append (source, start, source.Length - start);
			return sb != null ? sb.ToString () : source);
		}

		private void ProcessLogicalException (ref StringBuilder sb,
			string s, int i, ref int start)
		{
			// If no following character, do nothing.
			if (s.Length == i + 1)
				return;

			if (sb == null)
				sb = new StringBuilder (nfd.Length + 100);
			sb.Append (nfd, start, i);
			sb.Append (nfd [i + 1]);
			sb.Append (nfd [i++]);
			start = i + 1;
		}

/*
		private void ProcessNfd (ref StringBuilder sb,
			string s, int i, ref int start)
		{
			if (sb == null)
				sb = new StringBuilder (s.Length + 100);
			sb.Append (s, start, i);
			if (!Norm.IsMultiForm (s [i]))
				sb.Append (Norm.SingleForm (s [i]));
			else {
				Norm.MultiForm (s [i], nfdBuffer);
				for (int i = 0; i < nfdBuffer.Length; i++) {
					if (nfdBuffer [i] == 0)
						break;
					sb.Append (nfdBuffer [i]);
				}
			}
			start = i + 1;
		}
*/
		#endregion Normalization

		public int IndexOf (string source, char value,
			int startIndex, int count)
		{
			return IndexOf (source, new string (value, 1),
				startIndex, count);
		}

		public int IndexOf (string source, string value,
			int startIndex, int count)
		{
			throw new NotImplementedException ();
		}

		public int LastIndexOf (string source, char value,
			int startIndex, int count)
		{
			return LastIndexOf (source, new string (value, 1),
				startIndex, count);
		}

		public int LastIndexOf (string source, string value,
			int startIndex, int count)
		{
			throw new NotImplementedException ();
		}

		public bool IsPrefix (string source, string prefix)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (prefix == null)
				throw new ArgumentNullException("prefix");

			// quick check
			if (Compare (source, 0, prefix.Length,
				prefix, 0, prefix.Length) == 0)
				return true;

			// FIXME: it could be compared like above, after
			// some transliteration. It is sometimes too heavy.
			return IndexOf (source, prefix, 0, prefix.Length) == 0;
		}

		public bool IsSuffix (string source, string suffix)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (suffix == null)
				throw new ArgumentNullException("suffix");

			// quick check
			if (Compare (source, source.Length - suffix.Length,
				suffix.Length, suffix, 0, suffix.Length) == 0)
				return true;

			// FIXME: it could be compared like above, after
			// some transliteration. It is sometimes too heavy.
			int i = LastIndexOf (source, suffix, 0, suffix.Length);
			// Note that LastIndexOf() might return different
			// value than suffix.Length. So we still have to check
			// the exact last sequence.
			return Compare (source, i, source.Length - i,
				suffix, 0, suffix.Length) == 0;
		}
	}

	internal abstract class Collator
	{
		public abstract int Compare (string s1, int start1, int len1,
			string s2, int start2, int len2, CompareOptions options);
		public abstract SortKey GetSortKey (string s, CompareOptions options);
		public abstract int IndexOf (string s1, string value,
			int start, int len, CompareOptions options);
		public abstract int IndexOf (string s1, char value,
			int start, int len, CompareOptions options);

		public abstract Collator Clone ();
	}

	//
	// Rule-based collator. It does not handle actual comparison, but 
	// just holds parsed rules. Since comparison depends on options where
	// we don't want to create CompareInfo instances every time and also
	// don't want Monitor, it is done by CollatorComparer created with
	// CultureInfo and CompareOptions.
	//
	internal class RuleBasedCollator : Collator
	{
		CultureInfo culture;
		CollatorComparer defaultComparer;
		CollatorComparer cache;

		string ruleString;

		public RuleBasedCollator (CultureInfo ci)
		{
			culture = ci;
			defaultComparer = new CollatorComparer (
				this, CompareOptions.None);

			// FIXME: get tailored collatiojn element table (rule)
			// from resource or runtime internal, parse and store.
		}

		private RuleBasedCollator (RuleBasedCollator other)
		{
			throw new NotImplementedException ();

			culture = other.culture;
		}

		public CultureInfo Culture {
			get { return culture; }
		}

		public override Collator Clone ()
		{
			return new RuleBasedCollator (this);
		}

		private CollatorComparer PopulateComparer (CompareOptions options)
		{
			CollatorComparer c = defaultComparer;
			if (c.Options != options)
				c = new CollatorComparer (this, options);
			else {
				c = cache;
				if (c == null || c.Options == options)
					c = new CollatorComparer (this, options);
				cache = c;
			}
			return c;
		}

		// This must be thread safe.
		// To avoid lock and consumptive creation, here we use
		// default comparer instance and additional cache.
		public override int Compare (string s1, int start1, int len1,
			string s2, int start2, int len2, CompareOptions options)
		{
			return PopulateComparer (options)
				.Compare (s1, start1, len1, s2, start2, len2);
		}

		public override SortKey GetSortKey (
			string source, CompareOptions options)
		{
			return PopulateComparer (options).GetSortKey (source);
		}

		public override int IndexOf (string source, char value,
			int startIndex, int count, CompareOptions options)
		{
			return PopulateComparer (options).IndexOf (source,
				value, startIndex, count);
		}

		public override int IndexOf (string source, string value,
			int startIndex, int count, CompareOptions options)
		{
			return PopulateComparer (options).IndexOf (source,
				value, startIndex, count);
		}
	}
}

