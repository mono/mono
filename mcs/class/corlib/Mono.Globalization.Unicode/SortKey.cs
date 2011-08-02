//
// System.Globalization.SortKey.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
// This file works with our managed collation implementation.
//

using System;
using System.IO;
using System.Globalization;

namespace System.Globalization
{
	[System.Runtime.InteropServices.ComVisible (true)]
	[Serializable]
	public class SortKey
	{
		#region Static members
		public static int Compare (SortKey sortkey1, SortKey sortkey2)
		{
			if (sortkey1 == null)
				throw new ArgumentNullException ("sortkey1");
			if (sortkey2 == null)
				throw new ArgumentNullException ("sortkey2");

			if (Object.ReferenceEquals (sortkey1, sortkey2)
				|| Object.ReferenceEquals (sortkey1.OriginalString,
				sortkey2.OriginalString))
				return 0;

			byte [] d1 = sortkey1.KeyData;
			byte [] d2 = sortkey2.KeyData;

			int len = d1.Length > d2.Length ? d2.Length : d1.Length;
			for (int i = 0; i < len; i++)
				if (d1 [i] != d2 [i])
					return d1 [i] < d2 [i] ? -1 : 1;
			return d1.Length == d2.Length ? 0 : d1.Length < d2.Length ? -1 : 1;
		}
		#endregion

		readonly string source;
		readonly byte [] key;
		readonly CompareOptions options;
		readonly int lcid;

		// for legacy unmanaged one
		internal SortKey (int lcid, string source, CompareOptions opt)
		{
			this.lcid = lcid;
			this.source = source;
			this.options = opt;
		}

		internal SortKey (int lcid, string source, byte [] buffer, CompareOptions opt,
			int lv1Length, int lv2Length, int lv3Length,
			int kanaSmallLength, int markTypeLength,
			int katakanaLength, int kanaWidthLength,
			int identLength)
		{
			this.lcid = lcid;
			this.source = source;
			this.key = buffer;
			this.options = opt;
		}

		public virtual string OriginalString {
			get { return source; }
		}

		public virtual byte [] KeyData {
			get { return key; }
		}

		// copy from original SortKey.cs
		public override bool Equals (object value)
		{
			SortKey other = (value as SortKey);
			if (other != null) {
				if ((this.lcid == other.lcid) &&
				   (this.options == other.options) &&
				   (Compare (this, other) == 0)) {
					return true;
				}
			}

			return false;
		}

		public override int GetHashCode ()
		{
			if (key.Length == 0)
				return 0; // should not happen though.
			int val = key [0];
			for (int i = 1; i < key.Length; i++)
				val ^= (int) key [i] << (i & 3);
			return (int) val;
		}

		// copy from original SortKey.cs
		public override string ToString()
		{
			return "SortKey - " + lcid + ", " + options + ", " + source;
		}
	}
}
