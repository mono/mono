//
// NodeChangedEventArgs.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
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
