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
//	Jordi Mas i Hernandez, jordi@ximian.com
//
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;


namespace System.Windows.Forms
{

	[DefaultProperty("Items")]
	[DefaultEvent("SelectedIndexChanged")]
	[Designer ("System.Windows.Forms.Design.ComboBoxDesigner, " + Consts.AssemblySystem_Design, (string)null)]
	public class ComboBox : ListControl
	{
		private DrawMode draw_mode;
		private ComboBoxStyle dropdown_style;
		private int dropdown_width;		
		private const int preferred_height = 20;
		private int selected_index;
		private object selected_item;
		internal ObjectCollection items = null;
		private bool suspend_ctrlupdate;
		private int maxdrop_items;
		private bool integral_height;
		private bool sorted;
		internal ComboBoxInfo combobox_info;
		private readonly int def_button_width = 16;
		private bool clicked;		
		private int max_length;
		private ComboListBox listbox_ctrl;
		private StringFormat string_format;
		private TextBox textbox_ctrl;
		private bool process_textchanged_event;

		internal class ComboBoxInfo
		{
			internal int item_height; 		/* Item's height */
			internal Rectangle textarea;		/* Rectangle of the editable text area  */
			internal Rectangle textarea_drawable;	/* Rectangle of the editable text area - decorations - button if present*/
			internal Rectangle button_rect;
			internal bool show_button;		/* Is the DropDown button shown? */
			internal ButtonState button_status;	/* Drop button status */
			internal Size listbox_size;
			internal Rectangle listbox_area;	/* ListBox area in Simple combox, not used in the rest */
			internal bool droppeddown;		/* Is the associated ListBox dropped down? */

			public ComboBoxInfo ()
			{
				button_status = ButtonState.Normal;
				show_button = false;
				item_height = 0;
				droppeddown = false;
			}
		}

		internal class ComboBoxItem
		{
			internal int Index;
			internal int ItemHeight;		/* Only used for OwnerDrawVariable */

			public ComboBoxItem (int index)
			{
				Index = index;
				ItemHeight = -1;
			}			
		}

		public ComboBox ()
		{
			items = new ObjectCollection (this);
			listbox_ctrl = null;
			textbox_ctrl = null;
			combobox_info = new ComboBoxInfo ();
			combobox_info.item_height = FontHeight + 2;
			DropDownStyle = ComboBoxStyle.DropDown;
			BackColor = ThemeEngine.Current.ColorWindow;
			draw_mode = DrawMode.Normal;
			selected_index = -1;
			selected_item = null;
			maxdrop_items = 8;			
			suspend_ctrlupdate = false;
			clicked = false;
			dropdown_width = -1;
			max_length = 0;
			integral_height = true;
			process_textchanged_event = true;
			
			string_format = new StringFormat ();			

			/* Events */
			MouseDown += new MouseEventHandler (OnMouseDownCB);
			MouseUp += new MouseEventHandler (OnMouseUpCB);
			MouseMove += new MouseEventHandler (OnMouseMoveCB);
		}

		#region events
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged;		
		
		public event DrawItemEventHandler DrawItem;		
		public event EventHandler DropDown;		
		public event EventHandler DropDownStyleChanged;		
		public event MeasureItemEventHandler MeasureItem;
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint;
		
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

    				if (BackgroundImageChanged != null)
					BackgroundImageChanged (this, EventArgs.Empty);

