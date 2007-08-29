//
// System.ComponentModel.Design.DesignerEventService
//
// Authors:	 
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006-2007 Ivan N. Zlatev

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
using System.ComponentModel;
using System.Collections;

namespace System.ComponentModel.Design
{
	// IDesignerEventService provides a global eventing mechanism for designer events. With this mechanism,
	// an application is informed when a designer becomes active. The service provides a collection of
	// designers and a single place where global objects, such as the Properties window, can monitor selection
	// change events.
	//
	// IDesignerEventService is practicly replaced by the DesignSurfaceManager and also the
	// meaning of Designer in IDesignerEventService is actually DesignerHost.
	//
	internal sealed class DesignerEventService : IDesignerEventService
	{
		
		public DesignerEventService ()
		{
			_designerList = new ArrayList ();
		}

		private ArrayList _designerList;
		private IDesignerHost _activeDesigner;

		public IDesignerHost ActiveDesigner {
			get { 
				return _activeDesigner; 
			}
			internal set {
				IDesignerHost old = _activeDesigner;
				_activeDesigner = value;
				if (ActiveDesignerChanged != null)
					ActiveDesignerChanged (this, new ActiveDesignerEventArgs (old, value));
			}
		}

		public DesignerCollection Designers {
			get { return new DesignerCollection (_designerList); }
		}

		public event ActiveDesignerEventHandler ActiveDesignerChanged;
		public event DesignerEventHandler DesignerCreated;
		public event DesignerEventHandler DesignerDisposed;
		public event EventHandler SelectionChanged;
		
		public void RaiseDesignerCreated (IDesignerHost host)
		{
			if (DesignerCreated != null)
				DesignerCreated (this, new DesignerEventArgs (host));
		}
		
		public void RaiseDesignerDisposed (IDesignerHost host)
		{
			if (DesignerDisposed != null)
				DesignerDisposed (this, new DesignerEventArgs (host));
		}
		
		public void RaiseSelectionChanged ()
		{
			if (SelectionChanged != null)
				SelectionChanged (this, EventArgs.Empty);
		}
		
	}
}
#endif
