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
//	Jackson Harper (jackson@ximian.com)


using System;
using System.Drawing;
using System.Collections;


namespace System.Windows.Forms {

	public class TabControl : Control {

		private int selected_index = -1;
		private TabAlignment alignment;
		private TabAppearance appearance;
		private TabDrawMode draw_mode;
		private bool multiline;
		private ImageList image_list;
		private Size item_size;
		private Point padding;
		private int row_count = 1;
		private bool hottrack;
		private TabPageCollection tab_pages;
		private bool show_tool_tips;
		private TabSizeMode size_mode;
		private bool redraw;

		public TabControl ()
		{
			tab_pages = new TabPageCollection (this);
		}

		public TabAlignment Alignment {
			get { return alignment; }
			set {
				if (alignment == value)
					return;
				alignment = value;
				if (alignment == TabAlignment.Left || alignment == TabAlignment.Right)
					multiline = true;
				Refresh ();
			}
		}

		public TabAppearance Appearance {
			get { return appearance; }
			set {
				if (appearance == value)
					return;
				appearance = value;
				Refresh ();
			}
		}

		public override Color BackColor {
			get { return base.BackColor; }
			set { /* nothing happens on set on MS */ }
		}

		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		public override Rectangle DisplayRectangle {
			get { return base.DisplayRectangle; }
		}

		public TabDrawMode DrawMode {
			get { return draw_mode; }
			set {
				if (draw_mode == value)
					return;
				draw_mode = value;
				Refresh ();
			}
		}

		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		public bool HotTrack {
			get { return hottrack; }
			set {
				if (hottrack == value)
					return;
				hottrack = value;
				Refresh ();
			}
		}

		public ImageList ImageList {
			get { return image_list; }
			set { image_list = value; }
		}

		public Size ItemSize {
			get {
				if (item_size != Size.Empty)
					return item_size;
				if (!IsHandleCreated)
					return DefaultItemSize;
				return TabSize;
			}
			set {
				if (value.Height < 0 || value.Width < 0)
					throw new ArgumentException ("'" + value + "' is not a valid value for 'ItemSize'.");
				item_size = value;
				Refresh ();
			}
		}

		public bool Multiline {
			get { return multiline; }
			set {
				if (multiline == value)
					return;
				multiline = value;
				if (!multiline && alignment == TabAlignment.Left || alignment == TabAlignment.Right)
					alignment = TabAlignment.Top;
				Refresh ();
			}
		}

		public Point Padding {
			get { return padding; }
			set {
				if (value.X < 0 || value.Y < 0)
					throw new ArgumentException ("'" + value + "' is not a valid value for 'Padding'.");
				if (padding == value)
					return;
				padding = value;
				Refresh ();
			}

		}

		public int RowCount {
			get { return row_count; }
		}

		public int SelectedIndex {
			get { return selected_index; }
			set {
				if (selected_index == value)
					return;
				if (selected_index < -1) {
					throw new ArgumentException ("'" + value + "' is not a valid value for 'value'. " +
							"'value' must be greater than or equal to -1.");
				}
				// OnSelectedIndexChanged (EventArgs.Empty);
				Refresh ();
			}
		}

		public TabPage SelectedTab {
			get {
				if (selected_index == -1)
					return null;
				return tab_pages [selected_index];
			}
			set {
				int index = IndexForTabPage (value);
				if (index == selected_index)
					return;
				selected_index = index;
				Refresh ();
			}
		}

		public bool ShowToolTips {
			get { return show_tool_tips; }
			set {
				if (show_tool_tips == value)
					return;
				show_tool_tips = value;
				Refresh ();
			}
		}

		public TabSizeMode SizeMode {
			get { return size_mode; }
			set {
				if (size_mode == value)
					return;
				size_mode = value;
				Refresh ();
			}
		}

		public int TabCount {
			get {
				return tab_pages.Count;
			}
		}

		public TabPageCollection TabPages {
			get { return tab_pages; }
		}

		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		
		[MonoTODO ("Anything special need to be done?")]
		protected override CreateParams CreateParams {
			get {
				CreateParams c = base.CreateParams;
				// Do we need to do anything here?
				return c;
			}
		}

		protected override Size DefaultSize {
			get { return new Size (200, 100); }  
		}

		[MonoTODO]
		private Size TabSize {
			get {
				return new Size ();
			}
		}

		[MonoTODO]
		private Size DefaultItemSize {
			get {
				return new Size ();
			}
		}

		public new event EventHandler BackColorChanged {
			add { base.BackColorChanged += value; }
			remove { base.BackColorChanged -= value; }
		}

		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		public event EventHandler DrawItem;
		public event EventHandler SelectedIndexChanged;

		public Control GetControl (int index)
		{
			return GetTab (index);
		}

