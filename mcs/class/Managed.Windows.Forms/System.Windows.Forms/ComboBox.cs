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

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Forms
{

	public class ComboBox : ListControl
	{
		private DrawMode draw_mode;
		private ComboBoxStyle dropdown_style;
		private int dropdown_width;
		private int max_length;
		private int preferred_height;
		private int selected_index;
		private object selected_item;
		internal ObjectCollection items;
		private bool suspend_ctrlupdate;
		private int maxdrop_items;
		private bool integral_height;
		private bool sorted;
		internal ComboBoxInfo combobox_info;
		private readonly int def_button_width = 16;
		private bool clicked;
		private ListBoxPopUp listbox_popup;
		private StringFormat string_format;

		internal class ComboBoxInfo
		{
			internal int item_height; 		/* Item's height */
			internal Rectangle textarea_rect;	/* Rectangle of the editable text area - decorations */
			internal Rectangle button_rect;
			internal bool show_button;		/* Is the DropDown button shown? */
			internal Rectangle client_rect;		/* Client Rectangle. Usually = ClientRectangle except when IntegralHeight has been applied*/
			internal ButtonState button_status;	/* Drop button status */
			internal Size listbox_size;

			public ComboBoxInfo ()
			{
				button_status = ButtonState.Normal;
				show_button = false;
				item_height = 0;
			}
		}

		internal class ComboBoxItem
		{
			internal int Index;

			public ComboBoxItem (int index)
			{
				Index = index;
			}
		}

		public ComboBox ()
		{
			BackColor = ThemeEngine.Current.ColorWindow;
			draw_mode = DrawMode.Normal;
			selected_index = -1;
			selected_item = null;
			maxdrop_items = 8;
			combobox_info = new ComboBoxInfo ();
			combobox_info.item_height = FontHeight;
			suspend_ctrlupdate = false;
			clicked = false;

			items = new ObjectCollection (this);
			string_format = new StringFormat ();
			CBoxInfo.show_button = true;
			listbox_popup = null;

			/* Events */
			MouseDown += new MouseEventHandler (OnMouseDownCB);
			MouseUp += new MouseEventHandler (OnMouseUpCB);

		}

		#region Events
		public new event EventHandler BackgroundImageChanged;
		public event DrawItemEventHandler DrawItem;
		public event EventHandler DropDown;
		public event EventHandler DropDownStyleChanged;
		public event MeasureItemEventHandler MeasureItem;
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

		public ComboBoxStyle DropDownStyle {
			get { return dropdown_style; }

    			set {
				if (!Enum.IsDefined (typeof (ComboBoxStyle), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for ComboBoxStyle", value));

				if (dropdown_style == value)
					return;

    				dropdown_style = value;
				Refresh ();
    			}
		}

		public int DropDownWidth {
			get { return dropdown_width; }
			set {

				if (dropdown_width == value)
					return;

    				dropdown_width = value;
				Refresh ();
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

		public bool IntegralHeight {
			get { return integral_height; }
			set {
				if (integral_height == value)
					return;

    				integral_height = value;
			}
		}

		public virtual int ItemHeight {
			get { return combobox_info.item_height; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("The item height value is less than zero");

				combobox_info.item_height = value;
				Refresh ();
			}
		}


		public ComboBox.ObjectCollection Items {
			get { return items; }
		}

		public int MaxDropDownItems {
			get { return maxdrop_items; }
			set {
				if (maxdrop_items == value)
					return;

    				maxdrop_items = value;
			}
		}

		public int MaxLength {
			get { return max_length; }
			set {
				if (max_length == value)
					return;

    				max_length = value;
			}
		}

		public int PreferredHeight {
			get { return preferred_height; }
		}

		public override int SelectedIndex {
			get { return selected_index; }
			set {
				if (selected_index == value)
					return;

    				selected_index = value;
			}
		}

		public object SelectedItem {
			get { return selected_item; }
			set {
				if (selected_item == value)
					return;

    				selected_item = value;
			}
		}
		
		public string SelectedText {
			get {throw new NotImplementedException ();}
			set {}
		}

		public int SelectionLength {
			get {throw new NotImplementedException ();}
			set {}
		}

		public int SelectionStart {
			get {throw new NotImplementedException (); }
			set {}
		}

		public bool Sorted {
				get { return sorted; }

    			set {
				if (sorted == value)
					return;

    				sorted = value;
    			}
    		}

		public override string Text {
			get { return ""; /*throw new NotImplementedException ();*/ }
			set {}
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

		protected virtual void Dispose (bool disposing)
		{

		}

		public void EndUpdate ()
		{
			suspend_ctrlupdate = false;
			Refresh ();
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
			throw new NotImplementedException ();
		}

		protected override bool IsInputKey (Keys keyData)
		{
			return false;
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
			if (DrawItem != null && (DrawMode == DrawMode.OwnerDrawFixed || DrawMode == DrawMode.OwnerDrawVariable))
				DrawItem (this, e);

			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
					(ThemeEngine.Current.ColorHilight), e.Bounds);

				e.Graphics.DrawString (Items[e.Index].ToString (), e.Font,
					ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorHilightText),
					e.Bounds, string_format);
			}
			else {
				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
					(e.BackColor), e.Bounds);

				e.Graphics.DrawString (Items[e.Index].ToString (), e.Font,
					ThemeEngine.Current.ResPool.GetSolidBrush (e.ForeColor),
					e.Bounds, string_format);
			}
		}

		protected virtual void OnDropDownStyleChanged (EventArgs e)
		{

		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
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

		}

		protected virtual void OnMeasureItem (MeasureItemEventArgs e)
		{

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


		}

		protected override void OnSelectedValueChanged (EventArgs e)
		{

		}

		protected virtual void OnSelectionChangeCommitted (EventArgs e)
		{

		}

		protected override void RefreshItem (int index)
		{

		}


		protected virtual void Select (int start, int lenght)
		{

		}

		public void SelectAll ()
		{

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

		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
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
			combobox_info.textarea_rect = ClientRectangle;
			combobox_info.textarea_rect.Y += ThemeEngine.Current.DrawComboBoxDecorationTop ();
			combobox_info.textarea_rect.X += ThemeEngine.Current.DrawComboBoxDecorationLeft ();
			combobox_info.textarea_rect.Height -= ThemeEngine.Current.DrawComboBoxDecorationBottom ();
			combobox_info.textarea_rect.Width -= ThemeEngine.Current.DrawComboBoxDecorationRight ();

			if (CBoxInfo.show_button) {
				combobox_info.textarea_rect.Width -= def_button_width;

				combobox_info.button_rect = new Rectangle (combobox_info.textarea_rect.X + combobox_info.textarea_rect.Width,
					combobox_info.textarea_rect.Y, 	def_button_width, combobox_info.textarea_rect.Height);
			}
		}

		internal void CreateListBoxPopUp ()
		{
			listbox_popup = new ListBoxPopUp (this);
			listbox_popup.Location = PointToScreen (new Point (ClientRectangle.X, ClientRectangle.Y + ClientRectangle.Height));
			listbox_popup.Size = combobox_info.listbox_size;
		}

		internal void Draw (Rectangle clip)
		{
			// Fill edit area
			DeviceContext.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), ClientRectangle);

			if (CBoxInfo.show_button) {
				Console.WriteLine ("Draw ButtonStatus {0}", combobox_info.button_status);
				DeviceContext.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorButtonFace),
					combobox_info.button_rect);
										
				ThemeEngine.Current.CPDrawComboButton (DeviceContext,
					combobox_info.button_rect, combobox_info.button_status);
			}

			ThemeEngine.Current.DrawComboBoxDecorations (DeviceContext, this);
		}


		internal virtual void OnMouseDownCB (object sender, MouseEventArgs e)
    		{
    			/* Click On button*/
    			if (clicked == false && combobox_info.button_rect.Contains (e.X, e.Y)) {

    				clicked = true;

    				if (combobox_info.button_status == ButtonState.Normal) {
    						combobox_info.button_status = ButtonState.Pushed;
    				}
					else {
    					if (combobox_info.button_status == ButtonState.Pushed) {
    						combobox_info.button_status = ButtonState.Normal;
    					}
    				}

    				if (combobox_info.button_status == ButtonState.Pushed) {
    					if (listbox_popup == null)
    						CreateListBoxPopUp ();

					listbox_popup.CalcListBoxArea ();
    					listbox_popup.Show ();
					listbox_popup.Refresh ();
    				}

    				Console.WriteLine ("ComboBox.OnMouseDownCB clicked {0}", combobox_info.button_status);
    				Invalidate (combobox_info.button_rect);
    			}
    		}

    		internal virtual void OnMouseUpCB (object sender, MouseEventArgs e)
    		{
    			/* Click on button*/
    			if (clicked == true && combobox_info.button_rect.Contains (e.X, e.Y)) {

    				clicked = false;
    			}

    		}

		private void OnPaintCB (PaintEventArgs pevent)
		{
			Console.WriteLine ("OnPaintCB");
			if (Width <= 0 || Height <=  0 || Visible == false || suspend_ctrlupdate == true)
    				return;

			/* Copies memory drawing buffer to screen*/
			Draw (ClientRectangle);
			pevent.Graphics.DrawImage (ImageBuffer, ClientRectangle, ClientRectangle, GraphicsUnit.Pixel);

			if (Paint != null)
				Paint (this, pevent);
		}

		#endregion Private Methods


		/*
			ComboBox.ObjectCollection
		*/
		public class ObjectCollection : IList, ICollection, IEnumerable
		{

			private ComboBox owner;
			internal ArrayList object_items = new ArrayList ();
			internal ArrayList listbox_items = new ArrayList ();

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

			#region Public Methods
			public int Add (object item)
			{
				int idx;

				idx = AddItem (item);
				return idx;
			}

			public void AddRange (object[] items)
			{
				int cnt = Count;

				foreach (object mi in items)
					AddItem (mi);
			}


			public virtual void Clear ()
			{
				object_items.Clear ();
				listbox_items.Clear ();

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
				throw new NotImplementedException ();
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
				listbox_items.RemoveAt (index);
				//owner.UpdateItemInfo (false, -1, -1);
			}
			#endregion Public Methods

			#region Private Methods
			private int AddItem (object item)
			{
				int cnt = object_items.Count;
				object_items.Add (item);
				listbox_items.Add (new ComboBox.ComboBoxItem (cnt));
				return cnt;
			}

			internal ComboBox.ComboBoxItem GetComboBoxItem (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				return (ComboBox.ComboBoxItem) listbox_items[index];
			}

			internal void SetComboBoxItem (ComboBox.ComboBoxItem item, int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				listbox_items[index] = item;
			}

			#endregion Private Methods
		}

		/*
			class ListBoxPopUp
		*/
		internal class ListBoxPopUp : Control
		{
			private ComboBox owner;
			private bool need_vscrollbar;
			private VScrollBar vscrollbar_ctrl;
			private int top_item;			/* First item that we show the in the current page */
			private int last_item;			/* Last visible item */
			private Rectangle textarea_rect;	/* Rectangle of the drawable text area */


			public ListBoxPopUp (ComboBox owner): base ()
			{
				this.owner = owner;
				need_vscrollbar = false;
				top_item = 0;
				last_item = 0;

				MouseDown += new MouseEventHandler (OnMouseDownPUW);
				MouseMove += new MouseEventHandler (OnMouseMovePUW);
				MouseUp += new MouseEventHandler (OnMouseUpPUW);
				Paint += new PaintEventHandler (OnPaintPUW);
				SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
				SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);

				/* Vertical scrollbar */
				vscrollbar_ctrl = new VScrollBar ();
				vscrollbar_ctrl.Minimum = 0;
				vscrollbar_ctrl.SmallChange = 1;
				vscrollbar_ctrl.LargeChange = 1;
				vscrollbar_ctrl.Maximum = 0;
				//vscrollbar_ctrl.ValueChanged += new EventHandler (VerticalScrollEvent);
				vscrollbar_ctrl.Visible = false;

			}

			protected override CreateParams CreateParams
			{
				get {
					CreateParams cp = base.CreateParams;
					cp.Style = unchecked ((int)(WindowStyles.WS_POPUP | WindowStyles.WS_VISIBLE | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN));
					cp.ExStyle |= (int)WindowStyles.WS_EX_TOOLWINDOW;
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

				width = owner.ClientRectangle.Width;

				if (owner.Items.Count <= owner.MaxDropDownItems) {
					height = owner.ItemHeight * owner.Items.Count;
					need_vscrollbar = false;
				}
				else {
					height = owner.ItemHeight * owner.MaxDropDownItems;
					need_vscrollbar = true;

					vscrollbar_ctrl.Height = height - 2;
					vscrollbar_ctrl.Location = new Point (width - vscrollbar_ctrl.Width - 2, 1);
				}

				if (vscrollbar_ctrl.Visible != need_vscrollbar)
					vscrollbar_ctrl.Visible = need_vscrollbar;

				Size = new Size (width, height);
				textarea_rect = ClientRectangle;

				// Exclude decorations
				textarea_rect.X += 1;
				textarea_rect.Y += 1;
				textarea_rect.Width -= 1;
				textarea_rect.Height -= 1;

				if (need_vscrollbar)
					textarea_rect.Width -= vscrollbar_ctrl.Width;

				last_item = LastVisibleItem ();
				
			}

			private void Draw (Rectangle clip)
			{
				Console.WriteLine ("ListBoxPopUp.Draw top {0} last {1}", top_item, last_item);
				
				Rectangle cl = ClientRectangle;

				if (owner.Items.Count > 0) {
					Rectangle item_rect;
					DrawItemState state = DrawItemState.None;

					for (int i = top_item; i < last_item; i++) {
						item_rect = GetItemDisplayRectangle (i, top_item);

						if (clip.IntersectsWith (item_rect) == false)
							continue;

						/* Draw item */
						state = DrawItemState.None;
						
						Console.WriteLine ("ListBoxPopUp.DrawItem {0}", item_rect);

						owner.OnDrawItem (new DrawItemEventArgs (DeviceContext, owner.Font, item_rect,
							i, state, owner.ForeColor, owner.BackColor));
					}
				}

				//DeviceContext.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
				//	(owner.BackColor), ClientRectangle);

				DeviceContext.DrawRectangle (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorWindowFrame),
					cl.X, cl.Y, cl.Width - 1, cl.Height - 1);
			}

			private Rectangle GetItemDisplayRectangle (int index, int first_displayble)
			{
				if (index < 0 || index >= owner.Items.Count)
					throw new  ArgumentOutOfRangeException ("GetItemRectangle index out of range.");

				Rectangle item_rect = new Rectangle ();

				item_rect.X = 0;
				item_rect.Y = owner.ItemHeight * (index - first_displayble);
				item_rect.Height = owner.ItemHeight;
				item_rect.Width = textarea_rect.Width;

				return item_rect;
			}


			private int LastVisibleItem ()
			{
				Rectangle item_rect;
				int top_y = textarea_rect.Y + textarea_rect.Height;
				int i = 0;

				for (i = top_item; i < owner.Items.Count; i++) {
					item_rect = GetItemDisplayRectangle (i, top_item);
					if (item_rect.Y > top_y)
						return i;
				}
				return i;
			}

			private void OnMouseDownPUW (object sender, MouseEventArgs e)
	    		{
	    			/* Click outside the client area destroys the popup */
	    			if (ClientRectangle.Contains (e.X, e.Y) == false) {
	    				Hide ();
	    			}
			}

			private void OnMouseUpPUW (object sender, MouseEventArgs e)
	    		{

			}

			private void OnMouseMovePUW (object sender, MouseEventArgs e)
			{

			}

			private void OnPaintPUW (Object o, PaintEventArgs pevent)
			{
				if (Width <= 0 || Height <=  0 || Visible == false)
	    				return;

				Draw (pevent.ClipRectangle);
				pevent.Graphics.DrawImage (ImageBuffer, pevent.ClipRectangle, pevent.ClipRectangle, GraphicsUnit.Pixel);
			}


			#endregion Private Methods
		}
	}

}

