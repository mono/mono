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
	public class ListViewUpdatedEventArgs : EventArgs
	{
		IOrderedDictionary _newValues;
		IOrderedDictionary _oldValues;

		internal ListViewUpdatedEventArgs (int affectedRows, Exception exception, IOrderedDictionary newValues, IOrderedDictionary oldValues)
			: this (affectedRows, exception)
		{
			_newValues = newValues;
			_oldValues = oldValues;
		}
		
		public ListViewUpdatedEventArgs (int affectedRows, Exception exception)
		{
			AffectedRows = affectedRows;
			Exception = exception;
			ExceptionHandled = false;
			KeepInEditMode = false;
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
		
		public bool KeepInEditMode {
			get;
			set;
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
				return _newValues;
			}
		}
	}
}
