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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//
//

// NOT COMPLETE

using System.ComponentModel;
using System.Collections;

namespace System.Windows.Forms
{
	public abstract class MenuItem : Menu
	{
		internal Menu parent_menu = null;
		internal bool separator;
		internal bool break_;
		internal bool bar_break;
		private	string text;

		public MenuItem (): base (null)
		{
			CommonConstructor ();
		}

		public MenuItem (string s) : base (null)
		{
			CommonConstructor ();
			Text = s;	// Text can change separator status
		}

		public MenuItem (string s, EventHandler e) : base (null)
		{
			CommonConstructor ();
		}

		public MenuItem (string s, MenuItem[] items) : base (items)
		{
			CommonConstructor ();
		}

		public MenuItem (string s, EventHandler e, Shortcut shortcut) : base (null)
		{
			CommonConstructor ();
			throw new NotImplementedException ();
		}

		public MenuItem (MenuMerge mergeType, int mergeOrder, Shortcut shortcut, string text,
			EventHandler onClick, EventHandler onPopup,  EventHandler onSelect,  MenuItem[] items)
			: base (items)
		{
			CommonConstructor ();
			throw new NotImplementedException ();
		}

		private void CommonConstructor ()
		{
			Text = string.Empty;
			separator = false;
			break_ = false;
			bar_break = false;
		}

		#region Public Properties

		public bool BarBreak {
			get { return break_; }
			set { break_ = value; }
		}
		
		public bool Break {
			get { return bar_break; }
			set { bar_break = value; } 
		}
		
		public bool Checked {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}		
		
		public bool DefaultItem {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		public bool Enabled {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}		
		
		public int Index {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		public override bool IsParent {
			get {
				throw new NotImplementedException ();
			}		
		}

		public bool MdiList {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		public MenuItem MdiListItem {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		
		public int MergeOrder{
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		public System.Windows.Forms.MenuMerge MergeType {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		public char Mnemonic {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		public bool OwnerDraw {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		public Menu Parent {
			get { return parent_menu;}
		}
		
		public bool RadioCheck {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		public Shortcut Shortcut {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		public bool ShowShortcut {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}		
		
		public string Text {
			get { return text; }
			set {
				text = value;

				if (text == "-")
					separator = true;
				else
					separator = false;

				//TODO: Force recalc sizes
			}
		}
		
		public bool Visible {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}

		#endregion Public Properties

		#region Private Properties

		internal bool IsPopup {
			get {
				if (menu_items.Count > 0)
					return true;
				else
					return false;
			}			
		}

		internal bool Separator {
			get { return separator; }
			set { separator = value; }
		}

		#endregion Private Properties

		#region Public Methods

		public virtual MenuItem CloneMenu ()
		{
			throw new NotImplementedException ();
		}
		
		protected void CloneMenu (MenuItem menuitem)
		{
			throw new NotImplementedException ();
		}

		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}
		
		public virtual void Dispose ()
		{
			throw new NotImplementedException ();
		}		
		
		public virtual void MergeMenu ()
		{
			throw new NotImplementedException ();
		}
		
		public virtual void MergeMenu (Menu menu)
		{
			throw new NotImplementedException ();
		}
		public void MergeMenu (MenuItem menuteim)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnClick (EventArgs e)
		{

		}

		protected virtual void OnDrawItem (DrawItemEventArgs e)
		{

		}

		
		protected virtual void OnInitMenuPopup (EventArgs e)
		{

		}

		protected virtual void OnMeasureItem (MeasureItemEventArgs e)
		{

		}

		protected virtual void OnPopup (EventArgs e)
		{

		}

		protected virtual void OnSelect (EventArgs e)
		{

		}
		
		public void PerformClick ()
		{
			throw new NotImplementedException ();
		}

		public virtual void PerformSelect ()
		{
			throw new NotImplementedException ();
		}
		
		public override string ToString ()
		{
			return "item:" + text;
		}

		#endregion Public Methods

		#region Private Methods

		internal void Create ()
		{
			MenuAPI.InsertMenuItem (Parent.Handle, -1, true, this);
		}

		#endregion Private Methods

	}
}


