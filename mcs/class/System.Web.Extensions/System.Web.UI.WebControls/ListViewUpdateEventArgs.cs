//
// System.Web.UI.WebControls.ListView
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2008-2007 Novell, Inc
//

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
#if NET_3_5
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	public class ListViewUpdateEventArgs : CancelEventArgs
	{
		IOrderedDictionary _keys;
		IOrderedDictionary _newValues;
		IOrderedDictionary _oldValues;
		
		public ListViewUpdateEventArgs (int itemIndex)
		{
			ItemIndex = itemIndex;
		}
		
		public int ItemIndex {
			get;
			private set;
		}
		
		public IOrderedDictionary Keys {
			get {
				if (_keys == null)
					_keys = new OrderedDictionary ();
				return _keys;
			}
		}
		
		public IOrderedDictionary NewValues {
			get {
				if (_newValues == null)
					_newValues = new OrderedDictionary ();
				return _newValues;
			}
		}
		
		public IOrderedDictionary OldValues {
			get {
				if (_oldValues == null)
					_oldValues = new OrderedDictionary ();
				return _oldValues;
			}
		}
	}
}
#endif
