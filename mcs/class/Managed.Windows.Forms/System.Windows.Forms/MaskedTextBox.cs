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


#if NET_2_0// && notyet

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
		}

		public MaskedTextBox (MaskedTextProvider maskedTextProvider)
		{
			if (provider == null) {
				throw new ArgumentNullException ();
			}
			provider = maskedTextProvider;
		}

		public MaskedTextBox (string mask)
		{
			if (mask == null) {
				throw new ArgumentNullException ();
			}
			provider = new MaskedTextProvider (mask, CultureInfo.CurrentCulture);
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
#if notyet
		public override char GetCharFromPosition (Point pt)
		{
			return base.GetCharFromPosition (pt);
		}

		public override int GetCharIndexFromPosition (Point pt)
		{
			return base.GetCharIndexFromPosition (pt);
		}
#endif	
		[EditorBrowsable (EditorBrowsableState.Never)]
		public int GetFirstCharIndexFromLine (int lineNumber)
		{
			return 0;
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public int GetFirstCharIndexOfCurrentLine ()
		{
			return 0;
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override int GetLineFromCharIndex (int index)
		{
			return 0;
		}
#if notyet
		public override Point GetPositionFromCharIndex (int index)
		{
			return base.GetPositionFromCharIndex (index);
		}
#endif
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
			base.OnKeyDown (e);
		}

		protected override void OnKeyPress (KeyPressEventArgs e)
		{
			base.OnKeyPress (e);
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
			switch ((Msg) m.Msg)
			{
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
			}
		}
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (MaskFormat.IncludeLiterals)]
		public MaskFormat CutCopyMaskFormat {
			get {
				return cut_copy_mask_format;
			}
			set {
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
				return base.Lines;
			}
			set {
				// Do nothing, not supported by MTB.
			}
		}
		
		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.MaskPropertyEditor, " + Consts.AssemblySystem_Design, typeof (UITypeEditor))]
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ("")]
		public string Mask {
			get {
				return provider.Mask;
			}
			set {
				provider = new MaskedTextProvider (value, provider.Culture, provider.AllowPromptAsInput, provider.PromptChar, provider.PasswordChar, provider.AsciiOnly);
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
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool Multiline {
			get {
				return false;
			}
		}
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ('\0')]
		public char PasswordChar {
			get {
				return provider.PasswordChar;
			}
			set {
				provider.PasswordChar = value;
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
				return provider.ToString ();
			}
			set {
				provider.Set (value);
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
				provider.IncludeLiterals = (value == MaskFormat.IncludeLiterals) || (value == MaskFormat.IncludePromptAndLiterals);
				provider.IncludePrompt = (value == MaskFormat.IncludePrompt) || (value == MaskFormat.IncludePromptAndLiterals);
			}
		}
			
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public bool UseSystemPasswordChar {
			get {
				return use_system_password_char;
			}
			set {
				use_system_password_char = value;
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
		internal override Color ChangeBackColor (Color backColor)
		{
#if NET_2_0
			backcolor_set = false;
			if (!ReadOnly) {
				backColor = SystemColors.Window;
			}
#else
				backColor = SystemColors.Window;
#endif
			return backColor;
		}
#endregion
	}
}
#endif

