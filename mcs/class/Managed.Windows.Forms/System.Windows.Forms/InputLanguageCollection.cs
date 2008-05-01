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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//


// COMPLETE

using System.Collections;
using System.Globalization;

namespace System.Windows.Forms {
	public class InputLanguageCollection : ReadOnlyCollectionBase {
		#region Private Constructor
		internal InputLanguageCollection (InputLanguage[] data)
		{
			base.InnerList.AddRange (data);
		}
		#endregion	// Private Constructor

		#region Public Instance Methods
		public InputLanguage this [int index] {
			get {
				if (index >= base.InnerList.Count) {
					throw new ArgumentOutOfRangeException("index");
				}
				return base.InnerList[index] as InputLanguage;
			}
		}

		public bool Contains(InputLanguage value) {
			for (int i = 0; i < base.InnerList.Count; i++) {
				if ((this[i].Culture == value.Culture) && (this[i].LayoutName == value.LayoutName)) {
					return true;
				}
			}
			return false;
		}

		public void CopyTo(InputLanguage[] array, int index) {
			if (base.InnerList.Count > 0) {
				base.InnerList.CopyTo (array, index);
			}
		}

		public int IndexOf(InputLanguage value) {
			for (int i = 0; i < base.InnerList.Count; i++) {
				if ((this[i].Culture == value.Culture) && (this[i].LayoutName == value.LayoutName)) {
					return i;
				}
			}
			return -1;
		}
		#endregion	// Public Instance Methods
	}
}
