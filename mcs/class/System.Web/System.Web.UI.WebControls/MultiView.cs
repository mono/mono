//
// System.Web.UI.WebControls.MultiView.cs
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
//	[ControlBuilder (typeof(MultiViewControlBuilder)]
	[Designer ("System.Web.UI.Design.WebControls.MultiViewDesigner, System.Design, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.IDesigner")]
	[ToolboxData ("<{0}:MultiView runat=\"server\"></{0}:MultiView>")]
	[ParseChildren (false, ChildControlType = typeof(View))]
	[DefaultEvent ("ActiveViewChanged")]
	public class MultiView: Control
	{
		public static readonly string NextViewCommandName = "NextView";
		public static readonly string PreviousViewCommandName = "PrevView";
		public static readonly string SwitchViewByIDCommandName = "SwitchViewByID";
		public static readonly string SwitchViewByIndexCommandName = "SwitchViewByIndex";
		
		private static readonly object ActiveViewChangedEvent = new object();
		
		int viewIndex = -1;
		int initialIndex = -1;
		bool initied;
		
		public event EventHandler ActiveViewChanged {
			add { Events.AddHandler (ActiveViewChangedEvent, value); }
			remove { Events.RemoveHandler (ActiveViewChangedEvent, value); }
		}
		
		protected override void AddParsedSubObject (object ob)
		{
			if (ob is View)
				Controls.Add (ob as View);
		}
		
		protected override ControlCollection CreateControlCollection ()
		{
			return new ViewCollection (this);
		}
		
		public View GetActiveView ()
		{
			if (viewIndex < 0 || viewIndex >= Controls.Count)
				throw new HttpException ("The ActiveViewIndex is not set to a valid View control");
			return Controls [viewIndex] as View;
		}
		
		public void SetActiveView (View view)
		{
			int i = Controls.IndexOf (view);
			if (i == -1)
				throw new HttpException ("The provided view is not contained in the MultiView control.");
				
			ActiveViewIndex = i;
		}
		
		[DefaultValue (-1)]
		public virtual int ActiveViewIndex {
			get { return viewIndex; }
			set {
				if (!initied) {
					initialIndex = value;
					return;
				}
				
				if (value < -1 || value >= Controls.Count)
					throw new ArgumentOutOfRangeException ();

				if (viewIndex != -1)
					((View)Controls [viewIndex]).NotifyActivation (false);

				viewIndex = value;

				if (viewIndex != -1)
					((View)Controls [viewIndex]).NotifyActivation (true);
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[Browsable (false)]
		public virtual ViewCollection Views {
			get { return Controls as ViewCollection; }
		}
		
		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			CommandEventArgs ca = e as CommandEventArgs;
			if (ca != null) {
				switch (ca.CommandName) {
					case "NextView":
						if (viewIndex < Controls.Count - 1 && Controls.Count > 0)
							ActiveViewIndex = viewIndex + 1;
						break;
						
					case "PrevView": 
						if (viewIndex > 0)
							ActiveViewIndex = viewIndex - 1;
						break;
						
					case "SwitchViewByID":
						foreach (View v in Controls)
							if (v.ID == ca.CommandArgument) {
								SetActiveView (v);
								break;
							}
						break;
						
					case "SwitchViewByIndex":
						int i = (int) Convert.ChangeType (ca.CommandArgument, typeof(int));
						ActiveViewIndex = i;
						break;
				}
			}
			return false;
		}
		
		protected override void OnInit (EventArgs e)
		{
			initied = true;
			Page.RegisterRequiresControlState (this);
			ActiveViewIndex = initialIndex;
			base.OnInit (e);
		}
		
		protected internal override void RemovedControl (Control ctl)
		{
			if (viewIndex >= Controls.Count)
				viewIndex = Controls.Count - 1;

			base.RemovedControl (ctl);
		}
		
		protected internal override void LoadControlState (object state)
		{
			if (state != null) viewIndex = (int)state;
			else viewIndex = -1;
		}
		
		protected internal override object SaveControlState ()
		{
			if (viewIndex != -1) return viewIndex;
			else return null;
		}
		
		protected virtual void OnActiveViewChanged (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [ActiveViewChangedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected override void Render (HtmlTextWriter writer)
		{
			if (viewIndex != -1)
				GetActiveView ().Render (writer);
		}
	}
}

#endif
