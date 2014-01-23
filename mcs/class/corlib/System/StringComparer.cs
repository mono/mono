//
// System.StringComparer
//
// Authors:
//	Marek Safar (marek.safar@seznam.cz)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace System
{
	[Serializable, ComVisible(true)]
	public abstract class StringComparer : IComparer, IEqualityComparer, IComparer<string>, IEqualityComparer<string>
	{
		static class Predefined
		{
			public static readonly StringComparer invariantCultureIgnoreCase = new CultureAwareComparer (CultureInfo.InvariantCulture, true);
			public static readonly StringComparer invariantCulture = new CultureAwareComparer (CultureInfo.InvariantCulture, false);
			public static readonly StringComparer ordinalIgnoreCase = new OrdinalComparer (true);
			public static readonly StringComparer ordinal = new OrdinalComparer (false);
		}
		
		// Constructors
		protected StringComparer ()
		{
		}

		// Properties
		public static StringComparer CurrentCulture {
			get {
				return new CultureAwareComparer (CultureInfo.CurrentCulture, false);
			}
		}

		public static StringComparer CurrentCultureIgnoreCase {
			get {
				return new CultureAwareComparer (CultureInfo.CurrentCulture, true);
			}
		}

		public static StringComparer InvariantCulture {
			get {
				return Predefined.invariantCulture;
			}
		}

		public static StringComparer InvariantCultureIgnoreCase {
			get {
				return Predefined.invariantCultureIgnoreCase;
			}
		}

		public static StringComparer Ordinal {
			get { return Predefined.ordinal; }
		}

		public static StringComparer OrdinalIgnoreCase {
			get { return Predefined.ordinalIgnoreCase; }
		}

		// Methods
		public static StringComparer Create (CultureInfo culture, bool ignoreCase)
		{
			if (culture == null)
				throw new ArgumentNullException ("culture");

			return new CultureAwareComparer (culture, ignoreCase);
		}

		public int Compare (object x, object y)
		{
			if (x == y)
				return 0;
			if (x == null)
				return -1;
			if (y == null)
				return 1;

			string s_x = x as string;
			if (s_x != null) {
				string s_y = y as string;
				if (s_y != null)
					return Compare (s_x, s_y);
			}

			IComparable ic = x as IComparable;
			if (ic == null)
				throw new ArgumentException ();

			return ic.CompareTo (y);
		}

		public new bool Equals (object x, object y)
		{
			if (x == y)
				return true;
			if (x == null || y == null)
				return false;

			string s_x = x as string;
			if (s_x != null) {
				string s_y = y as string;
				if (s_y != null)
					return Equals (s_x, s_y);
			}
			return x.Equals (y);
		}

		public int GetHashCode (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			string s = obj as string;
			return s == null ? obj.GetHashCode (): GetHashCode(s);
		}

		public abstract int Compare (string x, string y);
		public abstract bool Equals (string x, string y);
		public abstract int GetHashCode (string obj);
	}

	[Serializable]
	sealed class CultureAwareComparer : StringComparer
	{
		readonly bool _ignoreCase;
		readonly CompareInfo _compareInfo;

		public CultureAwareComparer (CultureInfo ci, bool ignore_case)
		{
			_compareInfo = ci.CompareInfo;
			_ignoreCase = ignore_case;
		}

		public override int Compare (string x, string y)
		{
			CompareOptions co = _ignoreCase ? CompareOptions.IgnoreCase : 
				CompareOptions.None;
			return _compareInfo.Compare (x, y, co);
		}

		public override bool Equals (string x, string y)
		{
			return Compare (x, y) == 0;
		}

		public override int GetHashCode (string s)
		{
			if (s == null)
				throw new ArgumentNullException("s");

			CompareOptions co = _ignoreCase ? CompareOptions.IgnoreCase : 
				CompareOptions.None;
			return _compareInfo.GetSortKey (s, co).GetHashCode ();
		}
	}

	[Serializable]
	internal sealed class OrdinalComparer : StringComparer
	{
		readonly bool _ignoreCase;

		public OrdinalComparer (bool ignoreCase)
		{
			_ignoreCase = ignoreCase;
		}

		public override int Compare (string x, string y)
		{
			if (_ignoreCase)
				return String.CompareOrdinalCaseInsensitiveUnchecked (x, 0, Int32.MaxValue, y, 0, Int32.MaxValue);
			else
				return String.CompareOrdinalUnchecked (x, 0, Int32.MaxValue, y, 0, Int32.MaxValue);
		}

		public override bool Equals (string x, string y)
		{
			if (_ignoreCase)
				return Compare (x, y) == 0;

			return x == y;
		}

		public override int GetHashCode (string s)
		{
			if (s == null)
				throw new ArgumentNullException ("s");

			if (_ignoreCase)
				return s.GetCaseInsensitiveHashCode ();
			else
				return s.GetHashCode ();
		}
	}
}
