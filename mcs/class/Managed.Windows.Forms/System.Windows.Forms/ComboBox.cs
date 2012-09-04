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
//	Daniel Nauck    (dna(at)mono-project(dot)de)

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Windows.Forms
{
	[DefaultProperty("Items")]
	[DefaultEvent("SelectedIndexChanged")]
	[Designer ("System.Windows.Forms.Design.ComboBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DefaultBindingProperty ("Text")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class ComboBox : ListControl
	{
		private DrawMode draw_mode = DrawMode.Normal;
		private ComboBoxStyle dropdown_style;
		private int dropdown_width = -1;
		private int selected_index = -1;
		private ObjectCollection items;
		private bool suspend_ctrlupdate;
		private int maxdrop_items = 8;
		private bool integral_height = true;
		private bool sorted;
		private int max_length;
		private ComboListBox listbox_ctrl;
		private ComboTextBox textbox_ctrl;
		private bool process_textchanged_event = true;
		private bool process_texchanged_autoscroll = true;
		private bool item_height_specified;
		private int item_height;
		private int requested_height = -1;
		private Hashtable item_heights;
		private bool show_dropdown_button;
		private ButtonState button_state = ButtonState.Normal;
		private bool dropped_down;
		private Rectangle text_area;
		private Rectangle button_area;
		private Rectangle listbox_area;
		private const int button_width = 16;
		bool drop_down_button_entered;
		private AutoCompleteStringCollection auto_complete_custom_source = null;
		private AutoCompleteMode auto_complete_mode = AutoCompleteMode.None;
		private AutoCompleteSource auto_complete_source = AutoCompleteSource.None;
		private FlatStyle flat_style;
		private int drop_down_height;
		const int default_drop_down_height = 106;

		[ComVisible(true)]
		public class ChildAccessibleObject : AccessibleObject {

			public ChildAccessibleObject (ComboBox owner, IntPtr handle)
				: base (owner)
			{
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
			background_color = ThemeEngine.Current.ColorWindow;
			border_style = BorderStyle.None;

			drop_down_height = default_drop_down_height;
			flat_style = FlatStyle.Standard;

			/* Events */
			MouseDown += new MouseEventHandler (OnMouseDownCB);
			MouseUp += new MouseEventHandler (OnMouseUpCB);
			MouseMove += new MouseEventHandler (OnMouseMoveCB);
			MouseWheel += new MouseEventHandler (OnMouseWheelCB);
			MouseEnter += new EventHandler (OnMouseEnter);
			MouseLeave += new EventHandler (OnMouseLeave);
			KeyDown +=new KeyEventHandler(OnKeyDownCB);
		}

		#region events
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged
		{
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick
		{
			add { base.DoubleClick += value; }
			remove { base.DoubleClick -= value; }
		}

		static object DrawItemEvent = new object ();
		static object DropDownEvent = new object ();
		static object DropDownStyleChangedEvent = new object ();
		static object MeasureItemEvent = new object ();
		static object SelectedIndexChangedEvent = new object ();
		static object SelectionChangeCommittedEvent = new object ();
		static object DropDownClosedEvent = new object ();
		static object TextUpdateEvent = new object ();

		public event DrawItemEventHandler DrawItem {
			add { Events.AddHandler (DrawItemEvent, value); }
			remove { Events.RemoveHandler (DrawItemEvent, value); }
		}

		public event EventHandler DropDown {
			add { Events.AddHandler (DropDownEvent, value); }
			remove { Events.RemoveHandler (DropDownEvent, value); }
		}
		public event EventHandler DropDownClosed
		{
			add { Events.AddHandler (DropDownClosedEvent, value); }
			remove { Events.RemoveHandler (DropDownClosedEvent, value); }
		}

		public event EventHandler DropDownStyleChanged {
			add { Events.AddHandler (DropDownStyleChangedEvent, value); }
			remove { Events.RemoveHandler (DropDownStyleChangedEvent, value); }
		}

		public event MeasureItemEventHandler MeasureItem {
			add { Events.AddHandler (MeasureItemEvent, value); }
			remove { Events.RemoveHandler (MeasureItemEvent, value); }
		}
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler PaddingChanged
		{
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}
		
		public event EventHandler SelectedIndexChanged {
			add { Events.AddHandler (SelectedIndexChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangedEvent, value); }
		}

		public event EventHandler SelectionChangeCommitted {
			add { Events.AddHandler (SelectionChangeCommittedEvent, value); }
			remove { Events.RemoveHandler (SelectionChangeCommittedEvent, value); }
		}
		public event EventHandler TextUpdate
		{
			add { Events.AddHandler (TextUpdateEvent, value); }
			remove { Events.RemoveHandler (TextUpdateEvent, value); }
		}

		#endregion Events

		#region Public Properties
		[MonoTODO("AutoCompletion algorithm is currently not implemented.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.ListControlStringCollectionEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public AutoCompleteStringCollection AutoCompleteCustomSource { 
			get {
				if(auto_complete_custom_source == null) {
					auto_complete_custom_source = new AutoCompleteStringCollection ();
					auto_complete_custom_source.CollectionChanged += new CollectionChangeEventHandler (OnAutoCompleteCustomSourceChanged);
				}
				return auto_complete_custom_source;
			}
			set {
				if(auto_complete_custom_source == value)
					return;

				if(auto_complete_custom_source != null) //remove eventhandler from old collection
					auto_complete_custom_source.CollectionChanged -= new CollectionChangeEventHandler (OnAutoCompleteCustomSourceChanged);

				auto_complete_custom_source = value;

				if(auto_complete_custom_source != null)
					auto_complete_custom_source.CollectionChanged += new CollectionChangeEventHandler (OnAutoCompleteCustomSourceChanged);

				SetTextBoxAutoCompleteData ();
			}
		}

		[MonoTODO("AutoCompletion algorithm is currently not implemented.")]
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DefaultValue (AutoCompleteMode.None)]
		public AutoCompleteMode AutoCompleteMode {
			get { return auto_complete_mode; }
			set {
				if(auto_complete_mode == value)
					return;

				if((value < AutoCompleteMode.None) || (value > AutoCompleteMode.SuggestAppend))
					throw new InvalidEnumArgumentException (Locale.GetText ("Enum argument value '{0}' is not valid for AutoCompleteMode", value));

				auto_complete_mode = value;
				SetTextBoxAutoCompleteData ();
			}
		}

		[MonoTODO("AutoCompletion algorithm is currently not implemented.")]
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DefaultValue (AutoCompleteSource.None)]
		public AutoCompleteSource AutoCompleteSource {
			get { return auto_complete_source; }
			set {
				if(auto_complete_source == value)
					return;

				if(!Enum.IsDefined (typeof (AutoCompleteSource), value))
					throw new InvalidEnumArgumentException (Locale.GetText ("Enum argument value '{0}' is not valid for AutoCompleteSource", value));

				auto_complete_source = value;
				SetTextBoxAutoCompleteData ();
			}
		}

		void SetTextBoxAutoCompleteData ()
		{
			if (textbox_ctrl == null)
				return;

			textbox_ctrl.AutoCompleteMode = auto_complete_mode;

			if (auto_complete_source == AutoCompleteSource.ListItems) {
				textbox_ctrl.AutoCompleteSource = AutoCompleteSource.CustomSource;
				textbox_ctrl.AutoCompleteCustomSource = null;
				textbox_ctrl.AutoCompleteInternalSource = this;
			} else {
				textbox_ctrl.AutoCompleteSource = auto_complete_source;
				textbox_ctrl.AutoCompleteCustomSource = auto_complete_custom_source;
				textbox_ctrl.AutoCompleteInternalSource = null;
			}
		}
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

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		protected override CreateParams CreateParams {
			get { return base.CreateParams;}
		}

		[DefaultValue ((string)null)]
		[AttributeProvider (typeof (IListSource))]
		[RefreshProperties (RefreshProperties.Repaint)]
		[MWFCategory("Data")]
		public new object DataSource {
			get { return base.DataSource; }
			set { base.DataSource = value; }
		}

		protected override Size DefaultSize {
			get { return new Size (121, 21); }
		}

		[RefreshProperties(RefreshProperties.Repaint)]
		[DefaultValue (DrawMode.Normal)]
		[MWFCategory("Behavior")]
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

		[Browsable (true)]
		[DefaultValue (106)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[MWFCategory("Behavior")]
		public int DropDownHeight {
			get {
				return drop_down_height;
			}
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException ("DropDownHeight", "DropDownHeight must be greater than 0.");
					
				if (value == drop_down_height)
					return;

				drop_down_height = value;
				IntegralHeight = false;
			}
		}

		[DefaultValue (ComboBoxStyle.DropDown)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[MWFCategory("Appearance")]
		public ComboBoxStyle DropDownStyle {
			get { return dropdown_style; }
			set {
				if (!Enum.IsDefined (typeof (ComboBoxStyle), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for ComboBoxStyle", value));

				if (dropdown_style == value)
					return;

				SuspendLayout ();

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
					Controls.AddImplicit (listbox_ctrl);
					listbox_ctrl.Visible = true;

					// This should give us a 150 default height
					// for Simple mode if size hasn't been set
					// (DefaultSize doesn't work for us in this case)
					if (requested_height == -1)
						requested_height = 150;
				} else {
					show_dropdown_button = true;
					button_state = ButtonState.Normal;
				}
	
				if (dropdown_style != ComboBoxStyle.DropDownList && textbox_ctrl == null) {
					textbox_ctrl = new ComboTextBox (this);
					object selected_item = SelectedItem;
					if (selected_item != null)
						textbox_ctrl.Text = GetItemText (selected_item);
					textbox_ctrl.BorderStyle = BorderStyle.None;
					textbox_ctrl.TextChanged += new EventHandler (OnTextChangedEdit);
					textbox_ctrl.KeyPress += new KeyPressEventHandler (OnTextKeyPress);
					textbox_ctrl.Click += new EventHandler (OnTextBoxClick);
					textbox_ctrl.ContextMenu = ContextMenu;
					textbox_ctrl.TopMargin = 1; // since we don't have borders, adjust manually the top

					if (IsHandleCreated == true)
						Controls.AddImplicit (textbox_ctrl);
					SetTextBoxAutoCompleteData ();
				}
				
				ResumeLayout ();
				OnDropDownStyleChanged (EventArgs.Empty);
				
				LayoutComboBox ();
				UpdateComboBoxBounds ();
				Refresh ();
			}
		}

		[MWFCategory("Behavior")]
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
					throw new ArgumentOutOfRangeException ("DropDownWidth",
						"The DropDownWidth value is less than one.");

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
					listbox_ctrl.HideWindow ();
			}
		}

		[DefaultValue (FlatStyle.Standard)]
		[Localizable (true)]
		[MWFCategory("Appearance")]
		public FlatStyle FlatStyle {
			get { return flat_style; }
			set {
				if (!Enum.IsDefined (typeof (FlatStyle), value))
					throw new InvalidEnumArgumentException ("FlatStyle", (int) value, typeof (FlatStyle));
				
				flat_style = value;
				LayoutComboBox ();
				Invalidate ();
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
		[MWFCategory("Behavior")]
		public bool IntegralHeight {
			get { return integral_height; }
			set {
				if (integral_height == value)
					return;
				integral_height = value;
				UpdateComboBoxBounds ();
				Refresh ();
			}
		}

		[Localizable (true)]
		[MWFCategory("Behavior")]
		public int ItemHeight {
			get {
				if (item_height == -1) {
					SizeF sz = TextRenderer.MeasureString ("The quick brown Fox", Font);
					item_height = (int) sz.Height;
				}
				return item_height;
			}
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException ("ItemHeight",
						"The item height value is less than one.");

				item_height_specified = true;
				item_height = value;
				if (IntegralHeight)
					UpdateComboBoxBounds ();
				LayoutComboBox ();
				Refresh ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.ListControlStringCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[MergableProperty (false)]
		[MWFCategory("Data")]
		public ComboBox.ObjectCollection Items {
			get { return items; }
		}

		[DefaultValue (8)]
		[Localizable (true)]
		[MWFCategory("Behavior")]
		public int MaxDropDownItems {
			get { return maxdrop_items; }
			set {
				if (maxdrop_items == value)
					return;
				maxdrop_items = value;
			}
		}

		public override Size MaximumSize {
			get { return base.MaximumSize; }
			set {
				base.MaximumSize = new Size (value.Width, 0);
			}
		}

		[DefaultValue (0)]
		[Localizable (true)]
		[MWFCategory("Behavior")]
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

		public override Size MinimumSize {
			get { return base.MinimumSize; }
			set {
				base.MinimumSize = new Size (value.Width, 0);
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public new Padding Padding  {
			get { return base.Padding; }
			set { base.Padding = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public int PreferredHeight {
			get { return Font.Height + 8; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override int SelectedIndex {
			get { return selected_index; }
			set {
				SetSelectedIndex (value, false);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Bindable(true)]
		public object SelectedItem {
			get { return selected_index == -1 ? null : Items [selected_index]; }
			set {
				object item = selected_index == -1 ? null : Items [selected_index];
				if (item == value)
					return;

				if (value == null)
					SelectedIndex = -1;
				else
					SelectedIndex = Items.IndexOf (value);
			}
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string SelectedText {
			get {
				if (dropdown_style == ComboBoxStyle.DropDownList)
					return string.Empty;
					
				string retval = textbox_ctrl.SelectedText;
				
				return retval;
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
				
				int result = textbox_ctrl.SelectionLength;
				return result == -1 ? 0 : result;
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
		[MWFCategory("Behavior")]
		public bool Sorted {
			get { return sorted; }
			set {
				if (sorted == value)
					return;
				sorted = value;
				SelectedIndex = -1;
				if (sorted) {
					Items.Sort ();
					LayoutComboBox ();
				}
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
					if (SelectedIndex == -1) {
						if (dropdown_style != ComboBoxStyle.DropDownList)
							SetControlText (string.Empty, false);
					} else {
						SelectedIndex = -1;
					}
					return;
				}

				// don't set the index if value exactly matches text of selected item
				if (SelectedItem == null || string.Compare (value, GetItemText (SelectedItem), false, CultureInfo.CurrentCulture) != 0)
				{
					// find exact match using case-sensitive comparison, and if does
					// not result in any match then use case-insensitive comparison
					int index = FindStringExact (value, -1, false);
					if (index == -1) {
						index = FindStringExact (value, -1, true);
					}
					if (index != -1) {
						SelectedIndex = index;
						return;
					}
				}

				// set directly the passed value
				if (dropdown_style != ComboBoxStyle.DropDownList)
					textbox_ctrl.Text = value;
			}
		}

		#endregion Public Properties

		#region Internal Properties
		internal Rectangle ButtonArea {
			get { return button_area; }
		}

		internal Rectangle TextArea {
			get { return text_area; }
		}
		#endregion

		#region UIA Framework Properties

		internal TextBox UIATextBox {
			get { return textbox_ctrl; }
		}

		internal ComboListBox UIAComboListBox {
			get { return listbox_ctrl; }
		}

		#endregion UIA Framework Properties

		#region Public Methods
		[Obsolete ("This method has been deprecated")]
		protected virtual void AddItemsCore (object[] value)
		{
			
		}

		public void BeginUpdate ()
		{
			suspend_ctrlupdate = true;
		}

		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return base.CreateAccessibilityInstance ();
		}
		
		protected override void CreateHandle ()
		{
			base.CreateHandle ();
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
			if (s == null || Items.Count == 0) 
				return -1;

			if (startIndex < -1 || startIndex >= Items.Count)
				throw new ArgumentOutOfRangeException ("startIndex");

			int i = startIndex;
			if (i == (Items.Count - 1))
				i = -1;
			do {
				i++;
				if (string.Compare (s, 0, GetItemText (Items [i]), 0, s.Length, true) == 0)
					return i;
				if (i == (Items.Count - 1))
					i = -1;
			} while (i != startIndex);

			return -1;
		}

		public int FindStringExact (string s)
		{
			return FindStringExact (s, -1);
		}

		public int FindStringExact (string s, int startIndex)
		{
			return FindStringExact (s, startIndex, true);
		}

		private int FindStringExact (string s, int startIndex, bool ignoreCase)
		{
			if (s == null || Items.Count == 0) 
				return -1;

			if (startIndex < -1 || startIndex >= Items.Count)
				throw new ArgumentOutOfRangeException ("startIndex");

			int i = startIndex;
			if (i == (Items.Count - 1))
				i = -1;
			do {
				i++;
				if (string.Compare (s, GetItemText (Items [i]), ignoreCase, CultureInfo.CurrentCulture) == 0)
					return i;
				if (i == (Items.Count - 1))
					i = -1;
			} while (i != startIndex);

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
			switch (keyData & ~Keys.Modifiers) {
			case Keys.Up:
			case Keys.Down:
			case Keys.Left:
			case Keys.Right:
			case Keys.PageUp:
			case Keys.PageDown:
			case Keys.Home:
			case Keys.End:
				return true;
			
			default:
				return false;
			}
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);

			if (textbox_ctrl != null)
				textbox_ctrl.BackColor = BackColor;
		}

		protected override void OnDataSourceChanged (EventArgs e)
		{
			base.OnDataSourceChanged (e);
			BindDataItems ();
			
			/** 
			 ** This 'Debugger.IsAttached' hack is here because of
			 ** Xamarin Bug #2234, which noted that when changing
			 ** the DataSource, in Windows exceptions are eaten
			 ** when SelectedIndexChanged is fired.  However, when
			 ** the debugger is running (i.e. in MonoDevelop), we
			 ** want to be alerted of exceptions.
			 **/

			if (Debugger.IsAttached) {
				SetSelectedIndex ();
			} else {
				try {
					SetSelectedIndex ();
				} catch {
					//ignore exceptions here per 
					//bug 2234
				}
			}
		}

		private void SetSelectedIndex ()
		{
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

			if (DataManager == null)
				return;

			SelectedIndex = DataManager.Position;

			if (selected_index != -1 && DropDownStyle != ComboBoxStyle.DropDownList)
				SetControlText (GetItemText (Items [selected_index]), true);

			if (!IsHandleCreated)
				return;

			Invalidate ();
		}

		protected virtual void OnDrawItem (DrawItemEventArgs e)
		{
			DrawItemEventHandler eh = (DrawItemEventHandler)(Events [DrawItemEvent]);
			if (eh != null)
				eh (this, e);
		}

		internal void HandleDrawItem (DrawItemEventArgs e)
		{
			// Only raise OnDrawItem if we are in an OwnerDraw mode
			switch (DrawMode) {
				case DrawMode.OwnerDrawFixed:
				case DrawMode.OwnerDrawVariable:
					OnDrawItem (e);
					break;
				default:
					ThemeEngine.Current.DrawComboBoxItem (this, e);
					break;
			}
		}

		protected virtual void OnDropDown (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DropDownEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDropDownClosed (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [DropDownClosedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDropDownStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DropDownStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);

			if (textbox_ctrl != null)
				textbox_ctrl.Font = Font;
			
			if (!item_height_specified)
				item_height = Font.Height + 2;

			if (IntegralHeight)
				UpdateComboBoxBounds ();

			LayoutComboBox ();
		}

		protected override void OnForeColorChanged (EventArgs e)
		{
			base.OnForeColorChanged (e);
			if (textbox_ctrl != null)
				textbox_ctrl.ForeColor = ForeColor;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnGotFocus (EventArgs e)
		{
			if (dropdown_style == ComboBoxStyle.DropDownList) {
				// We draw DDL styles manually, so they require a
				// refresh to have their selection drawn
				Invalidate ();
			}
			
			if (textbox_ctrl != null) {
				textbox_ctrl.SetSelectable (false);
				textbox_ctrl.ShowSelection = Enabled;
				textbox_ctrl.ActivateCaret (true);
				textbox_ctrl.SelectAll ();
			}

			base.OnGotFocus (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnLostFocus (EventArgs e)
		{
			if (dropdown_style == ComboBoxStyle.DropDownList) {
				// We draw DDL styles manually, so they require a
				// refresh to have their selection drawn
				Invalidate ();
			}

			if (listbox_ctrl != null && dropped_down) {
				listbox_ctrl.HideWindow ();
			}

			if (textbox_ctrl != null) {
				textbox_ctrl.SetSelectable (true);
				textbox_ctrl.ActivateCaret (false);
				textbox_ctrl.ShowSelection = false;
				textbox_ctrl.SelectionLength = 0;
				textbox_ctrl.HideAutoCompleteList ();
			}

			base.OnLostFocus (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);

			SetBoundsInternal (Left, Top, Width, PreferredHeight, BoundsSpecified.None);

			if (textbox_ctrl != null)
				Controls.AddImplicit (textbox_ctrl);

			LayoutComboBox ();
			UpdateComboBoxBounds ();
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected override void OnKeyPress (KeyPressEventArgs e)
		{
			if (dropdown_style == ComboBoxStyle.DropDownList) {
				int index = FindStringCaseInsensitive (e.KeyChar.ToString (), SelectedIndex + 1);
				if (index != -1) {
					SelectedIndex = index;
					if (DroppedDown) { //Scroll into view
						if (SelectedIndex >= listbox_ctrl.LastVisibleItem ())
							listbox_ctrl.Scroll (SelectedIndex - listbox_ctrl.LastVisibleItem () + 1);
						// Or, selecting an item earlier in the list.
						if (SelectedIndex < listbox_ctrl.FirstVisibleItem ())
							listbox_ctrl.Scroll (SelectedIndex - listbox_ctrl.FirstVisibleItem ());
					}
				}
			}

			base.OnKeyPress (e);
		}

		protected virtual void OnMeasureItem (MeasureItemEventArgs e)
		{
			MeasureItemEventHandler eh = (MeasureItemEventHandler)(Events [MeasureItemEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnParentBackColorChanged (EventArgs e)
		{
			base.OnParentBackColorChanged (e);
		}

		protected override void OnResize (EventArgs e)
		{
			LayoutComboBox ();
			if (listbox_ctrl != null)
				listbox_ctrl.CalcListBoxArea ();
		}

		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			base.OnSelectedIndexChanged (e);

			EventHandler eh = (EventHandler)(Events [SelectedIndexChangedEvent]);
			if (eh != null)
				eh (this, e);
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
			EventHandler eh = (EventHandler)(Events [SelectionChangeCommittedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void RefreshItem (int index)
		{
			if (index < 0 || index >= Items.Count)
				throw new ArgumentOutOfRangeException ("index");
				
			if (draw_mode == DrawMode.OwnerDrawVariable)
				item_heights.Remove (Items [index]);
		}

		protected override void RefreshItems ()
		{
			for (int i = 0; i < Items.Count; i++) {
				RefreshItem (i);
			}

			LayoutComboBox ();
			Refresh ();

			if (selected_index != -1 && DropDownStyle != ComboBoxStyle.DropDownList)
				SetControlText (GetItemText (Items [selected_index]), false);
		}

		public override void ResetText ()
		{
			Text = String.Empty;
		}
		
		protected override bool ProcessKeyEventArgs (ref Message m)
		{
			return base.ProcessKeyEventArgs (ref m);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnKeyDown (KeyEventArgs e)
		{
			base.OnKeyDown (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnValidating (CancelEventArgs e)
		{
			base.OnValidating (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnTextChanged (EventArgs e)
		{
			base.OnTextChanged (e);
		}

		protected virtual void OnTextUpdate (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [TextUpdateEvent];
			if (eh != null)
				eh (this, e);
		}
		protected override void OnMouseLeave (EventArgs e)
		{
			if (flat_style == FlatStyle.Popup)
				Invalidate ();
			base.OnMouseLeave (e);
		}
		
		protected override void OnMouseEnter (EventArgs e)
		{
			if (flat_style == FlatStyle.Popup)
				Invalidate ();
			base.OnMouseEnter (e);
		}

		protected override void ScaleControl (SizeF factor, BoundsSpecified specified)
		{
			base.ScaleControl (factor, specified);
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

			if (textbox_ctrl != null) {
				textbox_ctrl.ShowSelection = true;
				textbox_ctrl.SelectAll ();
			}
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			bool vertically_anchored = (Anchor & AnchorStyles.Top) != 0 && (Anchor & AnchorStyles.Bottom) != 0;
			bool vertically_docked = Dock == DockStyle.Left || Dock == DockStyle.Right || Dock == DockStyle.Fill;

			if ((specified & BoundsSpecified.Height) != 0 ||
				(specified == BoundsSpecified.None && (vertically_anchored || vertically_docked))) {

				requested_height = height;
				height = SnapHeight (height);
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
			BeginUpdate ();
			try {
				Items.Clear ();
				Items.AddRange (value);
			} finally {
				EndUpdate ();
			}
		}

		public override string ToString ()
		{
			return base.ToString () + ", Items.Count:" + Items.Count;
		}

		protected override void WndProc (ref Message m)
		{
			switch ((Msg) m.Msg) {
			case Msg.WM_KEYUP:
			case Msg.WM_KEYDOWN:
				Keys keys = (Keys) m.WParam.ToInt32 ();
				// Don't pass the message to base if auto complete is being used and available.
				if (textbox_ctrl != null && textbox_ctrl.CanNavigateAutoCompleteList) {
					XplatUI.SendMessage (textbox_ctrl.Handle, (Msg) m.Msg, m.WParam, m.LParam);
					return;
				}
				if (keys == Keys.Up || keys == Keys.Down)
					break;
				goto case Msg.WM_CHAR;
			case Msg.WM_CHAR:
				// Call our own handler first and send the message to the TextBox if still needed
				if (!ProcessKeyMessage (ref m) && textbox_ctrl != null)
					XplatUI.SendMessage (textbox_ctrl.Handle, (Msg) m.Msg, m.WParam, m.LParam);
				return;
			case Msg.WM_MOUSELEAVE:
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
		void OnAutoCompleteCustomSourceChanged(object sender, CollectionChangeEventArgs e) {
			if(auto_complete_source == AutoCompleteSource.CustomSource) {
				//FIXME: handle add, remove and refresh events in AutoComplete algorithm.
			}
		}

		internal override bool InternalCapture {
			get { return Capture; }
			set {}
		}
		
		void LayoutComboBox ()
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
				if (flat_style == FlatStyle.Popup || flat_style == FlatStyle.Flat) {
					button_area.Inflate (1, 1);
					button_area.X += 2;
					button_area.Width -= 2;
				}
			}

			if (button_area != prev_button_area) {
				prev_button_area.Y -= border;
				prev_button_area.Width += border;
				prev_button_area.Height += 2 * border;
				Invalidate (prev_button_area);
				Invalidate (button_area);
			}

			if (textbox_ctrl != null) {
				int text_border = border + 1;
				textbox_ctrl.Location = new Point (text_area.X + text_border, text_area.Y + text_border);
				textbox_ctrl.Width = text_area.Width - button_area.Width - text_border * 2;
				textbox_ctrl.Height = text_area.Height - text_border * 2;
			}

			if (listbox_ctrl != null && dropdown_style == ComboBoxStyle.Simple) {
				listbox_ctrl.Location = listbox_area.Location;
				listbox_ctrl.CalcListBoxArea ();
			}
		}

		private void CreateComboListBox ()
		{
			listbox_ctrl = new ComboListBox (this);
			listbox_ctrl.HighlightedIndex = SelectedIndex;
		}
		
		internal void Draw (Rectangle clip, Graphics dc)
		{
			Theme theme = ThemeEngine.Current;
			FlatStyle style = FlatStyle.Standard;
			bool is_flat = false;

			style = this.FlatStyle;
			is_flat = style == FlatStyle.Flat || style == FlatStyle.Popup;

			theme.ComboBoxDrawBackground (this, dc, clip, style);

			int border = theme.Border3DSize.Width;

			// No edit control, we paint the edit ourselves
			if (dropdown_style == ComboBoxStyle.DropDownList) {
				DrawItemState state = DrawItemState.None;
				Color back_color = BackColor;
				Color fore_color = ForeColor;
				Rectangle item_rect = text_area;
				item_rect.X += border;
				item_rect.Y += border;
				item_rect.Width -= (button_area.Width + 2 * border);
				item_rect.Height -= 2 * border;
				
				if (Focused) {
					state = DrawItemState.Selected;
					state |= DrawItemState.Focus;
					back_color = SystemColors.Highlight;
					fore_color = SystemColors.HighlightText;
				}
				
				state |= DrawItemState.ComboBoxEdit;
				HandleDrawItem (new DrawItemEventArgs (dc, Font, item_rect, SelectedIndex, state, fore_color, back_color));
			}
			
			if (show_dropdown_button) {
				ButtonState current_state;
				if (is_enabled)
					current_state = button_state;
				else
					current_state = ButtonState.Inactive;

				if (is_flat || theme.ComboBoxNormalDropDownButtonHasTransparentBackground (this, current_state))
					dc.FillRectangle (theme.ResPool.GetSolidBrush (theme.ColorControl), button_area);

				if (is_flat) {
					theme.DrawFlatStyleComboButton (dc, button_area, current_state);
				} else {
					theme.ComboBoxDrawNormalDropDownButton (this, dc, clip, button_area, current_state); 
				}
			}
		}

		internal bool DropDownButtonEntered {
			get { return drop_down_button_entered; }
			private set {
				if (drop_down_button_entered == value)
					return;
				drop_down_button_entered = value;
				if (ThemeEngine.Current.ComboBoxDropDownButtonHasHotElementStyle (this))
					Invalidate (button_area);
			}
		}

		internal void DropDownListBox ()
		{
			DropDownButtonEntered = false;

			if (DropDownStyle == ComboBoxStyle.Simple)
				return;
			
			if (listbox_ctrl == null)
				CreateComboListBox ();

			listbox_ctrl.Location = PointToScreen (new Point (text_area.X, text_area.Y + text_area.Height));

			FindMatchOrSetIndex(SelectedIndex);

			if (textbox_ctrl != null)
				textbox_ctrl.HideAutoCompleteList ();

			if (listbox_ctrl.ShowWindow ())
				dropped_down = true;

			button_state = ButtonState.Pushed;
			if (dropdown_style == ComboBoxStyle.DropDownList)
				Invalidate (text_area);
		}
		
		internal void DropDownListBoxFinished ()
		{
			if (DropDownStyle == ComboBoxStyle.Simple)
				return;
				
			FindMatchOrSetIndex (SelectedIndex);
			button_state = ButtonState.Normal;
			Invalidate (button_area);
			dropped_down = false;
			OnDropDownClosed (EventArgs.Empty);
			/*
			 * Apples X11 looses override-redirect when doing a Unmap/Map on a previously mapped window
			 * this causes the popup to appear under the main form.  This is horrible but necessary
			 */
			 
			 // If the user opens a new form in an event, it will close our dropdown,
			 // so we need a null check here
			 if (listbox_ctrl != null) {
				listbox_ctrl.Dispose ();
				listbox_ctrl = null;
			}
			 // The auto complete list could have been shown after the listbox,
			 // so make sure it's hidden.
			 if (textbox_ctrl != null)
				 textbox_ctrl.HideAutoCompleteList ();
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

		// Search in the list for the substring, starting the search at the list 
		// position specified, the search wraps thus covering all the list.
		internal int FindStringCaseInsensitive (string search, int start_index)
		{
			if (search.Length == 0) {
				return -1;
			}
			// Accept from first item to after last item. i.e. all cases of (SelectedIndex+1).
			if (start_index < 0 || start_index > Items.Count)
				throw new ArgumentOutOfRangeException("start_index");

			for (int i = 0; i < Items.Count; i++) {
				int index = (i + start_index) % Items.Count;
				if (String.Compare (GetItemText (Items [index]), 0, search, 0, search.Length, true) == 0)
					return index;
			}

			return -1;
		}

		internal override bool IsInputCharInternal (char charCode)
		{
			return true;
		}

		internal override ContextMenu ContextMenuInternal {
			get {
				return base.ContextMenuInternal;
			}
			set {
				base.ContextMenuInternal = value;
				if (textbox_ctrl != null) {
					textbox_ctrl.ContextMenu = value;
				}
			}
		}

		internal void RestoreContextMenu ()
		{
			textbox_ctrl.RestoreContextMenu ();
		}

		private void OnKeyDownCB(object sender, KeyEventArgs e)
		{
			if (Items.Count == 0)
				return;

			// for keyboard navigation, we have to do our own scroll, since
			// the default behaviour for the SelectedIndex property is a little different,
			// setting the selected index in the top always

			int offset;
			switch (e.KeyCode) 
			{
				case Keys.Up:
					FindMatchOrSetIndex(Math.Max(SelectedIndex - 1, 0));

					if (DroppedDown)
						if (SelectedIndex < listbox_ctrl.FirstVisibleItem ())
							listbox_ctrl.Scroll (SelectedIndex - listbox_ctrl.FirstVisibleItem ());
					break;
	
				case Keys.Down:
					if ((e.Modifiers & Keys.Alt) == Keys.Alt)
						DropDownListBox ();
					else
						FindMatchOrSetIndex(Math.Min(SelectedIndex + 1, Items.Count - 1));
						
					if (DroppedDown)
						if (SelectedIndex >= listbox_ctrl.LastVisibleItem ())
							listbox_ctrl.Scroll (SelectedIndex - listbox_ctrl.LastVisibleItem () + 1);
					break;
				
				case Keys.PageUp:
					offset = listbox_ctrl == null ? MaxDropDownItems - 1 : listbox_ctrl.page_size - 1;
					if (offset < 1)
						offset = 1;

					SetSelectedIndex (Math.Max (SelectedIndex - offset, 0), true);

					if (DroppedDown)
						if (SelectedIndex < listbox_ctrl.FirstVisibleItem ())
							listbox_ctrl.Scroll (SelectedIndex - listbox_ctrl.FirstVisibleItem ());
					break;
	
				case Keys.PageDown:
					if (SelectedIndex == -1) {
						SelectedIndex = 0;
						if (dropdown_style != ComboBoxStyle.Simple)
							return;
					}

					offset = listbox_ctrl == null ? MaxDropDownItems - 1 : listbox_ctrl.page_size - 1;
					if (offset < 1)
						offset = 1;

					SetSelectedIndex (Math.Min (SelectedIndex + offset, Items.Count - 1), true);

					if (DroppedDown)
						if (SelectedIndex >= listbox_ctrl.LastVisibleItem ())
							listbox_ctrl.Scroll (SelectedIndex - listbox_ctrl.LastVisibleItem () + 1);
					break;
				
				case Keys.Enter:	
				case Keys.Escape:
					if (listbox_ctrl != null && listbox_ctrl.Visible)
						DropDownListBoxFinished ();
					break;
					
				case Keys.Home:
					if (dropdown_style == ComboBoxStyle.DropDownList) {
						SelectedIndex = 0;

						if (DroppedDown)
							if (SelectedIndex < listbox_ctrl.FirstVisibleItem ())
								listbox_ctrl.Scroll (SelectedIndex - listbox_ctrl.FirstVisibleItem ());
					}
					
					break;
				case Keys.End:
					if (dropdown_style == ComboBoxStyle.DropDownList) {
						SetSelectedIndex (Items.Count - 1, true);

						if (DroppedDown)
							if (SelectedIndex >= listbox_ctrl.LastVisibleItem ())
								listbox_ctrl.Scroll (SelectedIndex - listbox_ctrl.LastVisibleItem () + 1);
					}
					
					break;
				default:
					break;
			}
		}

		void SetSelectedIndex (int value, bool supressAutoScroll)
		{
			if (selected_index == value)
				return;

			if (value <= -2 || value >= Items.Count)
				throw new ArgumentOutOfRangeException ("SelectedIndex");

			selected_index = value;

			if (dropdown_style != ComboBoxStyle.DropDownList) {
				if (value == -1)
					SetControlText (string.Empty, false, supressAutoScroll);
				else
					SetControlText (GetItemText (Items [value]), false, supressAutoScroll);
			}

			if (DropDownStyle == ComboBoxStyle.DropDownList)
				Invalidate ();

			if (listbox_ctrl != null)
				listbox_ctrl.HighlightedIndex = value;

			OnSelectedValueChanged (EventArgs.Empty);
			OnSelectedIndexChanged (EventArgs.Empty);
			OnSelectedItemChanged (EventArgs.Empty);
		}

		// If no item is currently selected, and an item is found matching the text 
		// in the textbox, then selected that item.  Otherwise the item at the given 
		// index is selected.
		private void FindMatchOrSetIndex(int index)
		{
			int match = -1;
			if (SelectedIndex == -1 && Text.Length != 0)
				match = FindStringCaseInsensitive(Text);
			if (match != -1)
				SetSelectedIndex (match, true);
			else
				SetSelectedIndex (index, true);
		}
		
		void OnMouseDownCB (object sender, MouseEventArgs e)
		{
			Rectangle area;
			if (DropDownStyle == ComboBoxStyle.DropDownList)
				area = ClientRectangle;
			else
				area = button_area;

			if (area.Contains (e.X, e.Y)) {
				if (Items.Count > 0)
					DropDownListBox ();
				else {
					button_state = ButtonState.Pushed;
					OnDropDown (EventArgs.Empty);
				}
				
				Invalidate (button_area);
				Update ();
			}
			Capture = true;
		}

		void OnMouseEnter (object sender, EventArgs e)
		{
			if (ThemeEngine.Current.CombBoxBackgroundHasHotElementStyle (this))
				Invalidate ();
		}

		void OnMouseLeave (object sender, EventArgs e)
		{
			if (ThemeEngine.Current.CombBoxBackgroundHasHotElementStyle (this)) {
				drop_down_button_entered = false;
				Invalidate ();
			} else {
				if (show_dropdown_button)
					DropDownButtonEntered = false;
			}
		}

		void OnMouseMoveCB (object sender, MouseEventArgs e)
		{
			if (show_dropdown_button && !dropped_down)
				DropDownButtonEntered = button_area.Contains (e.Location);

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
			
			button_state = ButtonState.Normal;
			Invalidate (button_area);

			OnClick (EventArgs.Empty);

			if (dropped_down)
				listbox_ctrl.Capture = true;
		}

		private void OnMouseWheelCB (object sender, MouseEventArgs me)
		{
			if (Items.Count == 0)
				return;

			if (listbox_ctrl != null && listbox_ctrl.Visible) {
				int lines = me.Delta / 120 * SystemInformation.MouseWheelScrollLines;
				listbox_ctrl.Scroll (-lines);
			} else {
				int lines = me.Delta / 120;
				int index = SelectedIndex - lines;
				if (index < 0)
					index = 0;
				else if (index >= Items.Count)
					index = Items.Count - 1;
				SelectedIndex = index;
			}
		}

		MouseEventArgs TranslateMouseEventArgs (MouseEventArgs args)
		{
			Point loc = PointToClient (Control.MousePosition);
			return new MouseEventArgs (args.Button, args.Clicks, loc.X, loc.Y, args.Delta);
		}

		internal override void OnPaintInternal (PaintEventArgs pevent)
		{
			if (suspend_ctrlupdate)
				return;

			Draw (ClientRectangle, pevent.Graphics);
		}
		
		private void OnTextBoxClick (object sender, EventArgs e)
		{
			OnClick (e);
		}

		private void OnTextChangedEdit (object sender, EventArgs e)
		{
			if (process_textchanged_event == false)
				return; 

			int item = FindStringCaseInsensitive (textbox_ctrl.Text);
			
			if (item == -1) {
				// Setting base.Text below will raise this event
				// if we found something
				OnTextChanged (EventArgs.Empty);
				return;
			}
			
			if (listbox_ctrl != null) {
				// Set as top item
				if (process_texchanged_autoscroll)
					listbox_ctrl.EnsureTop (item);
			}

			base.Text = textbox_ctrl.Text;
		}

		private void OnTextKeyPress (object sender, KeyPressEventArgs e)
		{
			selected_index = -1;
			if (listbox_ctrl != null)
				listbox_ctrl.HighlightedIndex = -1;
		}

		internal void SetControlText (string s, bool suppressTextChanged)
		{
			SetControlText (s, suppressTextChanged, false);
		}

		internal void SetControlText (string s, bool suppressTextChanged, bool supressAutoScroll)
		{
			if (suppressTextChanged)
				process_textchanged_event = false;
			if (supressAutoScroll)
				process_texchanged_autoscroll = false;
				
			textbox_ctrl.Text = s;
			textbox_ctrl.SelectAll ();
			process_textchanged_event = true;
			process_texchanged_autoscroll = true;
		}
		
		void UpdateComboBoxBounds ()
		{
			if (requested_height == -1)
				return;

			// Save the requested height since set bounds can destroy it
			int save_height = requested_height;
			SetBounds (bounds.X, bounds.Y, bounds.Width, SnapHeight (requested_height),
				BoundsSpecified.Height);
			requested_height = save_height;
		}

		int SnapHeight (int height)
		{
			if (DropDownStyle == ComboBoxStyle.Simple && height > PreferredHeight) {
				if (IntegralHeight) {
					int border = ThemeEngine.Current.Border3DSize.Height;
					int lb_height = (height - PreferredHeight - 2) - border * 2;
					if (lb_height > ItemHeight) {
						int partial = (lb_height) % ItemHeight;
						height -= partial;
					} else if (lb_height < ItemHeight)
						height = PreferredHeight;
				}
			} else
				height = PreferredHeight;

			return height;
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
			
			#region UIA Framework Events

			//NOTE:
			//	We are using Reflection to add/remove internal events.
			//	Class ListProvider uses the events.
			//
			//Event used to generate UIA StructureChangedEvent
			static object UIACollectionChangedEvent = new object ();

			internal event CollectionChangeEventHandler UIACollectionChanged {
				add { owner.Events.AddHandler (UIACollectionChangedEvent, value); }
				remove { owner.Events.RemoveHandler (UIACollectionChangedEvent, value); }
			}
			
			internal void OnUIACollectionChangedEvent (CollectionChangeEventArgs args)
			{
				CollectionChangeEventHandler eh
					= (CollectionChangeEventHandler) owner.Events [UIACollectionChangedEvent];
				if (eh != null)
					eh (owner, args);
			}

			#endregion UIA Framework Events

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
						throw new ArgumentOutOfRangeException ("index");

					return object_items[index];
				}
				set {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("index");
					if (value == null)
						throw new ArgumentNullException ("value");

					//UIA Framework event: Item Removed
					OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Remove, object_items [index]));

					object_items[index] = value;
					
					//UIA Framework event: Item Added
					OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, value));

					if (owner.listbox_ctrl != null)
						owner.listbox_ctrl.InvalidateItem (index);
					if (index == owner.SelectedIndex) {
						if (owner.textbox_ctrl == null)
							owner.Refresh ();
						else {
							owner.textbox_ctrl.Text = value.ToString ();
							owner.textbox_ctrl.SelectAll ();
						}
					}
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

				idx = AddItem (item, false);
				owner.UpdatedItems ();
				return idx;
			}

			public void AddRange (object[] items)
			{
				if (items == null)
					throw new ArgumentNullException ("items");

				foreach (object mi in items)
					AddItem (mi, true);

				if (owner.sorted)
					Sort ();
				
				owner.UpdatedItems ();
			}

			public void Clear ()
			{
				owner.selected_index = -1;
				object_items.Clear ();
				owner.UpdatedItems ();
				owner.Refresh ();
				
				//UIA Framework event: Items list cleared
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Refresh, null));
			}
			
			public bool Contains (object value)
			{
				if (value == null)
					throw new ArgumentNullException ("value");

				return object_items.Contains (value);
			}

			public void CopyTo (object [] destination, int arrayIndex)
			{
				object_items.CopyTo (destination, arrayIndex);
			}

			void ICollection.CopyTo (Array destination, int index)
			{
				object_items.CopyTo (destination, index);
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
				if (value == null)
					throw new ArgumentNullException ("value");

				return object_items.IndexOf (value);
			}

			public void Insert (int index,  object item)
			{
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("index");
				if (item == null)
					throw new ArgumentNullException ("item");
				
				owner.BeginUpdate ();
				
				if (owner.Sorted)
					AddItem (item, false);
				else {
					object_items.Insert (index, item);
					//UIA Framework event: Item added
					OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, item));
				}
				
				owner.EndUpdate ();	// Calls UpdatedItems
			}

			public void Remove (object value)
			{
				if (value == null)
					return;
				int index = IndexOf (value);
				if (index >= 0)
					RemoveAt (index);
			}

			public void RemoveAt (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("index");
					
				if (index < owner.SelectedIndex)
					--owner.SelectedIndex;
				else if (index == owner.SelectedIndex)
					owner.SelectedIndex = -1;

				object removed = object_items [index];

				object_items.RemoveAt (index);
				owner.UpdatedItems ();
				
				//UIA Framework event: Item removed
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Remove, removed));
			}
			#endregion Public Methods

			#region Private Methods
			private int AddItem (object item, bool suspend)
			{
				// suspend means do not sort as we put new items in, we will do a
				// big sort at the end
				if (item == null)
					throw new ArgumentNullException ("item");

				if (owner.Sorted && !suspend) {
					int index = 0;
					foreach (object o in object_items) {
						if (String.Compare (item.ToString (), o.ToString ()) < 0) {
							object_items.Insert (index, item);
							
							// If we added the new item before the selectedindex
							// bump the selectedindex by one, behavior differs if
							// Handle has not been created.
							if (index <= owner.selected_index && owner.IsHandleCreated)
								owner.selected_index++;
								
							//UIA Framework event: Item added
							OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, item));

							return index;
						}
						index++;
					}
				}
				object_items.Add (item);
				
				//UIA Framework event: Item added
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, item));
				
				return object_items.Count - 1;
			}
			
			internal void AddRange (IList items)
			{
				foreach (object mi in items)
					AddItem (mi, false);
				
				if (owner.sorted)
					Sort ();
				
				owner.UpdatedItems ();
			}

			internal void Sort ()
			{
				// If the objects the user put here don't have their own comparer,
				// use one that compares based on the object's ToString
				if (object_items.Count > 0 && object_items[0] is IComparer)
					object_items.Sort ();
				else
					object_items.Sort (new ObjectComparer (owner));
			}

			private class ObjectComparer : IComparer
			{
				private ListControl owner;
				
				public ObjectComparer (ListControl owner)
				{
					this.owner = owner;
				}
				
				#region IComparer Members
				public int Compare (object x, object y)
				{
					return string.Compare (owner.GetItemText (x), owner.GetItemText (y));
				}
				#endregion
			}
			#endregion Private Methods
		}

		internal class ComboTextBox : TextBox {

			private ComboBox owner;

			public ComboTextBox (ComboBox owner)
			{
				this.owner = owner;
				ShowSelection = false;
				owner.EnabledChanged += OwnerEnabledChangedHandler;
				owner.LostFocus += OwnerLostFocusHandler;
			}

			void OwnerEnabledChangedHandler (object o, EventArgs args)
			{
				ShowSelection = owner.Focused && owner.Enabled;
			}

			void OwnerLostFocusHandler (object o, EventArgs args)
			{
				if (IsAutoCompleteAvailable)
					owner.Text = Text;
			}

			protected override void OnKeyDown (KeyEventArgs args)
			{
				if (args.KeyCode == Keys.Enter && IsAutoCompleteAvailable)
					owner.Text = Text;

				base.OnKeyDown (args);
			}

			internal override void OnAutoCompleteValueSelected (EventArgs args)
			{
				base.OnAutoCompleteValueSelected (args);
				owner.Text = Text;
			}

			internal void SetSelectable (bool selectable)
			{
				SetStyle (ControlStyles.Selectable, selectable);
			}

			internal void ActivateCaret (bool active)
			{
				if (active)
					document.CaretHasFocus ();
				else
					document.CaretLostFocus ();
			}
			
			internal override void OnTextUpdate ()
			{
				base.OnTextUpdate ();
				owner.OnTextUpdate (EventArgs.Empty);
			}
			
			protected override void OnGotFocus (EventArgs e)
			{
				owner.Select (false, true);
			}

			protected override void OnLostFocus (EventArgs e)
			{
				owner.Select (false, true);
			}

			// We have to pass these events to our owner - MouseMove is not, however.

			protected override void OnMouseDown (MouseEventArgs e)
			{
				base.OnMouseDown (e);
				owner.OnMouseDown (owner.TranslateMouseEventArgs (e));
			}

			protected override void OnMouseUp (MouseEventArgs e)
			{
				base.OnMouseUp (e);
				owner.OnMouseUp (owner.TranslateMouseEventArgs (e));
			}

			protected override void OnMouseClick (MouseEventArgs e)
			{
				base.OnMouseClick (e);
				owner.OnMouseClick (owner.TranslateMouseEventArgs (e));
			}

			protected override void OnMouseDoubleClick (MouseEventArgs e)
			{
				base.OnMouseDoubleClick (e);
				owner.OnMouseDoubleClick (owner.TranslateMouseEventArgs (e));
			}

			public override bool Focused {
				get {
					return owner.Focused;
				}
			}
			
			internal override bool ActivateOnShow { get { return false; } }
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

			#region UIA Framework: Properties

			internal int UIATopItem {
				get { return top_item; }
			}
			
			internal int UIALastItem {
				get { return last_item; }
			}

			internal ScrollBar UIAVScrollBar {
				get { return vscrollbar_ctrl; }
			}

			#endregion
			
			class VScrollBarLB : VScrollBar
			{
				public VScrollBarLB ()
				{
				}
				
				internal override bool InternalCapture {
					get { return Capture; }
					set { }
				}

				public void FireMouseDown (MouseEventArgs e) 
				{
					if (!Visible) 
						return;

					e = TranslateEvent (e);
					OnMouseDown (e);
				}
				
				public void FireMouseUp (MouseEventArgs e) 
				{
					if (!Visible)
						return;

					e = TranslateEvent (e);
					OnMouseUp (e);
				}
				
				public void FireMouseMove (MouseEventArgs e) 
				{
					if (!Visible)
						return;

					e = TranslateEvent (e);
					OnMouseMove (e);
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

				MouseWheel += new MouseEventHandler (OnMouseWheelCLB);

				SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
				SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);

				this.is_visible = false;

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

					cp.Style ^= (int)WindowStyles.WS_CHILD;
					cp.Style ^= (int)WindowStyles.WS_VISIBLE;
					cp.Style |= (int)WindowStyles.WS_POPUP;
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

			internal override bool ActivateOnShow { get { return false; } }
			#region Private Methods

			// Calcs the listbox area
			internal void CalcListBoxArea ()
			{
				int width, height;
				bool show_scrollbar;

				if (owner.DropDownStyle == ComboBoxStyle.Simple) {
					Rectangle area = owner.listbox_area;
					width = area.Width;
					height = area.Height;
					show_scrollbar = owner.Items.Count * owner.ItemHeight > height;

					// No calculation needed
					if (height <= 0 || width <= 0)
						return;

				}
				else { // DropDown or DropDownList
					
					width = owner.DropDownWidth;
					int visible_items_count = (owner.Items.Count <= owner.MaxDropDownItems) ? owner.Items.Count : owner.MaxDropDownItems;
					
					if (owner.DrawMode == DrawMode.OwnerDrawVariable) {
						height = 0;
						for (int i = 0; i < visible_items_count; i++) {
							height += owner.GetItemHeight (i);
						}

						show_scrollbar = owner.Items.Count > owner.MaxDropDownItems;
						
					} else	{
						if (owner.DropDownHeight == default_drop_down_height) { // ignore DropDownHeight
							height = owner.ItemHeight * visible_items_count;
							show_scrollbar = owner.Items.Count > owner.MaxDropDownItems;
						} else {
							// ignore visible items count, and use manual height instead
							height = owner.DropDownHeight;
							show_scrollbar = (owner.Items.Count * owner.ItemHeight) > height;
						}
					}
				}
				
				page_size = Math.Max (height / owner.ItemHeight, 1);

				ComboBoxStyle dropdown_style = owner.DropDownStyle;
				if (!show_scrollbar) {

					if (vscrollbar_ctrl != null)
						vscrollbar_ctrl.Visible = false;
					if (dropdown_style != ComboBoxStyle.Simple)
						height = owner.ItemHeight * owner.items.Count;
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
					
					vscrollbar_ctrl.Dock = DockStyle.Right;

					vscrollbar_ctrl.Maximum = owner.Items.Count - 1;
					int large = page_size;
					if (large < 1)
						large = 1;
					vscrollbar_ctrl.LargeChange = large;
					vscrollbar_ctrl.Visible = true;

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
						Color back_color = owner.BackColor;
						Color fore_color = owner.ForeColor;

						if (i == HighlightedIndex) {
							state |= DrawItemState.Selected;
							back_color = SystemColors.Highlight;
							fore_color = SystemColors.HighlightText;
							
							if (owner.DropDownStyle == ComboBoxStyle.DropDownList) {
								state |= DrawItemState.Focus;
							}
						}

						owner.HandleDrawItem (new DrawItemEventArgs (dc, owner.Font, item_rect,
							i, state, fore_color, back_color));
					}
				}
			}

			int highlighted_index = -1;

			public int HighlightedIndex {
				get { return highlighted_index; }
				set { 
					if (highlighted_index == value)
						return;

					if (highlighted_index != -1 && highlighted_index < this.owner.Items.Count)
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

			public void InvalidateItem (int index)
			{
				if (Visible)
					Invalidate (GetItemDisplayRectangle (index, top_item));
			}

			public int LastVisibleItem ()
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

			public void SetTopItem (int item)
			{
				if (top_item == item)
					return;
				top_item = item;
				UpdateLastVisibleItem ();
				Invalidate ();
			}

			public int FirstVisibleItem ()
			{
				return top_item;
			}

			public void EnsureTop (int item)
			{
				if (owner.Items.Count == 0)
					return;
				if (vscrollbar_ctrl == null || !vscrollbar_ctrl.Visible)
					return;

				int max = vscrollbar_ctrl.Maximum - page_size + 1;
				if (item > max)
					item = max;
				else if (item < vscrollbar_ctrl.Minimum)
					item = vscrollbar_ctrl.Minimum;

				vscrollbar_ctrl.Value = item;
			}

			bool scrollbar_grabbed = false;

			bool InScrollBar {
				get {
					if (vscrollbar_ctrl == null || !vscrollbar_ctrl.is_visible)
						return false;

					return vscrollbar_ctrl.Bounds.Contains (PointToClient (Control.MousePosition));
				}
			}

			protected override void OnMouseDown (MouseEventArgs e)
			{
				if (InScrollBar) {
					vscrollbar_ctrl.FireMouseDown (e);
					scrollbar_grabbed = true;
				}
			}

			protected override void OnMouseMove (MouseEventArgs e)
			{
				if (owner.DropDownStyle == ComboBoxStyle.Simple)
					return;

				if (scrollbar_grabbed || (!Capture && InScrollBar)) {
					vscrollbar_ctrl.FireMouseMove (e);
					return;
				}

				Point pt = PointToClient (Control.MousePosition);
				int index = IndexFromPointDisplayRectangle (pt.X, pt.Y);

				if (index != -1)
					HighlightedIndex = index;
			}
			
			protected override void OnMouseUp (MouseEventArgs e)
			{
				int index = IndexFromPointDisplayRectangle (e.X, e.Y);

				if (scrollbar_grabbed) {
					vscrollbar_ctrl.FireMouseUp (e);
					scrollbar_grabbed = false;
					if (index != -1)
						HighlightedIndex = index;
					return;
				}

				if (index == -1) {
					HideWindow ();
					return;
				}

				bool is_change = owner.SelectedIndex != index;
				
				owner.SetSelectedIndex (index, true);
				owner.OnSelectionChangeCommitted (new EventArgs ());
				
				// If the user selected the already selected item, SelectedIndex
				// won't fire these events, but .Net does, so we do it here
				if (!is_change) {
					owner.OnSelectedValueChanged (EventArgs.Empty);
					owner.OnSelectedIndexChanged (EventArgs.Empty);
				}
				
				HideWindow ();
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

			public void Scroll (int delta)
			{
				if (delta == 0 || vscrollbar_ctrl == null || !vscrollbar_ctrl.Visible)
					return;

				int max = vscrollbar_ctrl.Maximum - page_size + 1;

				int val = vscrollbar_ctrl.Value + delta;
				if (val > max)
					val = max;
				else if (val < vscrollbar_ctrl.Minimum)
					val = vscrollbar_ctrl.Minimum;
				vscrollbar_ctrl.Value = val;
			}

			private void OnMouseWheelCLB (object sender, MouseEventArgs me)
			{
				if (owner.Items.Count == 0)
					return;

				int lines = me.Delta / 120 * SystemInformation.MouseWheelScrollLines;
				Scroll (-lines);
			}

			// Value Changed
			private void VerticalScrollEvent (object sender, EventArgs e)
			{
				if (top_item == vscrollbar_ctrl.Value)
					return;

				top_item =  vscrollbar_ctrl.Value;
				UpdateLastVisibleItem ();
				Invalidate ();
			}
			
			protected override void WndProc(ref Message m) {
				if (m.Msg == (int)Msg.WM_SETFOCUS) {
					owner.Select (false, true);
				}
				base.WndProc (ref m);
			}

			#endregion Private Methods
		}
	}
}
