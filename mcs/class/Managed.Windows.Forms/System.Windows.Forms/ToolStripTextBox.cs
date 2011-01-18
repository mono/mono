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

using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ToolStripItemDesignerAvailability (ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.MenuStrip | ToolStripItemDesignerAvailability.ContextMenuStrip)]
	public class ToolStripTextBox : ToolStripControlHost
	{
		private BorderStyle border_style;

		#region Public Constructors
		public ToolStripTextBox () : base (new ToolStripTextBoxControl ())
		{
			ToolStripTextBoxControl text_box = TextBox as ToolStripTextBoxControl;
			text_box.OwnerItem = this;
			text_box.border_style = BorderStyle.None;
			text_box.TopMargin = 3; // need to explicitly set the margin
			text_box.Border = BorderStyle.Fixed3D; // ToolStripTextBoxControl impl, not TextBox
			this.border_style = BorderStyle.Fixed3D;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public ToolStripTextBox (Control c) : base (c)
		{
			throw new NotSupportedException ("This construtor cannot be used.");
		}

		public ToolStripTextBox (string name) : this ()
		{
			base.Name = name;
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

		[MonoTODO ("AutoCompletion algorithm is currently not implemented.")]
		[Browsable (true)]
		[Localizable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Editor ("System.Windows.Forms.Design.ListControlStringCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public AutoCompleteStringCollection AutoCompleteCustomSource {
			get { return this.TextBox.AutoCompleteCustomSource; }
			set { this.TextBox.AutoCompleteCustomSource = value; }
		}
		
		[MonoTODO("AutoCompletion algorithm is currently not implemented.")]
		[Browsable (true)]
		[DefaultValue (AutoCompleteMode.None)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public AutoCompleteMode AutoCompleteMode {
			get { return this.TextBox.AutoCompleteMode; }
			set { this.TextBox.AutoCompleteMode = value; }
		}

		[MonoTODO("AutoCompletion algorithm is currently not implemented.")]
		[Browsable (true)]
		[DefaultValue (AutoCompleteSource.None)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public AutoCompleteSource AutoCompleteSource {
			get { return this.TextBox.AutoCompleteSource; }
			set { this.TextBox.AutoCompleteSource = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		[DefaultValue (BorderStyle.Fixed3D)]
		[DispId (-504)]
		public BorderStyle BorderStyle {
			get { return this.border_style; }
			set { 
				if (this.border_style != value) {
					this.border_style = value;
					(base.Control as ToolStripTextBoxControl).Border = value;
					this.OnBorderStyleChanged (EventArgs.Empty);
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Editor ("System.Windows.Forms.Design.StringArrayEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
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
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Modified {
			get { return this.TextBox.Modified; }
			set { this.TextBox.Modified = value; }
		}

		[Localizable (true)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.All)]
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
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string SelectedText {
			get { return this.TextBox.SelectedText; }
			set { this.TextBox.SelectedText = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int SelectionLength {
			get { return this.TextBox.SelectionLength == -1 ? 0 : this.TextBox.SelectionLength; }
			set { this.TextBox.SelectionLength = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int SelectionStart {
			get { return this.TextBox.SelectionStart; }
			set { this.TextBox.SelectionStart = value; }
		}

		[DefaultValue (true)]
		public bool ShortcutsEnabled {
			get { return this.TextBox.ShortcutsEnabled; }
			set { this.TextBox.ShortcutsEnabled = value; }
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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

		public void DeselectAll ()
		{
			this.TextBox.DeselectAll ();
		}
		
		public char GetCharFromPosition (Point pt)
		{
			return this.TextBox.GetCharFromPosition (pt);
		}
		
		public int GetCharIndexFromPosition (Point pt)
		{
			return this.TextBox.GetCharIndexFromPosition (pt);
		}
		
		public int GetFirstCharIndexFromLine (int lineNumber)
		{
			return this.TextBox.GetFirstCharIndexFromLine (lineNumber);
		}
		
		public int GetFirstCharIndexOfCurrentLine ()
		{
			return this.TextBox.GetFirstCharIndexOfCurrentLine ();
		}
		
		public int GetLineFromCharIndex (int index)
		{
			return this.TextBox.GetLineFromCharIndex (index);
		}
		
		public Point GetPositionFromCharIndex (int index)
		{
			return this.TextBox.GetPositionFromCharIndex (index);
		}
		
		public override Size GetPreferredSize (Size constrainingSize)
		{
			return base.GetPreferredSize (constrainingSize);
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
			this.TextBox.HideSelectionChanged += new EventHandler (HandleHideSelectionChanged);
			this.TextBox.ModifiedChanged += new EventHandler (HandleModifiedChanged);
			this.TextBox.MultilineChanged += new EventHandler (HandleMultilineChanged);
			this.TextBox.ReadOnlyChanged += new EventHandler (HandleReadOnlyChanged);
			this.TextBox.TextAlignChanged += new EventHandler (HandleTextAlignChanged);
			this.TextBox.TextChanged += new EventHandler (HandleTextChanged);
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

		private void HandleAcceptsTabChanged (object sender, EventArgs e)
		{
			OnAcceptsTabChanged (e);
		}

		private void HandleTextChanged (object sender, EventArgs e)
		{
			OnTextChanged (e);
		}
		#endregion

		private class ToolStripTextBoxControl : TextBox
		{
			private BorderStyle border;
			private Timer tooltip_timer;
			private ToolTip tooltip_window;
			private ToolStripItem owner_item;
			
			public ToolStripTextBoxControl () : base ()
			{
			}

			protected override void OnLostFocus (EventArgs e)
			{
				base.OnLostFocus (e);
				Invalidate ();
			}
			
			protected override void OnMouseEnter (EventArgs e)
			{
				base.OnMouseEnter (e);
				Invalidate ();

				if (ShowToolTips)
					ToolTipTimer.Start ();

			}

			protected override void OnMouseLeave (EventArgs e)
			{
				base.OnMouseLeave (e);
				Invalidate ();

				ToolTipTimer.Stop ();
				ToolTipWindow.Hide (this);
			}

			internal override void OnPaintInternal (PaintEventArgs e)
			{
				base.OnPaintInternal (e);

				if ((this.Focused || this.Entered || border == BorderStyle.FixedSingle) && border != BorderStyle.None) {
					ToolStripRenderer tsr = (this.Parent as ToolStrip).Renderer;

					if (tsr is ToolStripProfessionalRenderer)
						using (Pen p = new Pen ((tsr as ToolStripProfessionalRenderer).ColorTable.ButtonSelectedBorder))
							e.Graphics.DrawRectangle (p, new Rectangle (0, 0, this.Width - 1, this.Height - 1));
				}
			}
			
			internal BorderStyle Border {
				set {
					border = value;
					Invalidate ();
				}
			}

			internal ToolStripItem OwnerItem {
				set { owner_item = value; }
			}
					
			#region Stuff for ToolTips
			private bool ShowToolTips {
				get {
					if (Parent == null)
						return false;
						
					return (Parent as ToolStrip).ShowItemToolTips;
				}
			}

			private Timer ToolTipTimer {
				get {
					if (tooltip_timer == null) {
						tooltip_timer = new Timer ();
						tooltip_timer.Enabled = false;
						tooltip_timer.Interval = 500;
						tooltip_timer.Tick += new EventHandler (ToolTipTimer_Tick);
					}

					return tooltip_timer;
				}
			}

			private ToolTip ToolTipWindow {
				get {
					if (tooltip_window == null)
						tooltip_window = new ToolTip ();

					return tooltip_window;
				}
			}

			private void ToolTipTimer_Tick (object o, EventArgs args)
			{
				string tooltip = owner_item.GetToolTip ();

				if (!string.IsNullOrEmpty (tooltip))
					ToolTipWindow.Present (this, tooltip);

				ToolTipTimer.Stop ();
			}
			#endregion
		}
	}
}
