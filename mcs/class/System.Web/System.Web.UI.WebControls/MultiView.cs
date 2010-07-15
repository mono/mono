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
	[ControlBuilder (typeof(MultiViewControlBuilder))]
	[Designer ("System.Web.UI.Design.WebControls.MultiViewDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ToolboxData ("<{0}:MultiView runat=\"server\"></{0}:MultiView>")]
#if NET_2_0
	[ParseChildren (typeof(View))]
#else
	[ParseChildren (false)]
#endif
	[DefaultEvent ("ActiveViewChanged")]
	public class MultiView: Control
	{
		public static readonly string NextViewCommandName = "NextView";
		public static readonly string PreviousViewCommandName = "PrevView";
		public static readonly string SwitchViewByIDCommandName = "SwitchViewByID";
		public static readonly string SwitchViewByIndexCommandName = "SwitchViewByIndex";
		
		static readonly object ActiveViewChangedEvent = new object();
		
		int viewIndex = -1;
		int initialIndex = -1;
		
		public event EventHandler ActiveViewChanged {
			add { Events.AddHandler (ActiveViewChangedEvent, value); }
			remove { Events.RemoveHandler (ActiveViewChangedEvent, value); }
		}
		
		protected override void AddParsedSubObject (object ob)
		{
			if (ob is View)
				Controls.Add (ob as View);
			// LAMESPEC: msdn talks that only View contorls are allowed, for others controls HttpException should be thrown
			// but actually, aspx praser adds LiteralControl controls.
			//else
			//	throw new HttpException ("MultiView cannot have children of type 'Control'.  It can only have children of type View.");
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
			get
			{
				if (Controls.Count == 0)
					return initialIndex;

				return viewIndex;
			}
			set 
			{
				if (Controls.Count == 0) {
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

				UpdateViewVisibility ();

				OnActiveViewChanged (EventArgs.Empty);
			}
		}

		[Browsable (true)]
		public virtual new bool EnableTheming
		{
			get { return base.EnableTheming; }
			set { base.EnableTheming = value; }
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
							if (v.ID == (string)ca.CommandArgument) {
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
		
		protected internal override void OnInit (EventArgs e)
		{
			Page.RegisterRequiresControlState (this);
			if (initialIndex != -1) {
				ActiveViewIndex = initialIndex;
				initialIndex = -1;
			}
			base.OnInit (e);
		}
		
		void UpdateViewVisibility ()
		{
			for (int n=0; n<Views.Count; n++)
				Views [n].VisibleInternal = (n == viewIndex);
		}
		
		protected internal override void RemovedControl (Control ctl)
		{
			if (viewIndex >= Controls.Count) {
				viewIndex = Controls.Count - 1;
				UpdateViewVisibility ();
			}

			base.RemovedControl (ctl);
		}
		
		protected internal override void LoadControlState (object state)
		{
			if (state != null) {
				viewIndex = (int)state;
				UpdateViewVisibility ();
			}
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
		
		protected internal override void Render (HtmlTextWriter writer)
		{
			if ((Controls.Count == 0) && (initialIndex != -1)) 
				viewIndex = initialIndex;
			if (viewIndex != -1)
				GetActiveView ().Render (writer);
		}
	}
}

#endif