				Refresh ();
			}
		}

		protected override CreateParams CreateParams {
			get { return base.CreateParams;}
		}

		protected override Size DefaultSize {
			get { return new Size (121, PreferredHeight); }
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

    				draw_mode = value;
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
						listbox_ctrl.Dispose ();
						Controls.Remove (listbox_ctrl);
						listbox_ctrl = null;
					}
				}

				if (dropdown_style != ComboBoxStyle.DropDownList && value == ComboBoxStyle.DropDownList) {
					if (textbox_ctrl != null) {
						Controls.Remove (textbox_ctrl);
						textbox_ctrl.Dispose ();
						textbox_ctrl = null;
					}
				}				
				
				dropdown_style = value;					
				
				if (dropdown_style == ComboBoxStyle.Simple) {
					CBoxInfo.show_button = false;					
					CreateComboListBox ();
					Controls.Add (listbox_ctrl);
				}
				else {
					CBoxInfo.show_button = true;
					CBoxInfo.button_status = ButtonState.Normal;
				}				
				
				if (dropdown_style != ComboBoxStyle.DropDownList && textbox_ctrl == null) {
					textbox_ctrl = new TextBox ();
					textbox_ctrl.TextChanged += new EventHandler (OnTextChangedEdit);
					Controls.Add (textbox_ctrl);										
				}
				
				if (DropDownStyleChanged  != null)
					DropDownStyleChanged (this, EventArgs.Empty);
    				
				CalcTextArea ();
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
				
				return CBoxInfo.droppeddown;
			}
			set {
				if (dropdown_style == ComboBoxStyle.Simple) 				
					return;
					
					
				if (value == true) {
					DropDownListBox ();
				}
				else {
					listbox_ctrl.Hide ();
				}
				
				if (DropDown != null)
					DropDown (this, EventArgs.Empty);
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
    				Refresh ();
			}
		}

		[Localizable (true)]
		public virtual int ItemHeight {
			get { return combobox_info.item_height; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("The item height value is less than zero");

				combobox_info.item_height = value;
				CalcTextArea ();
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
			get { return preferred_height; }
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override int SelectedIndex {
			get { return selected_index; }
			set {
				if (value < -2 || value >= Items.Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				if (selected_index == value)
					return;

    				selected_index = value;
    				
    				if (dropdown_style != ComboBoxStyle.DropDownList) {
    					SetControlText (Items[selected_index].ToString ());
    				}
    				
    				OnSelectedIndexChanged  (new EventArgs ());
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
				if (selected_item == value)
					return;

    				int index = Items.IndexOf (value);

				if (index == -1)
					return;

				selected_index = index;
				
				if (dropdown_style != ComboBoxStyle.DropDownList) {
					SetControlText (Items[selected_index].ToString ());
    				}
				
				OnSelectedItemChanged  (new EventArgs ());
				Refresh ();
			}
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string SelectedText {
			get {
				if (dropdown_style == ComboBoxStyle.DropDownList)
					return "";
					
				return textbox_ctrl.Text;
			}
			set {
				if (dropdown_style == ComboBoxStyle.DropDownList)
					return;
				
				if (textbox_ctrl.Text == value)
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
				if (SelectedItem != null) 
					return SelectedItem.ToString ();
								
				return base.Text;				
			}
			set {				
				if (value == null) {
					SelectedIndex = -1;
					return;
				}
				
				int index = FindString (value);
				
				if (index != -1) {
					SelectedIndex = -1;
					return;					
				}
				
				if (dropdown_style != ComboBoxStyle.DropDownList) {
					textbox_ctrl.Text = value.ToString ();
				}				
			}
		}

		#endregion Public Properties

		#region Private Properties
		internal ComboBoxInfo CBoxInfo {
			get { return combobox_info; }
		}

		#endregion Private Properties

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
			if (disposing == true) {
				if (listbox_ctrl != null) {
					listbox_ctrl.Dispose ();
					Controls.Remove (listbox_ctrl);
					listbox_ctrl = null;
				}			
			
				if (textbox_ctrl != null) {
					Controls.Remove (textbox_ctrl);
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
		}

		public int FindString (string s)
		{
			return FindString (s, 0);
		}

		public int FindString (string s, int startIndex)
		{
			for (int i = startIndex; i < Items.Count; i++) {
				if ((Items[i].ToString ()).StartsWith (s))
					return i;
			}

			return -1;
		}

		public int FindStringExact (string s)
		{
			return FindStringExact (s, 0);
		}

		public int FindStringExact (string s, int startIndex)
		{
			for (int i = startIndex; i < Items.Count; i++) {
				if ((Items[i].ToString ()).Equals (s))
					return i;
			}

			return -1;
		}

		public int GetItemHeight (int index)
		{
			if (index < 0 || index >= Items.Count )
				throw new ArgumentOutOfRangeException ("The item height value is less than zero");
				
			if (DrawMode == DrawMode.OwnerDrawVariable && IsHandleCreated == true) {
				
				if ((Items.GetComboBoxItem (index)).ItemHeight != -1) {
					return (Items.GetComboBoxItem (index)).ItemHeight;
				}
				
				MeasureItemEventArgs args = new MeasureItemEventArgs (DeviceContext, index, ItemHeight);
				OnMeasureItem (args);
				(Items.GetComboBoxItem (index)).ItemHeight = args.ItemHeight;
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
		}

		protected override void OnDisplayMemberChanged (EventArgs e)
		{
			base.OnDisplayMemberChanged (e);
		}

		protected virtual void OnDrawItem (DrawItemEventArgs e)
		{
			if (DrawItem != null && (DrawMode == DrawMode.OwnerDrawFixed || DrawMode == DrawMode.OwnerDrawVariable)) {
				DrawItem (this, e);
				return;
			}
			
			Color back_color, fore_color;
			Rectangle text_draw = e.Bounds;
			
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				back_color = ThemeEngine.Current.ColorHilight;
				fore_color = ThemeEngine.Current.ColorHilightText;
			}
			else {
				back_color = e.BackColor;
				fore_color = e.ForeColor;
			}			
							
			e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (back_color), e.Bounds);

			if (e.Index != -1) {
				e.Graphics.DrawString (Items[e.Index].ToString (), e.Font,
					ThemeEngine.Current.ResPool.GetSolidBrush (fore_color),
					text_draw, string_format);
			}
			
			if ((e.State & DrawItemState.Focus) == DrawItemState.Focus) {
				ThemeEngine.Current.CPDrawFocusRectangle (e.Graphics, e.Bounds, fore_color, back_color);
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
			
			if (textbox_ctrl != null) {
				textbox_ctrl.Font = Font;
			}
			
			combobox_info.item_height = FontHeight + 2;
			CalcTextArea ();
		}

		protected override void OnForeColorChanged (EventArgs e)
		{
			base.OnForeColorChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
			CalcTextArea ();
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
			base.OnResize (e);			
			CalcTextArea ();			
		}

		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			base.OnSelectedIndexChanged (e);

			if (SelectedIndexChanged != null)
				SelectedIndexChanged (this, e);
		}

		protected virtual void OnSelectedItemChanged (EventArgs e)
		{
			if (SelectedIndexChanged != null)
				SelectedIndexChanged (this, e);
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
				
			if (draw_mode == DrawMode.OwnerDrawVariable) {
				(Items.GetComboBoxItem (index)).ItemHeight = -1;
			}
		}

		public void Select (int start, int lenght)
		{
			if (start < 0)
				throw new ArgumentException ("Start cannot be less than zero");
				
			if (lenght < 0)
				throw new ArgumentException ("Start cannot be less than zero");
				
			if (dropdown_style == ComboBoxStyle.DropDownList)
				return;
				
			textbox_ctrl.Select (start, lenght);
		}

		public void SelectAll ()
		{
			if (dropdown_style == ComboBoxStyle.DropDownList)
				return;
				
			textbox_ctrl.SelectAll ();
		}		

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
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

			case Msg.WM_PAINT: {
				PaintEventArgs	paint_event;
				paint_event = XplatUI.PaintEventStart (Handle);
				OnPaintCB (paint_event);
				XplatUI.PaintEventEnd (Handle);
				return;
			}

			case Msg.WM_ERASEBKGND:
				m.Result = (IntPtr) 1;
				return;

			default:
				break;
			}

			base.WndProc (ref m);

		}

		#endregion Public Methods

		#region Private Methods
		
		// Calcs the text area size
		internal void CalcTextArea ()
		{			
			combobox_info.textarea = ClientRectangle;
					
			/* Edit area */
			combobox_info.textarea.Height = ItemHeight + ThemeEngine.Current.DrawComboBoxEditDecorationTop () +
					ThemeEngine.Current.DrawComboBoxEditDecorationBottom () + 2;
					// TODO: Does the +2 change at different font resolutions?
			
			/* Edit area - minus decorations (text drawable area) */
			combobox_info.textarea_drawable = combobox_info.textarea;
			combobox_info.textarea_drawable.Y += ThemeEngine.Current.DrawComboBoxEditDecorationTop ();
			combobox_info.textarea_drawable.X += ThemeEngine.Current.DrawComboBoxEditDecorationLeft ();
			combobox_info.textarea_drawable.Height -= ThemeEngine.Current.DrawComboBoxEditDecorationBottom ();
			combobox_info.textarea_drawable.Height -= ThemeEngine.Current.DrawComboBoxEditDecorationTop();
			combobox_info.textarea_drawable.Width -= ThemeEngine.Current.DrawComboBoxEditDecorationRight ();
			combobox_info.textarea_drawable.Width -= ThemeEngine.Current.DrawComboBoxEditDecorationLeft ();
			
			/* Non-drawable area */
			Region area = new Region (ClientRectangle);
			area.Exclude (combobox_info.textarea);
			RectangleF bounds = area.GetBounds (DeviceContext);
			combobox_info.listbox_area = new Rectangle ((int)bounds.X, (int)bounds.Y, 
				(int)bounds.Width, (int)bounds.Height);				
			
			if (CBoxInfo.show_button) {
				combobox_info.textarea_drawable.Width -= def_button_width;

				combobox_info.button_rect = new Rectangle (combobox_info.textarea_drawable.X + combobox_info.textarea_drawable.Width,
					combobox_info.textarea_drawable.Y, def_button_width, combobox_info.textarea_drawable.Height);				
					
			}
			
			if (dropdown_style != ComboBoxStyle.DropDownList) { /* There is an edit control*/
				if (textbox_ctrl != null) {
					textbox_ctrl.Location = new Point (combobox_info.textarea_drawable.X, combobox_info.textarea_drawable.Y);
					textbox_ctrl.Size = new Size (combobox_info.textarea_drawable.Width, combobox_info.textarea_drawable.Height);					
				}
			}
			
			if (listbox_ctrl != null && dropdown_style == ComboBoxStyle.Simple) {
				listbox_ctrl.Location = new Point (combobox_info.textarea.X, combobox_info.textarea.Y +
					combobox_info.textarea.Height);
				listbox_ctrl.CalcListBoxArea ();
			}
		}

		private void CreateComboListBox ()
		{			
			listbox_ctrl = new ComboListBox (this);			
		}
		
		internal void Draw (Rectangle clip)
		{					
			// No edit control, we paint the edit ourselfs
			if (dropdown_style == ComboBoxStyle.DropDownList) {
				DrawItemState state = DrawItemState.None;
				Rectangle item_rect = combobox_info.textarea_drawable;
				item_rect.Height = ItemHeight + 2;
				
				if (CBoxInfo.droppeddown == false) {
					state = DrawItemState.Focus |DrawItemState.Selected;
				}
				
				OnDrawItem (new DrawItemEventArgs (DeviceContext, Font, item_rect,
							selected_index, state, ForeColor, BackColor));
			}						
			
			if (clip.IntersectsWith (combobox_info.listbox_area) == true) {
				DeviceContext.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (Parent.BackColor), 
						combobox_info.listbox_area);
			}
			
			if (CBoxInfo.show_button) {
				DeviceContext.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorButtonFace),
					combobox_info.button_rect);

				ThemeEngine.Current.CPDrawComboButton (DeviceContext,
					combobox_info.button_rect, combobox_info.button_status);
			}			
			
			ThemeEngine.Current.DrawComboBoxEditDecorations (DeviceContext, this, combobox_info.textarea);
		}

		internal void DropDownListBox ()
		{
			if (DropDownStyle == ComboBoxStyle.Simple)
				return;			
			
			if (listbox_ctrl == null) {
    				CreateComboListBox ();
    			}

			listbox_ctrl.Location = PointToScreen (new Point (combobox_info.textarea.X, combobox_info.textarea.Y +
				combobox_info.textarea.Height));
						
    			if (listbox_ctrl.ShowWindow () == true) {    				
    				CBoxInfo.droppeddown = true;        				
    			}
    			
    			combobox_info.button_status = ButtonState.Pushed;				
    			if (dropdown_style == ComboBoxStyle.DropDownList) {
    				Invalidate (combobox_info.textarea_drawable);
    			}
		}
		
		internal void DropDownListBoxFinished ()
		{
			if (DropDownStyle == ComboBoxStyle.Simple)
				return;			
				
			combobox_info.button_status = ButtonState.Normal;
			Invalidate (combobox_info.button_rect);
			CBoxInfo.droppeddown = false;
			clicked = false;			
		}
		
		private int FindStringCaseInsensitive (string search)
		{			
			for (int i = 0; i < Items.Count; i++) 
			{				
				if (String.Compare (Items[i].ToString (), 0, search, 0, search.Length, true) == 0)
					return i;
			}

			return -1;
		}

		internal virtual void OnMouseDownCB (object sender, MouseEventArgs e)
    		{    			
    			/* Click On button*/    			
    			Rectangle hit_rect;
    			
    			if (dropdown_style == ComboBoxStyle.DropDownList) {
    				hit_rect = combobox_info.textarea;
    			} else {
    				hit_rect = combobox_info.button_rect;
    			}    			
    			
			if (hit_rect.Contains (e.X, e.Y)) {
				if (clicked == false) {
	    				clicked = true;
	    				DropDownListBox ();	    				
	    			} else {
	    				listbox_ctrl.Hide ();
	    				DropDownListBoxFinished ();
	    			}
	    			
	    			Invalidate (combobox_info.button_rect);
    			}
    		}
    		
    		internal virtual void OnMouseMoveCB (object sender, MouseEventArgs e)
    		{    			
    			/* When there are no items, act as a regular button */
    			if (clicked == true && Items.Count == 0 &&
    				 combobox_info.button_rect.Contains (e.X, e.Y) == false) {
    				DropDownListBoxFinished ();
    			}
    		}

    		internal virtual void OnMouseUpCB (object sender, MouseEventArgs e)
    		{
    			/* Click on button*/
    			if (clicked == true && combobox_info.button_rect.Contains (e.X, e.Y)) {    				
    				DropDownListBoxFinished ();
    			}
    		}

		private void OnPaintCB (PaintEventArgs pevent)
		{
			if (Width <= 0 || Height <=  0 || Visible == false || suspend_ctrlupdate == true)
    				return;
    				
    			/* Copies memory drawing buffer to screen*/
			Draw (ClientRectangle);			
			pevent.Graphics.DrawImage (ImageBuffer, ClientRectangle, ClientRectangle, GraphicsUnit.Pixel);

			if (Paint != null)
				Paint (this, pevent);
		}
		
		private void OnTextChangedEdit (object sender, EventArgs e)
		{
			if (process_textchanged_event == false)
				return; 
				
			int item = FindStringCaseInsensitive (textbox_ctrl.Text);
			
			if (item == -1)
				return;
			
			listbox_ctrl.SetTopItem (item);
			listbox_ctrl.SetHighLightedItem (item);
		}
		
		internal void SetControlText (string s)
		{		
			process_textchanged_event = false; 
    			textbox_ctrl.Text = s;
    			process_textchanged_event = true;
    		}
		
		private void UpdatedItems ()
		{
			if (dropdown_style != ComboBoxStyle.Simple)
				return;
				
			listbox_ctrl.UpdateLastVisibleItem ();
			listbox_ctrl.CalcListBoxArea ();
			listbox_ctrl.Refresh ();
		}

		#endregion Private Methods


		/*
			ComboBox.ObjectCollection
		*/
		[ListBindableAttribute (false)]
		public class ObjectCollection : IList, ICollection, IEnumerable
		{

			private ComboBox owner;
			internal ArrayList object_items = new ArrayList ();
			internal ArrayList combobox_items = new ArrayList ();

			public ObjectCollection (ComboBox owner)
			{
				this.owner = owner;
			}

			#region Public Properties
			public virtual int Count {
				get { return object_items.Count; }
			}

			public virtual bool IsReadOnly {
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
			
			internal ArrayList ListBoxItems {
				get { return combobox_items;}
				set {
					combobox_items = value;
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

			public virtual void Clear ()
			{
				object_items.Clear ();
				combobox_items.Clear ();
				owner.UpdatedItems ();
				owner.selected_index = -1;				
			}
			
			public virtual bool Contains (object obj)
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

			public virtual IEnumerator GetEnumerator ()
			{
				return object_items.GetEnumerator ();
			}

			int IList.Add (object item)
			{
				return Add (item);
			}

			public virtual int IndexOf (object value)
			{
				return object_items.IndexOf (value);
			}

			public virtual void Insert (int index,  object item)
			{
				if (index < 0 || index >= Count)
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
				ListBoxItems = new_items.ListBoxItems;
				
				if (sel_item != null) {
					int idx = IndexOf (sel_item);
					owner.selected_index = idx;
					owner.listbox_ctrl.SetHighLightedItem (idx);
				}
												
				owner.EndUpdate ();	// Calls UpdateItemInfo				
			}

			public virtual void Remove (object value)
			{
				RemoveAt (IndexOf (value));				
			}

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				object_items.RemoveAt (index);
				combobox_items.RemoveAt (index);
				owner.UpdatedItems ();
			}
			#endregion Public Methods

			#region Private Methods
			private int AddItem (object item)
			{
				int cnt = object_items.Count;
				object_items.Add (item);
				combobox_items.Add (new ComboBox.ComboBoxItem (cnt));				
				return cnt;
			}
			
			internal void AddRange (IList items)
			{
				foreach (object mi in items)
					AddItem (mi);
										
				owner.UpdatedItems ();
			}

			internal ComboBox.ComboBoxItem GetComboBoxItem (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				return (ComboBox.ComboBoxItem) combobox_items[index];
			}

			internal void SetComboBoxItem (ComboBox.ComboBoxItem item, int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				combobox_items[index] = item;
			}

			#endregion Private Methods
		}

		/*
			class ComboListBox
		*/
		internal class ComboListBox : Control
		{
			private ComboBox owner;			
			private VScrollBarLB vscrollbar_ctrl;
			private int top_item;			/* First item that we show the in the current page */
			private int last_item;			/* Last visible item */
			private int highlighted_item;		/* Item that is currently selected */
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
				
				public void FireMouseDown (MouseEventArgs e) 
				{
					OnMouseDown (e);
				}	
				
				public void FireMouseUp (MouseEventArgs e) 
				{
					OnMouseUp (e);
				}
				
				public void FireMouseMove (MouseEventArgs e) 
				{
					OnMouseMove (e);
				}
			}

			public ComboListBox (ComboBox owner) : base ()
			{	
				this.owner = owner;								
				top_item = 0;
				last_item = 0;
				page_size = 0;
				highlighted_item = -1;

				MouseDown += new MouseEventHandler (OnMouseDownPUW);
				MouseUp += new MouseEventHandler (OnMouseUpPUW);
				MouseMove += new MouseEventHandler (OnMouseMovePUW);				
				KeyDown += new KeyEventHandler (OnKeyDownPUW);
				Paint += new PaintEventHandler (OnPaintPUW);				
				SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
				SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
				
			}

			protected override CreateParams CreateParams
			{
				get {
					CreateParams cp = base.CreateParams;					
					if (owner != null && owner.DropDownStyle != ComboBoxStyle.Simple) {
						cp.Style = unchecked ((int)(WindowStyles.WS_POPUP | WindowStyles.WS_VISIBLE | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN));
						cp.ExStyle |= (int)(WindowStyles.WS_EX_TOOLWINDOW | WindowStyles.WS_EX_TOPMOST);
					}					
					return cp;
				}
			}

			#region Private Methods

			protected override void CreateHandle ()
			{			
				base.CreateHandle ();				
			}

			// Calcs the listbox area
			internal void CalcListBoxArea ()
			{				
				int width, height;
				int item_height = owner.ItemHeight;
				
				if (owner.DropDownStyle == ComboBoxStyle.Simple) {
					width = owner.CBoxInfo.listbox_area.Width;
					height = owner.CBoxInfo.listbox_area.Height;

					if (owner.IntegralHeight == true) {
						int remaining = (height -
							ThemeEngine.Current.DrawComboListBoxDecorationBottom (owner.DropDownStyle) -
							ThemeEngine.Current.DrawComboListBoxDecorationTop (owner.DropDownStyle)) %
							(item_height - 2);							
		
						if (remaining > 0) {
							height -= remaining;							
						}
					}
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
						height = (item_height - 2) * count;
					}
					
					
					height += ThemeEngine.Current.DrawComboListBoxDecorationBottom (owner.DropDownStyle);				
					height += ThemeEngine.Current.DrawComboListBoxDecorationTop (owner.DropDownStyle);
				}
				
				if (owner.Items.Count <= owner.MaxDropDownItems) {					
					
					/* Does not need vertical scrollbar*/
					if (vscrollbar_ctrl != null) {
						vscrollbar_ctrl.Dispose ();
						Controls.Remove (vscrollbar_ctrl);
						vscrollbar_ctrl = null;
					}					
				}
				else {
					/* Need vertical scrollbar */
					vscrollbar_ctrl = new VScrollBarLB ();
					vscrollbar_ctrl.Minimum = 0;
					vscrollbar_ctrl.SmallChange = 1;
					vscrollbar_ctrl.LargeChange = 1;
					vscrollbar_ctrl.Maximum = 0;
					vscrollbar_ctrl.ValueChanged += new EventHandler (VerticalScrollEvent);
					
					vscrollbar_ctrl.Height = height - ThemeEngine.Current.DrawComboListBoxDecorationBottom (owner.DropDownStyle) -
						ThemeEngine.Current.DrawComboListBoxDecorationTop (owner.DropDownStyle);
						
					vscrollbar_ctrl.Location = new Point (width - vscrollbar_ctrl.Width - ThemeEngine.Current.DrawComboListBoxDecorationRight (owner.DropDownStyle), 
						ThemeEngine.Current.DrawComboListBoxDecorationTop (owner.DropDownStyle));					
						
					vscrollbar_ctrl.Maximum = owner.Items.Count - owner.MaxDropDownItems;
					Controls.Add (vscrollbar_ctrl);
				}
				
				Size = new Size (width, height);
				textarea_drawable = ClientRectangle;
				textarea_drawable.Width = width;
				textarea_drawable.Height = height;				

				// Exclude decorations
				textarea_drawable.X += ThemeEngine.Current.DrawComboListBoxDecorationLeft (owner.DropDownStyle);
				textarea_drawable.Y += ThemeEngine.Current.DrawComboListBoxDecorationTop (owner.DropDownStyle);
				textarea_drawable.Width -= ThemeEngine.Current.DrawComboListBoxDecorationRight (owner.DropDownStyle);
				textarea_drawable.Width -= ThemeEngine.Current.DrawComboListBoxDecorationLeft (owner.DropDownStyle);
				textarea_drawable.Height -= ThemeEngine.Current.DrawComboListBoxDecorationBottom (owner.DropDownStyle);				
				textarea_drawable.Height -= ThemeEngine.Current.DrawComboListBoxDecorationTop (owner.DropDownStyle);
				
				if (vscrollbar_ctrl != null)
					textarea_drawable.Width -= vscrollbar_ctrl.Width;

				last_item = LastVisibleItem ();				
				page_size = textarea_drawable.Height / (item_height - 2);				
			}			

			private void Draw (Rectangle clip)
			{	
				DeviceContext.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
					(owner.BackColor), ClientRectangle);

				if (owner.Items.Count > 0) {
					Rectangle item_rect;
					DrawItemState state = DrawItemState.None;
					
					for (int i = top_item; i <= last_item; i++) {
						item_rect = GetItemDisplayRectangle (i, top_item);

						if (clip.IntersectsWith (item_rect) == false)
							continue;

						/* Draw item */
						state = DrawItemState.None;

						if (i == highlighted_item) {
							state |= DrawItemState.Selected;
							
							if (owner.DropDownStyle == ComboBoxStyle.DropDownList) {
								state |= DrawItemState.Focus;
							}							
						}
							
						owner.OnDrawItem (new DrawItemEventArgs (DeviceContext, owner.Font, item_rect,
							i, state, owner.ForeColor, owner.BackColor));
					}
				}			
				
				ThemeEngine.Current.DrawComboListBoxDecorations (DeviceContext, owner, ClientRectangle);
			}

			private Rectangle GetItemDisplayRectangle (int index, int first_displayble)
			{
				if (index < 0 || index >= owner.Items.Count)
					throw new  ArgumentOutOfRangeException ("GetItemRectangle index out of range.");

				Rectangle item_rect = new Rectangle ();
				int height = owner.GetItemHeight (index);

				item_rect.X = ThemeEngine.Current.DrawComboListBoxDecorationRight (owner.DropDownStyle);
				item_rect.Width = textarea_drawable.Width;
				item_rect.Y = 2 + ((height - 2) * (index - first_displayble));
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
				case ItemNavigation.Next: {
					if (highlighted_item + 1 < owner.Items.Count) {
						
						if (highlighted_item + 1 > last_item) {
							top_item++;
							vscrollbar_ctrl.Value = top_item;
						}
						item = highlighted_item + 1;
					}
					break;
				}
				
				case ItemNavigation.Previous: {
					if (highlighted_item > 0) {						
						
						if (highlighted_item - 1 < top_item) {							
							top_item--;
							vscrollbar_ctrl.Value = top_item;							
						}
						item = highlighted_item - 1;
					}					
					break;
				}
				
				case ItemNavigation.NextPage: {
					if (highlighted_item + page_size - 1 >= owner.Items.Count) {
						top_item = owner.Items.Count - page_size;
						vscrollbar_ctrl.Value = top_item; 						
						item = owner.Items.Count - 1;
					}
					else {
						if (highlighted_item + page_size - 1  > last_item) {
							top_item = highlighted_item;
							vscrollbar_ctrl.Value = highlighted_item;
						}
					
						item = highlighted_item + page_size - 1;
					}					
					break;
				}
				
				case ItemNavigation.PreviousPage: {					
					
					/* Go to the first item*/
					if (highlighted_item - (page_size - 1) <= 0) {
																		
						top_item = 0;
						vscrollbar_ctrl.Value = top_item;
						item = 0;			
					}
					else { /* One page back */
						if (highlighted_item - (page_size - 1)  < top_item) {
							top_item = highlighted_item - (page_size - 1);
							vscrollbar_ctrl.Value = top_item;
						}
					
						item = highlighted_item - (page_size - 1);
					}
					
					break;
				}				
					
				default:
					break;
				}	
				
				if (item != -1) {
					SetHighLightedItem (item);
					
					owner.OnSelectionChangeCommitted (new EventArgs ());
					
					if (owner.DropDownStyle == ComboBoxStyle.Simple) {
						owner.SetControlText (owner.Items[item].ToString ());
					}
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
			
			public void SetHighLightedItem (int index)
			{
				Rectangle invalidate;
				
				if (highlighted_item == index)
					return;
				
				/* Previous item */
    				if (highlighted_item != -1) {
					invalidate = GetItemDisplayRectangle (highlighted_item, top_item);
	    				if (ClientRectangle.Contains (invalidate))
	    					Invalidate (invalidate);
	    			}
				
    				highlighted_item = index;

    				 /* Current item */
    				invalidate = GetItemDisplayRectangle (highlighted_item, top_item);
    				if (ClientRectangle.Contains (invalidate))
    					Invalidate (invalidate);   					
    				
			}			

			public void SetTopItem (int item)
			{
				top_item = item;
				UpdateLastVisibleItem ();
				Refresh ();
			}			
			
			private void OnMouseDownPUW (object sender, MouseEventArgs e)
	    		{	
	    			Rectangle scrollbar_screenrect;
	    			Point mouse_screen, scrollbar_screen;
	    			mouse_screen = PointToScreen (new Point (e.X, e.Y));
	    				
    				/* Click on an element ? */    				
	    			int index = IndexFromPointDisplayRectangle (e.X, e.Y);
    				if (index != -1) {    					
					owner.SelectedIndex = index;
					SetHighLightedItem (index);
					owner.OnSelectionChangeCommitted (new EventArgs ());
					HideWindow ();
					return;
				}
				
				if (owner.DropDownStyle == ComboBoxStyle.Simple)
					return;		
				
				/* Reroute event to scrollbar */				
				if (vscrollbar_ctrl != null) {				
		    			scrollbar_screenrect = vscrollbar_ctrl.ClientRectangle;
		    			scrollbar_screen = PointToScreen (vscrollbar_ctrl.Location);
		    			scrollbar_screenrect.X = scrollbar_screen.X;
		    			scrollbar_screenrect.Y = scrollbar_screen.Y;
		    			
		    			if (scrollbar_screenrect.Contains (mouse_screen)){	    				
		    				Point pnt_client = vscrollbar_ctrl.PointToClient (mouse_screen);	    				
		    				vscrollbar_ctrl.FireMouseDown (new MouseEventArgs (e.Button, e.Clicks,
		    					pnt_client.X, pnt_client.Y, e.Delta));	    					
		    			} else	{ /* Click in a non-client area*/
		    				HideWindow ();	    			
		    			}	    			
		    		} else	{ /* Click in a non-client area*/
		    			HideWindow ();
		    		}
			}

			private void OnMouseMovePUW (object sender, MouseEventArgs e)
			{			
				if (owner.DropDownStyle == ComboBoxStyle.Simple)
					return;
						
				int index = IndexFromPointDisplayRectangle (e.X, e.Y);

    				if (index != -1) {
					SetHighLightedItem (index);
					return;
				}
				
				if (owner.DropDownStyle == ComboBoxStyle.Simple)
					return;		
				
				/* Reroute event to scrollbar */
				if (vscrollbar_ctrl != null) {	
					Rectangle scrollbar_screenrect;
		    			Point mouse_screen, scrollbar_screen;
		    			mouse_screen = PointToScreen (new Point (e.X, e.Y));
		    			
		    			scrollbar_screenrect = vscrollbar_ctrl.ClientRectangle;
		    			scrollbar_screen = PointToScreen (vscrollbar_ctrl.Location);
		    			scrollbar_screenrect.X = scrollbar_screen.X;
		    			scrollbar_screenrect.Y = scrollbar_screen.Y;
		    			
		    			if (scrollbar_screenrect.Contains (mouse_screen)){	    				
		    				Point pnt_client = vscrollbar_ctrl.PointToClient (mouse_screen);
		    				
		    				vscrollbar_ctrl.FireMouseMove (new MouseEventArgs (e.Button, e.Clicks,
		    					pnt_client.X, pnt_client.Y, e.Delta));
		    			}
		    		}	    			
			}
			
			private void OnMouseUpPUW (object sender, MouseEventArgs e)
			{
				if (owner.DropDownStyle == ComboBoxStyle.Simple)
					return;					
					
				/* Reroute event to scrollbar */	
				Rectangle scrollbar_screenrect;
	    			Point mouse_screen, scrollbar_screen;
	    			mouse_screen = PointToScreen (new Point (e.X, e.Y));
	    			
	    			if (vscrollbar_ctrl != null) {	
		    			scrollbar_screenrect = vscrollbar_ctrl.ClientRectangle;
		    			scrollbar_screen = PointToScreen (vscrollbar_ctrl.Location);
		    			scrollbar_screenrect.X = scrollbar_screen.X;
		    			scrollbar_screenrect.Y = scrollbar_screen.Y;
		    			
		    			if (scrollbar_screenrect.Contains (mouse_screen)){	    				
		    				Point pnt_client = vscrollbar_ctrl.PointToClient (mouse_screen);	    				
		    				
		    				vscrollbar_ctrl.FireMouseUp (new MouseEventArgs (e.Button, e.Clicks,
		    					pnt_client.X, pnt_client.Y, e.Delta));
		    			}
		    		}
			}

			private void OnPaintPUW (Object o, PaintEventArgs pevent)
			{
				if (Width <= 0 || Height <=  0 || Visible == false)
	    				return;

				Draw (pevent.ClipRectangle);
				pevent.Graphics.DrawImage (ImageBuffer, pevent.ClipRectangle, pevent.ClipRectangle, GraphicsUnit.Pixel);
			}

			public bool ShowWindow ()
			{
				if (owner.DropDownStyle != ComboBoxStyle.Simple && owner.Items.Count == 0)
					return false;
				
				CalcListBoxArea ();				
				Show ();
				
				if (owner.DropDownStyle != ComboBoxStyle.Simple) {
					Capture = true;
				}
				
				Refresh ();
				
				if (owner.DropDown != null) {
					owner.DropDown (owner, EventArgs.Empty);
				}
				
				return true;
			}
			
			public void UpdateLastVisibleItem ()
			{
				last_item = LastVisibleItem ();
			}

			// Value Changed
			private void VerticalScrollEvent (object sender, EventArgs e)
			{				
				top_item =  vscrollbar_ctrl.Value;
				UpdateLastVisibleItem ();
				Refresh ();
			}			

			#endregion Private Methods
		}
	}
}

