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
		#region Local Variables
		internal InputLanguage[]	list;
		#endregion	// Local Variables

		#region Private Constructor
		internal InputLanguageCollection() {
		}
		#endregion	// Private Constructor

		#region Internal Instance Methods
		internal void Add(InputLanguage value) {
			list[list.Length]=value;
		}
		#endregion

		#region Public Instance Methods
		public InputLanguage this [int index] {
			get {
				if (index>=list.Length) {
					throw new ArgumentOutOfRangeException("index");
				}
				return list[index];
			}
		}

		public bool Contains(InputLanguage value) {
			for (int i=0; i<list.Length; i++) {
				if ((list[i].Culture==value.Culture) && (list[i].LayoutName==value.LayoutName)) {
					return true;
				}
			}
			return false;
		}

		public void CopyTo(InputLanguage[] dest, int index) {
			if (list.Length>0) {
				Array.Copy(list, 0, dest, index, list.Length);
			}
		}

		public int IndexOf(InputLanguage value) {
			for (int i=0; i<list.Length; i++) {
				if ((list[i].Culture==value.Culture) && (list[i].LayoutName==value.LayoutName)) {
					return i;
				}
			}
			return -1;
		}
		#endregion	// Public Instance Methods
	}
}