		protected override Control.ControlCollection CreateControlsInstance ()
		{
			return new TabControl.ControlCollection (this);
		}

		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		protected virtual object [] GetItems ()
		{
			TabPage [] pages = new TabPage [Controls.Count];
			Controls.CopyTo (pages, 0);
			return pages;
		}

		protected virtual object [] GetItems (Type type)
		{
			object [] pages = (object []) Array.CreateInstance (type, Controls.Count);
			Controls.CopyTo (pages, 0);
			return pages;
		}

		protected string GetToolTipText (object item)
		{
			TabPage page = (TabPage) item;
			return page.ToolTipText;
		}

		protected override void WndProc (ref Message m)
		{
			switch ((Msg) m.Msg) {
			case Msg.WM_PAINT:
				PaintEventArgs	paint_event;
				paint_event = XplatUI.PaintEventStart (Handle);
				PaintInternal (paint_event);
				XplatUI.PaintEventEnd (Handle);
				break;
			default:
				base.WndProc (ref m);
				break;
			}
		}

		internal void UpdateTabpage (TabPage page)
		{

		}

		internal int IndexForTabPage (TabPage page)
		{
			for (int i = 0; i < tab_pages.Count; i++) {
				if (page == tab_pages [i])
					return i;
			}
			return -1;
		}

		private void PaintInternal (PaintEventArgs pe)
		{
			if (this.Width <= 0 || this.Height <=  0 || this.Visible == false)
				return;

			Draw ();
			pe.Graphics.DrawImage (this.ImageBuffer, pe.ClipRectangle, pe.ClipRectangle, GraphicsUnit.Pixel);

			// On MS the Paint event never seems to be raised
		}

		private void Redraw (bool recalculate)
		{
			if (recalculate) {
				
			}
			redraw = true;
			Refresh ();
		}

		private void Draw ()
		{
			if (redraw) {

			}
			redraw = false;
		}

		private TabPage GetTab (int index)
		{
			return Controls [index] as TabPage;
		}

		private void SetTab (int index, TabPage value)
		{
			((IList) Controls).Insert (index, value);
			Refresh ();
		}

		public class ControlCollection : System.Windows.Forms.Control.ControlCollection {

			public ControlCollection (TabControl owner) : base (owner)
			{
			}

			public override void Add (Control value)
			{
				if (!(value is TabPage))
					throw new ArgumentException ("Cannot add " +
						value.GetType ().Name + " to TabControl. " +
						"Only TabPages can be directly added to TabControls.");
				base.Add (value);
			}
		}

		public class TabPageCollection	: IList, ICollection, IEnumerable {

			private TabControl owner;
			private IList controls;

			public TabPageCollection (TabControl owner)
			{
				if (owner == null)
					throw new ArgumentNullException ("Value cannot be null.");
				this.owner = owner;
				controls = owner.Controls;
			}

			public virtual int Count {
				get { return owner.Controls.Count; }
			}

			public virtual bool IsReadOnly {
				get { return false; }
			}

			public virtual TabPage this [int index] {
				get {
					return owner.GetTab (index);
				}
				set {
					owner.SetTab (index, value);
				}    
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return false; }
			}

			object IList.this [int index] {
				get {
					return owner.GetTab (index);
				}
				set {
					owner.SetTab (index, (TabPage) value);
				}
			}

			public void Add (TabPage page)
			{
				if (page == null)
					throw new ArgumentNullException ("Value cannot be null.");
				owner.Controls.Add (page);
			}

			public void AddRange (TabPage [] pages)
			{
				if (pages == null)
					throw new ArgumentNullException ("Value cannot be null.");
				owner.Controls.AddRange (pages);
			}

			public virtual void Clear ()
			{
				owner.Controls.Clear ();
			}

			public bool Contains (TabPage page)
			{
				if (page == null)
					throw new ArgumentNullException ("Value cannot be null.");
				return owner.Controls.Contains (page);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return owner.Controls.GetEnumerator ();
			}

			public int IndexOf (TabPage page)
			{
				return owner.Controls.IndexOf (page);
			}

			public void Remove (TabPage page)
			{
				owner.Controls.Remove (page);
			}

			public virtual void RemoveAt (int index)
			{
				owner.Controls.RemoveAt (index);
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				owner.Controls.CopyTo (dest, index);
			}

			int IList.Add (object value)
			{
				//	 return owner.Controls.Add ((TabPage) value);
				return -1;
			}

			bool IList.Contains (object page)
			{
				return Contains ((TabPage) page);
			}

			int IList.IndexOf (object page)
			{
				return IndexOf ((TabPage) page);
			}

			void IList.Insert (int index, object value)
			{
				controls.Insert (index, (TabPage) value);
			}

			void IList.Remove (object value)
			{
				Remove ((TabPage) value);
			}
		}
	}
}


