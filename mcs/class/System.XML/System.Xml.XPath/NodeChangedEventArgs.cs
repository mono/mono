//
// NodeChangedEventArgs.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
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

#if NET_2_0

using System;

namespace System.Xml.XPath
{
	public class NodeChangedEventArgs : EventArgs
	{
		XPathDocumentNodeChangedAction action;
		XPathEditableNavigator item;

		public XPathDocumentNodeChangedAction Action {
			get { return action; }
		} 

		public XPathEditableNavigator Item {
			get { throw new NotImplementedException (); }
		}

		public XPathEditableNavigator NewParent {
			get { throw new NotImplementedException (); }
		}

		public XPathEditableNavigator NewPreviousItem {
			get { throw new NotImplementedException (); }
		}

		public object NewValue {
			get { throw new NotImplementedException (); }
		}

		public XPathEditableNavigator OldParent {
			get { throw new NotImplementedException (); }
		}

		public XPathEditableNavigator OldPreviousItem {
			get { throw new NotImplementedException (); }
		}

		public object OldValue {
			get { throw new NotImplementedException (); }
		}
	}
}

#endif
