//
// System.Globalization.SortKey.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Globalization {

#if NET_2_0
	[System.Runtime.InteropServices.ComVisible(true)]
#endif
	[Serializable]
	public class SortKey {
		private string str;
		private CompareOptions options;
		private byte[] key;
		private int lcid;
		
		/* Hide the .ctor() */
		SortKey () {}

		internal SortKey (int lcid, string source, CompareOptions options)
		{
			this.lcid = lcid;
			str = source;
			this.options = options;
		}

		public virtual byte[] KeyData
		{
			get {
				return key;
			}
		}

		public virtual string OriginalString
		{
			get {
				return str;
			}
		}

		public static int Compare (SortKey sortkey1, SortKey sortkey2)
		{
			if (sortkey1 == null) {
				throw new ArgumentNullException ("sortkey1");
			}
			if (sortkey2 == null) {
				throw new ArgumentNullException ("sortkey2");
			}

			byte[] keydata1 = sortkey1.key;
			byte[] keydata2 = sortkey2.key;

			if (keydata1.Length == 0) {
				if (keydata2.Length == 0) {
					return 0;
				}
				return -1;
			}
			
			int min_len = (keydata1.Length < keydata2.Length) ? keydata1.Length : keydata2.Length;

			for (int i = 0; i < min_len; i++) {
				if (keydata1[i] > keydata2[i]) {
					return 1;
				} else if (keydata1[i] < keydata2[i]) {
					return -1;
				}
			}

			if (keydata1.Length < keydata2.Length) {
				return -1;
			} else if (keydata1.Length > keydata2.Length) {
				return 1;
			} else {
				return 0;
			}
		}

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
			return str.GetHashCode ();
		}

		public override string ToString ()
		{
			return ("SortKey - " + lcid + ", " + options + ", " + str);
		}
	}
}
