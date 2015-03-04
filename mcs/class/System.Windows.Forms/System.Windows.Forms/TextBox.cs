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
// Copyright (c) 2004-2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//     Daniel Nauck    (dna(at)mono-project(dot)de)
//

// NOT COMPLETE

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	[ComVisible(true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[Designer ("System.Windows.Forms.Design.TextBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class TextBox : TextBoxBase {
		#region Variables
		private ContextMenu	menu;
		private MenuItem	undo;
		private MenuItem	cut;
		private MenuItem	copy;
		private MenuItem	paste;
		private MenuItem	delete;
		private MenuItem	select_all;

		private bool use_system_password_char;
		private AutoCompleteStringCollection auto_complete_custom_source;
		private AutoCompleteMode auto_complete_mode = AutoCompleteMode.None;
		private AutoCompleteSource auto_complete_source = AutoCompleteSource.None;
		private AutoCompleteListBox auto_complete_listbox;
		private string auto_complete_original_text;
		private int auto_complete_selected_index = -1;
		private List<string> auto_complete_matches;
		private ComboBox auto_complete_cb_source;
		#endregion	// Variables

		#region Public Constructors
		public TextBox() {

			scrollbars = RichTextBoxScrollBars.None;
			alignment = HorizontalAlignment.Left;
			this.LostFocus +=new EventHandler(TextBox_LostFocus);
			this.RightToLeftChanged += new EventHandler (TextBox_RightToLeftChanged);
			MouseWheel += new MouseEventHandler (TextBox_MouseWheel);

			BackColor = SystemColors.Window;
			ForeColor = SystemColors.WindowText;
			backcolor_set = false;

			SetStyle (ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
			SetStyle (ControlStyles.FixedHeight, true);

			undo = new MenuItem(Locale.GetText("&Undo"));
			cut = new MenuItem(Locale.GetText("Cu&t"));
			copy = new MenuItem(Locale.GetText("&Copy"));
			paste = new MenuItem(Locale.GetText("&Paste"));
			delete = new MenuItem(Locale.GetText("&Delete"));
			select_all = new MenuItem(Locale.GetText("Select &All"));

			menu = new ContextMenu(new MenuItem[] { undo, new MenuItem("-"), cut, copy, paste, delete, new MenuItem("-"), select_all});
			ContextMenu = menu;

			menu.Popup += new EventHandler(menu_Popup);
			undo.Click += new EventHandler(undo_Click);
			cut.Click += new EventHandler(cut_Click);
			copy.Click += new EventHandler(copy_Click);
			paste.Click += new EventHandler(paste_Click);
			delete.Click += new EventHandler(delete_Click);
			select_all.Click += new EventHandler(select_all_Click);

			document.multiline = false;
		}

		#endregion	// Public Constructors

		#region Private & Internal Methods

		void TextBox_RightToLeftChanged (object sender, EventArgs e)
		{
			UpdateAlignment ();
		}

		private void TextBox_LostFocus (object sender, EventArgs e) {
			if (hide_selection)
				document.InvalidateSelectionArea ();
			if (auto_complete_listbox != null && auto_complete_listbox.Visible)
				auto_complete_listbox.HideListBox (false);
		}

		private void TextBox_MouseWheel (object o, MouseEventArgs args)
		{
			if (auto_complete_listbox == null || !auto_complete_listbox.Visible)
				return;

			int lines = args.Delta / 120;
			auto_complete_listbox.Scroll (-lines);
		}

		// Receives either WM_KEYDOWN or WM_CHAR that will likely need the generation/lookup
		// of new matches
		private void ProcessAutoCompleteInput (ref Message m, bool deleting_chars)
		{
			// Need to call base.WndProc before to have access to
			// the updated Text property value
			base.WndProc (ref m);
			auto_complete_original_text = Text;
			ShowAutoCompleteListBox (deleting_chars);
		}

		private void ShowAutoCompleteListBox (bool deleting_chars)
		{
			// 
			// We only support CustomSource by now
			//

			IList source = auto_complete_cb_source == null ? auto_complete_custom_source : (IList)auto_complete_cb_source.Items;

			bool append = auto_complete_mode == AutoCompleteMode.Append || auto_complete_mode == AutoCompleteMode.SuggestAppend;
			bool suggest = auto_complete_mode == AutoCompleteMode.Suggest || auto_complete_mode == AutoCompleteMode.SuggestAppend;

			if (Text.Length == 0) {
				if (auto_complete_listbox != null)
					auto_complete_listbox.HideListBox (false);
				return;
			}

			if (auto_complete_matches == null)
				auto_complete_matches = new List<string> ();

			string text = Text;
			auto_complete_matches.Clear ();

			for (int i = 0; i < source.Count; i++) {
				string item_text = auto_complete_cb_source == null ? auto_complete_custom_source [i] :
					auto_complete_cb_source.GetItemText (auto_complete_cb_source.Items [i]);
				if (item_text.StartsWith (text, StringComparison.CurrentCultureIgnoreCase))
					auto_complete_matches.Add (item_text);
			}

			auto_complete_matches.Sort ();

			// Return if we have a single exact match
			if ((auto_complete_matches.Count == 0) || (auto_complete_matches.Count == 1 && 
						auto_complete_matches [0].Equals (text, StringComparison.CurrentCultureIgnoreCase))) {

				if (auto_complete_listbox != null && auto_complete_listbox.Visible)
					auto_complete_listbox.HideListBox (false);
				return;
			}

			auto_complete_selected_index = suggest ? -1 : 0;

			if (suggest) {
				if (auto_complete_listbox == null)
					auto_complete_listbox = new AutoCompleteListBox (this);

				// Show or update auto complete listbox contents
				auto_complete_listbox.Location = PointToScreen (new Point (0, Height));
				auto_complete_listbox.ShowListBox ();
			}

			if (append && !deleting_chars)
				AppendAutoCompleteMatch (0);

			document.MoveCaret (CaretDirection.End);
		}

		internal void HideAutoCompleteList ()
		{
			if (auto_complete_listbox != null)
				auto_complete_listbox.HideListBox (false);
		}

		internal bool IsAutoCompleteAvailable {
			get {
				if (auto_complete_source == AutoCompleteSource.None || auto_complete_mode == AutoCompleteMode.None)
					return false;

				// We only support CustomSource by now, as well as an internal custom source used by ComboBox
				if (auto_complete_source != AutoCompleteSource.CustomSource)
					return false;
				IList custom_source = auto_complete_cb_source == null ? auto_complete_custom_source : (IList)auto_complete_cb_source.Items;
				if (custom_source == null || custom_source.Count == 0)
					return false;

				return true;
			}
		}

		internal ComboBox AutoCompleteInternalSource {
			get {
				return auto_complete_cb_source;
			}
			set {
				auto_complete_cb_source = value;
			}
		}

		internal bool CanNavigateAutoCompleteList {
			get {
				if (auto_complete_mode == AutoCompleteMode.None)
					return false;
				if (auto_complete_matches == null || auto_complete_matches.Count == 0)
					return false;

				bool suggest_window_visible = auto_complete_listbox != null && auto_complete_listbox.Visible;
				if (auto_complete_mode == AutoCompleteMode.Suggest && !suggest_window_visible)
					return false;

				return true;
			}
		}

		bool NavigateAutoCompleteList (Keys key)
		{
			if (auto_complete_matches == null || auto_complete_matches.Count == 0)
				return false;

			bool suggest_window_visible = auto_complete_listbox != null && auto_complete_listbox.Visible;
			if (!suggest_window_visible && auto_complete_mode == AutoCompleteMode.Suggest)
				return false;

			int index = auto_complete_selected_index;

			switch (key) {
				case Keys.Up:
					index -= 1;
					if (index < -1)
						index = auto_complete_matches.Count - 1;
					break;
				case Keys.Down:
					index += 1;
					if (index >= auto_complete_matches.Count)
						index = -1;
					break;
				case Keys.PageUp:
					if (auto_complete_mode == AutoCompleteMode.Append || !suggest_window_visible)
						goto case Keys.Up;

					if (index == -1)
						index = auto_complete_matches.Count - 1;
					else if (index == 0)
						index = -1;
					else {
						index -= auto_complete_listbox.page_size - 1;
						if (index < 0)
							index = 0;
					}
					break;
				case Keys.PageDown:
					if (auto_complete_mode == AutoCompleteMode.Append || !suggest_window_visible)
						goto case Keys.Down;

					if (index == -1)
						index = 0;
					else if (index == auto_complete_matches.Count - 1)
						index = -1;
					else {
						index += auto_complete_listbox.page_size - 1;
						if (index >= auto_complete_matches.Count)
							index = auto_complete_matches.Count - 1;
					}
					break;
				default:
					break;
			}

			// In SuggestAppend mode the navigation mode depends on the visibility of the suggest lb.
			bool suggest = auto_complete_mode == AutoCompleteMode.Suggest || auto_complete_mode == AutoCompleteMode.SuggestAppend;
			if (suggest && suggest_window_visible) {
				Text = index == -1 ? auto_complete_original_text : auto_complete_matches [index];
				auto_complete_listbox.HighlightedIndex = index;
			} else
				// Append only, not suggest at all
				AppendAutoCompleteMatch (index < 0 ? 0 : index);
				
			auto_complete_selected_index = index;
			document.MoveCaret (CaretDirection.End);

			return true;
		}

		void AppendAutoCompleteMatch (int index)
		{
			Text = auto_complete_original_text + auto_complete_matches [index].Substring (auto_complete_original_text.Length);
			SelectionStart = auto_complete_original_text.Length;
			SelectionLength = auto_complete_matches [index].Length - auto_complete_original_text.Length;
		}

		// this is called when the user selects a value from the autocomplete list
		// *with* the mouse
		internal virtual void OnAutoCompleteValueSelected (EventArgs args)
		{
		}

		private void UpdateAlignment ()
		{
			HorizontalAlignment new_alignment = alignment;
			RightToLeft rtol = GetInheritedRtoL ();

			if (rtol == RightToLeft.Yes) {
				if (new_alignment == HorizontalAlignment.Left)
					new_alignment = HorizontalAlignment.Right;
				else if (new_alignment == HorizontalAlignment.Right)
					new_alignment = HorizontalAlignment.Left;
			}

			document.alignment = new_alignment;

			// MS word-wraps if alignment isn't left
			if (Multiline) {
				if (alignment != HorizontalAlignment.Left) {
					document.Wrap = true;
				} else {
					document.Wrap = word_wrap;
				}
			}

			for (int i = 1; i <= document.Lines; i++) {
				document.GetLine (i).Alignment = new_alignment;
			}

			document.RecalculateDocument (CreateGraphicsInternal ());

			Invalidate ();	// Make sure we refresh
		}

		internal override Color ChangeBackColor (Color backColor)
		{
			if (backColor == Color.Empty) {
				if (!ReadOnly)
					backColor = SystemColors.Window;

				backcolor_set = false;
			}

			return backColor;
		}

		void OnAutoCompleteCustomSourceChanged(object sender, CollectionChangeEventArgs e) {
			if(auto_complete_source == AutoCompleteSource.CustomSource) {
				//FIXME: handle add, remove and refresh events in AutoComplete algorithm.
			}
		}
		#endregion	// Private & Internal Methods

		#region Public Instance Properties
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
			}
		}

		[MonoTODO("AutoCompletion algorithm is currently not implemented.")]
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DefaultValue (AutoCompleteSource.None)]
		[TypeConverter (typeof (TextBoxAutoCompleteSourceConverter))]
		public AutoCompleteSource AutoCompleteSource {
			get { return auto_complete_source; }
			set {
				if(auto_complete_source == value)
					return;

				if(!Enum.IsDefined (typeof (AutoCompleteSource), value))
					throw new InvalidEnumArgumentException (Locale.GetText ("Enum argument value '{0}' is not valid for AutoCompleteSource", value));

				auto_complete_source = value;
			}
		}

		[DefaultValue(false)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public bool UseSystemPasswordChar {
			get {
				return use_system_password_char;
			}

			set {
				if (use_system_password_char != value) {
					use_system_password_char = value;
					
					if (!Multiline)
						document.PasswordChar = PasswordChar.ToString ();
					else
						document.PasswordChar = string.Empty;
					Invalidate ();
				}
			}
		}

		[DefaultValue(false)]
		[MWFCategory("Behavior")]
		public bool AcceptsReturn {
			get {
				return accepts_return;
			}

			set {
				if (value != accepts_return) {
					accepts_return = value;
				}
			}
		}

		[DefaultValue(CharacterCasing.Normal)]
		[MWFCategory("Behavior")]
		public CharacterCasing CharacterCasing {
			get {
				return character_casing;
			}

			set {
				if (value != character_casing) {
					character_casing = value;
				}
			}
		}

		[Localizable(true)]
		[DefaultValue('\0')]
		[MWFCategory("Behavior")]
		[RefreshProperties (RefreshProperties.Repaint)]
		public char PasswordChar {
			get {
				if (use_system_password_char) {
					return '*';
				}
				return password_char;
			}

			set {
				if (value != password_char) {
					password_char = value;
					if (!Multiline) {
						document.PasswordChar = PasswordChar.ToString ();
					} else {
						document.PasswordChar = string.Empty;
					}
					this.CalculateDocument();
				}
			}
		}

		[DefaultValue(ScrollBars.None)]
		[Localizable(true)]
		[MWFCategory("Appearance")]
		public ScrollBars ScrollBars {
			get {
				return (ScrollBars)scrollbars;
			}

			set {
				if (!Enum.IsDefined (typeof (ScrollBars), value))
					throw new InvalidEnumArgumentException ("value", (int) value,
						typeof (ScrollBars));

				if (value != (ScrollBars)scrollbars) {
					scrollbars = (RichTextBoxScrollBars)value;
					base.CalculateScrollBars();
				}
			}
		}

		public override string Text {
			get {
				return base.Text;
			}

			set {
				base.Text = value;
			}
		}

		[DefaultValue(HorizontalAlignment.Left)]
		[Localizable(true)]
		[MWFCategory("Appearance")]
		public HorizontalAlignment TextAlign {
			get {
				return alignment;
			}

			set {
				if (value != alignment) {
					alignment = value;

					UpdateAlignment ();

					OnTextAlignChanged(EventArgs.Empty);
				}
			}
		}
		#endregion	// Public Instance Properties

		public void Paste (string text)
		{
			document.ReplaceSelection (CaseAdjust (text), false);

			ScrollToCaret();
			OnTextChanged(EventArgs.Empty);
		}
		#region Protected Instance Methods
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		protected override bool IsInputKey (Keys keyData)
		{
			return base.IsInputKey (keyData);
		}

		protected override void OnGotFocus (EventArgs e)
		{
			base.OnGotFocus (e);
			if (selection_length == -1 && !has_been_focused)
				SelectAllNoScroll ();
			has_been_focused = true;
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected virtual void OnTextAlignChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [TextAlignChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void WndProc (ref Message m)
		{
			switch ((Msg)m.Msg) {
				case Msg.WM_KEYDOWN:
					if (!IsAutoCompleteAvailable)
						break;

					Keys key_data = (Keys)m.WParam.ToInt32 ();
					switch (key_data) {
						case Keys.Down:
						case Keys.Up:
						case Keys.PageDown:
						case Keys.PageUp:
							if (NavigateAutoCompleteList (key_data)) {
								m.Result = IntPtr.Zero;
								return;
							}
							break;
						case Keys.Enter:
							if (auto_complete_listbox != null && auto_complete_listbox.Visible)
								auto_complete_listbox.HideListBox (false);
							SelectAll ();
							break;
						case Keys.Escape:
							if (auto_complete_listbox != null && auto_complete_listbox.Visible)
								auto_complete_listbox.HideListBox (false);
							break;
						case Keys.Delete:
							ProcessAutoCompleteInput (ref m, true);
							return;
						default:
							break;
					}
					break;
				case Msg.WM_CHAR:
					if (!IsAutoCompleteAvailable)
						break;

					// Don't handle either Enter or Esc - they are handled in the WM_KEYDOWN case
					int char_value = m.WParam.ToInt32 ();
					if (char_value == 13 || char_value == 27)
						break;

					ProcessAutoCompleteInput (ref m, char_value == 8);
					return;
				case Msg.WM_LBUTTONDOWN:
					// When the textbox gets focus by LBUTTON (but not by middle or right)
					// it does not do the select all / scroll thing.
					has_been_focused = true;
					FocusInternal (true);
					break;
			}

			base.WndProc(ref m);
		}
		#endregion	// Protected Instance Methods

		#region Events
		static object TextAlignChangedEvent = new object ();

		public event EventHandler TextAlignChanged {
			add { Events.AddHandler (TextAlignChangedEvent, value); }
			remove { Events.RemoveHandler (TextAlignChangedEvent, value); }
		}
		#endregion	// Events

		#region Private Methods

		internal override ContextMenu ContextMenuInternal {
			get {
				ContextMenu res = base.ContextMenuInternal;
				if (res == menu)
					return null;
				return res;
			}
			set {
				base.ContextMenuInternal = value;
			}
		}

		internal void RestoreContextMenu ()
		{
			ContextMenuInternal = menu;
		}

		private void menu_Popup(object sender, EventArgs e) {
			if (SelectionLength == 0) {
				cut.Enabled = false;
				copy.Enabled = false;
			} else {
				cut.Enabled = true;
				copy.Enabled = true;
			}

			if (SelectionLength == TextLength) {
				select_all.Enabled = false;
			} else {
				select_all.Enabled = true;
			}

			if (!CanUndo) {
				undo.Enabled = false;
			} else {
				undo.Enabled = true;
			}

			if (ReadOnly) {
				undo.Enabled = cut.Enabled = paste.Enabled = delete.Enabled = false;
			}
		}

		private void undo_Click(object sender, EventArgs e) {
			Undo();
		}

		private void cut_Click(object sender, EventArgs e) {
			Cut();
		}

		private void copy_Click(object sender, EventArgs e) {
			Copy();
		}

		private void paste_Click(object sender, EventArgs e) {
			Paste();
		}

		private void delete_Click(object sender, EventArgs e) {
			SelectedText = string.Empty;
		}

		private void select_all_Click(object sender, EventArgs e) {
			SelectAll();
		}
		#endregion	// Private Methods

		public override bool Multiline {
			get {
				return base.Multiline;
			}

			set {
				base.Multiline = value;
			}
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}
		
		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		class AutoCompleteListBox : Control
		{
			TextBox owner;
			VScrollBar vscroll;
			int top_item;
			int last_item;
			internal int page_size;
			int item_height;
			int highlighted_index = -1;
			bool user_defined_size;
			bool resizing;
			Rectangle resizer_bounds;

			const int DefaultDropDownItems = 7;

			public AutoCompleteListBox (TextBox tb)
			{
				owner = tb;
				item_height = FontHeight + 2;

				vscroll = new VScrollBar ();
				vscroll.ValueChanged += VScrollValueChanged;
				Controls.Add (vscroll);

				is_visible = false;
				InternalBorderStyle = BorderStyle.FixedSingle;
			}

			protected override CreateParams CreateParams {
				get {
					CreateParams cp = base.CreateParams;

					cp.Style ^= (int)WindowStyles.WS_CHILD;
					cp.Style ^= (int)WindowStyles.WS_VISIBLE;
					cp.Style |= (int)WindowStyles.WS_POPUP;
					cp.ExStyle |= (int)WindowExStyles.WS_EX_TOPMOST | (int)WindowExStyles.WS_EX_TOOLWINDOW;
					return cp;
				}
			}

			public int HighlightedIndex {
				get {
					return highlighted_index;
				}
				set {
					if (value == highlighted_index)
						return;

					if (highlighted_index != -1)
						Invalidate (GetItemBounds (highlighted_index));
					highlighted_index = value;
					if (highlighted_index != -1)
						Invalidate (GetItemBounds (highlighted_index));

					if (highlighted_index != -1)
						EnsureVisible (highlighted_index);
				}
			}

			public void Scroll (int lines)
			{
				int max = vscroll.Maximum - page_size + 1;
				int val = vscroll.Value + lines;
				if (val > max)
					val = max;
				else if (val < vscroll.Minimum)
					val = vscroll.Minimum;

				vscroll.Value = val;
			}

			public void EnsureVisible (int index)
			{
				if (index < top_item) {
					vscroll.Value = index;
				} else {
					int max = vscroll.Maximum - page_size + 1;
					int rows = Height / item_height;
					if (index > top_item + rows - 1) {
						index = index - rows + 1;
						vscroll.Value = index > max ? max : index;
					}
				}
			}

			internal override bool ActivateOnShow {
				get {
					return false;
				}
			}

			void VScrollValueChanged (object o, EventArgs args)
			{
				if (top_item == vscroll.Value)
					return;

				top_item = vscroll.Value;
				last_item = GetLastVisibleItem ();
				Invalidate ();
			}

			int GetLastVisibleItem ()
			{
				int top_y = Height;

				for (int i = top_item; i < owner.auto_complete_matches.Count; i++) {
					int pos = i - top_item; // relative to visible area
					if ((pos * item_height) + item_height >= top_y)
						return i;
				}

				return owner.auto_complete_matches.Count - 1;
			}

			Rectangle GetItemBounds (int index)
			{
				int pos = index - top_item;
				Rectangle bounds = new Rectangle (0, pos * item_height, Width, item_height);
				if (vscroll.Visible)
					bounds.Width -= vscroll.Width;

				return bounds;
			}

			int GetItemAt (Point loc)
			{
				if (loc.Y > (last_item - top_item) * item_height + item_height)
					return -1;

				int retval = loc.Y / item_height;
				retval += top_item;

				return retval;
			}

			void LayoutListBox ()
			{
				int total_height = owner.auto_complete_matches.Count * item_height;
				page_size = Math.Max (Height / item_height, 1);
				last_item = GetLastVisibleItem ();

				if (Height < total_height) {
					vscroll.Visible = true;
					vscroll.Maximum = owner.auto_complete_matches.Count - 1;
					vscroll.LargeChange = page_size;
					vscroll.Location = new Point (Width - vscroll.Width, 0);
					vscroll.Height = Height - item_height;
				} else
					vscroll.Visible = false;

				resizer_bounds = new Rectangle (Width - item_height, Height - item_height,
						item_height, item_height);
			}

			public void HideListBox (bool set_text)
			{
				if (set_text)
					owner.Text = owner.auto_complete_matches [HighlightedIndex];

				Capture = false;
				Hide ();
			}

			public void ShowListBox ()
			{
				if (!user_defined_size) {
					// This should call the Layout routine for us
					int height = owner.auto_complete_matches.Count > DefaultDropDownItems ? 
						DefaultDropDownItems * item_height : (owner.auto_complete_matches.Count + 1) * item_height;
					Size = new Size (owner.Width, height);
				} else
					LayoutListBox ();

				vscroll.Value = 0;
				HighlightedIndex = -1;

				Show ();
				// make sure we are on top - call the raw routine, since we are parentless
				XplatUI.SetZOrder (Handle, IntPtr.Zero, true, false);
				Invalidate ();
			}

			protected override void OnResize (EventArgs args)
			{
				base.OnResize (args);

				LayoutListBox ();
				Refresh ();
			}

			protected override void OnMouseDown (MouseEventArgs args)
			{
				base.OnMouseDown (args);

				if (!resizer_bounds.Contains (args.Location))
					return;

				user_defined_size = true;
				resizing = true;
				Capture = true;
			}

			protected override void OnMouseMove (MouseEventArgs args)
			{
				base.OnMouseMove (args);

				if (resizing) {
					Point mouse_loc = Control.MousePosition;
					Point ctrl_loc = PointToScreen (Point.Empty);

					Size new_size = new Size (mouse_loc.X - ctrl_loc.X, mouse_loc.Y - ctrl_loc.Y);
					if (new_size.Height < item_height)
						new_size.Height = item_height;
					if (new_size.Width < item_height)
						new_size.Width = item_height;

					Size = new_size;
					return;
				}

				Cursor = resizer_bounds.Contains (args.Location) ? Cursors.SizeNWSE : Cursors.Default;

				int item_idx = GetItemAt (args.Location);
				if (item_idx != -1)
					HighlightedIndex = item_idx;
			}

			protected override void OnMouseUp (MouseEventArgs args)
			{
				base.OnMouseUp (args);

				int item_idx = GetItemAt (args.Location);
				if (item_idx != -1 && !resizing)
					HideListBox (true);

				owner.OnAutoCompleteValueSelected (EventArgs.Empty); // internal
				resizing = false;
				Capture = false;
			}

			internal override void OnPaintInternal (PaintEventArgs args)
			{
				Graphics g = args.Graphics;
				Brush brush = ThemeEngine.Current.ResPool.GetSolidBrush (ForeColor);

				int highlighted_idx = HighlightedIndex;

				int y = 0;
				int last = GetLastVisibleItem ();
				for (int i = top_item; i <= last; i++) {
					Rectangle item_bounds = GetItemBounds (i);
					if (!item_bounds.IntersectsWith (args.ClipRectangle))
						continue;

					if (i == highlighted_idx) {
						g.FillRectangle (SystemBrushes.Highlight, item_bounds);
						g.DrawString (owner.auto_complete_matches [i], Font, SystemBrushes.HighlightText, item_bounds);
					} else 
						g.DrawString (owner.auto_complete_matches [i], Font, brush, item_bounds);

					y += item_height;
				}

				ThemeEngine.Current.CPDrawSizeGrip (g, SystemColors.Control, resizer_bounds);
			}
		}
	}
	
	internal class TextBoxAutoCompleteSourceConverter : EnumConverter
	{
		public TextBoxAutoCompleteSourceConverter(Type type)
			: base(type)
		{ }

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			StandardValuesCollection stdv = base.GetStandardValues(context);
			AutoCompleteSource[] arr = new AutoCompleteSource[stdv.Count];
			stdv.CopyTo(arr, 0);
			AutoCompleteSource[] arr2 = Array.FindAll(arr, delegate (AutoCompleteSource value) {
				// No "ListItems" in a TextBox.
				return value != AutoCompleteSource.ListItems;
			});
			return new StandardValuesCollection(arr2);
		}
	}
}
