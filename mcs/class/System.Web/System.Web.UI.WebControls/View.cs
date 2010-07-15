//
// System.Web.UI.WebControls.View.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[ParseChildrenAttribute (false)]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.ViewDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ToolboxDataAttribute ("<{0}:View runat=\"server\"></{0}:View>")]
	public class View: Control
	{
		static readonly object ActivateEvent = new object();
		static readonly object DeactivateEvent = new object();

		[BrowsableAttribute(true)]
		public override bool EnableTheming {
			get { return base.EnableTheming; }
			set { base.EnableTheming = value; }
		}

		public View ()
		{
			base.Visible = false;
		}
		
		internal void NotifyActivation (bool activated)
		{
			if (activated) OnActivate (EventArgs.Empty);
			else OnDeactivate (EventArgs.Empty);
		}
	
		public event EventHandler Activate {
			add { Events.AddHandler (ActivateEvent, value); }
			remove { Events.RemoveHandler (ActivateEvent, value); }
		}
		
		public event EventHandler Deactivate {
			add { Events.AddHandler (DeactivateEvent, value); }
			remove { Events.RemoveHandler (DeactivateEvent, value); }
		}
		
		protected virtual void OnActivate (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [ActivateEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnDeactivate (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [DeactivateEvent];
				if (eh != null) eh (this, e);
			}
		}

		internal bool VisibleInternal
		{
			get { return base.Visible; }
			set { base.Visible = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool Visible {
			get { return base.Visible; }
			set {
				throw new InvalidOperationException ("The Visible property of a View control can only be set by setting the active View of a MultiView.");
			}
		}
	}
}

#endif
