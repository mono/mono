//
// System.Web.UI.Design.ViewEventArgs
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Drawing;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design
{
	public class ViewEventArgs : EventArgs
	{
		ViewEvent event_type;
		DesignerRegion region;
		EventArgs event_args;

		public ViewEventArgs (ViewEvent eventType, DesignerRegion region, EventArgs eventArgs)
		{
			this.event_type = eventType;
			this.region = region;
			this.event_args = eventArgs;
		}

		public ViewEvent EventType {
			get { return event_type; }
		}

		public DesignerRegion Region {
			get { return region; }
		}

		public EventArgs EventArgs {
			get { return event_args; }
		}
	}
}

#endif
