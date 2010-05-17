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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)


using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms.Theming;
using System.Windows.Forms.VisualStyles;

namespace System.Windows.Forms {
#if NET_2_0
	[ComVisibleAttribute (true)]
	[ClassInterfaceAttribute (ClassInterfaceType.AutoDispatch)]
#endif
	[DefaultEvent("SelectedIndexChanged")]
	[DefaultProperty("TabPages")]
	[Designer("System.Windows.Forms.Design.TabControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class TabControl : Control {
		#region Fields
		private int selected_index = -1;
		private TabAlignment alignment;
		private TabAppearance appearance;
		private TabDrawMode draw_mode;
		private bool multiline;
		private ImageList image_list;
		private Size item_size = Size.Empty;
		private bool item_size_manual;
		private Point padding;
		private int row_count = 0;
		private bool hottrack;
		private TabPageCollection tab_pages;
		private bool show_tool_tips;
		private TabSizeMode size_mode;
		private bool show_slider = false;
		private PushButtonState right_slider_state = PushButtonState.Normal;
		private PushButtonState left_slider_state = PushButtonState.Normal;
		private int slider_pos = 0;
		TabPage entered_tab_page;
		bool mouse_down_on_a_tab_page;
#if NET_2_0
		private bool rightToLeftLayout;
#endif		
		#endregion	// Fields

		#region UIA Framework Events
#if NET_2_0
		static object UIAHorizontallyScrollableChangedEvent = new object ();

		internal event EventHandler UIAHorizontallyScrollableChanged {
			add { Events.AddHandler (UIAHorizontallyScrollableChangedEvent, value); }
			remove { Events.RemoveHandler (UIAHorizontallyScrollableChangedEvent, value); }
		}

		internal void OnUIAHorizontallyScrollableChanged (EventArgs e)
		{
			EventHandler eh
				= (EventHandler) Events [UIAHorizontallyScrollableChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		static object UIAHorizontallyScrolledEvent = new object ();

		internal event EventHandler UIAHorizontallyScrolled {
			add { Events.AddHandler (UIAHorizontallyScrolledEvent, value); }
			remove { Events.RemoveHandler (UIAHorizontallyScrolledEvent, value); }
		}

		internal void OnUIAHorizontallyScrolled (EventArgs e)
		{
			EventHandler eh
				= (EventHandler) Events [UIAHorizontallyScrolledEvent];
			if (eh != null)
				eh (this, e);
		}
#endif
		#endregion

		#region UIA Framework Property
#if NET_2_0
		internal double UIAHorizontalViewSize {
			get { return LeftScrollButtonArea.Left * 100 / TabPages [TabCount - 1].TabBounds.Right; }
		}
#endif
		#endregion

		#region Public Constructors
		public TabControl ()
		{
			tab_pages = new TabPageCollection (this);
			SetStyle (ControlStyles.UserPaint, false);
			padding = ThemeEngine.Current.TabControlDefaultPadding;

			MouseDown += new MouseEventHandler (MouseDownHandler);
			MouseLeave += new EventHandler (OnMouseLeave);
			MouseMove += new MouseEventHandler (OnMouseMove);
			MouseUp += new MouseEventHandler (MouseUpHandler);
			SizeChanged += new EventHandler (SizeChangedHandler);
		}

		#endregion	// Public Constructors

		#region Public Instance Properties
		[DefaultValue(TabAlignment.Top)]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.All)]
		public TabAlignment Alignment {
			get { return alignment; }
			set {
				if (alignment == value)
					return;
				alignment = value;
				if (alignment == TabAlignment.Left || alignment == TabAlignment.Right)
					multiline = true;
				Redraw ();
			}
		}

		[DefaultValue(TabAppearance.Normal)]
		[Localizable(true)]
		public TabAppearance Appearance {
			get { return appearance; }
			set {
				if (appearance == value)
					return;
				appearance = value;
				Redraw ();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color BackColor {
			get { return ThemeEngine.Current.ColorControl; }
			set { /* nothing happens on set on MS */ }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}
#endif
		
		public override Rectangle DisplayRectangle {
			get {
				return ThemeEngine.Current.TabControlGetDisplayRectangle (this);
			}
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override bool DoubleBuffered {
			get { return base.DoubleBuffered; }
			set { base.DoubleBuffered = value; }
		}
#endif

		[DefaultValue(TabDrawMode.Normal)]
		public TabDrawMode DrawMode {
			get { return draw_mode; }
			set {
				if (draw_mode == value)
					return;
				draw_mode = value;
				Redraw ();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		[DefaultValue(false)]
		public bool HotTrack {
			get { return hottrack; }
			set {
				if (hottrack == value)
					return;
				hottrack = value;
				Redraw ();
			}
		}

#if NET_2_0
		[RefreshProperties (RefreshProperties.Repaint)]
#endif
		[DefaultValue(null)]
		public ImageList ImageList {
			get { return image_list; }
			set { 
				image_list = value; 
				Redraw ();
			}
		}

		[Localizable(true)]
		public Size ItemSize {
			get {
				if (item_size_manual)
					return item_size;

				if (!IsHandleCreated)
					return Size.Empty;

				Size size = item_size;
				if (SizeMode != TabSizeMode.Fixed) {
					size.Width += padding.X * 2;
					size.Height += padding.Y * 2;
				}

				if (tab_pages.Count == 0)
					size.Width = 0;

				return size;
			}
			set {
				if (value.Height < 0 || value.Width < 0)
					throw new ArgumentException ("'" + value + "' is not a valid value for 'ItemSize'.");
				item_size = value;
				item_size_manual = true;
				Redraw ();
			}
		}

		[DefaultValue(false)]
		public bool Multiline {
			get { return multiline; }
			set {
				if (multiline == value)
					return;
				multiline = value;
				if (!multiline && alignment == TabAlignment.Left || alignment == TabAlignment.Right)
					alignment = TabAlignment.Top;
				Redraw ();
			}
		}

		[Localizable(true)]
		public
#if NET_2_0
		new
#endif
		Point Padding {
			get { return padding; }
			set {
				if (value.X < 0 || value.Y < 0)
					throw new ArgumentException ("'" + value + "' is not a valid value for 'Padding'.");
				if (padding == value)
					return;
				padding = value;
				Redraw ();
			}

		}

#if NET_2_0
		[MonoTODO ("RTL not supported")]
		[Localizable (true)]
		[DefaultValue (false)]
		public virtual bool RightToLeftLayout {
			get { return this.rightToLeftLayout; }
			set {
				if (value != this.rightToLeftLayout) {
					this.rightToLeftLayout = value;
					this.OnRightToLeftLayoutChanged (EventArgs.Empty);
				}
			}
		}
#endif

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int RowCount {
			get { return row_count; }
		}

		[DefaultValue(-1)]
		[Browsable(false)]
		public int SelectedIndex {
			get { return selected_index; }
			set {

				if (value < -1) {
#if NET_2_0
					throw new ArgumentOutOfRangeException ("SelectedIndex", "Value of '" + value + "' is valid for 'SelectedIndex'. " +
						"'SelectedIndex' must be greater than or equal to -1.");
#else
					throw new ArgumentException ("'" + value + "' is not a valid value for 'value'. " +
						"'value' must be greater than or equal to -1.");
#endif
				}
				if (!this.IsHandleCreated) {
					if (selected_index != value) {
						selected_index = value;
#if !NET_2_0
						OnSelectedIndexChanged (EventArgs.Empty);
#endif
					}
					return;
				}

				if (value >= TabCount) {
					if (value != selected_index)
						OnSelectedIndexChanged (EventArgs.Empty);
					return;
				}

				if (value == selected_index) {
					if (selected_index > -1)
						Invalidate(GetTabRect (selected_index));
					return;
				}

#if NET_2_0
				TabControlCancelEventArgs ret = new TabControlCancelEventArgs (SelectedTab, selected_index, false, TabControlAction.Deselecting);
				OnDeselecting (ret);
				if (ret.Cancel)
					return;

#endif
				Focus ();
				int old_index = selected_index;
				int new_index = value;

				selected_index = new_index;

#if NET_2_0
				ret = new TabControlCancelEventArgs (SelectedTab, selected_index, false, TabControlAction.Selecting);
				OnSelecting (ret);
				if (ret.Cancel) {
					selected_index = old_index;
					return;
				}
#endif

				SuspendLayout ();

				Rectangle invalid = Rectangle.Empty;
				bool refresh = false;

				if (new_index != -1 && show_slider && new_index < slider_pos) {
					slider_pos = new_index;
					refresh = true;
				}

				if (new_index != -1) {
					int le = TabPages[new_index].TabBounds.Right;
					int re = LeftScrollButtonArea.Left;
					if (show_slider && le > re) {
						int i = 0;
						for (i = 0; i < new_index - 1; i++) {
							if (TabPages [i].TabBounds.Left < 0) // tab scrolled off the visible area, ignore
								continue;

							if (TabPages [new_index].TabBounds.Right - TabPages[i].TabBounds.Right < re) {
								i++;
								break;
							}
						}
						slider_pos = i;
						refresh = true;
					}
				}

				if (old_index != -1 && new_index != -1) {
					if (!refresh)
						invalid = GetTabRect (old_index);
					((TabPage) Controls[old_index]).SetVisible (false);
				}

				TabPage selected = null;

				if (new_index != -1) {
					selected = (TabPage) Controls[new_index];
					invalid = Rectangle.Union (invalid, GetTabRect (new_index));
					selected.SetVisible (true);
				}

				OnSelectedIndexChanged (EventArgs.Empty);

				ResumeLayout ();

				if (refresh) {
					SizeTabs ();
					Refresh ();
				} else if (new_index != -1 && selected.Row != BottomRow) {
					DropRow (TabPages[new_index].Row);
					// calculating what to invalidate here seems to be slower then just
					// refreshing the whole thing
					SizeTabs ();
					Refresh ();
				} else {
					SizeTabs ();
					// The lines are drawn on the edges of the tabs so the invalid area should
					// needs to include the extra pixels of line width (but should not
					// overflow the control bounds).
					if (appearance == TabAppearance.Normal) {
						invalid.Inflate (6, 4);
						invalid.Intersect (ClientRectangle);
					}
					Invalidate (invalid);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
				SelectedIndex = index;
			}
		}

		[DefaultValue(false)]
		[Localizable(true)]
		public bool ShowToolTips {
			get { return show_tool_tips; }
			set {
				if (show_tool_tips == value)
					return;
				show_tool_tips = value;
				Redraw ();
			}
		}

		[DefaultValue(TabSizeMode.Normal)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public TabSizeMode SizeMode {
			get { return size_mode; }
			set {
				if (size_mode == value)
					return;
				size_mode = value;
				Redraw ();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int TabCount {
			get {
				return tab_pages.Count;
			}
		}

#if NET_2_0
		[Editor ("System.Windows.Forms.Design.TabPageCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
#else
		[DefaultValue(null)]
#endif
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MergableProperty(false)]
		public TabPageCollection TabPages {
			get { return tab_pages; }
		}

		[Browsable(false)]
		[Bindable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}
		#endregion	// Public Instance Properties

		#region Internal Properties
		internal bool ShowSlider {
			get { return show_slider; }
			set {
				show_slider = value;

#if NET_2_0
				// UIA Framework Event: HorizontallyScrollable Changed
				OnUIAHorizontallyScrollableChanged (EventArgs.Empty);
#endif
			}
		}

		internal int SliderPos {
			get { return slider_pos; }
		}

		internal PushButtonState RightSliderState {
			get { return right_slider_state; }
			private set {
				if (right_slider_state == value)
					return;
				PushButtonState old_value = right_slider_state;
				right_slider_state = value;
				if (NeedsToInvalidateScrollButton (old_value, value))
					Invalidate (RightScrollButtonArea);
			}
		}

		internal PushButtonState LeftSliderState {
			get { return left_slider_state; }
			set {
				if (left_slider_state == value)
					return;
				PushButtonState old_value = left_slider_state;
				left_slider_state = value;
				if (NeedsToInvalidateScrollButton (old_value, value))
					Invalidate (LeftScrollButtonArea);
			}
		}

		bool NeedsToInvalidateScrollButton (PushButtonState oldState, PushButtonState newState)
		{
			if ((oldState == PushButtonState.Hot && newState == PushButtonState.Normal) ||
				(oldState == PushButtonState.Normal && newState == PushButtonState.Hot))
				return HasHotElementStyles;
			return true;
		}

		internal TabPage EnteredTabPage {
			get { return entered_tab_page; }
			private set {
				if (entered_tab_page == value)
					return;
				if (HasHotElementStyles) {
					Region area_to_invalidate = new Region ();
					area_to_invalidate.MakeEmpty ();
					if (entered_tab_page != null)
						area_to_invalidate.Union (entered_tab_page.TabBounds);
					entered_tab_page = value;
					if (entered_tab_page != null)
						area_to_invalidate.Union (entered_tab_page.TabBounds);
					Invalidate (area_to_invalidate);
					area_to_invalidate.Dispose ();
				} else
					entered_tab_page = value;
			}
		}
		#endregion	// Internal Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				CreateParams c = base.CreateParams;
				return c;
			}
		}

		protected override Size DefaultSize {
			get { return new Size (200, 100); }  
		}

		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public Rectangle GetTabRect (int index)
		{
			TabPage page = GetTab (index);
			return page.TabBounds;
		}

		public Control GetControl (int index)
		{
			return GetTab (index);
		}

#if NET_2_0
		public void SelectTab (TabPage tabPage)
		{
			if (tabPage == null)
				throw new ArgumentNullException ("tabPage");

			SelectTab (this.tab_pages [tabPage]);
		}

		public void SelectTab (string tabPageName)
		{
			if (tabPageName == null)
				throw new ArgumentNullException ("tabPageName");

			SelectTab (this.tab_pages [tabPageName]);
		}

		public void SelectTab (int index)
		{
			if (index < 0 || index > this.tab_pages.Count - 1)
				throw new ArgumentOutOfRangeException ("index");
				
			SelectedIndex = index;
		}

		public void DeselectTab (TabPage tabPage)
		{
			if (tabPage == null)
				throw new ArgumentNullException ("tabPage");

			DeselectTab (this.tab_pages [tabPage]);
		}

		public void DeselectTab (string tabPageName)
		{
			if (tabPageName == null)
				throw new ArgumentNullException ("tabPageName");

			DeselectTab (this.tab_pages [tabPageName]);
		}

		public void DeselectTab (int index)
		{
			if (index == SelectedIndex) {
				if (index >= 0 && index < this.tab_pages.Count - 1)
					SelectedIndex = ++index;
				else
					SelectedIndex = 0;
			}
		}

#endif

		public override string ToString ()
		{
			string res = String.Concat (base.ToString (),
					", TabPages.Count: ",
					TabCount);
			if (TabCount > 0)
				res = String.Concat (res, ", TabPages[0]: ",
						TabPages [0]);
			return res;
		}

		#endregion	// Public Instance Methods

		#region Protected Instance Methods

		#region Handles
		protected override Control.ControlCollection CreateControlsInstance ()
		{
			return new TabControl.ControlCollection (this);
		}

		protected override void CreateHandle ()
		{
			base.CreateHandle ();
			selected_index = (selected_index >= TabCount ? (TabCount > 0 ? 0 : -1) : selected_index);

			if (TabCount > 0) {
				if (selected_index > -1)
					this.SelectedTab.SetVisible(true);
				else
					tab_pages[0].SetVisible(true);
			}
			ResizeTabPages ();
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		#endregion

		#region Events
		protected virtual void OnDrawItem (DrawItemEventArgs e)
		{
			if (DrawMode != TabDrawMode.OwnerDrawFixed)
				return;

			DrawItemEventHandler eh = (DrawItemEventHandler)(Events [DrawItemEvent]);
			if (eh != null)
				eh (this, e);
		}

		internal void OnDrawItemInternal (DrawItemEventArgs e)
		{
			OnDrawItem (e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
			ResizeTabPages ();
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
		}

		protected override void OnStyleChanged (EventArgs e)
		{
			base.OnStyleChanged (e);
		}

		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) (Events[SelectedIndexChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		internal override void OnPaintInternal (PaintEventArgs pe)
		{
			if (GetStyle (ControlStyles.UserPaint))
				return;

			Draw (pe.Graphics, pe.ClipRectangle);
			pe.Handled = true;
		}

#if NET_2_0
		protected override void OnEnter (EventArgs e)
		{
			base.OnEnter (e);
			if (SelectedTab != null)
				SelectedTab.FireEnter ();
		}

		protected override void OnLeave (EventArgs e)
		{
			if (SelectedTab != null)
				SelectedTab.FireLeave ();
			base.OnLeave (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnRightToLeftLayoutChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) (Events[RightToLeftLayoutChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override void ScaleCore (float dx, float dy)
		{
			base.ScaleCore (dx, dy);
		}

		protected virtual void OnDeselecting (TabControlCancelEventArgs e)
		{
			TabControlCancelEventHandler eh = (TabControlCancelEventHandler) (Events[DeselectingEvent]);
			if (eh != null)
				eh (this, e);

			if (!e.Cancel)
				OnDeselected (new TabControlEventArgs (SelectedTab, selected_index, TabControlAction.Deselected));
		}

		protected virtual void OnDeselected (TabControlEventArgs e)
		{
			TabControlEventHandler eh = (TabControlEventHandler) (Events[DeselectedEvent]);
			if (eh != null)
				eh (this, e);

			if (this.SelectedTab != null)
				this.SelectedTab.FireLeave ();
		}

		protected virtual void OnSelecting (TabControlCancelEventArgs e)
		{
			TabControlCancelEventHandler eh = (TabControlCancelEventHandler) (Events[SelectingEvent]);
			if (eh != null)
				eh (this, e);

			if (!e.Cancel)
				OnSelected (new TabControlEventArgs (SelectedTab, selected_index, TabControlAction.Selected));
		}

		protected virtual void OnSelected (TabControlEventArgs e)
		{
			TabControlEventHandler eh = (TabControlEventHandler) (Events[SelectedEvent]);
			if (eh != null)
				eh (this, e);

			if (this.SelectedTab != null)
				this.SelectedTab.FireEnter ();
		}
#endif

		#endregion

		#region Keys
		protected override bool ProcessKeyPreview (ref Message m)
		{
			return base.ProcessKeyPreview (ref m);
		}

		protected override void OnKeyDown (KeyEventArgs ke)
		{
			base.OnKeyDown (ke);
			if (ke.Handled)
				return;

			if (ke.KeyCode == Keys.Tab && (ke.KeyData & Keys.Control) != 0) {
				if ((ke.KeyData & Keys.Shift) == 0)
					SelectedIndex = (SelectedIndex + 1) % TabCount;
				else
					SelectedIndex = (SelectedIndex + TabCount - 1) % TabCount;
				ke.Handled = true;
			} else if (ke.KeyCode == Keys.Home) {
				SelectedIndex = 0;
				ke.Handled = true;
			} else if (ke.KeyCode == Keys.End) {
				SelectedIndex = TabCount - 1;
				ke.Handled = true;
			} else if (NavigateTabs (ke.KeyCode))
				ke.Handled = true;
		}

		protected override bool IsInputKey (Keys keyData)
		{
			switch (keyData & Keys.KeyCode) {
			case Keys.Home:
			case Keys.End:
			case Keys.Left:
			case Keys.Right:
			case Keys.Up:
			case Keys.Down:
				return true;
			}
			return base.IsInputKey (keyData);
		}
		
		private bool NavigateTabs (Keys keycode)
		{
			bool move_left = false;
			bool move_right = false;
			
			if (alignment == TabAlignment.Bottom || alignment == TabAlignment.Top) {
				if (keycode == Keys.Left)
					move_left = true;
				else if (keycode == Keys.Right)
					move_right = true;
			} else {
				if (keycode == Keys.Up)
					move_left = true;
				else if (keycode == Keys.Down)
					move_right = true;
			}
				
			if (move_left) {
				if (SelectedIndex > 0) {
					SelectedIndex--;
					return true;
				}
			}
			
			if (move_right) {
				if (SelectedIndex < TabCount - 1) {
					SelectedIndex++;
					return true;
				}
			}
			
			return false;
		}
		#endregion

		#region Pages Collection
		protected void RemoveAll ()
		{
			Controls.Clear ();
		}

		protected virtual object [] GetItems ()
		{
			TabPage [] pages = new TabPage [Controls.Count];
			Controls.CopyTo (pages, 0);
			return pages;
		}

		protected virtual object [] GetItems (Type baseType)
		{
			object[] pages = (object[])Array.CreateInstance (baseType, Controls.Count);
			Controls.CopyTo (pages, 0);
			return pages;
		}
		#endregion

#if NET_2_0
		protected void UpdateTabSelection (bool updateFocus)
#else
		protected void UpdateTabSelection (bool uiselected)
#endif
		{
			ResizeTabPages ();
		}

		protected string GetToolTipText (object item)
		{
			TabPage page = (TabPage) item;
			return page.ToolTipText;
		}

		protected override void WndProc (ref Message m)
		{
			switch ((Msg)m.Msg) {
			case Msg.WM_SETFOCUS:
				if (selected_index == -1 && this.TabCount > 0)
					this.SelectedIndex = 0;
				if (selected_index != -1)
					Invalidate(GetTabRect(selected_index));
				base.WndProc (ref m);
				break;
			case Msg.WM_KILLFOCUS:
				if (selected_index != -1)
					Invalidate(GetTabRect(selected_index));
				base.WndProc (ref m);
				break;
			default:
				base.WndProc (ref m);
				break;
			}
		}

		#endregion	// Protected Instance Methods

		#region Internal & Private Methods
		private bool CanScrollRight {
			get {
				return (slider_pos < TabCount - 1);
			}
		}

		private bool CanScrollLeft {
			get { return slider_pos > 0; }
		}

		private void MouseDownHandler (object sender, MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) == 0)
				return;

			if (ShowSlider) {
				Rectangle right = RightScrollButtonArea;
				Rectangle left = LeftScrollButtonArea;
				if (right.Contains (e.X, e.Y)) {
					right_slider_state = PushButtonState.Pressed;
					if (CanScrollRight) {
						slider_pos++;
						SizeTabs ();

#if NET_2_0
						// UIA Framework Event: Horizontally Scrolled
						OnUIAHorizontallyScrolled (EventArgs.Empty);
#endif

						switch (this.Alignment) {
							case TabAlignment.Top:
								Invalidate (new Rectangle (0, 0, Width, ItemSize.Height));
								break;
							case TabAlignment.Bottom:
								Invalidate (new Rectangle (0, DisplayRectangle.Bottom, Width, Height - DisplayRectangle.Bottom));
								break;
							case TabAlignment.Left:
								Invalidate (new Rectangle (0, 0, DisplayRectangle.Left, Height));
								break;
							case TabAlignment.Right:
								Invalidate (new Rectangle (DisplayRectangle.Right, 0, Width - DisplayRectangle.Right, Height));
								break;
						}
						
					} else {
						Invalidate (right);
					}
					return;
				} else if (left.Contains (e.X, e.Y)) {
					left_slider_state = PushButtonState.Pressed;
					if (CanScrollLeft) {
						slider_pos--;
						SizeTabs ();

#if NET_2_0
						// UIA Framework Event: Horizontally Scrolled
						OnUIAHorizontallyScrolled (EventArgs.Empty);
#endif

						switch (this.Alignment) {
							case TabAlignment.Top:
								Invalidate (new Rectangle (0, 0, Width, ItemSize.Height));
								break;
							case TabAlignment.Bottom:
								Invalidate (new Rectangle (0, DisplayRectangle.Bottom, Width, Height - DisplayRectangle.Bottom));
								break;
							case TabAlignment.Left:
								Invalidate (new Rectangle (0, 0, DisplayRectangle.Left, Height));
								break;
							case TabAlignment.Right:
								Invalidate (new Rectangle (DisplayRectangle.Right, 0, Width - DisplayRectangle.Right, Height));
								break;
						}
					} else {
						Invalidate (left);
					}
					return;
				}
			}

			int count = Controls.Count;
			for (int i = SliderPos; i < count; i++) {
				if (!GetTabRect (i).Contains (e.X, e.Y))
					continue;
				SelectedIndex = i;
				mouse_down_on_a_tab_page = true;
				break;
			}
		}

		private void MouseUpHandler (object sender, MouseEventArgs e)
		{
			mouse_down_on_a_tab_page = false;
			if (ShowSlider && (left_slider_state == PushButtonState.Pressed || right_slider_state == PushButtonState.Pressed)) {
				Rectangle invalid;
				if (left_slider_state == PushButtonState.Pressed) {
					invalid = LeftScrollButtonArea;
					left_slider_state = GetScrollButtonState (invalid, e.Location);
				} else {
					invalid = RightScrollButtonArea;
					right_slider_state = GetScrollButtonState (invalid, e.Location);
				}
				Invalidate (invalid);
			}
		}

		bool HasHotElementStyles {
			get {
				return ThemeElements.CurrentTheme.TabControlPainter.HasHotElementStyles (this);
			}
		}

		Rectangle LeftScrollButtonArea {
			get {
				return ThemeElements.CurrentTheme.TabControlPainter.GetLeftScrollRect (this);
			}
		}

		Rectangle RightScrollButtonArea {
			get {
				return ThemeElements.CurrentTheme.TabControlPainter.GetRightScrollRect (this);
			}
		}

		static PushButtonState GetScrollButtonState (Rectangle scrollButtonArea, Point cursorLocation)
		{
			return scrollButtonArea.Contains (cursorLocation) ? PushButtonState.Hot : PushButtonState.Normal;
		}

		private void SizeChangedHandler (object sender, EventArgs e)
		{
			Redraw ();
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
			int xpos = 0;
			int ypos = 0;
			Size spacing = TabSpacing;

			if (TabPages.Count > 0)
				row_count = 1;
			show_slider = false;
			
			CalculateItemSize ();

			for (int i = 0; i < TabPages.Count; i++) {
				TabPage page = TabPages [i];
				int aux = 0;
				SizeTab (page, i, row_width, ref xpos, ref ypos, spacing, 0, ref aux, true);
			}

			if (SelectedIndex != -1 && TabPages.Count > SelectedIndex && TabPages[SelectedIndex].Row != BottomRow)
				DropRow (TabPages [SelectedIndex].Row);
		}

		// ItemSize per-se is used mostly only to retrieve the Height,
		// since the actual Width of the tabs is computed individually,
		// except when SizeMode is TabSizeMode.Fixed, where Width is used as well.
		private void CalculateItemSize ()
		{
			if (item_size_manual)
				return;

			SizeF size;
			if (tab_pages.Count > 0) {
				// .Net uses the first tab page if available.
				size = TextRenderer.MeasureString (tab_pages [0].Text, Font);

			} else {
				size = TextRenderer.MeasureString ("a", Font);
				size.Width = 0;
			}

			if (size_mode == TabSizeMode.Fixed)
				size.Width = 96;
			if (size.Width < MinimumTabWidth)
				size.Width = MinimumTabWidth;
			if (image_list != null && image_list.ImageSize.Height > size.Height)
				size.Height = image_list.ImageSize.Height;

			item_size = size.ToSize ();
		}

		private int BottomRow {
			get { return 1; }
		}

		private int Direction
		{
			get {
				return 1;
			}
		}

		private void DropRow (int row)
		{
			if (Appearance != TabAppearance.Normal)
				return;

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
			if (Alignment == TabAlignment.Bottom || Alignment == TabAlignment.Left)
				return ThemeEngine.Current.TabControlGetPanelRect (this).Bottom;

			if (Appearance == TabAppearance.Normal)
				return this.ClientRectangle.Y + ThemeEngine.Current.TabControlSelectedDelta.Y;

			return this.ClientRectangle.Y;

		}

		private int CalcXPos ()
		{
			if (Alignment == TabAlignment.Right)
				return ThemeEngine.Current.TabControlGetPanelRect (this).Right;

			if (Appearance == TabAppearance.Normal)
				return this.ClientRectangle.X + ThemeEngine.Current.TabControlSelectedDelta.X;

			return this.ClientRectangle.X;
		}

		private void SizeTabs ()
		{
			switch (Alignment) {
			case TabAlignment.Right:
			case TabAlignment.Left:
				SizeTabs (Height, true);
				break;
			default:
				SizeTabs (Width, false);
				break;
			}
		}
		
		private void SizeTabs (int row_width, bool vertical)
		{
			int ypos = 0;
			int xpos = 0;
			int prev_row = 1;
			Size spacing = TabSpacing;
			int begin_prev = 0;

			if (TabPages.Count == 0)
				return;

			prev_row = TabPages [0].Row;

			// Reset the slider position if the slider isn't needed
			// anymore (ie window size was increased so all tabs are visible)
			if (!show_slider)
				slider_pos = 0;
			else {
				// set X = -1 for marking tabs that are not visible due to scrolling
				for (int i = 0; i < slider_pos; i++) {
					TabPage page = TabPages[i];
					Rectangle x = page.TabBounds;
					x.X = -1;
					page.TabBounds = x;
				}
			}
			
			for (int i = slider_pos; i < TabPages.Count; i++) {
				TabPage page = TabPages[i];
				SizeTab (page, i, row_width, ref xpos, ref ypos, spacing, prev_row, ref begin_prev, false);
				prev_row = page.Row;
			}

			if (SizeMode == TabSizeMode.FillToRight && !ShowSlider) {
				FillRow (begin_prev, TabPages.Count - 1,
						((row_width - TabPages [TabPages.Count - 1].TabBounds.Right) / (TabPages.Count - begin_prev)), 
						spacing, vertical);
			}

			if (SelectedIndex != -1) {
				ExpandSelected (TabPages [SelectedIndex], 0, row_width - 1);
			}
		}
		
		private void SizeTab (TabPage page, int i, int row_width, ref int xpos, ref int ypos, 
								Size spacing, int prev_row, ref int begin_prev, bool widthOnly) 
		{				
			int width, height = 0;

			if (SizeMode == TabSizeMode.Fixed) {
				width = item_size.Width;
			} else {			
				width = MeasureStringWidth (DeviceContext, page.Text, page.Font);
				width += (Padding.X * 2) + 2;

				if (ImageList != null && page.ImageIndex >= 0) {
					width += ImageList.ImageSize.Width + ThemeEngine.Current.TabControlImagePadding.X;

					int image_size = ImageList.ImageSize.Height + ThemeEngine.Current.TabControlImagePadding.Y;
					if (item_size.Height < image_size)
						item_size.Height = image_size;
				}
			}

			// Use ItemSize property to recover the padding info as well.
			height = ItemSize.Height - ThemeEngine.Current.TabControlSelectedDelta.Height; // full height only for selected tab

			if (width < MinimumTabWidth)
				width = MinimumTabWidth;

			if (i == SelectedIndex)
				width += ThemeEngine.Current.TabControlSelectedSpacing;
			
			if (widthOnly) {
				page.TabBounds = new Rectangle (xpos, 0, width, 0);
				page.Row = row_count;
				if (xpos + width > row_width && multiline) {
					xpos = 0;
					row_count++;
				} else if (xpos + width > row_width) {
					show_slider = true;	
				}
				if (i == selected_index && show_slider) {
					for (int j = i-1; j >= 0; j--) {
						if (TabPages [j].TabBounds.Left < xpos + width - row_width) {
							slider_pos = j+1;
							break;
						}
					}
				}
			} else {
				if (page.Row != prev_row) {
					xpos = 0;
				}

				switch (Alignment) {
					case TabAlignment.Top:
						page.TabBounds = new Rectangle (
							xpos + CalcXPos (),
							ypos + (height + spacing.Height) * (row_count - page.Row) + CalcYPos (),
							width, 
							height);
						break;
					case TabAlignment.Bottom:
						page.TabBounds = new Rectangle (
							xpos + CalcXPos (),
							ypos + (height + spacing.Height) * (row_count - page.Row) + CalcYPos (),
							width, 
							height);
						break;
					case TabAlignment.Left:
						if (Appearance == TabAppearance.Normal) {
							// tab rows are positioned right to left
							page.TabBounds = new Rectangle (
								ypos + (height + spacing.Height) * (row_count - page.Row) + CalcXPos (),
								xpos,
								height, 
								width);
						} else {
							// tab rows are positioned left to right
							page.TabBounds = new Rectangle (
								ypos + (height + spacing.Height) * (page.Row - 1) + CalcXPos (),
								xpos,
								height, 
								width);
						}

						break;
					case TabAlignment.Right:
						if (Appearance == TabAppearance.Normal) {
							// tab rows are positioned left to right
							page.TabBounds = new Rectangle (
								ypos + (height + spacing.Height) * (page.Row - 1) + CalcXPos (),
								xpos,
								height, 
								width);
						} else {
							// tab rows are positioned right to left
							page.TabBounds = new Rectangle (
								ypos + (height + spacing.Height) * (row_count - page.Row) + CalcXPos (),
								xpos,
								height, 
								width);
						}

						break;
				}
			
				if (page.Row != prev_row) {
					if (SizeMode == TabSizeMode.FillToRight && !ShowSlider) {
						bool vertical = alignment == TabAlignment.Right || alignment == TabAlignment.Left;
						int offset = vertical ? TabPages [i - 1].TabBounds.Bottom : TabPages [i - 1].TabBounds.Right;
						FillRow (begin_prev, i - 1, ((row_width - offset) / (i - begin_prev)), spacing,
							vertical);
					}
					begin_prev = i;
				}	
			}

			xpos += width + spacing.Width + ThemeEngine.Current.TabControlColSpacing;
		}

		private void FillRow (int start, int end, int amount, Size spacing, bool vertical) 
		{
			if (vertical)
				FillRowV (start, end, amount, spacing);
			else
				FillRow (start, end, amount, spacing);
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

		private void FillRowV (int start, int end, int amount, Size spacing)
		{
			int ypos = TabPages [start].TabBounds.Top;
			for (int i = start; i <= end; i++) {
				TabPage page = TabPages [i];
				int top = ypos;
				int height = (i == end ? Height - top - 5 : page.TabBounds.Height + amount);

				page.TabBounds = new Rectangle (page.TabBounds.Left, top,
						page.TabBounds.Width, height);
				ypos = page.TabBounds.Bottom + 1;
			}
		}

		private void ExpandSelected (TabPage page, int left_edge, int right_edge)
		{
			if (Appearance != TabAppearance.Normal)
				return;

			Rectangle r = page.TabBounds;
			switch (Alignment) {
				case TabAlignment.Top:
				case TabAlignment.Left:
					r.Y -= ThemeEngine.Current.TabControlSelectedDelta.Y;
					r.X -= ThemeEngine.Current.TabControlSelectedDelta.X;
					break;
				case TabAlignment.Bottom:
					r.Y -= ThemeEngine.Current.TabControlSelectedDelta.Y;
					r.X -= ThemeEngine.Current.TabControlSelectedDelta.X;
					break;
				case TabAlignment.Right:
					r.Y -= ThemeEngine.Current.TabControlSelectedDelta.Y;
					r.X -= ThemeEngine.Current.TabControlSelectedDelta.X;
					break;
			}

			r.Width += ThemeEngine.Current.TabControlSelectedDelta.Width;
			r.Height += ThemeEngine.Current.TabControlSelectedDelta.Height;
			if (r.Left < left_edge)
				r.X = left_edge;
			// Adjustment can't be used for right alignment, since it is
			// the only one that has a different X origin than 0
			if (r.Right > right_edge && SizeMode != TabSizeMode.Normal &&
					alignment != TabAlignment.Right)
				r.Width = right_edge - r.X;
			page.TabBounds = r;
		}

		private void Draw (Graphics dc, Rectangle clip)
		{
			ThemeEngine.Current.DrawTabControl (dc, clip, this);
		}

		private TabPage GetTab (int index)
		{
			return Controls [index] as TabPage;
		}

		private void SetTab (int index, TabPage value)
		{
			if (!tab_pages.Contains (value)) {
				this.Controls.Add (value);
			}
			this.Controls.RemoveAt (index);
			this.Controls.SetChildIndex (value, index);
			Redraw ();
		}
#if NET_2_0
		private void InsertTab (int index, TabPage value)
		{
			if (!tab_pages.Contains (value)) {
				this.Controls.Add (value);
			}
			this.Controls.SetChildIndex (value, index);
			Redraw ();
		}
#endif
		internal void Redraw ()
		{
			if (!IsHandleCreated)
				return;

			ResizeTabPages ();
			Refresh ();
		}

		private int MeasureStringWidth (Graphics graphics, string text, Font font) 
		{
			if (text == String.Empty)
				return 0;
			StringFormat format = new StringFormat();
			RectangleF rect = new RectangleF(0, 0, 1000, 1000);
			CharacterRange[] ranges = { new CharacterRange(0, text.Length) };
			Region[] regions = new Region[1];

			format.SetMeasurableCharacterRanges(ranges);
			format.FormatFlags = StringFormatFlags.NoClip;
			format.FormatFlags |= StringFormatFlags.NoWrap;
			regions = graphics.MeasureCharacterRanges(text + "I", font, rect, format);
			rect = regions[0].GetBounds(graphics);

			return (int)(rect.Width);
		}

		void OnMouseMove (object sender, MouseEventArgs e)
		{
			if (!mouse_down_on_a_tab_page && ShowSlider) {
				if (LeftSliderState == PushButtonState.Pressed ||
					RightSliderState == PushButtonState.Pressed)
					return;
				if (LeftScrollButtonArea.Contains (e.Location)) {
					LeftSliderState = PushButtonState.Hot;
					RightSliderState = PushButtonState.Normal;
					EnteredTabPage = null;
					return;
				}
				if (RightScrollButtonArea.Contains (e.Location)) {
					RightSliderState = PushButtonState.Hot;
					LeftSliderState = PushButtonState.Normal;
					EnteredTabPage = null;
					return;
				}
				LeftSliderState = PushButtonState.Normal;
				RightSliderState = PushButtonState.Normal;
			}
			if (EnteredTabPage != null && EnteredTabPage.TabBounds.Contains (e.Location))
				return;
			for (int index = 0; index < TabCount; index++) {
				TabPage tab_page = TabPages[index];
				if (tab_page.TabBounds.Contains (e.Location)) {
					EnteredTabPage = tab_page;
					return;
				}
			}
			EnteredTabPage = null;
		}

		void OnMouseLeave (object sender, EventArgs e)
		{
			if (ShowSlider) {
				LeftSliderState = PushButtonState.Normal;
				RightSliderState = PushButtonState.Normal;
			}
			EnteredTabPage = null;
		}
		#endregion	// Internal & Private Methods

		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged {
			add { base.BackColorChanged += value; }
			remove { base.BackColorChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged
		{
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}
#endif

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		static object DrawItemEvent = new object ();
		static object SelectedIndexChangedEvent = new object ();

		public event DrawItemEventHandler DrawItem {
			add { Events.AddHandler (DrawItemEvent, value); }
			remove { Events.RemoveHandler (DrawItemEvent, value); }
		}

		public event EventHandler SelectedIndexChanged {
			add { Events.AddHandler (SelectedIndexChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangedEvent, value); }
		}
		
#if NET_2_0
		static object SelectedEvent = new object ();
		
		public event TabControlEventHandler Selected {
			add { Events.AddHandler (SelectedEvent, value); }
			remove { Events.RemoveHandler (SelectedEvent, value); }
		}

		static object DeselectedEvent = new object ();

		public event TabControlEventHandler Deselected
		{
			add { Events.AddHandler (DeselectedEvent, value); }
			remove { Events.RemoveHandler (DeselectedEvent, value); }
		}

		static object SelectingEvent = new object ();

		public event TabControlCancelEventHandler Selecting
		{
			add { Events.AddHandler (SelectingEvent, value); }
			remove { Events.RemoveHandler (SelectingEvent, value); }
		}

		static object DeselectingEvent = new object ();

		public event TabControlCancelEventHandler Deselecting
		{
			add { Events.AddHandler (DeselectingEvent, value); }
			remove { Events.RemoveHandler (DeselectingEvent, value); }
		}

		static object RightToLeftLayoutChangedEvent = new object ();
		public event EventHandler RightToLeftLayoutChanged
		{
			add { Events.AddHandler (RightToLeftLayoutChangedEvent, value); }
			remove { Events.RemoveHandler (RightToLeftLayoutChangedEvent, value); }
		}
#endif
		#endregion	// Events


		#region Class TaControl.ControlCollection
#if NET_2_0
		[ComVisible (false)]
#endif
		public new class ControlCollection : System.Windows.Forms.Control.ControlCollection {

			private TabControl owner;

			public ControlCollection (TabControl owner) : base (owner)
			{
				this.owner = owner;
			}

			public override void Add (Control value)
			{
				TabPage page = value as TabPage;
				if (page == null)
					throw new ArgumentException ("Cannot add " +
						value.GetType ().Name + " to TabControl. " +
						"Only TabPages can be directly added to TabControls.");

				page.SetVisible (false);
				base.Add (value);
				if (owner.TabCount == 1 && owner.selected_index < 0)
					owner.SelectedIndex = 0;
				owner.Redraw ();
			}

			public override void Remove (Control value)
			{
				bool change_index = false;
				
				TabPage page = value as TabPage;
				if (page != null && owner.Controls.Contains (page)) {
					int index = owner.IndexForTabPage (page);
					if (index < owner.SelectedIndex || owner.SelectedIndex == Count - 1)
						change_index = true;
				}
				
				base.Remove (value);
				
				// We don't want to raise SelectedIndexChanged until after we
				// have removed from the collection, so TabCount will be
				// correct for the user.
				if (change_index && Count > 0) {
					// Clear the selected index internally, to avoid trying to access the previous
					// selected tab when setting the new one - this is what .net seems to do
					int prev_selected_index = owner.SelectedIndex;
					owner.selected_index = -1;

					owner.SelectedIndex = --prev_selected_index;
				} else if (change_index) {
					owner.selected_index = -1;
					owner.OnSelectedIndexChanged (EventArgs.Empty);
				} else
					owner.Redraw ();
			}
		}
		#endregion	// Class TabControl.ControlCollection

		#region Class TabPage.TabPageCollection
		public class TabPageCollection	: IList, ICollection, IEnumerable {

			private TabControl owner;

			public TabPageCollection (TabControl owner)
			{
				if (owner == null)
					throw new ArgumentNullException ("Value cannot be null.");
				this.owner = owner;
			}

			[Browsable(false)]
			public int Count {
				get { return owner.Controls.Count; }
			}

			public bool IsReadOnly {
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
#if NET_2_0
			public virtual TabPage this [string key] {
				get {
					if (string.IsNullOrEmpty (key))
						return null;

					int index = this.IndexOfKey (key);
					if (index < 0 || index >= this.Count)
						return null;
					
					return this[index];
				}
			}
#endif

			internal int this[TabPage tabPage] {
				get {
					if (tabPage == null)
						return -1;

					for (int i = 0; i < this.Count; i++)
						if (this[i].Equals (tabPage))
							return i;

					return -1;
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

			public void Add (TabPage value)
			{
				if (value == null)
					throw new ArgumentNullException ("Value cannot be null.");
				owner.Controls.Add (value);
			}

#if NET_2_0
			public void Add (string text)
			{
				TabPage page = new TabPage (text);
				this.Add (page);
			}

			public void Add (string key, string text)
			{
				TabPage page = new TabPage (text);
				page.Name = key;
				this.Add (page);
			}

			public void Add (string key, string text, int imageIndex)
			{
				TabPage page = new TabPage (text);
				page.Name = key;
				page.ImageIndex = imageIndex;
				this.Add (page);
			}

			// .Net sets the ImageKey, but does not show the image when this is used
			public void Add (string key, string text, string imageKey)
			{
				TabPage page = new TabPage (text);
				page.Name = key;
				page.ImageKey = imageKey;
				this.Add (page);
			}
#endif

			public void AddRange (TabPage [] pages)
			{
				if (pages == null)
					throw new ArgumentNullException ("Value cannot be null.");
				owner.Controls.AddRange (pages);
			}

			public virtual void Clear ()
			{
				owner.Controls.Clear ();
				owner.Invalidate ();
			}

			public bool Contains (TabPage page)
			{
				if (page == null)
					throw new ArgumentNullException ("Value cannot be null.");
				return owner.Controls.Contains (page);
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				int index = this.IndexOfKey (key);
				return (index >= 0 && index < this.Count);
			}
#endif

			public IEnumerator GetEnumerator ()
			{
				return owner.Controls.GetEnumerator ();
			}

			public int IndexOf (TabPage page)
			{
				return owner.Controls.IndexOf (page);
			}

#if NET_2_0
			public virtual int IndexOfKey(string key)
			{
				if (string.IsNullOrEmpty (key))
					return -1;

				for (int i = 0; i < this.Count; i++) {
					if (string.Compare (this[i].Name, key, true, 
						System.Globalization.CultureInfo.InvariantCulture) == 0) {
						return i;
					}
				}

				return -1;
			}
#endif

			public void Remove (TabPage value)
			{
				owner.Controls.Remove (value);
				owner.Invalidate ();
			}

			public void RemoveAt (int index)
			{
				owner.Controls.RemoveAt (index);
				owner.Invalidate ();
			}

#if NET_2_0
			public virtual void RemoveByKey (string key)
			{
				int index = this.IndexOfKey (key);
				if (index >= 0 && index < this.Count)
					this.RemoveAt (index);
			}
#endif

			void ICollection.CopyTo (Array dest, int index)
			{
				owner.Controls.CopyTo (dest, index);
			}

			int IList.Add (object value)
			{
				TabPage page = value as TabPage;
				if (value == null)
					throw new ArgumentException ("value");
				owner.Controls.Add (page);
				return owner.Controls.IndexOf (page);
			}

			bool IList.Contains (object page)
			{
				TabPage tabPage = page as TabPage;
				if (tabPage == null)
					return false;
				return Contains (tabPage);
			}

			int IList.IndexOf (object page)
			{
				TabPage tabPage = page as TabPage;
				if (tabPage == null)
					return -1;
				return IndexOf (tabPage);
			}

#if NET_2_0
			void IList.Insert (int index, object tabPage)
#else
			void IList.Insert (int index, object value)
#endif
			{
				throw new NotSupportedException ();
			}

#if NET_2_0
			public void Insert (int index, string text)
			{
				owner.InsertTab (index, new TabPage (text));
			}
			
			public void Insert (int index, TabPage tabPage)
			{
				owner.InsertTab (index, tabPage);
			}

			public void Insert (int index, string key, string text)
			{
				TabPage page = new TabPage(text);
				page.Name = key;
				owner.InsertTab (index, page);
			}

			public void Insert (int index, string key, string text, int imageIndex) 
			{
				TabPage page = new TabPage(text);
				page.Name = key;
				owner.InsertTab (index, page);
				page.ImageIndex = imageIndex;
			}

			public void Insert (int index, string key, string text, string imageKey) 
			{
				TabPage page = new TabPage(text);
				page.Name = key;
				owner.InsertTab (index, page);
				page.ImageKey = imageKey;
			}
#endif
			void IList.Remove (object value)
			{
				TabPage page = value as TabPage;
				if (page == null)
					return;
				Remove ((TabPage) value);
			}
		}
		#endregion	// Class TabPage.TabPageCollection
	}
}


