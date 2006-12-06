//
// ToolStripTextBox.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//
#if NET_2_0
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public class ToolStripTextBox : ToolStripControlHost
	{
		private BorderStyle border_style;

		#region Public Constructors
		public ToolStripTextBox () : base (new ToolStripTextBoxControl ())
		{
			base.Control.border_style = BorderStyle.None;
			this.border_style = BorderStyle.Fixed3D;
		}

		public ToolStripTextBox (Control c) : base (c)
		{
		}

		public ToolStripTextBox (string name) : this ()
		{
			base.Control.Name = name;
		}
		#endregion

		#region Public Properties
		[DefaultValue (false)]
		public bool AcceptsReturn {
			get { return this.TextBox.AcceptsReturn; }
			set { this.TextBox.AcceptsReturn = value; }
		}

		[DefaultValue (false)]
		public bool AcceptsTab {
			get { return this.TextBox.AcceptsTab; }
			set { this.TextBox.AcceptsTab = value; }
		}

		[DefaultValue (BorderStyle.Fixed3D)]
		public BorderStyle BorderStyle {
			get { return this.border_style; }
			set { this.border_style = value; }
		}

		[Browsable (false)]
		public bool CanUndo {
			get { return this.TextBox.CanUndo; }
		}

		[DefaultValue (CharacterCasing.Normal)]
		public CharacterCasing CharacterCasing {
			get { return this.TextBox.CharacterCasing; }
			set { this.TextBox.CharacterCasing = value; }
		}

		[DefaultValue (true)]
		public bool HideSelection {
			get { return this.TextBox.HideSelection; }
			set { this.TextBox.HideSelection = value; }
		}

		[Localizable (true)]
		public string[] Lines {
			get { return this.TextBox.Lines; }
			set { this.TextBox.Lines = value; }
		}

		[Localizable (true)]
		[DefaultValue (32767)]
		public int MaxLength {
			get { return this.TextBox.MaxLength; }
			set { this.TextBox.MaxLength = value; }
		}

		[Browsable (false)]
		public bool Modified {
			get { return this.TextBox.Modified; }
			set { this.TextBox.Modified = value; }
		}

		[Localizable (true)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DefaultValue (false)]
		public bool Multiline {
			get { return this.TextBox.Multiline; }
			set { this.TextBox.Multiline = value; }
		}

		[DefaultValue (false)]
		public bool ReadOnly {
			get { return this.TextBox.ReadOnly; }
			set { this.TextBox.ReadOnly = value; }
		}

		[Browsable (false)]
		public string SelectedText {
			get { return this.TextBox.SelectedText; }
			set { this.TextBox.SelectedText = value; }
		}

		[Browsable (false)]
		public int SelectionLength {
			get { return this.TextBox.SelectionLength; }
			set { this.TextBox.SelectionLength = value; }
		}

		[Browsable (false)]
		public int SelectionStart {
			get { return this.TextBox.SelectionStart; }
			set { this.TextBox.SelectionStart = value; }
		}

		[Browsable (false)]
		public TextBox TextBox {
			get { return (TextBox)base.Control; }
		}

		[Localizable (true)]
		[DefaultValue (HorizontalAlignment.Left)]
		public HorizontalAlignment TextBoxTextAlign {
			get { return this.TextBox.TextAlign; }
			set { this.TextBox.TextAlign = value; }
		}

		[Browsable (false)]
		public int TextLength {
			get { return this.TextBox.TextLength; }
		}

		[Localizable (true)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DefaultValue (true)]
		public bool WordWrap {
			get { return this.TextBox.WordWrap; }
			set { this.TextBox.WordWrap = value; }
		}
		#endregion

		#region Protected Properties
		protected internal override Padding DefaultMargin { get { return new Padding (1, 0, 1, 0); } }
		protected override Size DefaultSize { get { return new Size (100, 22); } }
		#endregion

		#region Public Methods
		public void AppendText (string text)
		{
			this.TextBox.AppendText (text);
		}

		public void Clear ()
		{
			this.TextBox.Clear ();
		}

		public void ClearUndo ()
		{
			this.TextBox.ClearUndo ();
		}

		public void Copy ()
		{
			this.TextBox.Copy ();
		}

		public void Cut ()
		{
			this.TextBox.Cut ();
		}

		public override Size GetPreferredSize (Size constrainingSize)
		{
			return this.DefaultSize;
		}

		public void Paste ()
		{
			this.TextBox.Paste ();
		}

		public void ScrollToCaret ()
		{
			this.TextBox.ScrollToCaret ();
		}

		public void Select (int start, int length)
		{
			this.TextBox.Select (start, length);
		}

		public void SelectAll ()
		{
			this.TextBox.SelectAll ();
		}

		public void Undo ()
		{
			this.TextBox.Undo ();
		}
		#endregion

		#region Protected Methods
		protected virtual void OnAcceptsTabChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [AcceptsTabChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnBorderStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [BorderStyleChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnHideSelectionChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [HideSelectionChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnModifiedChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [ModifiedChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnMultilineChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [MultilineChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnReadOnlyChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [ReadOnlyChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected override void OnSubscribeControlEvents (Control control)
		{
			base.OnSubscribeControlEvents (control);

			this.TextBox.AcceptsTabChanged += new EventHandler (HandleAcceptsTabChanged);
			this.TextBox.BorderStyleChanged += new EventHandler (HandleBorderStyleChanged);
			this.TextBox.HideSelectionChanged += new EventHandler (HandleHideSelectionChanged);
			this.TextBox.ModifiedChanged += new EventHandler (HandleModifiedChanged);
			this.TextBox.MultilineChanged += new EventHandler (HandleMultilineChanged);
			this.TextBox.ReadOnlyChanged += new EventHandler (HandleReadOnlyChanged);
			this.TextBox.TextAlignChanged += new EventHandler (HandleTextAlignChanged);
		}

		protected override void OnUnsubscribeControlEvents (Control control)
		{
			base.OnUnsubscribeControlEvents (control);
		}
		#endregion

		#region Public Events
		static object AcceptsTabChangedEvent = new object ();
		static object BorderStyleChangedEvent = new object ();
		static object HideSelectionChangedEvent = new object ();
		static object ModifiedChangedEvent = new object ();
		static object MultilineChangedEvent = new object ();
		static object ReadOnlyChangedEvent = new object ();
		static object TextBoxTextAlignChangedEvent = new object ();

		public event EventHandler AcceptsTabChanged {
			add { Events.AddHandler (AcceptsTabChangedEvent, value); }
			remove {Events.RemoveHandler (AcceptsTabChangedEvent, value); }
		}
		public event EventHandler BorderStyleChanged {
			add { Events.AddHandler (BorderStyleChangedEvent, value); }
			remove {Events.RemoveHandler (BorderStyleChangedEvent, value); }
		}
		public event EventHandler HideSelectionChanged {
			add { Events.AddHandler (HideSelectionChangedEvent, value); }
			remove {Events.RemoveHandler (HideSelectionChangedEvent, value); }
		}
		public event EventHandler ModifiedChanged {
			add { Events.AddHandler (ModifiedChangedEvent, value); }
			remove {Events.RemoveHandler (ModifiedChangedEvent, value); }
		}
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler MultilineChanged {
			add { Events.AddHandler (MultilineChangedEvent, value); }
			remove {Events.RemoveHandler (MultilineChangedEvent, value); }
		}
		public event EventHandler ReadOnlyChanged {
			add { Events.AddHandler (ReadOnlyChangedEvent, value); }
			remove {Events.RemoveHandler (ReadOnlyChangedEvent, value); }
		}
		public event EventHandler TextBoxTextAlignChanged {
			add { Events.AddHandler (TextBoxTextAlignChangedEvent, value); }
			remove {Events.RemoveHandler (TextBoxTextAlignChangedEvent, value); }
		}
		#endregion

		#region Private Methods
		private void HandleTextAlignChanged (object sender, EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [TextBoxTextAlignChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		private void HandleReadOnlyChanged (object sender, EventArgs e)
		{
			OnReadOnlyChanged (e);
		}

		private void HandleMultilineChanged (object sender, EventArgs e)
		{
			OnMultilineChanged (e);
		}

		private void HandleModifiedChanged (object sender, EventArgs e)
		{
			OnModifiedChanged (e);
		}

		private void HandleHideSelectionChanged (object sender, EventArgs e)
		{
			OnHideSelectionChanged (e);
		}

		private void HandleBorderStyleChanged (object sender, EventArgs e)
		{
			OnBorderStyleChanged (e);
		}

		private void HandleAcceptsTabChanged (object sender, EventArgs e)
		{
			OnAcceptsTabChanged (e);
		}
		#endregion

		private class ToolStripTextBoxControl : TextBox
		{
			public ToolStripTextBoxControl () : base ()
			{
			}

			protected override void OnMouseEnter (EventArgs e)
			{
				base.OnMouseEnter (e);
				Invalidate ();
			}

			protected override void OnMouseLeave (EventArgs e)
			{
				base.OnMouseLeave (e);
				Invalidate ();
			}

			protected override void OnPaint (PaintEventArgs e)
			{
				base.OnPaint (e);
				if (this.Focused || is_entered || this.border_style == BorderStyle.FixedSingle) {
					ToolStripRenderer tsr = (this.Parent as ToolStrip).Renderer;

					if (tsr is ToolStripProfessionalRenderer)
						using (Pen p = new Pen ((tsr as ToolStripProfessionalRenderer).ColorTable.ButtonSelectedBorder))
							e.Graphics.DrawRectangle (p, new Rectangle (0, 0, this.Width - 1, this.Height - 1));
				}
			}
		}
	}
}
#endif
