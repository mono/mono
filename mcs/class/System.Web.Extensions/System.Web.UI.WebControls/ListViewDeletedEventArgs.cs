//
// System.Web.UI.WebControls.ListView
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007-2008 Novell, Inc
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
using System;
using System.Collections.Specialized;

namespace System.Web.UI.WebControls
{
	public class ListViewDeletedEventArgs : EventArgs
	{
		IOrderedDictionary _keys;
		IOrderedDictionary _values;

		internal ListViewDeletedEventArgs (int affectedRows, Exception exception, IOrderedDictionary keys, IOrderedDictionary values)
			: this (affectedRows, exception)
		{
			_keys = keys;
			_values = values;
		}
		
		public ListViewDeletedEventArgs (int affectedRows, Exception exception)
		{
			AffectedRows = affectedRows;
			Exception = exception;
			ExceptionHandled = false;
		}
		
		public int AffectedRows {
			get;
			private set;
		}
		
		public Exception Exception {
			get;
			private set;
		}
		
		public bool ExceptionHandled {
			get;
			set;
		}
		
		public IOrderedDictionary Keys {
			get {
				if (_keys == null)
					_keys = new OrderedDictionary ();
				return _keys;
			}
		}
		
		public IOrderedDictionary Values {
			get {
				if (_values == null)
					_values = new OrderedDictionary ();
				return _values;
			}
		}
	}
}
