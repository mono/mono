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


#if NET_2_0 && notyet

using System.ComponentModel;
using System.Windows.Forms;
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Globalization;
using System.Collections;

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
#region Events
		static object AcceptsTabChangedEvent = new object ();
		static object IsOverwriteModeChangedEvent = new object ();
		static object MaskChangedEvent = new object ();
		static object MaskInputRejectedEvent = new object ();
		static object MultilineChangedEvent = new object ();
		static object TextAlignChangedEvent = new object ();
		static object TypeValidationCompletedEvent = new object ();
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler AcceptsTabChanged {
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
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler MultilineChanged {
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

		static MaskedTextBox ()
		{
			throw new NotImplementedException ();
		}

		public MaskedTextBox ()
		{
			throw new NotImplementedException ();
		}

		public MaskedTextBox (MaskedTextProvider maskedTextProvider)
		{
			throw new NotImplementedException ();
		}

		public MaskedTextBox (string mask)
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public void ClearUndo ()
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[System.Security.Permissions.UIPermission (System.Security.Permissions.SecurityAction.InheritanceDemand, Window = System.Security.Permissions.UIPermissionWindow.AllWindows)]
		protected override void CreateHandle ()
		{
			throw new NotImplementedException ();
		}

		public override char GetCharFromPosition (Point pt)
		{
			throw new NotImplementedException ();
		}

		public override int GetCharIndexFromPosition (Point pt)
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public int GetFirstCharIndexFromLine (int lineNumber)
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public int GetFirstCharIndexOfCurrentLine ()
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override int GetLineFromCharIndex (int index)
		{
			throw new NotImplementedException ();
		}

		public override Point GetPositionFromCharIndex (int index)
		{
			throw new NotImplementedException ();
		}

		protected override bool IsInputKey (Keys keyData)
		{
			throw new NotImplementedException ();
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}

		protected override void OnKeyPress (KeyPressEventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected override void OnKeyUp (KeyEventArgs e)
		{
			throw new NotImplementedException ();
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
		
		protected override void OnTextChanged (EventArgs e){ throw new NotImplementedException (); }
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnValidating (CancelEventArgs e){ throw new NotImplementedException (); }

		[System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
		protected override bool ProcessCmdKey (ref Message msg, Keys keyData){ throw new NotImplementedException (); }

		[System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
		protected internal override bool ProcessKeyMessage (ref Message m){ throw new NotImplementedException (); }
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public void ScrollToCaret (){ throw new NotImplementedException (); }
		
		public override string ToString (){ throw new NotImplementedException (); }
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public void Undo (){ throw new NotImplementedException (); }
		
		public object ValidateText (){ throw new NotImplementedException (); }

		[System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
		protected override void WndProc (ref Message m){ throw new NotImplementedException (); }

#region Properties
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public bool AcceptsTab { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (true)]
		public bool AllowPromptAsInput { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (false)]
		public bool AsciiOnly { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (false)]
		public bool BeepOnError { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public bool CanUndo { get{ throw new NotImplementedException (); } }

		protected override CreateParams CreateParams { [System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)] get { throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		public CultureInfo Culture { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (MaskFormat.IncludeLiterals)]
		public MaskFormat CutCopyMaskFormat { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (null)]
		public IFormatProvider FormatProvider { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (false)]
		public bool HidePromptOnLeave { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (InsertKeyMode.Default)]
		public InsertKeyMode InsertKeyMode { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		public bool IsOverwriteMode { get{ throw new NotImplementedException (); } }
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public string [] Lines { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.MaskPropertyEditor, " + Consts.AssemblySystem_Design, typeof (UITypeEditor))]
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ("")]
		public string Mask { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		public bool MaskCompleted { get{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public MaskedTextProvider MaskedTextProvider { get{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		public bool MaskFull { get{ throw new NotImplementedException (); } }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override int MaxLength { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool Multiline { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ('\0')]
		public char PasswordChar { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
			
		[Localizable (true)]
		[DefaultValue ('_')]
		[RefreshProperties (RefreshProperties.Repaint)]
		public char PromptChar { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		public bool ReadOnly { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (false)]
		public bool RejectInputOnFirstFailure { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (true)]
		public bool ResetOnPrompt { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (true)]
		public bool ResetOnSpace { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		public override string SelectedText { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (true)]
		public bool SkipLiterals { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Bindable (true)]
		[Editor ("System.Windows.Forms.Design.MaskedTextBoxTextEditor, " + Consts.AssemblySystem_Design, typeof (UITypeEditor))]
		[Localizable (true)]
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ("")]
		public override string Text { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (HorizontalAlignment.Left)]
		[Localizable (true)]
		public HorizontalAlignment TextAlign { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		public override int TextLength { get{ throw new NotImplementedException (); } }
		
		[DefaultValue (MaskFormat.IncludeLiterals)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public MaskFormat TextMaskFormat { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
			
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public bool UseSystemPasswordChar { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (null)]
		[Browsable (false)]
		public Type ValidatingType { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public bool WordWrap { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
#endregion
	}
 

}
#endif
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


#if NET_2_0 && notyet

using System.ComponentModel;
using System.Windows.Forms;
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Globalization;
using System.Collections;

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
#region Events
		static object AcceptsTabChangedEvent = new object ();
		static object IsOverwriteModeChangedEvent = new object ();
		static object MaskChangedEvent = new object ();
		static object MaskInputRejectedEvent = new object ();
		static object MultilineChangedEvent = new object ();
		static object TextAlignChangedEvent = new object ();
		static object TypeValidationCompletedEvent = new object ();
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler AcceptsTabChanged {
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
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler MultilineChanged {
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

		static MaskedTextBox ()
		{
			throw new NotImplementedException ();
		}

		public MaskedTextBox ()
		{
			throw new NotImplementedException ();
		}

		public MaskedTextBox (MaskedTextProvider maskedTextProvider)
		{
			throw new NotImplementedException ();
		}

		public MaskedTextBox (string mask)
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public void ClearUndo ()
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[System.Security.Permissions.UIPermission (System.Security.Permissions.SecurityAction.InheritanceDemand, Window = System.Security.Permissions.UIPermissionWindow.AllWindows)]
		protected override void CreateHandle ()
		{
			throw new NotImplementedException ();
		}

		public override char GetCharFromPosition (Point pt)
		{
			throw new NotImplementedException ();
		}

		public override int GetCharIndexFromPosition (Point pt)
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public int GetFirstCharIndexFromLine (int lineNumber)
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public int GetFirstCharIndexOfCurrentLine ()
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override int GetLineFromCharIndex (int index)
		{
			throw new NotImplementedException ();
		}

		public override Point GetPositionFromCharIndex (int index)
		{
			throw new NotImplementedException ();
		}

		protected override bool IsInputKey (Keys keyData)
		{
			throw new NotImplementedException ();
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}

		protected override void OnKeyPress (KeyPressEventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected override void OnKeyUp (KeyEventArgs e)
		{
			throw new NotImplementedException ();
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
		
		protected override void OnTextChanged (EventArgs e){ throw new NotImplementedException (); }
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnValidating (CancelEventArgs e){ throw new NotImplementedException (); }

		[System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
		protected override bool ProcessCmdKey (ref Message msg, Keys keyData){ throw new NotImplementedException (); }

		[System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
		protected internal override bool ProcessKeyMessage (ref Message m){ throw new NotImplementedException (); }
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public void ScrollToCaret (){ throw new NotImplementedException (); }
		
		public override string ToString (){ throw new NotImplementedException (); }
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public void Undo (){ throw new NotImplementedException (); }
		
		public object ValidateText (){ throw new NotImplementedException (); }

		[System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
		protected override void WndProc (ref Message m){ throw new NotImplementedException (); }

#region Properties
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public bool AcceptsTab { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (true)]
		public bool AllowPromptAsInput { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (false)]
		public bool AsciiOnly { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (false)]
		public bool BeepOnError { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public bool CanUndo { get{ throw new NotImplementedException (); } }

		protected override CreateParams CreateParams { [System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)] get { throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		public CultureInfo Culture { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (MaskFormat.IncludeLiterals)]
		public MaskFormat CutCopyMaskFormat { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (null)]
		public IFormatProvider FormatProvider { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (false)]
		public bool HidePromptOnLeave { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (InsertKeyMode.Default)]
		public InsertKeyMode InsertKeyMode { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		public bool IsOverwriteMode { get{ throw new NotImplementedException (); } }
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public string [] Lines { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.MaskPropertyEditor, " + Consts.AssemblySystem_Design, typeof (UITypeEditor))]
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ("")]
		public string Mask { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		public bool MaskCompleted { get{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public MaskedTextProvider MaskedTextProvider { get{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		public bool MaskFull { get{ throw new NotImplementedException (); } }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override int MaxLength { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool Multiline { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ('\0')]
		public char PasswordChar { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
			
		[Localizable (true)]
		[DefaultValue ('_')]
		[RefreshProperties (RefreshProperties.Repaint)]
		public char PromptChar { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		public bool ReadOnly { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (false)]
		public bool RejectInputOnFirstFailure { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (true)]
		public bool ResetOnPrompt { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (true)]
		public bool ResetOnSpace { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		public override string SelectedText { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (true)]
		public bool SkipLiterals { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Bindable (true)]
		[Editor ("System.Windows.Forms.Design.MaskedTextBoxTextEditor, " + Consts.AssemblySystem_Design, typeof (UITypeEditor))]
		[Localizable (true)]
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ("")]
		public override string Text { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (HorizontalAlignment.Left)]
		[Localizable (true)]
		public HorizontalAlignment TextAlign { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		public override int TextLength { get{ throw new NotImplementedException (); } }
		
		[DefaultValue (MaskFormat.IncludeLiterals)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public MaskFormat TextMaskFormat { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
			
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public bool UseSystemPasswordChar { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (null)]
		[Browsable (false)]
		public Type ValidatingType { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public bool WordWrap { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
#endregion
	}
 

}
#endif
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


#if NET_2_0 && notyet

using System.ComponentModel;
using System.Windows.Forms;
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Globalization;
using System.Collections;

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
#region Events
		static object AcceptsTabChangedEvent = new object ();
		static object IsOverwriteModeChangedEvent = new object ();
		static object MaskChangedEvent = new object ();
		static object MaskInputRejectedEvent = new object ();
		static object MultilineChangedEvent = new object ();
		static object TextAlignChangedEvent = new object ();
		static object TypeValidationCompletedEvent = new object ();
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler AcceptsTabChanged {
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
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler MultilineChanged {
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

		static MaskedTextBox ()
		{
			throw new NotImplementedException ();
		}

		public MaskedTextBox ()
		{
			throw new NotImplementedException ();
		}

		public MaskedTextBox (MaskedTextProvider maskedTextProvider)
		{
			throw new NotImplementedException ();
		}

		public MaskedTextBox (string mask)
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public void ClearUndo ()
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[System.Security.Permissions.UIPermission (System.Security.Permissions.SecurityAction.InheritanceDemand, Window = System.Security.Permissions.UIPermissionWindow.AllWindows)]
		protected override void CreateHandle ()
		{
			throw new NotImplementedException ();
		}

		public override char GetCharFromPosition (Point pt)
		{
			throw new NotImplementedException ();
		}

		public override int GetCharIndexFromPosition (Point pt)
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public int GetFirstCharIndexFromLine (int lineNumber)
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public int GetFirstCharIndexOfCurrentLine ()
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override int GetLineFromCharIndex (int index)
		{
			throw new NotImplementedException ();
		}

		public override Point GetPositionFromCharIndex (int index)
		{
			throw new NotImplementedException ();
		}

		protected override bool IsInputKey (Keys keyData)
		{
			throw new NotImplementedException ();
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}

		protected override void OnKeyPress (KeyPressEventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected override void OnKeyUp (KeyEventArgs e)
		{
			throw new NotImplementedException ();
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
		
		protected override void OnTextChanged (EventArgs e){ throw new NotImplementedException (); }
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnValidating (CancelEventArgs e){ throw new NotImplementedException (); }

		[System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
		protected override bool ProcessCmdKey (ref Message msg, Keys keyData){ throw new NotImplementedException (); }

		[System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
		protected internal override bool ProcessKeyMessage (ref Message m){ throw new NotImplementedException (); }
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public void ScrollToCaret (){ throw new NotImplementedException (); }
		
		public override string ToString (){ throw new NotImplementedException (); }
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public void Undo (){ throw new NotImplementedException (); }
		
		public object ValidateText (){ throw new NotImplementedException (); }

		[System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
		protected override void WndProc (ref Message m){ throw new NotImplementedException (); }

#region Properties
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public bool AcceptsTab { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (true)]
		public bool AllowPromptAsInput { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (false)]
		public bool AsciiOnly { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (false)]
		public bool BeepOnError { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public bool CanUndo { get{ throw new NotImplementedException (); } }

		protected override CreateParams CreateParams { [System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)] get { throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		public CultureInfo Culture { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (MaskFormat.IncludeLiterals)]
		public MaskFormat CutCopyMaskFormat { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (null)]
		public IFormatProvider FormatProvider { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (false)]
		public bool HidePromptOnLeave { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (InsertKeyMode.Default)]
		public InsertKeyMode InsertKeyMode { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		public bool IsOverwriteMode { get{ throw new NotImplementedException (); } }
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public string [] Lines { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.MaskPropertyEditor, " + Consts.AssemblySystem_Design, typeof (UITypeEditor))]
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ("")]
		public string Mask { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		public bool MaskCompleted { get{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public MaskedTextProvider MaskedTextProvider { get{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		public bool MaskFull { get{ throw new NotImplementedException (); } }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override int MaxLength { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool Multiline { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ('\0')]
		public char PasswordChar { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
			
		[Localizable (true)]
		[DefaultValue ('_')]
		[RefreshProperties (RefreshProperties.Repaint)]
		public char PromptChar { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		public bool ReadOnly { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (false)]
		public bool RejectInputOnFirstFailure { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (true)]
		public bool ResetOnPrompt { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (true)]
		public bool ResetOnSpace { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		public override string SelectedText { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (true)]
		public bool SkipLiterals { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Bindable (true)]
		[Editor ("System.Windows.Forms.Design.MaskedTextBoxTextEditor, " + Consts.AssemblySystem_Design, typeof (UITypeEditor))]
		[Localizable (true)]
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ("")]
		public override string Text { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (HorizontalAlignment.Left)]
		[Localizable (true)]
		public HorizontalAlignment TextAlign { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[Browsable (false)]
		public override int TextLength { get{ throw new NotImplementedException (); } }
		
		[DefaultValue (MaskFormat.IncludeLiterals)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public MaskFormat TextMaskFormat { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
			
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public bool UseSystemPasswordChar { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DefaultValue (null)]
		[Browsable (false)]
		public Type ValidatingType { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public bool WordWrap { get{ throw new NotImplementedException (); } set{ throw new NotImplementedException (); } }
#endregion
	}
 

}
#endif