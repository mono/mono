// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:c
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
//
//

// NOT COMPLETE
#undef Debug
#undef DebugClick

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace System.Windows.Forms
{
#if NET_2_0
	[ComVisible (true)]
	[DefaultBindingProperty ("Text")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
#endif
	[DefaultEvent("TextChanged")]
	[Designer("System.Windows.Forms.Design.TextBoxBaseDesigner, " + Consts.AssemblySystem_Design)]
	public abstract class TextBoxBase : Control
	{
		#region Local Variables
		internal HorizontalAlignment	alignment;
		internal bool			accepts_tab;
		internal bool			accepts_return;
		internal bool			auto_size;
		internal bool			backcolor_set;
		internal CharacterCasing	character_casing;
		internal bool			hide_selection;
		int				max_length;
		internal bool			modified;
		internal char			password_char;
		internal bool			read_only;
		internal bool			word_wrap;
		internal Document		document;
		internal LineTag		caret_tag;		// tag our cursor is in
		internal int			caret_pos;		// position on the line our cursor is in (can be 0 = beginning of line)
		internal ImplicitHScrollBar	hscroll;
		internal ImplicitVScrollBar	vscroll;
		internal RichTextBoxScrollBars	scrollbars;
		internal Timer			scroll_timer;
		internal bool			richtext;
		internal bool			show_selection;		// set to true to always show selection, even if no focus is set
		internal ArrayList		list_links;		// currently showing links
		private LinkRectangle		current_link;		// currently hovering link
		private bool			enable_links;		// whether links are enabled
		
		internal bool has_been_focused;

		internal int			selection_length = -1;	// set to the user-specified selection length, or -1 if none
		internal bool show_caret_w_selection;  // TextBox shows the caret when the selection is visible
		internal int			canvas_width;
		internal int			canvas_height;
		static internal int		track_width = 2;	//
		static internal int		track_border = 5;	//
		internal DateTime		click_last;
		internal int			click_point_x;
		internal int 			click_point_y;
		internal CaretSelection		click_mode;
		internal BorderStyle actual_border_style;
		internal bool shortcuts_enabled = true;
		#if Debug
		internal static bool	draw_lines = false;
		#endif

		#endregion	// Local Variables

		#region Internal Constructor
		// Constructor will go when complete, only for testing - pdb
		internal TextBoxBase ()
		{
			alignment = HorizontalAlignment.Left;
			accepts_return = false;
			accepts_tab = false;
			auto_size = true;
			InternalBorderStyle = BorderStyle.Fixed3D;
			actual_border_style = BorderStyle.Fixed3D;
			character_casing = CharacterCasing.Normal;
			hide_selection = true;
			max_length = short.MaxValue;
			password_char = '\0';
			read_only = false;
			word_wrap = true;
			richtext = false;
			show_selection = false;
			enable_links = false;
			list_links = new ArrayList ();
			current_link = null;
			show_caret_w_selection = (this is TextBox);
			document = new Document(this);
			document.WidthChanged += new EventHandler(document_WidthChanged);
			document.HeightChanged += new EventHandler(document_HeightChanged);
			//document.CaretMoved += new EventHandler(CaretMoved);
			document.Wrap = false;
			click_last = DateTime.Now;
			click_mode = CaretSelection.Position;

			MouseDown += new MouseEventHandler(TextBoxBase_MouseDown);
			MouseUp += new MouseEventHandler(TextBoxBase_MouseUp);
			MouseMove += new MouseEventHandler(TextBoxBase_MouseMove);
			SizeChanged += new EventHandler(TextBoxBase_SizeChanged);
			FontChanged += new EventHandler(TextBoxBase_FontOrColorChanged);
			ForeColorChanged += new EventHandler(TextBoxBase_FontOrColorChanged);
			MouseWheel += new MouseEventHandler(TextBoxBase_MouseWheel);
			RightToLeftChanged += new EventHandler (TextBoxBase_RightToLeftChanged);
			
			scrollbars = RichTextBoxScrollBars.None;

			hscroll = new ImplicitHScrollBar();
			hscroll.ValueChanged += new EventHandler(hscroll_ValueChanged);
			hscroll.SetStyle (ControlStyles.Selectable, false);
			hscroll.Enabled = false;
			hscroll.Visible = false;
			hscroll.Maximum = Int32.MaxValue;

			vscroll = new ImplicitVScrollBar();
			vscroll.ValueChanged += new EventHandler(vscroll_ValueChanged);
			vscroll.SetStyle (ControlStyles.Selectable, false);
			vscroll.Enabled = false;
			vscroll.Visible = false;
			vscroll.Maximum = Int32.MaxValue;

			SuspendLayout ();
			this.Controls.AddImplicit (hscroll);
			this.Controls.AddImplicit (vscroll);
			ResumeLayout ();
			
			SetStyle(ControlStyles.UserPaint | ControlStyles.StandardClick, false);
#if NET_2_0
			SetStyle(ControlStyles.UseTextForAccessibility, false);
			
			base.SetAutoSizeMode (AutoSizeMode.GrowAndShrink);
#endif

			canvas_width = ClientSize.Width;
			canvas_height = ClientSize.Height;
			document.ViewPortWidth = canvas_width;
			document.ViewPortHeight = canvas_height;

			Cursor = Cursors.IBeam;
		}
		#endregion	// Internal Constructor

		#region Private and Internal Methods
		internal string CaseAdjust (string s)
		{
			if (character_casing == CharacterCasing.Normal)
				return s;
			if (character_casing == CharacterCasing.Lower)
				return s.ToLower();
			return s.ToUpper();
		}

#if NET_2_0
		internal override Size GetPreferredSizeCore (Size proposedSize)
		{
			return new Size (Width, Height);
		}
#endif

		internal override void HandleClick (int clicks, MouseEventArgs me)
		{
			// MS seems to fire the click event in spite of the styles they set
			bool click_set = GetStyle (ControlStyles.StandardClick);
			bool doubleclick_set = GetStyle (ControlStyles.StandardDoubleClick);

			// so explicitly set them to true first
			SetStyle (ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);

			base.HandleClick (clicks, me);

			// then revert to our previous state
			if (!click_set)
				SetStyle (ControlStyles.StandardClick, false);
			if (!doubleclick_set)
				SetStyle (ControlStyles.StandardDoubleClick, false);
		}

		internal override void PaintControlBackground (PaintEventArgs pevent)
		{
			if (!ThemeEngine.Current.TextBoxBaseShouldPaintBackground (this))
				return;
			base.PaintControlBackground (pevent);
		}
		#endregion	// Private and Internal Methods

		#region Public Instance Properties
		[DefaultValue(false)]
		[MWFCategory("Behavior")]
		public bool AcceptsTab {
			get {
				return accepts_tab;
			}

			set {
				if (value != accepts_tab) {
					accepts_tab = value;
					OnAcceptsTabChanged(EventArgs.Empty);
				}
			}
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
#endif
		[DefaultValue(true)]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[MWFCategory("Behavior")]
		public
#if NET_2_0
		override
#else
		virtual
#endif
		bool AutoSize {
			get {
				return auto_size;
			}

			set {
				if (value != auto_size) {
					auto_size = value;
					if (auto_size) {
						if (PreferredHeight != ClientSize.Height) {
							ClientSize = new Size(ClientSize.Width, PreferredHeight);
						}
					}
#if NET_1_1
					OnAutoSizeChanged(EventArgs.Empty);
#endif
				}
			}
		}

		[DispId(-501)]
		public override System.Drawing.Color BackColor {
			get {
				return base.BackColor;
			}
			set {
				backcolor_set = true;
				base.BackColor = ChangeBackColor (value);
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override System.Drawing.Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}
			set {
				base.BackgroundImage = value;
			}
		}

		[DefaultValue(BorderStyle.Fixed3D)]
		[DispId(-504)]
		[MWFCategory("Appearance")]
		public BorderStyle BorderStyle {
			get { return actual_border_style; }
			set {
				if (value == actual_border_style)
					return;

				if (actual_border_style != BorderStyle.Fixed3D || value != BorderStyle.Fixed3D)
					Invalidate ();

				actual_border_style = value;
				document.UpdateMargins ();

				if (value != BorderStyle.Fixed3D)
					value = BorderStyle.None;

				InternalBorderStyle = value; 
				OnBorderStyleChanged(EventArgs.Empty);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanUndo {
			get {
				return document.undo.CanUndo;
			}
		}

		[DispId(-513)]
		public override System.Drawing.Color ForeColor {
			get {
				return base.ForeColor;
			}
			set {
				base.ForeColor = value;
			}
		}

		[DefaultValue(true)]
		[MWFCategory("Behavior")]
		public bool HideSelection {
			get {
				return hide_selection;
			}

			set {
				if (value != hide_selection) {
					hide_selection = value;
					OnHideSelectionChanged(EventArgs.Empty);
				}
				document.selection_visible = !hide_selection;
				document.InvalidateSelectionArea();
			}
		}

#if NET_2_0
		[MergableProperty (false)]
#endif
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Editor("System.Windows.Forms.Design.StringArrayEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[Localizable(true)]
		[MWFCategory("Appearance")]
		public string[] Lines {
			get {
				int count;
				ArrayList lines;

				count = document.Lines;

				// Handle empty document
				if ((count == 1) && (document.GetLine (1).text.Length == 0)) {
					return new string [0];
				}

				lines = new ArrayList ();

				int i = 1;
				while (i <= count) {
					Line line;
					StringBuilder lt = new StringBuilder ();

					do {
						line = document.GetLine (i++);
						lt.Append (line.TextWithoutEnding ());
					} while (line.ending == LineEnding.Wrap && i <= count);

					lines.Add (lt.ToString ());	
				}

				return (string []) lines.ToArray (typeof (string));
			}

			set {
				StringBuilder sb = new StringBuilder ();
			
				for (int i = 0; i < value.Length; i++) {
					// Don't add the last line if it is just an empty line feed
					// the line feed is reflected in the previous line's ending 
					if (i == value.Length - 1 && value[i].Length == 0)
						break;
						
					sb.Append (value[i] + Environment.NewLine);
				}

				int newline_length = Environment.NewLine.Length;

				// We want to remove the final new line character
				if (sb.Length >= newline_length)
					sb.Remove (sb.Length - newline_length, newline_length);

				Text = sb.ToString ();
			}
		}

		[DefaultValue(32767)]
		[Localizable(true)]
		[MWFCategory("Behavior")]
		public virtual int MaxLength {
			get {
				if (max_length == (int.MaxValue - 1)) {	// We don't distinguish between single and multi-line limits
					return 0;
				}
				return max_length;
			}

			set {
				if (value != max_length) {
					if (value == 0)
						value = int.MaxValue - 1;

					max_length = value;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Modified {
			get {
				return modified;
			}

			set {
				if (value != modified) {
					modified = value;
					OnModifiedChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(false)]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.All)]
		[MWFCategory("Behavior")]
		public virtual bool Multiline {
			get {
				return document.multiline;
			}

			set {
				if (value != document.multiline) {
					document.multiline = value;

					if (this is TextBox)
						SetStyle (ControlStyles.FixedHeight, !value);

					// SetBoundsCore overrides the Height for multiline if it needs to,
					// so we don't need to worry about it here.
					SetBoundsCore (Left, Top, Width, ExplicitBounds.Height, BoundsSpecified.None);
					
					if (Parent != null)
						Parent.PerformLayout ();

					OnMultilineChanged(EventArgs.Empty);
				}

				if (document.multiline) {
					document.Wrap = word_wrap;
					document.PasswordChar = "";

				} else {
					document.Wrap = false;
					if (this.password_char != '\0') {
						if (this is TextBox)
							document.PasswordChar = (this as TextBox).PasswordChar.ToString ();
					} else {
						document.PasswordChar = "";
					}
				}

				if (IsHandleCreated)
					CalculateDocument ();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public int PreferredHeight {
			get {
				if (BorderStyle != BorderStyle.None)
					return Font.Height + 7;

				// usually in borderless mode the top margin is 0, but
				// try to access it, in case it was set manually, as ToolStrip* controls do
				return Font.Height + TopMargin;
			}
		}

#if NET_2_0
		[RefreshProperties (RefreshProperties.Repaint)]	
#endif
		[DefaultValue(false)]
		[MWFCategory("Behavior")]
		public bool ReadOnly {
			get {
				return read_only;
			}

			set {
				if (value != read_only) {
					read_only = value;
#if NET_2_0
					if (!backcolor_set) {
						if (read_only)
							background_color = SystemColors.Control;
						else
							background_color = SystemColors.Window;
					}
#endif
					OnReadOnlyChanged(EventArgs.Empty);
					Invalidate ();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual string SelectedText {
			get {
				string retval = document.GetSelection ();

#if ONLY_1_1
				if (!IsHandleCreated && retval == string.Empty)
					return null;
#endif
				return retval;
			}

			set {
				document.ReplaceSelection(CaseAdjust(value), false);

				ScrollToCaret();
				OnTextChanged(EventArgs.Empty);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual int SelectionLength {
			get {
				int res = document.SelectionLength ();

#if !NET_2_0
				if (res == 0 && !IsHandleCreated)
					res = -1;
#endif

				return res;
			}

			set {
				if (value < 0) {
					string msg = String.Format ("'{0}' is not a valid value for 'SelectionLength'", value);
#if NET_2_0
					throw new ArgumentOutOfRangeException ("SelectionLength", msg);
#else
					throw new ArgumentException (msg);
#endif
				}

				document.InvalidateSelectionArea ();
				if (value != 0) {
					int	start;
					Line	line;
					LineTag	tag;
					int	pos;

					selection_length = value;
					start = document.LineTagToCharIndex (document.selection_start.line, document.selection_start.pos);
					document.CharIndexToLineTag (start + value, out line, out tag, out pos);
					document.SetSelectionEnd (line, pos, true);
					document.PositionCaret (line, pos);
				} else {
					selection_length = -1;
					document.SetSelectionEnd (document.selection_start.line, document.selection_start.pos, true);
					document.PositionCaret (document.selection_start.line, document.selection_start.pos);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionStart {
			get {
				return document.LineTagToCharIndex(document.selection_start.line,
					document.selection_start.pos);
			}

			set {
				if (value < 0) {
					string msg = String.Format ("'{0}' is not a valid value for 'SelectionStart'", value);
#if NET_2_0
					throw new ArgumentOutOfRangeException ("SelectionStart", msg);
#else
					throw new ArgumentException (msg);
#endif
				}

				// If SelectionStart has been used, we don't highlight on focus
				has_been_focused = true;
				
				document.InvalidateSelectionArea ();
				document.SetSelectionStart (value, false);
				if (selection_length > -1)
					document.SetSelectionEnd (value + selection_length, true);
				else
					document.SetSelectionEnd (value, true);
				document.PositionCaret (document.selection_start.line, document.selection_start.pos);
				ScrollToCaret ();
			}
		}

#if NET_2_0
		[DefaultValue (true)]
		public virtual bool ShortcutsEnabled {
			get { return shortcuts_enabled; }
			set { shortcuts_enabled = value; }
		}

		[Editor ("System.ComponentModel.Design.MultilineStringEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
#endif
		[Localizable(true)]
		public override string Text {
			get {
				if (document == null || document.Root == null || document.Root.text == null)
					return string.Empty;

				StringBuilder sb = new StringBuilder();

				Line line = null;
				for (int i = 1; i <= document.Lines; i++) {
					line = document.GetLine (i);
					sb.Append(line.text.ToString ());
				}

				return sb.ToString();
			}

			set {
				// reset to force a select all next time the box gets focus
				has_been_focused = false;

				if (value == Text)
					return;

				document.Empty ();
				if ((value != null) && (value != "")) {
					document.Insert (document.GetLine (1), 0, false, value);
				} else {
					if (IsHandleCreated) {
						document.SetSelectionToCaret (true);
						CalculateDocument ();
					}
				}
							
				document.PositionCaret (document.GetLine (1), 0);
				document.SetSelectionToCaret (true);

				ScrollToCaret ();

				OnTextChanged(EventArgs.Empty);
			}
		}

		[Browsable(false)]
		public virtual int TextLength {
			get {
				if (document == null || document.Root == null || document.Root.text == null)
					return 0;
				return Text.Length;
			}
		}

		[DefaultValue(true)]
		[Localizable(true)]
		[MWFCategory("Behavior")]
		public bool WordWrap {
			get {
				return word_wrap;
			}

			set {
				if (value != word_wrap) {
					if (document.multiline) {
						word_wrap = value;
						document.Wrap = value;
					}
					CalculateDocument ();
				}
			}
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; } 
			set { base.BackgroundImageLayout = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Padding Padding {
			get { return base.Padding; }
			set { base.Padding = value; }
		}
		
		protected override Cursor DefaultCursor {
			get { return Cursors.IBeam; }
		}
#endif
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
#if NET_2_0
		protected override bool CanEnableIme {
			get {
				if (ReadOnly || password_char != '\0')
					return false;
					
				return true;
			}
		}
#endif

		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override System.Drawing.Size DefaultSize {
			get {
				return new Size(100, 20);
			}
		}

#if NET_2_0
		// Currently our double buffering breaks our scrolling, so don't let people enable this
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override bool DoubleBuffered {
			get { return false; }
			set { }
		}
#endif

		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void AppendText (string text)
		{
			// Save some cycles and only check the Text if we are one line
			bool is_empty = document.Lines == 1 && Text == String.Empty; 

			// make sure the caret begins at the end
			if (document.caret.line.line_no != document.Lines ||
			     (document.caret.pos) != document.caret.line.TextLengthWithoutEnding ()) {
				document.MoveCaret (CaretDirection.CtrlEnd);
			}
			document.Insert (document.caret.line, document.caret.pos, false, text, document.CaretTag);
			document.MoveCaret (CaretDirection.CtrlEnd);
			document.SetSelectionToCaret (true);

			if (!is_empty)
				ScrollToCaret ();

			//
			// Avoid the initial focus selecting all when append text is used
			//
			has_been_focused = true;

			Modified = false;
			OnTextChanged(EventArgs.Empty);
		}

		public void Clear ()
		{
			Modified = false;
			Text = string.Empty;
		}

		public void ClearUndo ()
		{
			document.undo.Clear();
		}

		public void Copy ()
		{
			DataObject o;

			o = new DataObject(DataFormats.Text, SelectedText);
			if (this is RichTextBox)
				o.SetData(DataFormats.Rtf, ((RichTextBox)this).SelectedRtf);
			Clipboard.SetDataObject(o);
		}

		public void Cut ()
		{
			DataObject	o;

			o = new DataObject(DataFormats.Text, SelectedText);
			if (this is RichTextBox)
				o.SetData(DataFormats.Rtf, ((RichTextBox)this).SelectedRtf);
			Clipboard.SetDataObject (o);

			document.undo.BeginUserAction (Locale.GetText ("Cut"));
			document.ReplaceSelection (String.Empty, false);
			document.undo.EndUserAction ();

			Modified = true;
			OnTextChanged (EventArgs.Empty);
		}

		public void Paste ()
		{
			Paste (Clipboard.GetDataObject(), null, false);
		}

		public void ScrollToCaret ()
		{
			if (IsHandleCreated)
				CaretMoved (this, EventArgs.Empty);
		}

		public void Select(int start, int length)
		{
			SelectionStart = start;
			SelectionLength = length;
		}

		public void SelectAll ()
		{
			Line	last;

			last = document.GetLine(document.Lines);
			document.SetSelectionStart(document.GetLine(1), 0, false);
			document.SetSelectionEnd(last, last.text.Length, true);
			document.PositionCaret (document.selection_end.line, document.selection_end.pos);
			selection_length = -1;

			CaretMoved (this, null);

			document.DisplayCaret ();
		}

		internal void SelectAllNoScroll ()
		{
			Line last;

			last = document.GetLine(document.Lines);
			document.SetSelectionStart(document.GetLine(1), 0, false);
			document.SetSelectionEnd(last, last.text.Length, false);
			document.PositionCaret (document.selection_end.line, document.selection_end.pos);
			selection_length = -1;

			document.DisplayCaret ();
		}

		public override string ToString ()
		{
			return String.Concat (base.ToString (), ", Text: ", Text);
		}

		[MonoInternalNote ("Deleting is classed as Typing, instead of its own Undo event")]
		public void Undo ()
		{
			if (document.undo.Undo ()) {
				Modified = true;
				OnTextChanged (EventArgs.Empty);
			}
		}

#if NET_2_0
		public void DeselectAll ()
		{
			SelectionLength = 0;
		}

		public virtual char GetCharFromPosition (Point pt)
		{
			return GetCharFromPositionInternal (pt);
		}
		
		internal virtual char GetCharFromPositionInternal (Point p)
		{
			int index;
			LineTag tag = document.FindCursor (p.X, p.Y, out index);
			if (tag == null)
				return (char) 0; // Shouldn't happen

			if (index >= tag.Line.text.Length) {
				
				if (tag.Line.ending == LineEnding.Wrap) {
					// If we have wrapped text, we return the first char of the next line
					Line line = document.GetLine (tag.Line.line_no + 1);
					if (line != null)
						return line.text [0];

				}

				if (tag.Line.line_no == document.Lines) {
					// Last line returns the last char
					return tag.Line.text [tag.Line.text.Length - 1];
				}

				// This really shouldn't happen
				return (char) 0;
			}
			return tag.Line.text [index];
		}

		public virtual int GetCharIndexFromPosition (Point pt)
		{
			int line_index;
			LineTag tag = document.FindCursor (pt.X, pt.Y, out line_index);
			if (tag == null)
				return 0;

			if (line_index >= tag.Line.text.Length) {

				if (tag.Line.ending == LineEnding.Wrap) {
					// If we have wrapped text, we return the first char of the next line
					Line line = document.GetLine (tag.Line.line_no + 1);
					if (line != null)
						return document.LineTagToCharIndex (line, 0);
				}

				if (tag.Line.line_no == document.Lines) {
					// Last line returns the last char
					return document.LineTagToCharIndex (tag.Line, tag.Line.text.Length - 1);
				}

				return 0;
			}

			return document.LineTagToCharIndex (tag.Line, line_index);
		}

		public virtual Point GetPositionFromCharIndex (int index)
		{
			int pos;
			Line line;
			LineTag tag;

			document.CharIndexToLineTag (index, out line, out tag, out pos);

			return new Point ((int) (line.widths [pos] +
							  line.X + document.viewport_x),
					line.Y + document.viewport_y + tag.Shift);
		}

		public int GetFirstCharIndexFromLine (int lineNumber)
		{
			Line line = document.GetLine (lineNumber + 1);
			if (line == null)
				return -1;
					
			return document.LineTagToCharIndex (line, 0);
		}

		public int GetFirstCharIndexOfCurrentLine ()
		{
			return document.LineTagToCharIndex (document.caret.line, 0);
		}
#endif
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void CreateHandle ()
		{
			CalculateDocument ();
			base.CreateHandle ();
			document.AlignCaret();
			ScrollToCaret();
		}

		internal virtual void HandleLinkClicked (LinkRectangle link_clicked)
		{
		}

		protected override bool IsInputKey (Keys keyData)
		{
			if ((keyData & Keys.Alt) != 0)
				return base.IsInputKey(keyData);

			switch (keyData & Keys.KeyCode) {
				case Keys.Enter: {
					return (accepts_return && document.multiline);
				}

				case Keys.Tab: {
					if (accepts_tab && document.multiline)
						if ((keyData & Keys.Control) == 0)
							return true;
					return false;
				}

				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Down:
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.Home:
				case Keys.End: {
					return true;
				}
			}
			return false;
		}

		protected virtual void OnAcceptsTabChanged(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AcceptsTabChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

#if ONLY_1_1
		protected virtual void OnAutoSizeChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AutoSizeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
#endif

		protected virtual void OnBorderStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [BorderStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);

			if (auto_size && !document.multiline) {
				if (PreferredHeight != ClientSize.Height) {
					Height = PreferredHeight;
				}
			}
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
			FixupHeight ();
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected virtual void OnHideSelectionChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [HideSelectionChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnModifiedChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ModifiedChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnMultilineChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [MultilineChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

#if NET_2_0
		protected override void OnPaddingChanged (EventArgs e)
		{
			base.OnPaddingChanged (e);
		}
#endif

		protected virtual void OnReadOnlyChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ReadOnlyChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

#if NET_2_0
		protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
		{
			return base.ProcessCmdKey (ref msg, keyData);
		}
#endif
		protected override bool ProcessDialogKey (Keys keyData)
		{
			// The user can use Ctrl-Tab or Ctrl-Shift-Tab to move control focus
			// instead of inserting a Tab.  However, the focus-moving-tab-stuffs
			// doesn't work if Ctrl is pushed, so we remove it before sending it.
			if (accepts_tab && (keyData & (Keys.Control | Keys.Tab)) == (Keys.Control | Keys.Tab))
				keyData ^= Keys.Control;
				
			return base.ProcessDialogKey(keyData);
		}

		private bool ProcessKey (Keys keyData)
		{
			bool control;
			bool shift;

			control = (Control.ModifierKeys & Keys.Control) != 0;
			shift = (Control.ModifierKeys & Keys.Shift) != 0;

			if (shortcuts_enabled) {
				switch (keyData & Keys.KeyCode) {
				case Keys.X:
					if (control && read_only == false) {
						Cut();
						return true;
					}
					return false;

				case Keys.C:
					if (control) {
						Copy();
						return true;
					}
					return false;

				case Keys.V:
					if (control && read_only == false) {
						return Paste(Clipboard.GetDataObject(), null, true);
					}
					return false;

				case Keys.Z:
					if (control && read_only == false) {
						Undo ();
						return true;
					}
					return false;

				case Keys.A:
					if (control) {
						SelectAll();
						return true;
					}
					return false;

				case Keys.Insert:

					if (read_only == false) {
						if (shift) {
							Paste (Clipboard.GetDataObject (), null, true);
							return true;
						}

						if (control) {
							Copy ();
							return true;
						}
					}

					return false;

				case Keys.Delete:

					if (read_only)
						break;

					if (shift && read_only == false) {
						Cut ();
						return true;
					}

					if (document.selection_visible) {
						document.ReplaceSelection("", false);
					} else {
						// DeleteChar only deletes on the line, doesn't do the combine
						if (document.CaretPosition >= document.CaretLine.TextLengthWithoutEnding ()) {
							if (document.CaretLine.LineNo < document.Lines) {
								Line	line;

								line = document.GetLine(document.CaretLine.LineNo + 1);

								// this line needs to be invalidated before it is combined
								// because once it is combined, all it's coordinates will
								// have changed
								document.Invalidate (line, 0, line, line.text.Length);
								document.Combine(document.CaretLine, line);

								document.UpdateView(document.CaretLine,
										document.Lines, 0);

							}
						} else {
							if (!control) {
								document.DeleteChar(document.CaretTag.Line, document.CaretPosition, true);
							} else {
								int end_pos;

								end_pos = document.CaretPosition;

								while ((end_pos < document.CaretLine.Text.Length) && !Document.IsWordSeparator(document.CaretLine.Text[end_pos])) {
									end_pos++;
								}

								if (end_pos < document.CaretLine.Text.Length) {
									end_pos++;
								}
								document.DeleteChars(document.CaretTag.Line, document.CaretPosition, end_pos - document.CaretPosition);
							}
						}
					}

					document.AlignCaret();
					document.UpdateCaret();
					CaretMoved(this, null);

					Modified = true;
					OnTextChanged (EventArgs.Empty);
		
					return true;
				}
			}

			switch (keyData & Keys.KeyCode) {
				case Keys.Left: {
					if (control) {
						document.MoveCaret(CaretDirection.WordBack);
					} else {
						if (!document.selection_visible || shift) {
							document.MoveCaret(CaretDirection.CharBack);
						} else {
							document.MoveCaret(CaretDirection.SelectionStart);
						}
					}
					
					if (!shift) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Right: {
					if (control) {
						document.MoveCaret(CaretDirection.WordForward);
					} else {
						if (!document.selection_visible || shift) {
							document.MoveCaret(CaretDirection.CharForward);
						} else {
							document.MoveCaret(CaretDirection.SelectionEnd);
						}
					}
					if (!shift) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Up: {
					if (control) {
						if (document.CaretPosition == 0) {
							document.MoveCaret(CaretDirection.LineUp);
						} else {
							document.MoveCaret(CaretDirection.Home);
						}
					} else {
						document.MoveCaret(CaretDirection.LineUp);
					}

					if ((Control.ModifierKeys & Keys.Shift) == 0) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Down: {
					if (control) {
						if (document.CaretPosition == document.CaretLine.Text.Length) {
							document.MoveCaret(CaretDirection.LineDown);
						} else {
							document.MoveCaret(CaretDirection.End);
						}
					} else {
						document.MoveCaret(CaretDirection.LineDown);
					}

					if ((Control.ModifierKeys & Keys.Shift) == 0) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Home: {
					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.CtrlHome);
					} else {
						document.MoveCaret(CaretDirection.Home);
					}

					if ((Control.ModifierKeys & Keys.Shift) == 0) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.End: {
					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.CtrlEnd);
					} else {
						document.MoveCaret(CaretDirection.End);
					}

					if ((Control.ModifierKeys & Keys.Shift) == 0) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				//case Keys.Enter: {
				//        // ignoring accepts_return, fixes bug #76355
				//        if (!read_only && document.multiline && (accepts_return || (FindForm() != null && FindForm().AcceptButton == null) || ((Control.ModifierKeys & Keys.Control) != 0))) {
				//                Line	line;

				//                if (document.selection_visible) {
				//                        document.ReplaceSelection("\n", false);
				//                }

				//                line = document.CaretLine;

				//                document.Split (document.CaretLine, document.CaretTag, document.CaretPosition);
				//                line.ending = LineEnding.Rich;
				//                document.InsertString (line, line.text.Length,
				//                                                document.LineEndingToString (line.ending));
				//                OnTextChanged(EventArgs.Empty);

				//                document.UpdateView (line, document.Lines - line.line_no, 0);
				//                CaretMoved(this, null);
				//                return true;
				//        }
				//        break;
				//}

				case Keys.Tab: {
					if (!read_only && accepts_tab && document.multiline) {
						document.InsertCharAtCaret ('\t', true);

						CaretMoved(this, null);
						Modified = true;
						OnTextChanged (EventArgs.Empty);

						return true;
					}
					break;
				}

				case Keys.PageUp: {
					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.CtrlPgUp);
					} else {
						document.MoveCaret(CaretDirection.PgUp);
					}
					document.DisplayCaret ();
					return true;
				}

				case Keys.PageDown: {
					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.CtrlPgDn);
					} else {
						document.MoveCaret(CaretDirection.PgDn);
					}
					document.DisplayCaret ();
					return true;
				}
			}

			return false;
		}

		internal virtual void RaiseSelectionChanged ()
		{
			// Do nothing, overridden in RTB
		}
		
		private void HandleBackspace (bool control)
		{
			bool	fire_changed;

			fire_changed = false;

			// delete only deletes on the line, doesn't do the combine
			if (document.selection_visible) {
				document.undo.BeginUserAction (Locale.GetText ("Delete"));
				document.ReplaceSelection("", false);
				document.undo.EndUserAction ();
				fire_changed = true;
				document.SetSelectionToCaret (true);
			} else {
			document.SetSelectionToCaret (true);

			if (document.CaretPosition == 0) {
				if (document.CaretLine.LineNo > 1) {
					Line	line;
					int	new_caret_pos;

					line = document.GetLine(document.CaretLine.LineNo - 1);
					new_caret_pos = line.TextLengthWithoutEnding ();

					// Invalidate the old line position before we do the combine
					document.Invalidate (line, 0, line, line.text.Length);
					document.Combine(line, document.CaretLine);

					document.UpdateView(line, document.Lines - line.LineNo, 0);
					document.PositionCaret(line, new_caret_pos);
					document.SetSelectionToCaret (true);
					document.UpdateCaret();
					fire_changed = true;
				}
			} else {
				if (!control || document.CaretPosition == 0) {

					// Move before we delete because the delete will change positions around
					// if we cross a wrap border
					LineTag tag = document.CaretTag;
					int pos = document.CaretPosition;
					document.MoveCaret (CaretDirection.CharBack);
					document.DeleteChar (tag.Line, pos, false);
					document.SetSelectionToCaret (true);
				} else {
					int start_pos;

					
					start_pos = document.CaretPosition - 1;
					while ((start_pos > 0) && !Document.IsWordSeparator(document.CaretLine.Text[start_pos - 1])) {
						start_pos--;
					}

					document.undo.BeginUserAction (Locale.GetText ("Delete"));
					document.DeleteChars(document.CaretTag.Line, start_pos, document.CaretPosition - start_pos);
					document.undo.EndUserAction ();
					document.PositionCaret(document.CaretLine, start_pos);
					document.SetSelectionToCaret (true);
				}
				document.UpdateCaret();
				fire_changed = true;
				}
			}

			CaretMoved (this, null);

			if (fire_changed) {
				Modified = true;
				OnTextChanged(EventArgs.Empty);
			}
		}

		private void HandleEnter ()
		{
			// ignoring accepts_return, fixes bug #76355
			if (!read_only && document.multiline && (accepts_return || (FindForm() != null && FindForm().AcceptButton == null) || ((Control.ModifierKeys & Keys.Control) != 0))) {
				Line	line;

				if (document.selection_visible)
					document.ReplaceSelection ("", false);

				line = document.CaretLine;

				document.Split (document.CaretLine, document.CaretTag, document.CaretPosition);
				line.ending = document.StringToLineEnding (Environment.NewLine);
				document.InsertString (line, line.text.Length, document.LineEndingToString (line.ending));
				
				document.UpdateView (line, document.Lines - line.line_no, 0);
				CaretMoved (this, null);
				Modified = true;
				OnTextChanged (EventArgs.Empty);
			}
		}
		
		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			// Make sure we don't get sized bigger than we want to be

			if (!richtext) {
				if (!document.multiline) {
					if (height != PreferredHeight) {
						// If the specified has Height, we need to store that in the
						// ExplicitBounds because we are going to override it
						if ((specified & BoundsSpecified.Height) != 0) {
							Rectangle r = ExplicitBounds;
							r.Height = height;
							ExplicitBounds = r;
							specified &= ~BoundsSpecified.Height;
						}
						
						height = PreferredHeight;
					}
				}
			}

			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void WndProc (ref Message m)
		{
			switch ((Msg)m.Msg) {
			case Msg.WM_KEYDOWN: {
				if (ProcessKeyMessage(ref m) || ProcessKey((Keys)m.WParam.ToInt32() | XplatUI.State.ModifierKeys)) {
					m.Result = IntPtr.Zero;
					return;
				}
				DefWndProc (ref m);
				return;
			}

			case Msg.WM_CHAR: {
				int	ch;

				if (ProcessKeyMessage(ref m)) {
					m.Result = IntPtr.Zero;
					return;
				}

				if (read_only) {
					return;
				}

				m.Result = IntPtr.Zero;

				ch = m.WParam.ToInt32();

				if (ch == 127) {
					HandleBackspace(true);
				} else if (ch >= 32) {
					if (document.selection_visible) {
						document.ReplaceSelection("", false);
					}

					char c = (char)m.WParam;
					switch (character_casing) {
					case CharacterCasing.Upper:
						c = Char.ToUpper((char) m.WParam);
						break;
					case CharacterCasing.Lower:
						c = Char.ToLower((char) m.WParam);
						break;
					}

					if (document.Length < max_length) {
						document.InsertCharAtCaret(c, true);
#if NET_2_0
						OnTextUpdate ();
#endif
						CaretMoved (this, null);
						Modified = true;
						OnTextChanged(EventArgs.Empty);

					} else {
						XplatUI.AudibleAlert(AlertType.Default);
					}
					return;
				} else if (ch == 8) {
					HandleBackspace(false);
				} else if (ch == 13)
					HandleEnter ();

				return;
			}

			case Msg.WM_SETFOCUS:
				base.WndProc(ref m);
				document.CaretHasFocus ();
				break;

			case Msg.WM_KILLFOCUS:
				base.WndProc(ref m);
				document.CaretLostFocus ();
				break;

			case Msg.WM_NCPAINT:
				if (!ThemeEngine.Current.TextBoxBaseHandleWmNcPaint (this, ref m))
					base.WndProc(ref m);
				break;

			default:
				base.WndProc(ref m);
				return;
			}
		}

		#endregion	// Protected Instance Methods

		#region Events
		static object AcceptsTabChangedEvent = new object ();
		static object AutoSizeChangedEvent = new object ();
		static object BorderStyleChangedEvent = new object ();
		static object HideSelectionChangedEvent = new object ();
		static object ModifiedChangedEvent = new object ();
		static object MultilineChangedEvent = new object ();
		static object ReadOnlyChangedEvent = new object ();
		static object HScrolledEvent = new object ();
		static object VScrolledEvent = new object ();

		public event EventHandler AcceptsTabChanged {
			add { Events.AddHandler (AcceptsTabChangedEvent, value); }
			remove { Events.RemoveHandler (AcceptsTabChangedEvent, value); }
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
#endif
		public
#if NET_2_0
		new
#endif
		event EventHandler AutoSizeChanged {
			add { Events.AddHandler (AutoSizeChangedEvent, value); }
			remove { Events.RemoveHandler (AutoSizeChangedEvent, value); }
		}

		public event EventHandler BorderStyleChanged {
			add { Events.AddHandler (BorderStyleChangedEvent, value); }
			remove { Events.RemoveHandler (BorderStyleChangedEvent, value); }
		}

		public event EventHandler HideSelectionChanged {
			add { Events.AddHandler (HideSelectionChangedEvent, value); }
			remove { Events.RemoveHandler (HideSelectionChangedEvent, value); }
		}

		public event EventHandler ModifiedChanged {
			add { Events.AddHandler (ModifiedChangedEvent, value); }
			remove { Events.RemoveHandler (ModifiedChangedEvent, value); }
		}

		public event EventHandler MultilineChanged {
			add { Events.AddHandler (MultilineChangedEvent, value); }
			remove { Events.RemoveHandler (MultilineChangedEvent, value); }
		}

		public event EventHandler ReadOnlyChanged {
			add { Events.AddHandler (ReadOnlyChangedEvent, value); }
			remove { Events.RemoveHandler (ReadOnlyChangedEvent, value); }
		}

		internal event EventHandler HScrolled {
			add { Events.AddHandler (HScrolledEvent, value); }
			remove { Events.RemoveHandler (HScrolledEvent, value); }
		}

		internal event EventHandler VScrolled {
			add { Events.AddHandler (VScrolledEvent, value); }
			remove { Events.RemoveHandler (VScrolledEvent, value); }
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
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event MouseEventHandler MouseClick {
			add { base.MouseClick += value; }
			remove { base.MouseClick -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler PaddingChanged {
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
#else
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
#endif
		public new event EventHandler Click {
			add { base.Click += value; }
			remove { base.Click -= value; }
		}

		// XXX should this not manipulate base.Paint?
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint;
		#endregion	// Events

		#region Private Methods
		internal Document Document {
			get {
				return document;
			}

			set {
				document = value;
			}
		}

		internal bool EnableLinks {
			get { return enable_links; }
			set {
				enable_links = value;

				document.EnableLinks = value;
			}
		}

		internal override bool ScaleChildrenInternal {
			get { return false; }
		}

		internal bool ShowSelection {
			get {
				if (show_selection || !hide_selection) {
					return true;
				}

				return has_focus;
			}

			set {
				if (show_selection == value)
					return;

				show_selection = value;
				// Currently InvalidateSelectionArea is commented out so do a full invalidate
				document.InvalidateSelectionArea();
			}
		}

		internal int TopMargin {
			get {
				return document.top_margin;
			}
			set {
				document.top_margin = value;
			}
		}

		#region UIA Framework Properties

		internal ScrollBar UIAHScrollBar {
			get { return hscroll; }
		}

		internal ScrollBar UIAVScrollBar {
			get { return vscroll; }
		}

		#endregion UIA Framework Properties

		internal Graphics CreateGraphicsInternal ()
		{
			if (IsHandleCreated)
				return base.CreateGraphics();
				
			return DeviceContext;
		}

		internal override void OnPaintInternal (PaintEventArgs pevent)
		{
			Draw (pevent.Graphics, pevent.ClipRectangle);

			//
			// OnPaint does not get raised on MS (see bug #80639)
			// 
			pevent.Handled = true;
		}

		internal void Draw (Graphics g, Rectangle clippingArea)
		{
			ThemeEngine.Current.TextBoxBaseFillBackground (this, g, clippingArea);
			
			// Draw the viewable document
			document.Draw(g, clippingArea);
		}

		private void FixupHeight ()
		{
			if (!richtext) {
				if (!document.multiline) {
					if (PreferredHeight != ClientSize.Height) {
						ClientSize = new Size (ClientSize.Width, PreferredHeight);
					}
				}
			}
		}

		private bool IsDoubleClick (MouseEventArgs e)
		{
			TimeSpan interval = DateTime.Now - click_last;
			if (interval.TotalMilliseconds > SystemInformation.DoubleClickTime)
				return false;
			Size dcs = SystemInformation.DoubleClickSize;
			if (e.X < click_point_x - dcs.Width / 2 || e.X > click_point_x + dcs.Width / 2)
				return false;
			if (e.Y < click_point_y - dcs.Height / 2 || e.Y > click_point_y + dcs.Height / 2)
				return false;
			return true;
		}

		private void TextBoxBase_MouseDown (object sender, MouseEventArgs e)
		{
			bool dbliclick = false;

			if (e.Button == MouseButtons.Left) {

				// Special case when shift key is pressed and
				// left mouse is clicked.. set selection from
				// current cursor to mouse
				if ((Control.ModifierKeys & Keys.Shift) > 0) {
					document.PositionCaret (e.X + document.ViewPortX, e.Y + document.ViewPortY);
					document.SetSelectionToCaret (false);
					document.DisplayCaret ();
					return;
				}

				dbliclick = IsDoubleClick (e);

				if (current_link != null) {
					HandleLinkClicked (current_link);
					return;
				}

				//ensure nothing is selected anymore BEFORE we
				//position the caret, so the caret is recreated
				//(caret is only visible when nothing is selected)
				if (document.selection_visible && dbliclick == false) {
					document.SetSelectionToCaret (true);
					click_mode = CaretSelection.Position;
				}

				document.PositionCaret(e.X + document.ViewPortX, e.Y + document.ViewPortY);

				if (dbliclick) {
					switch (click_mode) {
					case CaretSelection.Position:
						SelectWord ();
						click_mode = CaretSelection.Word;
						break;
					case CaretSelection.Word:

						if (this is TextBox) {
							document.SetSelectionToCaret (true);
							click_mode = CaretSelection.Position;
							break;
						}

						document.ExpandSelection (CaretSelection.Line, false);
						click_mode = CaretSelection.Line;
						break;
					case CaretSelection.Line:

						// Gotta do this first because Exanding to a word
						// from a line doesn't really work
						document.SetSelectionToCaret (true);

						SelectWord ();
						click_mode = CaretSelection.Word;
						break;
					}
				} else {
					document.SetSelectionToCaret (true);
					click_mode = CaretSelection.Position;
				}

				click_point_x = e.X;
				click_point_y = e.Y;
				click_last = DateTime.Now;
			}

			if ((e.Button == MouseButtons.Middle) && XplatUI.RunningOnUnix) {
				Document.Marker	marker;

				marker.tag = document.FindCursor(e.X + document.ViewPortX, e.Y + document.ViewPortY, out marker.pos);
				marker.line = marker.tag.Line;
				marker.height = marker.tag.Height;

				document.SetSelection(marker.line, marker.pos, marker.line, marker.pos);
				Paste (Clipboard.GetDataObject (true), null, true);
			}
		}

		private void TextBoxBase_MouseUp (object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) {
				if (click_mode == CaretSelection.Position) {
					document.SetSelectionToCaret(false);
					document.DisplayCaret();
					
					// Only raise if there is text.
					if (Text.Length > 0)
						RaiseSelectionChanged ();
				}

				if (scroll_timer != null) {
					scroll_timer.Enabled = false;
				}
				return;
			}
		}

		private void SizeControls ()
		{
			if (hscroll.Visible) {
				//vscroll.Maximum += hscroll.Height;
				canvas_height = ClientSize.Height - hscroll.Height;
			} else {
				canvas_height = ClientSize.Height;
			}

			if (vscroll.Visible) {
				//hscroll.Maximum += vscroll.Width;
				canvas_width = ClientSize.Width - vscroll.Width;

				if (GetInheritedRtoL () == RightToLeft.Yes) {
					document.OffsetX = vscroll.Width;
				} else {
					document.OffsetX = 0;
				}

			} else {
				canvas_width = ClientSize.Width;
				document.OffsetX = 0;
			}

			document.ViewPortWidth = canvas_width;
			document.ViewPortHeight = canvas_height;
		}

		private void PositionControls ()
		{
			if (canvas_height < 1 || canvas_width < 1)
				return;

			int hmod = vscroll.Visible ? vscroll.Width : 0;
			int vmod = hscroll.Visible ? hscroll.Height : 0;

			if (GetInheritedRtoL () == RightToLeft.Yes) {
				hscroll.Bounds = new Rectangle (ClientRectangle.Left + hmod, 
					Math.Max(0, ClientRectangle.Height - hscroll.Height), 
					ClientSize.Width, 
					hscroll.Height);

				vscroll.Bounds = new Rectangle (ClientRectangle.Left, 
					ClientRectangle.Top, 
					vscroll.Width, 
					Math.Max(0, ClientSize.Height - (vmod)));
			} else {
				hscroll.Bounds = new Rectangle (ClientRectangle.Left, 
					Math.Max(0, ClientRectangle.Height - hscroll.Height), 
					Math.Max(0, ClientSize.Width - hmod), 
					hscroll.Height);

				vscroll.Bounds = new Rectangle (
					Math.Max(0, ClientRectangle.Right - vscroll.Width), 
					ClientRectangle.Top, 
					vscroll.Width, 
					Math.Max(0, ClientSize.Height - vmod));
			}
		}

		internal RightToLeft GetInheritedRtoL ()
		{
			for (Control c = this; c != null; c = c.Parent)
				if (c.RightToLeft != RightToLeft.Inherit)
					return c.RightToLeft;
			return RightToLeft.No;
		}

		private void TextBoxBase_SizeChanged (object sender, EventArgs e)
		{
			if (IsHandleCreated)
				CalculateDocument ();
		}

		private void TextBoxBase_RightToLeftChanged (object o, EventArgs e)
		{
			if (IsHandleCreated)
				CalculateDocument ();
		}

		private void TextBoxBase_MouseWheel (object sender, MouseEventArgs e)
		{
			if (!vscroll.Enabled)
				return;

			if (e.Delta < 0)
				vscroll.Value = Math.Min (vscroll.Value + SystemInformation.MouseWheelScrollLines * 5,
						Math.Max (0, vscroll.Maximum - document.ViewPortHeight + 1));
			else
				vscroll.Value = Math.Max (0, vscroll.Value - SystemInformation.MouseWheelScrollLines * 5);
		}

		internal virtual void SelectWord ()
		{
			StringBuilder s = document.caret.line.text;
			int start = document.caret.pos;
			int end = document.caret.pos;

			if (s.Length < 1) {
				if (document.caret.line.line_no >= document.Lines)
					return;
				Line line = document.GetLine (document.caret.line.line_no + 1);
				document.PositionCaret (line, 0);
				return;
			}

			if (start > 0) {
				start--;
				end--;
			}

			// skip whitespace until we hit a word
			while (start > 0 && s [start] == ' ')
				start--;
			if (start > 0) {
				while (start > 0 && (s [start] != ' '))
					start--;
				if (s [start] == ' ')
					start++;
			}

			if (s [end] == ' ') {
				while (end < s.Length && s [end] == ' ')
					end++;
			} else {
				while (end < s.Length && s [end] != ' ')
					end++;
				while (end < s.Length && s [end] == ' ')
					end++;
			}

			document.SetSelection (document.caret.line, start, document.caret.line, end);
			document.PositionCaret (document.selection_end.line, document.selection_end.pos);
			document.DisplayCaret();
		}

		internal void CalculateDocument()
		{
			CalculateScrollBars ();
			document.RecalculateDocument (CreateGraphicsInternal ());


			if (document.caret.line != null && document.caret.line.Y < document.ViewPortHeight) {
				// The window has probably been resized, making the entire thing visible, so
				// we need to set the scroll position back to zero.
				vscroll.Value = 0;
			}

			Invalidate();
		}

		internal void CalculateScrollBars ()
		{
			// FIXME - need separate calculations for center and right alignment
			SizeControls ();

			if (document.Width >= document.ViewPortWidth) {
				hscroll.SetValues (0, Math.Max (1, document.Width), -1,
						document.ViewPortWidth < 0 ? 0 : document.ViewPortWidth);
				if (document.multiline)
					hscroll.Enabled = true;
			} else {
				hscroll.Enabled = false;
				hscroll.Maximum = document.ViewPortWidth;
			}

			if (document.Height >= document.ViewPortHeight) {
				vscroll.SetValues (0, Math.Max (1, document.Height), -1,
						document.ViewPortHeight < 0 ? 0 : document.ViewPortHeight);
				if (document.multiline)
					vscroll.Enabled = true;
			} else {
				vscroll.Enabled = false;
				vscroll.Maximum = document.ViewPortHeight;
			}

			if (!WordWrap) {
				switch (scrollbars) {
				case RichTextBoxScrollBars.Both:
				case RichTextBoxScrollBars.Horizontal:
					if (richtext)
						hscroll.Visible = hscroll.Enabled;
					else
						hscroll.Visible = this.Multiline;
					break;
				case RichTextBoxScrollBars.ForcedBoth:
				case RichTextBoxScrollBars.ForcedHorizontal:
					hscroll.Visible = true;
					break;
				default:
					hscroll.Visible = false;
					break;
				}
			} else {
				hscroll.Visible = false;
			}

			switch (scrollbars) {
			case RichTextBoxScrollBars.Both:
			case RichTextBoxScrollBars.Vertical:
				if (richtext)
					vscroll.Visible = vscroll.Enabled;
				else 
					vscroll.Visible = this.Multiline;
				break;
			case RichTextBoxScrollBars.ForcedBoth:
			case RichTextBoxScrollBars.ForcedVertical:
				vscroll.Visible = true;
				break;
			default:
				vscroll.Visible = false;
				break;
			}

			PositionControls ();

			SizeControls (); //Update sizings now we've decided whats visible
		}

		private void document_WidthChanged (object sender, EventArgs e)
		{
			CalculateScrollBars();
		}

		private void document_HeightChanged (object sender, EventArgs e)
		{
			CalculateScrollBars();
		}

		private void ScrollLinks (int xChange, int yChange)
		{
			foreach (LinkRectangle link in list_links)
				link.Scroll (xChange, yChange);
		}

		private void hscroll_ValueChanged (object sender, EventArgs e)
		{
			int old_viewport_x;

			old_viewport_x = document.ViewPortX;
			document.ViewPortX = this.hscroll.Value;

			//
			// Before scrolling we want to destroy the caret, then draw a new one after the scroll
			// the reason for this is that scrolling changes the coordinates of the caret, and we
			// will get tracers if we don't 
			//
			if (Focused)
				document.CaretLostFocus ();

			if (vscroll.Visible) {
				if (GetInheritedRtoL () == RightToLeft.Yes) {
					XplatUI.ScrollWindow (this.Handle, new Rectangle (vscroll.Width, 0, ClientSize.Width - vscroll.Width, ClientSize.Height), old_viewport_x - this.hscroll.Value, 0, false);
				} else {
					XplatUI.ScrollWindow (this.Handle, new Rectangle (0, 0, ClientSize.Width - vscroll.Width, ClientSize.Height), old_viewport_x - this.hscroll.Value, 0, false);
				}
			} else {
				XplatUI.ScrollWindow(this.Handle, ClientRectangle, old_viewport_x - this.hscroll.Value, 0, false);
			}

			ScrollLinks (old_viewport_x - this.hscroll.Value, 0);

			if (Focused)
				document.CaretHasFocus ();

			EventHandler eh = (EventHandler)(Events [HScrolledEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		private void vscroll_ValueChanged (object sender, EventArgs e)
		{
			int old_viewport_y;

			old_viewport_y = document.ViewPortY;
			document.ViewPortY = this.vscroll.Value;

			//
			// Before scrolling we want to destroy the caret, then draw a new one after the scroll
			// the reason for this is that scrolling changes the coordinates of the caret, and we
			// will get tracers if we don't 
			//
			if (Focused)
				document.CaretLostFocus ();

			if (hscroll.Visible) {
				XplatUI.ScrollWindow(this.Handle, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height - hscroll.Height), 0, old_viewport_y - this.vscroll.Value, false);
			} else {
				XplatUI.ScrollWindow(this.Handle, ClientRectangle, 0, old_viewport_y - this.vscroll.Value, false);
			}

			ScrollLinks (0, old_viewport_y - this.vscroll.Value);

			if (Focused)
				document.CaretHasFocus ();

			EventHandler eh = (EventHandler)(Events [VScrolledEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		private void TextBoxBase_MouseMove (object sender, MouseEventArgs e)
		{
			// FIXME - handle auto-scrolling if mouse is to the right/left of the window
			if (e.Button == MouseButtons.Left && Capture) {
				if (!ClientRectangle.Contains (e.X, e.Y)) {
					if (scroll_timer == null) {
						scroll_timer = new Timer ();
						scroll_timer.Interval = 100;
						scroll_timer.Tick += new EventHandler (ScrollTimerTickHandler);
					}

					if (!scroll_timer.Enabled) {
						scroll_timer.Start ();

						// Force the first tick
						ScrollTimerTickHandler (null, EventArgs.Empty);
					}
				}

				document.PositionCaret(e.X + document.ViewPortX, e.Y + document.ViewPortY);
				if (click_mode == CaretSelection.Position) {
					document.SetSelectionToCaret(false);
					document.DisplayCaret();
				}
			}

			//search through link boxes to see if the mouse is over one of them

			bool found_link = false;
			foreach (LinkRectangle link in list_links) {
				if (link.LinkAreaRectangle.Contains (e.X, e.Y)) {
					XplatUI.SetCursor (window.Handle, Cursors.Hand.handle);

					found_link = true;
					current_link = link;
					break;
				}
			}

			if (found_link == false) {
#if NET_2_0
				XplatUI.SetCursor (window.Handle, DefaultCursor.handle);
#else
				XplatUI.SetCursor(window.Handle, Cursors.IBeam.handle);
#endif
				current_link = null;
			}
		}

		private void TextBoxBase_FontOrColorChanged (object sender, EventArgs e)
		{
			Line	line;

			document.SuspendRecalc ();
			// Font changes apply to the whole document
			for (int i = 1; i <= document.Lines; i++) {
				line = document.GetLine(i);
				if (LineTag.FormatText(line, 1, line.text.Length, Font, ForeColor,
						Color.Empty, FormatSpecified.Font | FormatSpecified.Color))
					document.RecalculateDocument (CreateGraphicsInternal (), line.LineNo, line.LineNo, false);
			}
			document.ResumeRecalc (false);

			// Make sure the caret height is matching the new font height
			document.AlignCaret();
		}

		private void ScrollTimerTickHandler (object sender, EventArgs e)
		{
			Point pt = Cursor.Position;

			pt = PointToClient (pt);

			if (pt.X < ClientRectangle.Left) {
				document.MoveCaret(CaretDirection.CharBackNoWrap);
				document.SetSelectionToCaret(false);
				
				CaretMoved(this, null);
			} else if (pt.X > ClientRectangle.Right) {
				document.MoveCaret(CaretDirection.CharForwardNoWrap);
				document.SetSelectionToCaret(false);
				
				CaretMoved(this, null);
			} else if (pt.Y > ClientRectangle.Bottom) {
				document.MoveCaret(CaretDirection.LineDown);
				document.SetSelectionToCaret(false);
				
				CaretMoved(this, null);
			} else if (pt.Y < ClientRectangle.Top) {
				document.MoveCaret(CaretDirection.LineUp);
				document.SetSelectionToCaret(false);
				
				CaretMoved(this, null);
			}
		}

		/// <summary>Ensure the caret is always visible</summary>
		internal void CaretMoved (object sender, EventArgs e)
		{
			Point	pos;
			int	height;

			if (!IsHandleCreated || canvas_width < 1 || canvas_height < 1)
				return;

  			document.MoveCaretToTextTag ();
			pos = document.Caret;

			//Console.WriteLine("Caret now at {0} (Thumb: {1}x{2}, Canvas: {3}x{4}, Document {5}x{6})", pos, hscroll.Value, vscroll.Value, canvas_width, canvas_height, document.Width, document.Height);


			// Horizontal scrolling:
			// If the caret moves to the left outside the visible area, we jump the document into view, not just one
			// character, but 1/3 of the width of the document
			// If the caret moves to the right outside the visible area, we scroll just enough to keep the caret visible

			// Handle horizontal scrolling
			if (document.CaretLine.alignment == HorizontalAlignment.Left) {
				// Check if we moved out of view to the left
				if (pos.X < (document.ViewPortX)) {
					do {
						if ((hscroll.Value - document.ViewPortWidth / 3) >= hscroll.Minimum) {
							hscroll.SafeValueSet (hscroll.Value - document.ViewPortWidth / 3);
						} else {
							hscroll.Value = hscroll.Minimum;
						}
					} while (hscroll.Value > pos.X);
				}

				// Check if we moved out of view to the right
				if ((pos.X >= (document.ViewPortWidth + document.ViewPortX)) && (hscroll.Value != hscroll.Maximum)) {
					if ((pos.X - document.ViewPortWidth + 1) <= hscroll.Maximum) {
						if (pos.X - document.ViewPortWidth >= 0) {
							hscroll.SafeValueSet (pos.X - document.ViewPortWidth + 1);
						} else {
							hscroll.Value = 0;
						}
					} else {
						hscroll.Value = hscroll.Maximum;
					}
				}
			} else if (document.CaretLine.alignment == HorizontalAlignment.Right) {
//				hscroll.Value = pos.X;

//				if ((pos.X > (this.canvas_width + document.ViewPortX)) && (hscroll.Enabled && (hscroll.Value != hscroll.Maximum))) {
//					hscroll.Value = hscroll.Maximum;
//				}
			} else {
				// FIXME - implement center cursor alignment
			}

			if (Text.Length > 0)
				RaiseSelectionChanged ();
			
			if (!document.multiline)
				return;

			// Handle vertical scrolling
			height = document.CaretLine.Height + 1;

			if (pos.Y < document.ViewPortY)
				vscroll.SafeValueSet (pos.Y);
			if ((pos.Y + height) > (document.ViewPortY + canvas_height))
				vscroll.Value = Math.Min (vscroll.Maximum, pos.Y - canvas_height + height);
		}

		internal bool Paste (IDataObject clip, DataFormats.Format format, bool obey_length)
		{
			string		s;

			if (clip == null)
				return false;

			if (format == null) {
				if ((this is RichTextBox) && clip.GetDataPresent(DataFormats.Rtf)) {
					format = DataFormats.GetFormat(DataFormats.Rtf);
				} else if ((this is RichTextBox) && clip.GetDataPresent (DataFormats.Bitmap)) {
					format = DataFormats.GetFormat (DataFormats.Bitmap);
				} else if (clip.GetDataPresent(DataFormats.UnicodeText)) {
					format = DataFormats.GetFormat(DataFormats.UnicodeText);
				} else if (clip.GetDataPresent(DataFormats.Text)) {
					format = DataFormats.GetFormat(DataFormats.Text);
				} else {
					return false;
				}
			} else {
				if ((format.Name == DataFormats.Rtf) && !(this is RichTextBox)) {
					return false;
				}

				if (!clip.GetDataPresent(format.Name)) {
					return false;
				}
			}

			if (format.Name == DataFormats.Rtf) {
				document.undo.BeginUserAction (Locale.GetText ("Paste"));
				((RichTextBox)this).SelectedRtf = (string)clip.GetData(DataFormats.Rtf);
				document.undo.EndUserAction ();
				Modified = true;
				return true;
			} else if (format.Name == DataFormats.Bitmap) {
				document.undo.BeginUserAction (Locale.GetText ("Paste"));
				//	document.InsertImage (document.caret.line, document.caret.pos, (Image) clip.GetData (DataFormats.Bitmap));
				document.MoveCaret (CaretDirection.CharForward);
				document.undo.EndUserAction ();
				return true;
			} else if (format.Name == DataFormats.UnicodeText) {
				s = (string)clip.GetData(DataFormats.UnicodeText);
			} else if (format.Name == DataFormats.Text) {
				s = (string)clip.GetData(DataFormats.Text);
			} else {
				return false;
			}

			if (!obey_length) {
				document.undo.BeginUserAction (Locale.GetText ("Paste"));
				this.SelectedText = s;
				document.undo.EndUserAction ();
			} else {
				if ((s.Length + (document.Length - SelectedText.Length)) < max_length) {
					document.undo.BeginUserAction (Locale.GetText ("Paste"));
					this.SelectedText = s;
					document.undo.EndUserAction ();
				} else if ((document.Length - SelectedText.Length) < max_length) {
					document.undo.BeginUserAction (Locale.GetText ("Paste"));
					this.SelectedText = s.Substring (0, max_length - (document.Length - SelectedText.Length));
					document.undo.EndUserAction ();
				}
			}

			Modified = true;
			return true;
		}

		internal virtual Color ChangeBackColor (Color backColor)
		{
			return backColor;
		}

		internal override bool IsInputCharInternal (char charCode)
		{
			return true;
		}
		#endregion	// Private Methods

		#region Private Classes
		internal class LinkRectangle {
			private Rectangle link_area_rectangle;
			private LineTag link_tag;

			public LinkRectangle (Rectangle rect)
			{
				link_tag = null;
				link_area_rectangle = rect;
			}

			public Rectangle LinkAreaRectangle {
				get { return link_area_rectangle; }
				set { link_area_rectangle = value; }
			}

			public LineTag LinkTag {
				get { return link_tag; }
				set { link_tag = value; }
			}

			public void Scroll (int x_change, int y_change)
			{
				link_area_rectangle.X += x_change;
				link_area_rectangle.Y += y_change;
			}
		}
		#endregion

#if NET_2_0
		// This is called just before OnTextChanged is called.
		internal virtual void OnTextUpdate ()
		{
		}
		
		protected override void OnTextChanged (EventArgs e)
		{
			base.OnTextChanged (e);
		}

		public virtual int GetLineFromCharIndex (int index)
		{
			Line line_out;
			LineTag tag_out;
			int pos;
			
			document.CharIndexToLineTag (index, out line_out, out tag_out, out pos);

			return line_out.LineNo;
		}

		protected override void OnMouseUp (MouseEventArgs mevent)
		{
			base.OnMouseUp (mevent);
		}
#endif
	}
}
