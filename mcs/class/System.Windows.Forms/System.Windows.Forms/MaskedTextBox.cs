//
// MaskedTextBox.cs
//
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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Rolf Bjarne Kvinge (RKvinge@novell.com)
//


using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace System.Windows.Forms
{
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[DefaultProperty ("Mask")]
	[DefaultEvent ("MaskInputRejected")]
	[Designer ("System.Windows.Forms.Design.MaskedTextBoxDesigner, " + Consts.AssemblySystem_Design)]
	[DefaultBindingProperty ("Text")]
	public class MaskedTextBox : TextBoxBase
	{
#region Locals
		private MaskedTextProvider provider;
		private bool beep_on_error;
		private IFormatProvider format_provider;
		private bool hide_prompt_on_leave;
		private InsertKeyMode insert_key_mode;
		private bool insert_key_overwriting;
		private bool reject_input_on_first_failure;
		private HorizontalAlignment text_align;
		private MaskFormat cut_copy_mask_format;
		private bool use_system_password_char;
		private Type validating_type;
		private bool is_empty_mask;
		private bool setting_text;
#endregion

#region Events
		static object AcceptsTabChangedEvent = new object ();
		static object IsOverwriteModeChangedEvent = new object ();
		static object MaskChangedEvent = new object ();
		static object MaskInputRejectedEvent = new object ();
		static object MultilineChangedEvent = new object ();
		static object TextAlignChangedEvent = new object ();
		static object TypeValidationCompletedEvent = new object ();
		
		// This event is never raised by MaskedTextBox.
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler AcceptsTabChanged {
			add { Events.AddHandler (AcceptsTabChangedEvent, value);}
			remove { Events.RemoveHandler (AcceptsTabChangedEvent, value);}
		}

		public event EventHandler IsOverwriteModeChanged {
			add { Events.AddHandler (IsOverwriteModeChangedEvent, value); }
			remove { Events.RemoveHandler (IsOverwriteModeChangedEvent, value); }
		}
		
		public event EventHandler MaskChanged {
			add { Events.AddHandler (MaskChangedEvent, value); }
			remove { Events.RemoveHandler (MaskChangedEvent, value); }
		}
		
		public event MaskInputRejectedEventHandler MaskInputRejected {
			add { Events.AddHandler (MaskInputRejectedEvent, value); }
			remove { Events.RemoveHandler (MaskInputRejectedEvent, value); }
		}
		
		// This event is never raised by MaskedTextBox.
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MultilineChanged {
			add { Events.AddHandler (MultilineChangedEvent, value); }
			remove { Events.RemoveHandler (MultilineChangedEvent, value); }
		}
		
		public event EventHandler TextAlignChanged {
			add { Events.AddHandler (TextAlignChangedEvent, value); }
			remove { Events.RemoveHandler (TextAlignChangedEvent, value); }
		}
		
		public event TypeValidationEventHandler TypeValidationCompleted {
			add { Events.AddHandler (TypeValidationCompletedEvent, value); }
			remove { Events.RemoveHandler (TypeValidationCompletedEvent, value); }
		}
#endregion

#region Constructors
		public MaskedTextBox ()
		{
			provider = new MaskedTextProvider ("<>", CultureInfo.CurrentCulture);
			is_empty_mask = true;
			Init ();
		}

		public MaskedTextBox (MaskedTextProvider maskedTextProvider)
		{
			if (maskedTextProvider == null) {
				throw new ArgumentNullException ();
			}
			provider = maskedTextProvider;
			is_empty_mask = false;
			Init ();
		}

		public MaskedTextBox (string mask)
		{
			if (mask == null) {
				throw new ArgumentNullException ();
			}
			provider = new MaskedTextProvider (mask, CultureInfo.CurrentCulture);
			is_empty_mask = false;
			Init ();
		}

		private void Init ()
		{
			BackColor = SystemColors.Window;
			cut_copy_mask_format = MaskFormat.IncludeLiterals;
			insert_key_overwriting = false;
			UpdateVisibleText ();
		}
#endregion

#region Public and protected methods
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new void ClearUndo ()
		{
			// Do nothing, not supported by MTB
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[UIPermission (SecurityAction.InheritanceDemand, Window = UIPermissionWindow.AllWindows)]
		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}

		public override char GetCharFromPosition (Point pt)
		{
			return base.GetCharFromPosition (pt);
		}

		public override int GetCharIndexFromPosition (Point pt)
		{
			return base.GetCharIndexFromPosition (pt);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public new int GetFirstCharIndexFromLine (int lineNumber)
		{
			return 0;
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new int GetFirstCharIndexOfCurrentLine ()
		{
			return 0;
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override int GetLineFromCharIndex (int index)
		{
			return 0;
		}

		public override Point GetPositionFromCharIndex (int index)
		{
			return base.GetPositionFromCharIndex (index);
		}

		protected override bool IsInputKey (Keys keyData)
		{
			return base.IsInputKey (keyData);
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnIsOverwriteModeChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [IsOverwriteModeChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			// Only handle Delete or Insert here

			if (e.KeyCode == Keys.Insert && insert_key_mode == InsertKeyMode.Default) {
				// switch the internal overwriting mode, not the public one
				insert_key_overwriting = !insert_key_overwriting;
				OnIsOverwriteModeChanged (EventArgs.Empty);
				e.Handled = true;
				return;
			}

			if (e.KeyCode != Keys.Delete || is_empty_mask) {
				base.OnKeyDown (e);
				return;
			}

			int testPosition, endSelection;
			MaskedTextResultHint resultHint;
			bool result;

			// Use a slightly different approach than the one used for backspace
			endSelection = SelectionLength == 0 ? SelectionStart : SelectionStart + SelectionLength - 1;
			result = provider.RemoveAt (SelectionStart, endSelection, out testPosition, out resultHint);

			PostprocessKeyboardInput (result, testPosition, testPosition, resultHint);

			e.Handled = true;
		}

		protected override void OnKeyPress (KeyPressEventArgs e)
		{
			if (is_empty_mask) {
				base.OnKeyPress (e);
				return;
			}
			
			int testPosition, editPosition;
			MaskedTextResultHint resultHint;
			bool result;
			
			if (e.KeyChar == '\b') {
				if (SelectionLength == 0)
					result = provider.RemoveAt (SelectionStart - 1, SelectionStart - 1, out testPosition, out resultHint);
				else
					result = provider.RemoveAt (SelectionStart, SelectionStart + SelectionLength - 1, out testPosition, out resultHint);

				editPosition = testPosition;
			} else if (IsOverwriteMode || SelectionLength > 0) { // Replace
				int start = provider.FindEditPositionFrom (SelectionStart, true);
				int end = SelectionLength > 0 ? SelectionStart + SelectionLength - 1 : start;
				result = provider.Replace (e.KeyChar, start, end, out testPosition, out resultHint);

				editPosition = testPosition + 1;
			} else { 
				// Move chars to the right
				result = provider.InsertAt (e.KeyChar, SelectionStart, out testPosition, out resultHint);
				editPosition = testPosition + 1;
			}

			PostprocessKeyboardInput (result, editPosition, testPosition, resultHint);
			
			e.Handled = true;
		}

		void PostprocessKeyboardInput (bool result, int newPosition, int testPosition, MaskedTextResultHint resultHint)
		{
			if (!result)
				OnMaskInputRejected (new MaskInputRejectedEventArgs (testPosition, resultHint));
			else {
				if (newPosition != MaskedTextProvider.InvalidIndex)
					SelectionStart = newPosition;
				else
					SelectionStart = provider.Length;

				UpdateVisibleText ();
			}
		}

		protected override void OnKeyUp (KeyEventArgs e)
		{
			base.OnKeyUp (e);
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnMaskChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [MaskChangedEvent];
			if (eh != null)
				eh (this, e);
		}
		
		private void OnMaskInputRejected (MaskInputRejectedEventArgs e)
		{
			MaskInputRejectedEventHandler eh = (MaskInputRejectedEventHandler) Events [MaskInputRejectedEvent];
			if (eh != null)
				eh (this, e);
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override void OnMultilineChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [MultilineChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnTextAlignChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [TextAlignChangedEvent];
			if (eh != null)
				eh (this, e);
		}
		
		protected override void OnTextChanged (EventArgs e)
		{
			base.OnTextChanged (e);
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnValidating (CancelEventArgs e)
		{
			base.OnValidating (e);
		}

		//[SecurityPermission (SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
		{
			return base.ProcessCmdKey (ref msg, keyData);
		}

		//[SecurityPermission (SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected internal override bool ProcessKeyMessage (ref Message m)
		{
			return base.ProcessKeyMessage (ref m);
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new void ScrollToCaret ()
		{
			// MSDN: this method is overridden to perform no actions.	
		}
		
		public override string ToString ()
		{
			return base.ToString () + ", Text: " + provider.ToString (false, false);
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new void Undo ()
		{
			// Do nothing, not supported by MTB.
		}
		
		public object ValidateText ()
		{
			throw new NotImplementedException ();
		}

		//[SecurityPermission (SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected override void WndProc (ref Message m)
		{
			switch ((Msg) m.Msg) {
			default:
				base.WndProc (ref m);
				return;
			}
		}
#endregion

#region Properties
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new bool AcceptsTab {
			get {
				return false;
			}
			set {
			}
		}
		
		[DefaultValue (true)]
		public bool AllowPromptAsInput {
			get {
				return provider.AllowPromptAsInput;
			}
			set {
				provider = new MaskedTextProvider (provider.Mask, provider.Culture, value, provider.PromptChar, provider.PasswordChar, provider.AsciiOnly);
				UpdateVisibleText ();
			}
		}
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (false)]
		public bool AsciiOnly {
			get {
				return provider.AsciiOnly;
			}
			set {
				provider = new MaskedTextProvider (provider.Mask, provider.Culture, provider.AllowPromptAsInput, provider.PromptChar, provider.PasswordChar, value);
				UpdateVisibleText ();
			}
		}
		
		[DefaultValue (false)]
		public bool BeepOnError {
			get {
				return beep_on_error;
			}
			set {
				beep_on_error = value;
			}
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool CanUndo {
			get {
				return false;
			}
		}

		protected override CreateParams CreateParams {
			//[SecurityPermission (SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)] 
			get {
				return base.CreateParams;
			}
		}
		
		[RefreshProperties (RefreshProperties.Repaint)]
		public CultureInfo Culture {
			get {
				return provider.Culture;
			}
			set {
				provider = new MaskedTextProvider (provider.Mask, value, provider.AllowPromptAsInput, provider.PromptChar, provider.PasswordChar, provider.AsciiOnly);
				UpdateVisibleText ();
			}
		}
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (MaskFormat.IncludeLiterals)]
		public MaskFormat CutCopyMaskFormat {
			get {
				return cut_copy_mask_format;
			}
			set {
				if (!Enum.IsDefined (typeof (MaskFormat), value)) {
					throw new InvalidEnumArgumentException ("value", (int)value, typeof (MaskFormat));
				}
				cut_copy_mask_format = value;
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public IFormatProvider FormatProvider {
			get {
				return format_provider;
			}
			set {
				format_provider = value;
			}
		}
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (false)]
		public bool HidePromptOnLeave {
			get {
				return hide_prompt_on_leave;
			}
			set {
				hide_prompt_on_leave = value;
			}
		}
		
		[DefaultValue (InsertKeyMode.Default)]
		public InsertKeyMode InsertKeyMode {
			get {
				return insert_key_mode;
			}
			set {
				if (!Enum.IsDefined (typeof (InsertKeyMode), value)) {
					throw new InvalidEnumArgumentException ("value", (int)value, typeof (InsertKeyMode));
				}
				insert_key_mode = value;
			}
		}
		
		[Browsable (false)]
		public bool IsOverwriteMode {
			get {
				if (insert_key_mode == InsertKeyMode.Default) {
					return insert_key_overwriting;
				} else {
					return insert_key_mode == InsertKeyMode.Overwrite;
				}
			}
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new string [] Lines {
			get {
				string text = Text;
				if (!is_empty_mask)
					text = provider.ToDisplayString ();
				if (text == null || text == string.Empty)
					return new string [] {};
				
				return text.Split (new string [] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
			}
			set {
				// Do nothing, not supported by MTB.
			}
		}
		
		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.MaskPropertyEditor, " + Consts.AssemblySystem_Design, typeof (UITypeEditor))]
		[RefreshProperties (RefreshProperties.Repaint)]
		[MergablePropertyAttribute (false)]
		[DefaultValue ("")]
		public string Mask {
			get {
				if (is_empty_mask)
					return string.Empty;
				
				return provider.Mask;
			}
			set {
				is_empty_mask = (value == string.Empty || value == null);
				if (is_empty_mask) {
					value = "<>";
				}
				
				provider = new MaskedTextProvider (value, provider.Culture, provider.AllowPromptAsInput, provider.PromptChar, provider.PasswordChar, provider.AsciiOnly);
				ReCalculatePasswordChar ();
				UpdateVisibleText ();
			}
		}
		
		[Browsable (false)]
		public bool MaskCompleted {
			get {
				return provider.MaskCompleted;
			}
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public MaskedTextProvider MaskedTextProvider {
			get {
				if (is_empty_mask)
					return null;
					
				return provider.Clone () as MaskedTextProvider;
			}
		}
		
		[Browsable (false)]
		public bool MaskFull {
			get {
				return provider.MaskFull;
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override int MaxLength {
			get {
				return base.MaxLength;
			}
			set {
				// Do nothing, MTB doesn't support this.
			}
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool Multiline {
			get {
				return false;
			}
			set {
				// Do nothing, MTB doesn't support this.
			}
		}
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ('\0')]
		public char PasswordChar {
			get {
				if (use_system_password_char)
					return '*';

				return provider.PasswordChar;
			}
			set {
				provider.PasswordChar = value;

				if (value != '\0') {
					provider.IsPassword = true;
				} else 
					provider.IsPassword = false;

				ReCalculatePasswordChar (true);
				CalculateDocument ();

				UpdateVisibleText ();
			}
		}
			
		[Localizable (true)]
		[DefaultValue ('_')]
		[RefreshProperties (RefreshProperties.Repaint)]
		public char PromptChar {
			get {
				return provider.PromptChar;
			}
			set {
				provider.PromptChar = value;
				UpdateVisibleText ();
			}
		}
		
		public new bool ReadOnly {
			get {
				return base.ReadOnly;
			}
			set {
				base.ReadOnly = value;
			}
		}
		
		[DefaultValue (false)]
		public bool RejectInputOnFirstFailure {
			get {
				return reject_input_on_first_failure;
			}
			set {
				reject_input_on_first_failure = value;
			}
		}
		
		[DefaultValue (true)]
		public bool ResetOnPrompt {
			get {
				return provider.ResetOnPrompt;
			}
			set {
				provider.ResetOnPrompt = value;
			}
		}
		
		[DefaultValue (true)]
		public bool ResetOnSpace {
			get {
				return provider.ResetOnSpace;
			}
			set {
				provider.ResetOnSpace = value;
			}
		}
		
		public override string SelectedText {
			get {
				return base.SelectedText;
			}
			set {
				base.SelectedText = value;
				UpdateVisibleText ();
			}
		}
		
		[DefaultValue (true)]
		public bool SkipLiterals {
			get {
				return provider.SkipLiterals;
			}
			set {
				provider.SkipLiterals = value;
			}
		}
		
		[Bindable (true)]
		[Editor ("System.Windows.Forms.Design.MaskedTextBoxTextEditor, " + Consts.AssemblySystem_Design, typeof (UITypeEditor))]
		[Localizable (true)]
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ("")]
		public override string Text {
			get {
				if (is_empty_mask || setting_text)
					return base.Text;
				
				// The base constructor may call Text before we get to create a provider, 
				// so it may be null even though it's not an empty mask.
				if (provider == null)
					return string.Empty;
					
				return provider.ToString ();
			}
			set {
				
				if (is_empty_mask) {
					setting_text = true;
					base.Text = value;
					setting_text = false;
				} else {
					InputText (value);
				}
				UpdateVisibleText ();
			}
		}
		
		[DefaultValue (HorizontalAlignment.Left)]
		[Localizable (true)]
		public HorizontalAlignment TextAlign {
			get {
				return text_align;
			}
			set {
				if (text_align != value) {
					if (!Enum.IsDefined (typeof (HorizontalAlignment), value)) {
						throw new InvalidEnumArgumentException ("value", (int) value, typeof (HorizontalAlignment));
					}
					text_align = value;
					OnTextAlignChanged (EventArgs.Empty);
				}
			}
		}
		
		[Browsable (false)]
		public override int TextLength {
			get {
				return Text.Length;
			}
		}
		
		[DefaultValue (MaskFormat.IncludeLiterals)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public MaskFormat TextMaskFormat {
			get {
				if (provider.IncludePrompt && provider.IncludeLiterals) {
					return MaskFormat.IncludePromptAndLiterals;
				} else if (provider.IncludeLiterals) {
					return MaskFormat.IncludeLiterals;
				} else if (provider.IncludePrompt) {
					return MaskFormat.IncludePrompt;
				} else {
					return MaskFormat.ExcludePromptAndLiterals;
				}
			}
			set {
				if (!Enum.IsDefined (typeof (MaskFormat), value)) {
					throw new InvalidEnumArgumentException ("value", (int)value, typeof (MaskFormat));
				}
				
				provider.IncludeLiterals = (value & MaskFormat.IncludeLiterals) == MaskFormat.IncludeLiterals;
				provider.IncludePrompt = (value & MaskFormat.IncludePrompt) == MaskFormat.IncludePrompt;
			}
		}
			
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public bool UseSystemPasswordChar {
			get {
				return use_system_password_char;
			}
			set {
				if (use_system_password_char != value) {
					use_system_password_char = value;

					if (use_system_password_char)
						PasswordChar = PasswordChar;
					else
						PasswordChar = '\0';
				}
			}
		}
		
		[DefaultValue (null)]
		[Browsable (false)]
		public Type ValidatingType {
			get {
				return validating_type;
			}
			set {
				validating_type = value;
			}
		}
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool WordWrap {
			get {
				return false;
			}
			set {
				// Do nothing, not supported by MTB
			}
		}
#endregion
#region Internal and private members

		private void ReCalculatePasswordChar ()
		{
			ReCalculatePasswordChar (PasswordChar != '\0');
		}

		private void ReCalculatePasswordChar (bool using_password)
		{
			if (using_password)
				if (is_empty_mask)
					document.PasswordChar = PasswordChar.ToString ();
				else
					document.PasswordChar = string.Empty;
		}

		internal override void OnPaintInternal (PaintEventArgs pevent)
		{
			base.OnPaintInternal (pevent);
			//pevent.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), ClientRectangle);
			//TextRenderer.DrawText (pevent.Graphics, Text, Font, ClientRectangle, ForeColor, TextFormatFlags.SingleLine);
			//pevent.Handled = true;
		}
		
		internal override Color ChangeBackColor (Color backColor)
		{
			return backColor;
		}
		
		private void UpdateVisibleText ()
		{
			string text = null;

			if (is_empty_mask || setting_text)
				text = base.Text;
			else
				if (provider == null)
					text = string.Empty;
				else
					text = provider.ToDisplayString ();

			setting_text = true;
			if (base.Text != text) {
				int selstart = SelectionStart;
				base.Text = text;
				SelectionStart = selstart;
			}
			setting_text = false;
		}
		
		private void InputText (string text)
		{
			string input = text;
				
			int testPosition;
			MaskedTextResultHint resultHint;
			bool result;
			
			if (RejectInputOnFirstFailure) {
				result = provider.Set (input, out testPosition, out resultHint);
				if (!result)
					OnMaskInputRejected (new MaskInputRejectedEventArgs (testPosition, resultHint));
			} else {
				provider.Clear ();
				testPosition = 0;

				// Unfortunately we can't break if we reach the end of the mask, since
				// .net iterates over _all_ the chars in the input
				for (int i = 0; i < input.Length; i++) {
					char c = input [i];

					result = provider.InsertAt (c, testPosition, out testPosition, out resultHint);
					if (result)
						testPosition++; // Move to the next free position
					else
						OnMaskInputRejected (new MaskInputRejectedEventArgs (testPosition, resultHint));
				}
			}
		}
#endregion
	}
}
