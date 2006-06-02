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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Mike Kestner  <mkestner@novell.com>
//
// NOT COMPLETE

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Runtime.InteropServices;


namespace System.Windows.Forms
{

	[DefaultProperty("Items")]
	[DefaultEvent("SelectedIndexChanged")]
	[Designer ("System.Windows.Forms.Design.ComboBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class ComboBox : ListControl
	{
		private DrawMode draw_mode = DrawMode.Normal;
		private ComboBoxStyle dropdown_style = (ComboBoxStyle)(-1);
		private int dropdown_width = -1;		
		private int selected_index = -1;
		private object selected_item;
		internal ObjectCollection items = null;
		private bool suspend_ctrlupdate;
		private int maxdrop_items = 8;			
		private bool integral_height = true;
		private bool sorted;
		private int max_length;
		private ComboListBox listbox_ctrl;		
		private TextBox textbox_ctrl;
		private bool process_textchanged_event = true;
		private bool item_height_specified = false;
		private int item_height;
		private int requested_height = -1;
		private Hashtable item_heights;
		private bool show_dropdown_button = false;
		private ButtonState button_state = ButtonState.Normal;
		private bool dropped_down;
		private Rectangle text_area;
		private Rectangle button_area;
		private Rectangle listbox_area;
		private const int button_width = 16;

		[ComVisible(true)]
		public class ChildAccessibleObject : AccessibleObject {
			private ComboBox	owner;
			private IntPtr		handle;

			public ChildAccessibleObject (ComboBox owner, IntPtr handle) {
				this.owner = owner;
				this.handle = handle;
			}

			public override string Name {
				get {
					return base.Name;
				}
			}
		}

		public ComboBox ()
		{
			items = new ObjectCollection (this);
			DropDownStyle = ComboBoxStyle.DropDown;
			item_height = FontHeight + 2;
			BackColor = ThemeEngine.Current.ColorWindow;
			border_style = BorderStyle.None;

			/* Events */
			MouseDown += new MouseEventHandler (OnMouseDownCB);
			MouseUp += new MouseEventHandler (OnMouseUpCB);
			MouseMove += new MouseEventHandler (OnMouseMoveCB);
			KeyDown +=new KeyEventHandler(OnKeyDownCB);
		}

		#region events
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}
		
		public event DrawItemEventHandler DrawItem;		
		public event EventHandler DropDown;		
		public event EventHandler DropDownStyleChanged;		
		public event MeasureItemEventHandler MeasureItem;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}
		
		public event EventHandler SelectedIndexChanged;		
		public event EventHandler SelectionChangeCommitted;
		#endregion Events

		#region Public Properties
		public override Color BackColor {
			get { return base.BackColor; }
			set {
				if (base.BackColor == value)
					return;

    				base.BackColor = value;
				Refresh ();
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set {
				if (base.BackgroundImage == value)
					return;

    				base.BackgroundImage = value;
				Refresh ();
			}
		}

		protected override CreateParams CreateParams {
			get { return base.CreateParams;}
		}

		protected override Size DefaultSize {
			get { return new Size (121, 21); }
		}

		[RefreshProperties(RefreshProperties.Repaint)]
		[DefaultValue (DrawMode.Normal)]
		public DrawMode DrawMode {
			get { return draw_mode; }

    			set {
				if (!Enum.IsDefined (typeof (DrawMode), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for DrawMode", value));

				if (draw_mode == value)
					return;

				if (draw_mode == DrawMode.OwnerDrawVariable)
					item_heights = null;
    				draw_mode = value;
				if (draw_mode == DrawMode.OwnerDrawVariable)
					item_heights = new Hashtable ();
				Refresh ();
    			}
		}

		[DefaultValue (ComboBoxStyle.DropDown)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public ComboBoxStyle DropDownStyle {
			get { return dropdown_style; }

    			set {
		
				if (!Enum.IsDefined (typeof (ComboBoxStyle), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for ComboBoxStyle", value));

				if (dropdown_style == value)
					return;					
									
				if (dropdown_style == ComboBoxStyle.Simple) {
					if (listbox_ctrl != null) {						
						Controls.RemoveImplicit (listbox_ctrl);
						listbox_ctrl.Dispose ();						
						listbox_ctrl = null;
					}
				}

				dropdown_style = value;					
				
				if (dropdown_style == ComboBoxStyle.DropDownList && textbox_ctrl != null) {
					Controls.RemoveImplicit (textbox_ctrl);
					textbox_ctrl.Dispose ();						
					textbox_ctrl = null;						
				}				

				if (dropdown_style == ComboBoxStyle.Simple) {
					show_dropdown_button = false;					
					
					CreateComboListBox ();

					if (IsHandleCreated)
						Controls.AddImplicit (listbox_ctrl);
				} else {
					show_dropdown_button = true;
					button_state = ButtonState.Normal;
				}				
	
				if (dropdown_style != ComboBoxStyle.DropDownList && textbox_ctrl == null) {
					textbox_ctrl = new FixedSizeTextBox ();
					if (selected_index != -1)
						textbox_ctrl.Text = GetItemText (Items [selected_index]);
					textbox_ctrl.BorderStyle = BorderStyle.None;
					textbox_ctrl.TextChanged += new EventHandler (OnTextChangedEdit);
					textbox_ctrl.KeyPress += new KeyPressEventHandler(textbox_ctrl_KeyPress);
					textbox_ctrl.KeyDown += new KeyEventHandler (OnKeyDownCB);
					textbox_ctrl.GotFocus += new EventHandler(textbox_ctrl_GotFocus);
					textbox_ctrl.LostFocus += new EventHandler(textbox_ctrl_LostFocus);
					textbox_ctrl.MouseDown += new MouseEventHandler(textbox_ctrl_MouseDown);
					textbox_ctrl.MouseMove += new MouseEventHandler(textbox_ctrl_MouseMove);
					textbox_ctrl.MouseUp += new MouseEventHandler(textbox_ctrl_MouseUp);

					if (IsHandleCreated == true) {
						Controls.AddImplicit (textbox_ctrl);
					}
				}
				
				OnDropDownStyleChanged (EventArgs.Empty);
    				
				Layout ();
				UpdateBounds ();
				Refresh ();
    			}
		}

		public int DropDownWidth {
			get { 
				if (dropdown_width == -1)
					return Width;
					
				return dropdown_width; 
			}
			set {
				if (dropdown_width == value)
					return;
					
				if (value < 1)
					throw new ArgumentException ("The DropDownWidth value is less than one");

    				dropdown_width = value;				
			}
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]		
		public bool DroppedDown {
			get { 
				if (dropdown_style == ComboBoxStyle.Simple) 				
					return true;
				
				return dropped_down;
			}
			set {
				if (dropdown_style == ComboBoxStyle.Simple || dropped_down == value)
					return;
					
				if (value) 
					DropDownListBox ();
				else
					listbox_ctrl.Hide ();
			}
		}		

		public override bool Focused {
			get { return base.Focused; }
		}

		public override Color ForeColor {
			get { return base.ForeColor; }
			set {
				if (base.ForeColor == value)
					return;

    				base.ForeColor = value;
				Refresh ();
			}
		}

		[DefaultValue (true)]
		[Localizable (true)]		
		public bool IntegralHeight {
			get { return integral_height; }
			set {
				if (integral_height == value)
					return;

    				integral_height = value;
				UpdateBounds ();
    				Refresh ();
			}
		}

		[Localizable (true)]
		public int ItemHeight {
			get {
				if (item_height == -1) {
					SizeF sz = DeviceContext.MeasureString ("The quick brown Fox", Font);
					item_height = (int) sz.Height;
				}
				return item_height;
			}
			set {
				if (value < 0)
					throw new ArgumentException ("The item height value is less than zero");

				item_height_specified = true;
				item_height = value;
				if (IntegralHeight)
					UpdateBounds ();
				Layout ();
				Refresh ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.ListControlStringCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]		
		public ComboBox.ObjectCollection Items {
			get { return items; }
		}

		[DefaultValue (8)]
		[Localizable (true)]
		public int MaxDropDownItems {
			get { return maxdrop_items; }
			set {
				if (maxdrop_items == value)
					return;

    				maxdrop_items = value;
			}
		}

		[DefaultValue (0)]
		[Localizable (true)]
		public int MaxLength {
			get { return max_length; }
			set {
				if (max_length == value)
					return;

				max_length = value;
				
				if (dropdown_style != ComboBoxStyle.DropDownList) {
					
					if (value < 0) {
						value = 0;
					}
					
					textbox_ctrl.MaxLength = value;
				}			
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]		
		public int PreferredHeight {
			get {
				return ItemHeight + 5;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override int SelectedIndex {
			get { return selected_index; }
			set {
				if (value <= -2 || value >= Items.Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				if (selected_index == value)
					return;

    				selected_index = value;
    				
    				if (dropdown_style != ComboBoxStyle.DropDownList) {
					if (selected_index == -1)
						SetControlText("");
					else {
						SetControlText (GetItemText (Items[selected_index]));
						SelectAll ();
					}
    				}
    				
				if (listbox_ctrl != null)
					listbox_ctrl.HighlightedIndex = value;

    				OnSelectedValueChanged (new EventArgs ());
    				OnSelectedIndexChanged  (new EventArgs ());
    				OnSelectedItemChanged (new EventArgs ());
				if (DropDownStyle == ComboBoxStyle.DropDownList)
    					Refresh ();
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Bindable(true)]
		public object SelectedItem {
			get {
				if (selected_index !=-1 && Items !=null && Items.Count > 0)
					return Items[selected_index];
				else
					return null;
			}
			set {				
    				int index = Items.IndexOf (value);

				if (index == -1)
					return;
					
				if (selected_index == index)
					return;

				SelectedIndex = index;
			}
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string SelectedText {
			get {
				if (dropdown_style == ComboBoxStyle.DropDownList)
					return "";
					
				return textbox_ctrl.SelectedText;
			}
			set {
				if (dropdown_style == ComboBoxStyle.DropDownList)
					return;
				
				textbox_ctrl.SelectedText = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int SelectionLength {
			get {
				if (dropdown_style == ComboBoxStyle.DropDownList) 
					return 0;
				
				return textbox_ctrl.SelectionLength;
			}
			set {
				if (dropdown_style == ComboBoxStyle.DropDownList) 
					return;
					
				if (textbox_ctrl.SelectionLength == value)
					return;
					
				textbox_ctrl.SelectionLength = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int SelectionStart {
			get { 
				if (dropdown_style == ComboBoxStyle.DropDownList) 
					return 0; 					
				
				return textbox_ctrl.SelectionStart;				
			}
			set {
				if (dropdown_style == ComboBoxStyle.DropDownList) 
					return;
				
				if (textbox_ctrl.SelectionStart == value)
					return;					
				
				textbox_ctrl.SelectionStart = value;
			}
		}

		[DefaultValue (false)]
		public bool Sorted {
			get { return sorted; }

    			set {
				if (sorted == value)
					return;

    				sorted = value;
    			}
    		}

		[Bindable (true)]
		[Localizable (true)]
		public override string Text {
			get {
				if (dropdown_style != ComboBoxStyle.DropDownList) {
					if (textbox_ctrl != null) {
						return textbox_ctrl.Text;
					}
				}

				if (SelectedItem != null)
					return GetItemText (SelectedItem);
								
				return base.Text;				
			}
			set {				
				if (value == null) {
					SelectedIndex = -1;
					return;
				}
				
				int index = FindString (value);
				
				if (index != -1) {
					SelectedIndex = index;
					return;					
				}
				
				if (dropdown_style != ComboBoxStyle.DropDownList)
					textbox_ctrl.Text = GetItemText (value);
			}
		}

		#endregion Public Properties

		#region Public Methods
		protected virtual void AddItemsCore (object[] value)
		{
			
		}

		public void BeginUpdate ()
		{
			suspend_ctrlupdate = true;
		}

		protected override void Dispose (bool disposing)
		{						
			if (disposing) {
				if (listbox_ctrl != null) {
					listbox_ctrl.Dispose ();
					Controls.RemoveImplicit (listbox_ctrl);
					listbox_ctrl = null;
				}			
			
				if (textbox_ctrl != null) {
					Controls.RemoveImplicit (textbox_ctrl);
					textbox_ctrl.Dispose ();
					textbox_ctrl = null;
				}			
			}
			
			base.Dispose (disposing);
		}

		public void EndUpdate ()
		{
			suspend_ctrlupdate = false;
			UpdatedItems ();
			Refresh ();
		}

		public int FindString (string s)
		{
			return FindString (s, -1);
		}

		public int FindString (string s, int startIndex)
		{
			if (Items.Count == 0) 
				return -1; // No exception throwing if empty

			if (startIndex < -1 || startIndex >= Items.Count - 1)
				throw new  ArgumentOutOfRangeException ("Index of out range");

			startIndex++;
			for (int i = startIndex; i < Items.Count; i++) {
				if ((GetItemText (Items[i])).StartsWith (s))
					return i;
			}

			return -1;
		}

		public int FindStringExact (string s)
		{
			return FindStringExact (s, -1);
		}

		public int FindStringExact (string s, int startIndex)
		{
			if (Items.Count == 0) 
				return -1; // No exception throwing if empty

			if (startIndex < -1 || startIndex >= Items.Count - 1)
				throw new ArgumentOutOfRangeException ("Index of out range");

			startIndex++;
			for (int i = startIndex; i < Items.Count; i++) {
				if ((GetItemText (Items[i])).Equals (s))
					return i;
			}

			return -1;
		}

		public int GetItemHeight (int index)
		{	
			if (DrawMode == DrawMode.OwnerDrawVariable && IsHandleCreated) {

				if (index < 0 || index >= Items.Count )
					throw new ArgumentOutOfRangeException ("The item height value is less than zero");
				
				object item = Items [index];
				if (item_heights.Contains (item))
					return (int) item_heights [item];
				
				MeasureItemEventArgs args = new MeasureItemEventArgs (DeviceContext, index, ItemHeight);
				OnMeasureItem (args);
				item_heights [item] = args.ItemHeight;
				return args.ItemHeight;
			}

			return ItemHeight;
		}

		protected override bool IsInputKey (Keys keyData)
		{
			switch (keyData) {
			case Keys.Up:
			case Keys.Down:
			case Keys.PageUp:
			case Keys.PageDown:			
				return true;
			
			default:					
				return false;
			}
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}

		protected override void OnDataSourceChanged (EventArgs e)
		{
			base.OnDataSourceChanged (e);
			BindDataItems (items);
			
			if (DataSource == null || DataManager == null) {
				SelectedIndex = -1;
			} 
			else {
				SelectedIndex = DataManager.Position;
			}
		}

		protected override void OnDisplayMemberChanged (EventArgs e)
		{
			base.OnDisplayMemberChanged (e);

			if (DataManager == null || !IsHandleCreated)
			       return;

			BindDataItems (items);
			SelectedIndex = DataManager.Position;
		}

		protected virtual void OnDrawItem (DrawItemEventArgs e)
		{
			switch (DrawMode) {
			case DrawMode.OwnerDrawFixed:
			case DrawMode.OwnerDrawVariable:
				if (DrawItem != null)
					DrawItem (this, e);
				break;
			default:
				ThemeEngine.Current.DrawComboBoxItem (this, e);
				break;
			}
		}		

		protected virtual void OnDropDown (EventArgs e)
		{
			if (DropDown != null)
				DropDown (this, e);
		}

		protected virtual void OnDropDownStyleChanged (EventArgs e)
		{
			if (DropDownStyleChanged != null)
				DropDownStyleChanged (this, e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);

			if (textbox_ctrl != null)
				textbox_ctrl.Font = Font;
			
			if (!item_height_specified) {
				SizeF sz = DeviceContext.MeasureString ("The quick brown Fox", Font);
				item_height = (int) sz.Height;
			}

			if (IntegralHeight)
				UpdateBounds ();

			Layout ();
		}

		protected override void OnForeColorChanged (EventArgs e)
		{
			base.OnForeColorChanged (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]		
		protected override void OnGotFocus (EventArgs e) {			
			has_focus = true;
			Invalidate ();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]		
		protected override void OnLostFocus (EventArgs e) {			
			has_focus = false;
			Invalidate ();
		}		

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);

			if (listbox_ctrl != null)
				Controls.AddImplicit (listbox_ctrl);
			
			if (textbox_ctrl != null)
				Controls.AddImplicit (textbox_ctrl);

			Layout ();
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected override void OnKeyPress (KeyPressEventArgs e)
		{
			base.OnKeyPress (e);
		}

		protected virtual void OnMeasureItem (MeasureItemEventArgs e)
		{
			if (MeasureItem != null)
				MeasureItem (this, e);
		}

		protected override void OnParentBackColorChanged (EventArgs e)
		{
			base.OnParentBackColorChanged (e);
		}

		protected override void OnResize (EventArgs e)
		{			
			Layout ();
			if (listbox_ctrl != null)
				listbox_ctrl.CalcListBoxArea ();
		}

		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			base.OnSelectedIndexChanged (e);

			if (SelectedIndexChanged != null)
				SelectedIndexChanged (this, e);
		}

		protected virtual void OnSelectedItemChanged (EventArgs e)
		{
			
		}

		protected override void OnSelectedValueChanged (EventArgs e)
		{
			base.OnSelectedValueChanged (e);
		}

		protected virtual void OnSelectionChangeCommitted (EventArgs e)
		{
			if (SelectionChangeCommitted != null)
				SelectionChangeCommitted (this, e);
		}

		protected override void RefreshItem (int index)
		{
			if (index < 0 || index >= Items.Count)
				throw new ArgumentOutOfRangeException ("Index of out range");
				
			if (draw_mode == DrawMode.OwnerDrawVariable)
				item_heights.Remove (Items [index]);
		}

		public void Select (int start, int length)
		{
			if (start < 0)
				throw new ArgumentException ("Start cannot be less than zero");
				
			if (length < 0)
				throw new ArgumentException ("length cannot be less than zero");
				
			if (dropdown_style == ComboBoxStyle.DropDownList)
				return;
				
			textbox_ctrl.Select (start, length);
		}

		public void SelectAll ()
		{
			if (dropdown_style == ComboBoxStyle.DropDownList)
				return;
				
			textbox_ctrl.SelectAll ();
		}		

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{			
			if ((specified & BoundsSpecified.Height) != 0) {
				requested_height = height;

				if (DropDownStyle == ComboBoxStyle.Simple && height > PreferredHeight) {
					if (IntegralHeight) {
						int border = ThemeEngine.Current.Border3DSize.Height;
						int lb_height = height - PreferredHeight - 2;
						if (lb_height - 2 * border > ItemHeight) {
							int partial = (lb_height - 2 * border) % ItemHeight;
							height -= partial;
						} else
							height = PreferredHeight;
					}
				} else
					height = PreferredHeight;
			}

			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void SetItemCore (int index, object value)
		{
			if (index < 0 || index >= Items.Count)
				return;

			Items[index] = value;
		}

		protected override void SetItemsCore (IList value)
		{
			Items.AddRange (value);
		}

		public override string ToString ()
		{
			return base.ToString () + ", Items.Count:" + Items.Count;
		}

		protected override void WndProc (ref Message m)
		{
			switch ((Msg) m.Msg) {
			case Msg.WM_MOUSE_LEAVE:
				Point location = PointToClient (Control.MousePosition);
				if (ClientRectangle.Contains (location))
					return;
				break;
			default:
				break;
			}
			base.WndProc (ref m);
		}

		#endregion Public Methods

		#region Private Methods

		internal override bool InternalCapture {
			get { return Capture; }
			set {}
		}

		private void textbox_ctrl_KeyPress(object sender, KeyPressEventArgs e) 
		{
			OnKeyPress (e);
		}
		
		private void textbox_ctrl_GotFocus(object sender, EventArgs e )
		{
			OnGotFocus(e);
		}
		
		private void textbox_ctrl_LostFocus(object sender, EventArgs e )
		{
			OnLostFocus(e);
		}
		
		private void textbox_ctrl_MouseDown(object sender, MouseEventArgs e )
		{
			OnMouseDown(e);
		}
		
		private void textbox_ctrl_MouseMove(object sender, MouseEventArgs e )
		{
			OnMouseMove(e);
		}
		
		private void textbox_ctrl_MouseUp(object sender, MouseEventArgs e )
		{
			OnMouseUp(e);
		}
		
		void Layout ()
		{			
			int border = ThemeEngine.Current.Border3DSize.Width;

			text_area = ClientRectangle;
			text_area.Height = PreferredHeight;
			
			listbox_area = ClientRectangle;
			listbox_area.Y = text_area.Bottom + 3;
			listbox_area.Height -= (text_area.Height + 2);

			Rectangle prev_button_area = button_area;

			if (DropDownStyle == ComboBoxStyle.Simple)
				button_area = Rectangle.Empty;
			else {
				button_area = text_area;
			       	button_area.X = text_area.Right - button_width - border;
			       	button_area.Y = text_area.Y + border;
				button_area.Width = button_width;
				button_area.Height = text_area.Height - 2 * border;
			}

			if (button_area != prev_button_area) {
				prev_button_area.Y -= border;
				prev_button_area.Width += border;
				prev_button_area.Height += 2 * border;
				Invalidate (prev_button_area);
				Invalidate (button_area);
			}

			if (textbox_ctrl != null) {
				textbox_ctrl.Location = new Point (text_area.X + border, text_area.Y + border);
				textbox_ctrl.Width = text_area.Width - button_area.Width - border * 2;
				textbox_ctrl.Height = text_area.Height - border * 2;
			}

			if (listbox_ctrl != null && dropdown_style == ComboBoxStyle.Simple) {
				listbox_ctrl.Location = listbox_area.Location;
				listbox_ctrl.CalcListBoxArea ();
			}
		}

		private void CreateComboListBox ()
		{			
			listbox_ctrl = new ComboListBox (this);			
			if (selected_index != -1)
				listbox_ctrl.HighlightedIndex = selected_index;
		}
		
		internal void Draw (Rectangle clip, Graphics dc)
		{				
			Theme theme = ThemeEngine.Current;

			if (DropDownStyle == ComboBoxStyle.Simple)
				dc.FillRectangle (theme.ResPool.GetSolidBrush (Parent.BackColor), ClientRectangle);

			if (clip.IntersectsWith (text_area))
				ControlPaint.DrawBorder3D (dc, text_area, Border3DStyle.Sunken);

			int border = theme.Border3DSize.Width;

			// No edit control, we paint the edit ourselves
			if (dropdown_style == ComboBoxStyle.DropDownList) {
				DrawItemState state = DrawItemState.None;
				Rectangle item_rect = text_area;
				item_rect.X += border;
				item_rect.Y += border;
				item_rect.Width -= (button_area.Width + 2 * border);				
				item_rect.Height -= 2 * border;				
								
				if (has_focus) {
					state = DrawItemState.Selected;
					state |= DrawItemState.Focus;
				}
				
				state |= DrawItemState.ComboBoxEdit;				
				OnDrawItem (new DrawItemEventArgs (dc, Font, item_rect,
							selected_index, state, ForeColor, BackColor));
			}						
			
			if (show_dropdown_button) {
				dc.FillRectangle (theme.ResPool.GetSolidBrush (theme.ColorControl), button_area);

				if (!is_enabled)
					button_state = ButtonState.Inactive;
				
				theme.CPDrawComboButton (dc, button_area, button_state);
			}			
		}

		internal void DropDownListBox ()
		{
			if (DropDownStyle == ComboBoxStyle.Simple)
				return;			
			
			if (listbox_ctrl == null)
    				CreateComboListBox ();

			listbox_ctrl.Location = PointToScreen (new Point (text_area.X, text_area.Y + text_area.Height));
						
    			if (listbox_ctrl.ShowWindow ())    				
    				dropped_down = true;        				
    			
    			button_state = ButtonState.Pushed;				
    			if (dropdown_style == ComboBoxStyle.DropDownList)
    				Invalidate (text_area);

			OnDropDown (EventArgs.Empty);
		}
		
		internal void DropDownListBoxFinished ()
		{
			if (DropDownStyle == ComboBoxStyle.Simple)
				return;			
				
			button_state = ButtonState.Normal;
			Invalidate (button_area);
			dropped_down = false;
		}
		
		private int FindStringCaseInsensitive (string search)
		{			
			if (search.Length == 0) {
				return -1;
			}
			
			for (int i = 0; i < Items.Count; i++) 
			{				
				if (String.Compare (GetItemText (Items[i]), 0, search, 0, search.Length, true) == 0)
					return i;
			}

			return -1;
		}

		private void OnKeyDownCB(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode) 
			{			
				case Keys.Up:
					SelectedIndex = Math.Max(SelectedIndex-1, 0);
					break;				
	
				case Keys.Down:			
					SelectedIndex = Math.Min(SelectedIndex+1, Items.Count-1);
					break;
				
				case Keys.PageUp:
					if (listbox_ctrl != null)
						SelectedIndex = Math.Max(SelectedIndex- (listbox_ctrl.page_size-1), 0);
					break;				
	
				case Keys.PageDown:		
					if (listbox_ctrl != null)		
						SelectedIndex = Math.Min(SelectedIndex+(listbox_ctrl.page_size-1), Items.Count-1);
					break;
				
				default:
					break;
			}
		}
		
		void OnMouseDownCB (object sender, MouseEventArgs e)
    		{    			
			Rectangle area;
			if (DropDownStyle == ComboBoxStyle.DropDownList)
				area = ClientRectangle;
			else
				area = button_area;

			if (area.Contains (e.X, e.Y)) {
	    			DropDownListBox ();	    				
	    			Invalidate (button_area);
				Update ();
    			}
			Capture = true;
    		}

    		void OnMouseMoveCB (object sender, MouseEventArgs e)
    		{    			
			if (DropDownStyle == ComboBoxStyle.Simple)
				return;

			if (listbox_ctrl != null && listbox_ctrl.Visible) {
				Point location = listbox_ctrl.PointToClient (Control.MousePosition);
				if (listbox_ctrl.ClientRectangle.Contains (location))
					listbox_ctrl.Capture = true;
			}
    		}

    		void OnMouseUpCB (object sender, MouseEventArgs e)
    		{
			Capture = false;
			OnClick (EventArgs.Empty);

			if (dropped_down)
				listbox_ctrl.Capture = true;
    		}

		internal override void OnPaintInternal (PaintEventArgs pevent)
		{
			if (suspend_ctrlupdate)
    				return;
    				
			Draw (ClientRectangle, pevent.Graphics);			
		}
		
		private void OnTextChangedEdit (object sender, EventArgs e)
		{
			if (process_textchanged_event == false)
				return; 
				
			int item = FindStringCaseInsensitive (textbox_ctrl.Text);
			
			if (item == -1)
				return;

			if (listbox_ctrl != null) {
				listbox_ctrl.SetTopItem (item);
				listbox_ctrl.HighlightedIndex = item;
			}
		}
		
		internal void SetControlText (string s)
		{		
			process_textchanged_event = false; 
    			textbox_ctrl.Text = s;
    			process_textchanged_event = true;
    		}
		
		void UpdateBounds ()
		{
			if (requested_height != -1)
				SetBoundsCore (0, 0, 0, requested_height, BoundsSpecified.Height);
		}

		private void UpdatedItems ()
		{
			if (listbox_ctrl != null) {
				listbox_ctrl.UpdateLastVisibleItem ();
				listbox_ctrl.CalcListBoxArea ();
				listbox_ctrl.Refresh ();
			}
		}

		#endregion Private Methods

		[ListBindableAttribute (false)]
		public class ObjectCollection : IList, ICollection, IEnumerable
		{

			private ComboBox owner;
			internal ArrayList object_items = new ArrayList ();

			public ObjectCollection (ComboBox owner)
			{
				this.owner = owner;
			}

			#region Public Properties
			public int Count {
				get { return object_items.Count; }
			}

			public bool IsReadOnly {
				get { return false; }
			}

			[Browsable (false)]
			[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
			public virtual object this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					return object_items[index];
				}
				set {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					object_items[index] = value;
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

			#endregion Public Properties
			
			#region Private Properties			
			internal ArrayList ObjectItems {
				get { return object_items;}
				set {
					object_items = value;
				}
			}
			
			#endregion Private Properties

			#region Public Methods
			public int Add (object item)
			{
				int idx;

				idx = AddItem (item);
				owner.UpdatedItems ();
				return idx;
			}

			public void AddRange (object[] items)
			{
				foreach (object mi in items)
					AddItem (mi);
					
				owner.UpdatedItems ();
			}

			public void Clear ()
			{
				owner.selected_index = -1;
				object_items.Clear ();
				owner.UpdatedItems ();
				owner.Refresh ();
			}
			
			public bool Contains (object obj)
			{
				return object_items.Contains (obj);
			}

			public void CopyTo (object[] dest, int arrayIndex)
			{
				object_items.CopyTo (dest, arrayIndex);
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				object_items.CopyTo (dest, index);
			}

			public IEnumerator GetEnumerator ()
			{
				return object_items.GetEnumerator ();
			}

			int IList.Add (object item)
			{
				return Add (item);
			}

			public int IndexOf (object value)
			{
				return object_items.IndexOf (value);
			}

			public void Insert (int index,  object item)
			{
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("Index of out range");					
				
				ObjectCollection new_items = new ObjectCollection (owner);				
    				object sel_item = owner.SelectedItem;
    				    								
				owner.BeginUpdate ();
				
				for (int i = 0; i < index; i++) {
					new_items.AddItem (ObjectItems[i]);
				}

				new_items.AddItem (item);

				for (int i = index; i < Count; i++){
					new_items.AddItem (ObjectItems[i]);
				}				

				ObjectItems = new_items.ObjectItems;
				
				if (sel_item != null) {
					int idx = IndexOf (sel_item);
					owner.selected_index = idx;
					owner.listbox_ctrl.HighlightedIndex = idx;
				}
												
				owner.EndUpdate ();	// Calls UpdatedItems
			}

			public void Remove (object value)
			{				
				if (IndexOf (value) == owner.SelectedIndex)
					owner.SelectedItem = null;
				
				RemoveAt (IndexOf (value));				
				
			}

			public void RemoveAt (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");
					
				if (index == owner.SelectedIndex)
					owner.SelectedItem = null;

				object_items.RemoveAt (index);
				owner.UpdatedItems ();
			}
			#endregion Public Methods

			#region Private Methods
			private int AddItem (object item)
			{
				int cnt = object_items.Count;
				object_items.Add (item);
				return cnt;
			}
			
			internal void AddRange (IList items)
			{
				foreach (object mi in items)
					AddItem (mi);
										
				owner.UpdatedItems ();
			}

			#endregion Private Methods
		}

		internal class ComboListBox : Control
		{
			private ComboBox owner;			
			private VScrollBarLB vscrollbar_ctrl;
			private int top_item;			/* First item that we show the in the current page */
			private int last_item;			/* Last visible item */
			internal int page_size;			/* Number of listbox items per page */
			private Rectangle textarea_drawable;	/* Rectangle of the drawable text area */
			
			internal enum ItemNavigation
			{
				First,
				Last,
				Next,
				Previous,
				NextPage,
				PreviousPage,
			}
			
			class VScrollBarLB : VScrollBar
			{
				public VScrollBarLB ()
				{					
				}
				
				internal override bool InternalCapture {
					get { return Capture; }
					set { }
				}

				public bool FireMouseDown (MouseEventArgs e) 
				{
					if (Visible) {
						e = TranslateEvent (e);
						if (ClientRectangle.Contains (e.X, e.Y)) {
							OnMouseDown (e);
							return true;
						}
					}
					return false;
				}	
				
				public void FireMouseUp (MouseEventArgs e) 
				{
					if (Visible) {
						e = TranslateEvent (e);
						if (ClientRectangle.Contains (e.X, e.Y))
							OnMouseUp (e);
					}
				}
				
				public void FireMouseMove (MouseEventArgs e) 
				{
					if (Visible) {
						e = TranslateEvent (e);
						if (ClientRectangle.Contains (e.X, e.Y))
							OnMouseMove (e);
					}
				}			
				
				MouseEventArgs TranslateEvent (MouseEventArgs e)
				{
					Point loc = PointToClient (Control.MousePosition);
		    			return new MouseEventArgs (e.Button, e.Clicks, loc.X, loc.Y, e.Delta);
				}
			}

			public ComboListBox (ComboBox owner)
			{					
				this.owner = owner;								
				top_item = 0;
				last_item = 0;
				page_size = 0;

				MouseDown += new MouseEventHandler (OnMouseDownPUW);
				MouseUp += new MouseEventHandler (OnMouseUpPUW);
				MouseMove += new MouseEventHandler (OnMouseMovePUW);				
				KeyDown += new KeyEventHandler (OnKeyDownPUW);
				SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
				SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);

				if (owner.DropDownStyle == ComboBoxStyle.Simple)
					InternalBorderStyle = BorderStyle.Fixed3D;
				else
					InternalBorderStyle = BorderStyle.FixedSingle;
			}

			protected override CreateParams CreateParams
			{
				get {
					CreateParams cp = base.CreateParams;					
					if (owner == null || owner.DropDownStyle == ComboBoxStyle.Simple)
						return cp;

					cp.Style ^= (int) WindowStyles.WS_CHILD;
					cp.Style |= (int) WindowStyles.WS_POPUP;
					cp.ExStyle |= (int) WindowExStyles.WS_EX_TOOLWINDOW | (int) WindowExStyles.WS_EX_TOPMOST;
					return cp;
				}
			}

			internal override bool InternalCapture {
				get {
					return Capture;
				}

				set {
				}
			}

			protected override void OnLostFocus(EventArgs e) {
				if (Capture) {
					HideWindow();
				}
				base.OnLostFocus (e);
			}


			int BorderWidth {
				get {
					switch (border_style) {
					case BorderStyle.Fixed3D:
						return ThemeEngine.Current.Border3DSize.Width;
					default:
						return ThemeEngine.Current.BorderSize.Width;
					}
				}
			}

			#region Private Methods			
			// Calcs the listbox area
			internal void CalcListBoxArea ()
			{				
				int width, height;
				bool show_scrollbar = false;
				
				if (owner.DropDownStyle == ComboBoxStyle.Simple) {
					Rectangle area = owner.listbox_area;
					width = area.Width;
					height = area.Height;
				}
				else { // DropDown or DropDownList
					
					width = owner.DropDownWidth;
					int count = (owner.Items.Count <= owner.MaxDropDownItems) ? owner.Items.Count : owner.MaxDropDownItems;				
					
					if (owner.DrawMode == DrawMode.OwnerDrawVariable) {						
						height = 0;
						for (int i = 0; i < count; i++) {
							height += owner.GetItemHeight (i);
						}
						
					} else	{
						height = owner.ItemHeight * count;
					}
				}
				
				if (owner.Items.Count <= owner.MaxDropDownItems) {					
					if (vscrollbar_ctrl != null)
						vscrollbar_ctrl.Visible = false;
				} else {					
					/* Need vertical scrollbar */
					if (vscrollbar_ctrl == null) {
						vscrollbar_ctrl = new VScrollBarLB ();
						vscrollbar_ctrl.Minimum = 0;
						vscrollbar_ctrl.SmallChange = 1;
						vscrollbar_ctrl.LargeChange = 1;
						vscrollbar_ctrl.Maximum = 0;
						vscrollbar_ctrl.ValueChanged += new EventHandler (VerticalScrollEvent);
						Controls.AddImplicit (vscrollbar_ctrl);
					}
					
					vscrollbar_ctrl.Height = height - 2 * BorderWidth;
							
					vscrollbar_ctrl.Location = new Point (width - vscrollbar_ctrl.Width - BorderWidth - 1, 0);

					vscrollbar_ctrl.Maximum = owner.Items.Count - (owner.DropDownStyle == ComboBoxStyle.Simple ? page_size : owner.maxdrop_items);
					show_scrollbar = vscrollbar_ctrl.Visible = true;

					int hli = HighlightedIndex;
					if (hli > 0) {
						hli = Math.Min (hli, vscrollbar_ctrl.Maximum);
						vscrollbar_ctrl.Value = hli;
					}
				}
				
				Size = new Size (width, height);
				textarea_drawable = ClientRectangle;
				textarea_drawable.Width = width;
				textarea_drawable.Height = height;				
				
				if (vscrollbar_ctrl != null && show_scrollbar)
					textarea_drawable.Width -= vscrollbar_ctrl.Width;

				last_item = LastVisibleItem ();
				page_size = textarea_drawable.Height / owner.ItemHeight;
			}			

			private void Draw (Rectangle clip, Graphics dc)
			{	
				dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (owner.BackColor), clip);

				if (owner.Items.Count > 0) {
					
					for (int i = top_item; i <= last_item; i++) {
						Rectangle item_rect = GetItemDisplayRectangle (i, top_item);

						if (!clip.IntersectsWith (item_rect))
							continue;

						DrawItemState state = DrawItemState.None;

						if (i == HighlightedIndex) {
							state |= DrawItemState.Selected;
							
							if (owner.DropDownStyle == ComboBoxStyle.DropDownList) {
								state |= DrawItemState.Focus;
							}							
						}
							
						owner.OnDrawItem (new DrawItemEventArgs (dc, owner.Font, item_rect,
							i, state, owner.ForeColor, owner.BackColor));
					}
				}
			}
			
			int highlighted_index = -1;

			public int HighlightedIndex {
				get { return highlighted_index; }
				set {
					if (highlighted_index == value)
						return;
				
    					if (highlighted_index != -1)    					
						Invalidate (GetItemDisplayRectangle (highlighted_index, top_item));
    					highlighted_index = value;
    					if (highlighted_index != -1)
						Invalidate (GetItemDisplayRectangle (highlighted_index, top_item));
				}
			}
			
			private Rectangle GetItemDisplayRectangle (int index, int top_index)
			{
				if (index < 0 || index >= owner.Items.Count)
					throw new  ArgumentOutOfRangeException ("GetItemRectangle index out of range.");

				Rectangle item_rect = new Rectangle ();
				int height = owner.GetItemHeight (index);

				item_rect.X = 0;
				item_rect.Width = textarea_drawable.Width;
				if (owner.DrawMode == DrawMode.OwnerDrawVariable) {
					item_rect.Y = 0;
					for (int i = top_index; i < index; i++)
						item_rect.Y += owner.GetItemHeight (i);
				} else
					item_rect.Y = height * (index - top_index);

				item_rect.Height = height;
				return item_rect;
			}

			public void HideWindow ()
			{
				if (owner.DropDownStyle == ComboBoxStyle.Simple)
					return;
					
				Capture = false;
				Hide ();
				owner.DropDownListBoxFinished ();
			}

			private int IndexFromPointDisplayRectangle (int x, int y)
			{
	    			for (int i = top_item; i <= last_item; i++) {
					if (GetItemDisplayRectangle (i, top_item).Contains (x, y) == true)
						return i;
				}

				return -1;
			}
			
			protected override bool IsInputKey (Keys keyData)
			{
				return owner.IsInputKey (keyData);
			}

			private int LastVisibleItem ()
			{
				Rectangle item_rect;
				int top_y = textarea_drawable.Y + textarea_drawable.Height;
				int i = 0;				
				
				for (i = top_item; i < owner.Items.Count; i++) {
					item_rect = GetItemDisplayRectangle (i, top_item);				
					if (item_rect.Y + item_rect.Height > top_y) {
						return i;
					}
				}
				return i - 1;
			}
			
			private void NavigateItemVisually (ItemNavigation navigation)
			{
				int item = -1;
				
				switch (navigation) {
				case ItemNavigation.Next:
					if (HighlightedIndex + 1 < owner.Items.Count) {
						if (HighlightedIndex + 1 > last_item) {
							top_item++;
							vscrollbar_ctrl.Value = top_item;
						}
						item = HighlightedIndex + 1;
					}
					break;
				
				case ItemNavigation.Previous:
					if (HighlightedIndex > 0) {						
						if (HighlightedIndex - 1 < top_item) {							
							top_item--;
							vscrollbar_ctrl.Value = top_item;							
						}
						item = HighlightedIndex - 1;
					}					
					break;
				
				case ItemNavigation.NextPage:
					if (HighlightedIndex + page_size - 1 >= owner.Items.Count) {
						top_item = owner.Items.Count - page_size;
						vscrollbar_ctrl.Value = top_item;
						item = owner.Items.Count - 1;
					} else {
						if (HighlightedIndex + page_size - 1  > last_item) {
							top_item = HighlightedIndex;
							vscrollbar_ctrl.Value = HighlightedIndex;
						}
						item = HighlightedIndex + page_size - 1;
					}					
					break;
				
				case ItemNavigation.PreviousPage:
					if (HighlightedIndex - page_size - 1 <= 0) {
						top_item = 0;
						vscrollbar_ctrl.Value = top_item;
						item = 0;			
					} else {
						if (HighlightedIndex - page_size - 1  < top_item) {
							top_item = HighlightedIndex - page_size - 1;
							vscrollbar_ctrl.Value = top_item;
						}
						item = HighlightedIndex - page_size - 1;
					}
					break;
					
				default:
					break;
				}	
				
				if (item != -1) {
					HighlightedIndex = item;
					owner.OnSelectionChangeCommitted (new EventArgs ());
					if (owner.DropDownStyle == ComboBoxStyle.Simple)
						owner.SetControlText (owner.GetItemText (owner.Items[item]));
				}
			}

			private void OnKeyDownPUW (object sender, KeyEventArgs e) 			
			{				
				switch (e.KeyCode) {			
				case Keys.Up:
					NavigateItemVisually (ItemNavigation.Previous);
					break;				
	
				case Keys.Down:				
					NavigateItemVisually (ItemNavigation.Next);
					break;
				
				case Keys.PageUp:
					NavigateItemVisually (ItemNavigation.PreviousPage);
					break;				
	
				case Keys.PageDown:				
					NavigateItemVisually (ItemNavigation.NextPage);
					break;
				
				default:
					break;
				}
			}
			
			public void SetTopItem (int item)
			{
				if (top_item == item)
					return;
				top_item = item;
				UpdateLastVisibleItem ();
				Refresh ();
			}			
			
			private void OnMouseDownPUW (object sender, MouseEventArgs e)
	    		{
	    			int index = IndexFromPointDisplayRectangle (e.X, e.Y);

    				if (index == -1) {    					
					if (vscrollbar_ctrl == null || !vscrollbar_ctrl.FireMouseDown (e))
		    				HideWindow ();
				} else {
					owner.OnSelectionChangeCommitted (new EventArgs ());
					owner.SelectedIndex = index;
					HighlightedIndex = index;
					HideWindow ();
				}
				
				if (owner.DropDownStyle == ComboBoxStyle.Simple) {
					owner.OnMouseDown (e);
					owner.textbox_ctrl.Focus ();
				}
			}

			private void OnMouseMovePUW (object sender, MouseEventArgs e)
			{
				if (owner.DropDownStyle == ComboBoxStyle.Simple) {
					owner.OnMouseMove (e);
					return;
				}
						
				Point pt = PointToClient (Control.MousePosition);
				int index = IndexFromPointDisplayRectangle (pt.X, pt.Y);

    				if (index != -1) {
					HighlightedIndex = index;
					return;
				}
				
				if (vscrollbar_ctrl != null)
					vscrollbar_ctrl.FireMouseMove (e);
			}
			
			private void OnMouseUpPUW (object sender, MouseEventArgs e)
			{
				if (owner.DropDownStyle == ComboBoxStyle.Simple) {
					owner.OnMouseUp (e);
					return;					
				}

	    			if (vscrollbar_ctrl != null)
		    			vscrollbar_ctrl.FireMouseUp (e);
			}

			internal override void OnPaintInternal (PaintEventArgs pevent)
			{				
				Draw (pevent.ClipRectangle,pevent.Graphics);
			}

			public bool ShowWindow ()
			{
				if (owner.DropDownStyle == ComboBoxStyle.Simple && owner.Items.Count == 0)
					return false;
					
				HighlightedIndex = owner.SelectedIndex;

				CalcListBoxArea ();				
				Show ();

				Refresh ();
				owner.OnDropDown (EventArgs.Empty);
				return true;
			}
			
			public void UpdateLastVisibleItem ()
			{
				last_item = LastVisibleItem ();
			}

			// Value Changed
			private void VerticalScrollEvent (object sender, EventArgs e)
			{				
				if (top_item == vscrollbar_ctrl.Value)
					return;

				top_item =  vscrollbar_ctrl.Value;
				UpdateLastVisibleItem ();
				Refresh ();
			}			
			
			#endregion Private Methods
		}
	}
}

