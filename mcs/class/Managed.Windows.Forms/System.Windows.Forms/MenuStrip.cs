//
// MenuStrip.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class MenuStrip : ToolStrip
	{
		private ToolStripMenuItem mdi_window_list_item;

		public MenuStrip () : base ()
		{
			base.CanOverflow = false;
			this.GripStyle = ToolStripGripStyle.Hidden;
			this.Stretch = true;
			this.Dock = DockStyle.Top;
		}

		#region Public Properties
		[DefaultValue (false)]
		[Browsable (false)]
		public new bool CanOverflow {
			get { return base.CanOverflow; }
			set { base.CanOverflow = value; }
		}

		[DefaultValue (ToolStripGripStyle.Hidden)]
		public new ToolStripGripStyle GripStyle {
			get { return base.GripStyle; }
			set { base.GripStyle = value; }
		}

		[DefaultValue (null)]
		[MergableProperty (false)]
		[TypeConverter (typeof (MdiWindowListItemConverter))]
		public ToolStripMenuItem MdiWindowListItem {
			get { return this.mdi_window_list_item; }
			set { 
				if (this.mdi_window_list_item != value) {
					this.mdi_window_list_item = value;
					this.RefreshMdiItems ();
				}
			}
		}
		
		[DefaultValue (false)]
		public new bool ShowItemToolTips {
			get { return base.ShowItemToolTips; }
			set { base.ShowItemToolTips = value; }
		}

		[DefaultValue (true)]
		public new bool Stretch {
			get { return base.Stretch; }
			set { base.Stretch = value; }
		}
		#endregion

		#region Protected Properties
		protected override Padding DefaultGripMargin { get { return new Padding (2, 2, 0, 2); } }
		protected override Padding DefaultPadding { get { return new Padding (6, 2, 0, 2); } }
		protected override bool DefaultShowItemToolTips { get { return false; } }
		protected override Size DefaultSize { get { return new Size (200, 24); } }
		#endregion

		#region Protected Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new MenuStripAccessibleObject ();
		}
		
		protected internal override ToolStripItem CreateDefaultItem (string text, Image image, EventHandler onClick)
		{
			return new ToolStripMenuItem (text, image, onClick);
		}

		protected virtual void OnMenuActivate (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [MenuActivateEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnMenuDeactivate (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [MenuDeactivateEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override bool ProcessCmdKey (ref Message m, Keys keyData)
		{
			return base.ProcessCmdKey (ref m, keyData);
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}
		#endregion

		#region Public Events
		static object MenuActivateEvent = new object ();
		static object MenuDeactivateEvent = new object ();

		public event EventHandler MenuActivate {
			add { Events.AddHandler (MenuActivateEvent, value); }
			remove { Events.RemoveHandler (MenuActivateEvent, value); }
		}

		public event EventHandler MenuDeactivate {
			add { Events.AddHandler (MenuDeactivateEvent, value); }
			remove { Events.RemoveHandler (MenuDeactivateEvent, value); }
		}
		#endregion

		#region Internal Properties
		internal override bool KeyboardActive {
			get { return base.KeyboardActive; }
			set {
				if (base.KeyboardActive != value) {
					base.KeyboardActive = value;
					
					if (value)
						this.OnMenuActivate (EventArgs.Empty);
					else
						this.OnMenuDeactivate (EventArgs.Empty);
				}
			}
		}
		
		internal bool MenuDroppedDown {
			get { return this.menu_selected; }
			set { this.menu_selected = value; }
		}
		#endregion
		
		#region Internal Methods
		internal override void Dismiss (ToolStripDropDownCloseReason reason)
		{
			// Make sure we don't auto-dropdown next time we're activated
			this.MenuDroppedDown = false;
			
			base.Dismiss (reason);
		}
		
		internal void FireMenuActivate ()
		{
			// The tracker lets us know when the form is clicked or loses focus
			ToolStripManager.AppClicked += new EventHandler (ToolStripMenuTracker_AppClicked);
			ToolStripManager.AppFocusChange += new EventHandler (ToolStripMenuTracker_AppFocusChange);
			
			this.OnMenuActivate (EventArgs.Empty);
		}

		internal void FireMenuDeactivate ()
		{
			// Detach from the tracker
			ToolStripManager.AppClicked -= new EventHandler (ToolStripMenuTracker_AppClicked); ;
			ToolStripManager.AppFocusChange -= new EventHandler (ToolStripMenuTracker_AppFocusChange);
		
			this.OnMenuDeactivate (EventArgs.Empty);
		}

		internal override bool OnMenuKey ()
		{
			// Set ourselves active and select our first item
			ToolStripManager.SetActiveToolStrip (this, true);
			ToolStripItem tsi = this.SelectNextToolStripItem (null, true);
			
			if (tsi == null)
				return false;
				
			if (tsi is MdiControlStrip.SystemMenuItem)
				this.SelectNextToolStripItem (tsi, true);
				
			return true;
		}
		
		private void ToolStripMenuTracker_AppFocusChange (object sender, EventArgs e)
		{
			this.GetTopLevelToolStrip ().Dismiss (ToolStripDropDownCloseReason.AppFocusChange);
		}

		private void ToolStripMenuTracker_AppClicked (object sender, EventArgs e)
		{
			this.GetTopLevelToolStrip ().Dismiss (ToolStripDropDownCloseReason.AppClicked);
		}
		
		internal void RefreshMdiItems ()
		{
			if (this.mdi_window_list_item == null)
				return;
			
			Form parent_form = this.FindForm ();
			
			if (parent_form == null || parent_form.MainMenuStrip != this)
				return;
				
			MdiClient mdi = parent_form.MdiContainer;
			
			// If there isn't a MdiContainer, we don't need to worry about MdiItems  :)
			if (mdi == null)
				return;
				
			// Make a copy so we can delete from the real one
			ToolStripItem[] loopitems = new ToolStripItem[this.mdi_window_list_item.DropDownItems.Count];
			this.mdi_window_list_item.DropDownItems.CopyTo (loopitems, 0);

			// If the mdi child has been removed, remove our menu item
			foreach (ToolStripItem tsi in loopitems)
				if (tsi is ToolStripMenuItem && (tsi as ToolStripMenuItem).IsMdiWindowListEntry)
					if (!mdi.mdi_child_list.Contains ((tsi as ToolStripMenuItem).MdiClientForm) || !(tsi as ToolStripMenuItem).MdiClientForm.Visible)
						this.mdi_window_list_item.DropDownItems.Remove (tsi);

			// Add the new forms and update state
			for (int i = 0; i < mdi.mdi_child_list.Count; i++) {
				Form mdichild = (Form)mdi.mdi_child_list[i];
				ToolStripMenuItem tsi;
				
				if (!mdichild.Visible)
					continue;
					
				if ((tsi = FindMdiMenuItemOfForm (mdichild)) == null) {
					if (CountMdiMenuItems () == 0 && this.mdi_window_list_item.DropDownItems.Count > 0 && !(this.mdi_window_list_item.DropDownItems[this.mdi_window_list_item.DropDownItems.Count - 1] is ToolStripSeparator))
						this.mdi_window_list_item.DropDownItems.Add (new ToolStripSeparator ());
						
					tsi = new ToolStripMenuItem ();
					tsi.MdiClientForm = mdichild;
					this.mdi_window_list_item.DropDownItems.Add (tsi);
				}
				
				tsi.Text = string.Format ("&{0} {1}", i + 1, mdichild.Text);
				tsi.Checked = parent_form.ActiveMdiChild == mdichild;
			}
			
			// Check that everything is in the correct order
			if (NeedToReorderMdi ())
				ReorderMdiMenu ();
		}
		
		private ToolStripMenuItem FindMdiMenuItemOfForm (Form f)
		{
			// Not terribly efficient, but Mdi window lists shouldn't get too big
			foreach (ToolStripItem tsi in this.mdi_window_list_item.DropDownItems)
				if (tsi is ToolStripMenuItem && (tsi as ToolStripMenuItem).MdiClientForm == f)
					return (ToolStripMenuItem)tsi;
					
			return null;
		}

		private int CountMdiMenuItems ()
		{
			int count = 0;
			
			foreach (ToolStripItem tsi in this.mdi_window_list_item.DropDownItems)
				if (tsi is ToolStripMenuItem && (tsi as ToolStripMenuItem).IsMdiWindowListEntry)
					count++;
					
			return count;
		}
		
		private bool NeedToReorderMdi ()
		{
			// Mdi menus must be: User Items, Separator, Mdi Items
			bool seenMdi = false;
			
			foreach (ToolStripItem tsi in this.mdi_window_list_item.DropDownItems) {
				if (tsi is ToolStripMenuItem) {
					if (!(tsi as ToolStripMenuItem).IsMdiWindowListEntry) {
						if (seenMdi)
							return true;
					} else 
						seenMdi = true;
				}
			}
			
			return false;
		}

		private void ReorderMdiMenu ()
		{
			ToolStripItem[] loopitems = new ToolStripItem[this.mdi_window_list_item.DropDownItems.Count];
			this.mdi_window_list_item.DropDownItems.CopyTo (loopitems, 0);

			this.mdi_window_list_item.DropDownItems.Clear ();

			foreach (ToolStripItem tsi in loopitems)
				if (tsi is ToolStripSeparator || !(tsi as ToolStripMenuItem).IsMdiWindowListEntry)
					this.mdi_window_list_item.DropDownItems.Add (tsi);
	
			int count = this.mdi_window_list_item.DropDownItems.Count;
			
			if (count > 0 && !(this.mdi_window_list_item.DropDownItems[count - 1] is ToolStripSeparator))
				this.mdi_window_list_item.DropDownItems.Add (new ToolStripSeparator ());

			foreach (ToolStripItem tsi in loopitems)
				if (tsi is ToolStripMenuItem && (tsi as ToolStripMenuItem).IsMdiWindowListEntry)
					this.mdi_window_list_item.DropDownItems.Add (tsi);
		}
		#endregion

		#region MenuStripAccessibleObject
		private class MenuStripAccessibleObject : AccessibleObject
		{
		}
		#endregion

	}
	
	#region MdiWindowListItemConverter
	internal class MdiWindowListItemConverter : TypeConverter
	{
	}
	#endregion
}
