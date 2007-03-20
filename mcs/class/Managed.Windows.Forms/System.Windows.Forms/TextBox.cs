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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
#if NET_2_0
using System.Runtime.InteropServices;
#endif

namespace System.Windows.Forms {

#if NET_2_0
	[ComVisible(true)]
#endif
	public class TextBox : TextBoxBase {
		#region Variables
		private ContextMenu	menu;
		private MenuItem	undo;
		private MenuItem	cut;
		private MenuItem	copy;
		private MenuItem	paste;
		private MenuItem	delete;
		private MenuItem	select_all;

		private bool has_been_focused;
#if NET_2_0
		private bool use_system_password_char = false;
		private AutoCompleteStringCollection auto_complete_custom_source = null;
		private AutoCompleteMode auto_complete_mode = AutoCompleteMode.None;
		private AutoCompleteSource auto_complete_source = AutoCompleteSource.None;
#endif
		#endregion	// Variables

		#region Public Constructors
		public TextBox() {

			scrollbars = RichTextBoxScrollBars.None;
			alignment = HorizontalAlignment.Left;
			this.LostFocus +=new EventHandler(TextBox_LostFocus);

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
		private void TextBox_LostFocus(object sender, EventArgs e) {
			if (hide_selection)
				document.InvalidateSelectionArea ();
		}
#if NET_2_0
		void OnAutoCompleteCustomSourceChanged(object sender, CollectionChangeEventArgs e) {
			if(auto_complete_source == AutoCompleteSource.CustomSource) {
				//FIXME: handle add, remove and refresh events in AutoComplete algorithm.
			}
		}
#endif
		#endregion	// Private & Internal Methods

		#region Public Instance Properties
#if NET_2_0
		[MonoTODO("AutoCompletion algorithm is currently not implemented.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[Localizable (true)]
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
		public bool UseSystemPasswordChar {
			get {
				return use_system_password_char;
			}

			set {
				use_system_password_char = value;
			}
		}
#endif

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
		public char PasswordChar {
			get {
#if NET_2_0
				if (use_system_password_char) {
					return '*';
				}
#endif
				return password_char;
			}

			set {
				if (value != password_char) {
					password_char = value;
					if (!Multiline) {
						document.PasswordChar = value.ToString();
					} else {
						document.PasswordChar = "";
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
				if (value != (ScrollBars)scrollbars) {
					scrollbars = (RichTextBoxScrollBars)value;
					base.CalculateScrollBars();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override int SelectionLength {
			get {
				return base.SelectionLength;
			}
			set {
				base.SelectionLength = value;
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

					document.alignment = value;

					// MS word-wraps if alignment isn't left
					if (Multiline) {
						if (alignment != HorizontalAlignment.Left) {
							document.Wrap = true;
						} else {
							document.Wrap = word_wrap;
						}
					}

					for (int i = 1; i <= document.Lines; i++) {
						document.GetLine(i).Alignment = value;
					}
					document.RecalculateDocument(CreateGraphicsInternal());
					OnTextAlignChanged(EventArgs.Empty);
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override ImeMode DefaultImeMode {
			get {
				return base.DefaultImeMode;
			}
		}
		#endregion	// Protected Instance Methods

		#region Protected Instance Methods
#if NET_2_0
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
#endif

		protected override bool IsInputKey(Keys keyData) {
			return base.IsInputKey (keyData);
		}

		protected override void OnGotFocus(EventArgs e) {
			base.OnGotFocus (e);
			if (!has_been_focused)
				SelectAll ();
			has_been_focused = true;
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp (e);
		}

		protected virtual void OnTextAlignChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [TextAlignChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void WndProc(ref Message m) {
			switch ((Msg)m.Msg) {
				case Msg.WM_LBUTTONDOWN:
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

		internal override ContextMenu GetContextMenuInternal ()
		{
			ContextMenu  res = base.GetContextMenuInternal ();
			if (res == menu)
				return null;
			return res;
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
			SelectedText = "";
		}

		private void select_all_Click(object sender, EventArgs e) {
			SelectAll();
		}
		#endregion	// Private Methods

#if NET_2_0
		public override bool Multiline {
			get {
				return base.Multiline;
			}

			set {
				base.Multiline = value;
			}
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}
#endif
	}
}
