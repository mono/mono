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
		private Size item_size = Size.Empty;
		private Point padding;
		private int row_count = 1;
		private bool hottrack;
		private TabPageCollection tab_pages;
		private bool show_tool_tips;
		private TabSizeMode size_mode;
		private bool redraw;
		private Rectangle display_rect;
		private bool show_slider = false;
		private ButtonState right_slider_state;
		private ButtonState left_slider_state;
		private int slider_pos = 0;
		
		public TabControl ()
		{
			tab_pages = new TabPageCollection (this);
			SetStyle (ControlStyles.UserPaint, true);
			padding = ThemeEngine.Current.TabControlDefaultPadding;
			item_size = ThemeEngine.Current.TabControlDefaultItemSize;

			MouseDown += new MouseEventHandler (MouseDownHandler);
			MouseUp += new MouseEventHandler (MouseUpHandler);
			SizeChanged += new EventHandler (SizeChangedHandler);
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
			get {
				return ThemeEngine.Current.GetTabControlDisplayRectangle (this);
			}
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
				return item_size;
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

				SuspendLayout ();
				if (selected_index != -1)
					Controls [selected_index].Visible = false;
				selected_index = value;
				if (selected_index != -1) 
					Controls [selected_index].Visible = true;
				ResumeLayout ();

				if (SelectedIndex != -1 && TabPages [SelectedIndex].Row != BottomRow) 
					DropRow (TabPages [selected_index].Row);
				SizeTabs ();
				
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

		internal bool ShowSlider {
			get { return show_slider; }
			set { show_slider = value; }
		}

		internal ButtonState RightSliderState {
			get { return right_slider_state; }
		}

		internal ButtonState LeftSliderState {
			get { return left_slider_state; }
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

		private Size DefaultItemSize {
			get {
				return ThemeEngine.Current.TabControlDefaultItemSize;
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

		public event DrawItemEventHandler DrawItem;
		public event EventHandler SelectedIndexChanged;

		public Rectangle GetTabRect (int index)
		{
			TabPage page = GetTab (index);
			return page.TabBounds;
		}

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
			ResizeTabPages ();
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

		private bool CanScrollRight {
			get { return slider_pos != 0; }
		}

		private bool CanScrollLeft {
			get {
				if (TabPages [TabCount - 1].TabBounds.Right > ClientRectangle.Right - 40)
					return true;
				return false;
			}
		}

		private void MouseDownHandler (object sender, MouseEventArgs e)
		{
			if (ShowSlider) {
				Rectangle right = ThemeEngine.Current.GetTabControlRightScrollRect (this);
				Rectangle left = ThemeEngine.Current.GetTabControlLeftScrollRect (this);
				if (right.Contains (e.X, e.Y)) {
					right_slider_state = ButtonState.Pushed;
					if (CanScrollRight) {
						slider_pos++;
						SizeTabs ();
					}
					Refresh ();
					return;
				} else if (left.Contains (e.X, e.Y)) {
					left_slider_state = ButtonState.Pushed;
					if (CanScrollLeft) {
						slider_pos--;
						SizeTabs ();
					}
					Refresh ();
					return;
				}

			}

			int count = Controls.Count;
			for (int i = 0; i<count; i++) {
				if (!GetTabRect (i).Contains (e.X, e.Y))
					continue;
				SelectedIndex = i;
				break;
			}
		}

		private void MouseUpHandler (object sender, MouseEventArgs e)
		{
			left_slider_state = ButtonState.Normal;
			right_slider_state = ButtonState.Normal;
			Refresh ();
		}

		private void SizeChangedHandler (object sender, EventArgs e)
		{
			ResizeTabPages ();
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

		private void ResizeTabPages ()
		{
			CalcTabRows ();
			SizeTabs ();
			Rectangle r = DisplayRectangle;
			foreach (TabPage page in Controls) {
				page.Bounds = r;
			}
		}

		private int MinimumTabWidth {
			get {
				return ThemeEngine.Current.TabControlMinimumTabWidth;
			}
		}

		private Size TabSpacing {
			get {
				return ThemeEngine.Current.TabControlGetSpacing (this);
			}
		}

		private void CalcTabRows ()
		{
			switch (Alignment) {
			case TabAlignment.Right:
			case TabAlignment.Left:
				CalcTabRows (Height);
				break;
			default:
				CalcTabRows (Width);
				break;
			}
		}

		private void CalcTabRows (int row_width)
		{
			int xpos = 4;
			Size spacing = TabSpacing;

			row_count = 1;
			show_slider = false;
			
			for (int i = 0; i < TabPages.Count; i++) {
				TabPage page = TabPages [i];
				int width;

				page.Row = 1;

				if (SizeMode == TabSizeMode.Fixed) {
					width = item_size.Width;
				} else {
					width = (int) DeviceContext.MeasureString (page.Text, Font).Width + (Padding.X * 2);
				}

				if (i == SelectedIndex)
					width += 8;
				if (width < MinimumTabWidth)
					width = MinimumTabWidth;

				if (xpos + width > row_width && multiline) {
					xpos = 4;
					for (int j = 0; j < i; j++) {
						TabPages [j].Row++;
					}
					row_count++;
				} else if (xpos + width > row_width) {
					show_slider = true;
				}

				xpos += width + 1 + spacing.Width;
			}

			if (SelectedIndex != -1 && TabPages [SelectedIndex].Row != BottomRow)
				DropRow (TabPages [SelectedIndex].Row);
		}

		private int BottomRow {
			get {
				switch (Alignment) {
				case TabAlignment.Right:
				case TabAlignment.Bottom:
					return row_count;
				default:
					return 1;
				}
			}
		}

		private int Direction
		{
			get {
				switch (Alignment) {
				case TabAlignment.Right:
				case TabAlignment.Bottom:
					return -1;
				default:
					return 1;
				}
			}
		}

		private void DropRow (int row)
		{
			int bottom = BottomRow;
			int direction = Direction;

			foreach (TabPage page in TabPages) {
				if (page.Row == row) {
					page.Row = bottom;
				} else if (direction == 1 && page.Row < row) {
					page.Row += direction;
				} else if (direction == -1 && page.Row > row) {
					page.Row += direction;
				}
			}
		}

		private int CalcYPos ()
		{
			if (Alignment == TabAlignment.Bottom) {
				Rectangle r = ThemeEngine.Current.GetTabControlDisplayRectangle (this);
				return r.Bottom + 3;
			}
			return 1;
		}

		private int CalcXPos ()
		{
			if (Alignment == TabAlignment.Right) {
				Rectangle r = ThemeEngine.Current.GetTabControlDisplayRectangle (this);
				return r.Right + 4;
			}
			return 4;

		}

		private void SizeTabs ()
		{
			switch (Alignment) {
			case TabAlignment.Right:
			case TabAlignment.Left:
				SizeTabsV (Height);
				break;
			default:
				SizeTabs (Width);
				break;
			}
		}

		private void SizeTabsV (int row_width)
		{
			int ypos = 1;
			int prev_row = 1;
			Size spacing = TabSpacing;
			int size = item_size.Height + 2 + spacing.Width;
			int xpos = CalcXPos ();

			if (TabPages.Count == 0)
				return;

			prev_row = TabPages [0].Row;

			for (int i = 0; i < TabPages.Count; i++) {
				TabPage page = TabPages [i];
				int width;

				if (SizeMode == TabSizeMode.Fixed) {
					width = item_size.Width;
				} else {
					width = (int) DeviceContext.MeasureString (page.Text, Font).Width + (Padding.X * 2);
				}

				if (width < MinimumTabWidth)
					width = MinimumTabWidth;
				if (page.Row != prev_row)
					ypos = 1;

				page.TabBounds = new Rectangle (xpos + (row_count - page.Row) * ((item_size.Height - 2) + spacing.Width),
						ypos, item_size.Height - 2, width);

				ypos += width + spacing.Width;
				prev_row = page.Row;
			}

			if (SelectedIndex != -1) {
				TabPage page = TabPages [SelectedIndex];
				ExpandSelected (TabPages [SelectedIndex], 1, row_width - 1);
			}
		}

		private void SizeTabs (int row_width)
		{
			int ypos = CalcYPos ();
			int prev_row = 1;
			Size spacing = TabSpacing;
			int size = item_size.Width + 2 + (spacing.Width * 2);
			int xpos = 4 + (slider_pos * size);
			int begin_prev = 0;

			if (TabPages.Count == 0)
				return;

			prev_row = TabPages [0].Row;

			for (int i = 0; i < TabPages.Count; i++) {
				TabPage page = TabPages [i];
				int width;

				if (SizeMode == TabSizeMode.Fixed) {
					width = item_size.Width;
				} else {
					width = (int) DeviceContext.MeasureString (page.Text, Font).Width + (Padding.X * 2);
				}

				if (width < MinimumTabWidth)
					width = MinimumTabWidth;
				if (page.Row != prev_row)
					xpos = 4 + (slider_pos * size);

				page.TabBounds = new Rectangle (xpos,
						ypos + (row_count - page.Row) * (item_size.Height + spacing.Height),
						width, item_size.Height);

				
				if (page.Row != prev_row) {
					if (SizeMode == TabSizeMode.FillToRight) {
						FillRow (begin_prev, i - 1, ((row_width - TabPages [i - 1].TabBounds.Right) / (i - begin_prev)), spacing);
					}
					begin_prev = i;
				}

				xpos += width + 1 + spacing.Width;
				prev_row = page.Row;				    
			}

			if (SizeMode == TabSizeMode.FillToRight) {
				FillRow (begin_prev, TabPages.Count - 1,
						((row_width - TabPages [TabPages.Count - 1].TabBounds.Right) / (TabPages.Count - begin_prev)), spacing);
			}

			if (SelectedIndex != -1) {
				TabPage page = TabPages [SelectedIndex];
				ExpandSelected (TabPages [SelectedIndex], 2, row_width - 1);
			}
		}

		private void FillRow (int start, int end, int amount, Size spacing)
		{
			int xpos = TabPages [start].TabBounds.Left;
			for (int i = start; i <= end; i++) {
				TabPage page = TabPages [i];
				int left = xpos;
				int width = (i == end ? Width - left - 3 : page.TabBounds.Width + amount);

				page.TabBounds = new Rectangle (left, page.TabBounds.Top,
						width, page.TabBounds.Height);
				xpos = page.TabBounds.Right + 1 + spacing.Width;
			}
		}

		private void ExpandSelected (TabPage page, int left_edge, int right_edge)
		{
			if (Appearance != TabAppearance.Normal)
				return;

			if (Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom) {
				int l = page.TabBounds.Left - 4;
				int r = page.TabBounds.Right + 4;
				int y = page.TabBounds.Y;
				int h = page.TabBounds.Height + 2;

				if (l < left_edge)
					l = left_edge;
				if (r > right_edge && SizeMode != TabSizeMode.Normal)
					r = right_edge;
				if (Alignment == TabAlignment.Top)
					y -= 1;
				if (Alignment == TabAlignment.Bottom)
					y -= 2;

				page.TabBounds = new Rectangle (l, y, r - l, h);
			} else {
				int l = page.TabBounds.Left - 3;
				int r = page.TabBounds.Right + 3;
				int t = page.TabBounds.Top - 3;
				int b = page.TabBounds.Bottom + 3;

				if (t < left_edge)
					t = left_edge;
				if (b > right_edge)
					b = right_edge;

				page.TabBounds = new Rectangle (l, t, r - l, b - t);
			}
		}

		private void PaintInternal (PaintEventArgs pe)
		{
			if (this.Width <= 0 || this.Height <=  0 || this.Visible == false)
				return;

			Draw ();
			pe.Graphics.DrawImageUnscaled (ImageBuffer, 0, 0);
			ImageBuffer.Save ("ImageBuffer.bmp");
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
			ThemeEngine.Current.DrawTabControl (DeviceContext, ClientRectangle, this);
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

			private TabControl owner;
			private ArrayList list = new ArrayList ();

			public ControlCollection (TabControl owner) : base (owner)
			{
				this.owner = owner;
			}

			public override void Add (Control value)
			{
				if (!(value is TabPage))
					throw new ArgumentException ("Cannot add " +
						value.GetType ().Name + " to TabControl. " +
						"Only TabPages can be directly added to TabControls.");

				value.Visible = false;
				base.Add (value);
				if (Count == 1) {
					owner.SelectedIndex = 0;
				} else {
					// Setting the selected index will calc the tab rows so
					// we don't need to do it again
					owner.CalcTabRows ();
				}
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


