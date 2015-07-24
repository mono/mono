//
// ToolStripComboBox.cs
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

namespace System.Windows.Forms
{
	[DefaultProperty ("Items")]
	[ToolStripItemDesignerAvailability (ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.MenuStrip | ToolStripItemDesignerAvailability.ContextMenuStrip)]
	public class ToolStripComboBox : ToolStripControlHost
	{
		#region Public Constructors
		public ToolStripComboBox () : base (new ToolStripComboBoxControl ())
		{
			// The default size of a new ToolStripComboBox doesn't seem
			// to be DefaultSize.
			Size = new Size (121, 21);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public ToolStripComboBox (Control c) : base (c)
		{
			throw new NotSupportedException ();
		}

		public ToolStripComboBox (string name) : this ()
		{
			base.Name = name;
		}
		#endregion

		#region Public Properties
		[Browsable (true)]
		[Localizable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Editor ("System.Windows.Forms.Design.ListControlStringCollectionEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public AutoCompleteStringCollection AutoCompleteCustomSource {
			get { return ComboBox.AutoCompleteCustomSource; }
			set { ComboBox.AutoCompleteCustomSource = value; }
		}

		[Browsable (true)]
		[DefaultValue (AutoCompleteMode.None)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public AutoCompleteMode AutoCompleteMode {
			get { return ComboBox.AutoCompleteMode; }
			set { ComboBox.AutoCompleteMode = value; }
		}

		[Browsable (true)]
		[DefaultValue (AutoCompleteSource.None)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public AutoCompleteSource AutoCompleteSource {
			get { return ComboBox.AutoCompleteSource; }
			set { ComboBox.AutoCompleteSource = value; }
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
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ComboBox ComboBox {
			get { return (ComboBox)base.Control; }
		}

		[Browsable (true)]
		[DefaultValue (106)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public int DropDownHeight {
			get { return this.ComboBox.DropDownHeight; }
			set { this.ComboBox.DropDownHeight = value; }
		}
		
		[DefaultValue (ComboBoxStyle.DropDown)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public ComboBoxStyle DropDownStyle {
			get { return this.ComboBox.DropDownStyle; }
			set { this.ComboBox.DropDownStyle = value; }
		}

		public int DropDownWidth {
			get { return this.ComboBox.DropDownWidth; }
			set { this.ComboBox.DropDownWidth = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool DroppedDown {
			get { return this.ComboBox.DroppedDown; }
			set { this.ComboBox.DroppedDown = value; }
		}

		[LocalizableAttribute (true)]
		[DefaultValue (FlatStyle.Popup)]
		public FlatStyle FlatStyle {
			get { return ComboBox.FlatStyle; }
			set { ComboBox.FlatStyle = value; }
		}

		[Localizable (true)]
		[DefaultValue (true)]
		public bool IntegralHeight {
			get { return this.ComboBox.IntegralHeight; }
			set { this.ComboBox.IntegralHeight = value; }
		}

		[Localizable (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Editor ("System.Windows.Forms.Design.ListControlStringCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public ComboBox.ObjectCollection Items {
			get { return this.ComboBox.Items; }
		}

		[Localizable (true)]
		[DefaultValue (8)]
		public int MaxDropDownItems {
			get { return this.ComboBox.MaxDropDownItems; }
			set { this.ComboBox.MaxDropDownItems = value; }
		}

		[Localizable (true)]
		[DefaultValue (0)]
		public int MaxLength {
			get { return this.ComboBox.MaxLength; }
			set { this.ComboBox.MaxLength = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int SelectedIndex {
			get { return this.ComboBox.SelectedIndex; }
			set {
				this.ComboBox.SelectedIndex = value;
				
				if (this.ComboBox.SelectedIndex >= 0)
					Text = Items [value].ToString ();
			}
		}

		[Bindable (true)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Object SelectedItem {
			get { return this.ComboBox.SelectedItem; }
			set { this.ComboBox.SelectedItem = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string SelectedText {
			get { return this.ComboBox.SelectedText; }
			set { this.ComboBox.SelectedText = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int SelectionLength {
			get { return this.ComboBox.SelectionLength; }
			set { this.ComboBox.SelectionLength = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int SelectionStart {
			get { return this.ComboBox.SelectionStart; }
			set { this.ComboBox.SelectionStart = value; }
		}

		[DefaultValue (false)]
		public bool Sorted {
			get { return this.ComboBox.Sorted; }
			set { this.ComboBox.Sorted = value; }
		}
		#endregion

		#region Protected Properties
		protected internal override Padding DefaultMargin { get { return new Padding (1, 0, 1, 0); } }
		protected override Size DefaultSize { get { return new Size (100, 22); } }
		#endregion

		#region Public Methods
		public void BeginUpdate ()
		{
			this.ComboBox.BeginUpdate ();
		}

		public void EndUpdate ()
		{
			this.ComboBox.EndUpdate ();
		}

		public int FindString (string s)
		{
			return this.ComboBox.FindString (s);
		}

		public int FindString (string s, int startIndex)
		{
		 	return this.ComboBox.FindString (s, startIndex);
		}

		public int FindStringExact (string s)
		{
			return this.ComboBox.FindStringExact (s);
		}

		public int FindStringExact (string s, int startIndex)
		{
			return this.ComboBox.FindStringExact (s, startIndex);
		}

		public int GetItemHeight (int index)
		{
			return this.ComboBox.GetItemHeight (index);
		}

		public override Size GetPreferredSize (Size constrainingSize)
		{
			return base.GetPreferredSize (constrainingSize);
		}

		public void Select (int start, int length)
		{
			this.ComboBox.Select (start, length);
		}

		public void SelectAll ()
		{
			this.ComboBox.SelectAll ();
		}

		public override string ToString ()
		{
			return this.ComboBox.ToString ();
		}
		#endregion

		#region Protected Methods
		protected virtual void OnDropDown (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DropDownEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDropDownClosed (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DropDownClosedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDropDownStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DropDownStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [SelectedIndexChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnSelectionChangeCommitted (EventArgs e)
		{
		}

		protected override void OnSubscribeControlEvents (Control control)
		{
			base.OnSubscribeControlEvents (control);

			this.ComboBox.DropDown += new EventHandler (HandleDropDown);
			this.ComboBox.DropDownClosed += new EventHandler(HandleDropDownClosed);
			this.ComboBox.DropDownStyleChanged += new EventHandler (HandleDropDownStyleChanged);
			this.ComboBox.SelectedIndexChanged += new EventHandler (HandleSelectedIndexChanged);
			this.ComboBox.TextChanged += new EventHandler (HandleTextChanged);
			this.ComboBox.TextUpdate += new EventHandler (HandleTextUpdate);
		}

		protected virtual void OnTextUpdate (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [TextUpdateEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnUnsubscribeControlEvents (Control control)
		{
			base.OnUnsubscribeControlEvents (control);
		}
		#endregion

		#region Public Events
		static object DropDownEvent = new object ();
		static object DropDownClosedEvent = new object ();
		static object DropDownStyleChangedEvent = new object ();
		static object SelectedIndexChangedEvent = new object ();
		static object TextUpdateEvent = new object ();

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick {
			add { base.DoubleClick += value; }
			remove { base.DoubleClick -= value; }
		}

		public event EventHandler DropDown {
			add { Events.AddHandler (DropDownEvent, value); }
			remove { Events.RemoveHandler (DropDownEvent, value); }
		}

		public event EventHandler DropDownClosed {
			add { Events.AddHandler (DropDownClosedEvent, value); }
			remove { Events.RemoveHandler (DropDownClosedEvent, value); }
		}

		public event EventHandler DropDownStyleChanged {
			add { Events.AddHandler (DropDownStyleChangedEvent, value); }
			remove { Events.RemoveHandler (DropDownStyleChangedEvent, value); }
		}

		public event EventHandler SelectedIndexChanged {
			add { Events.AddHandler (SelectedIndexChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangedEvent, value); }
		}

		public event EventHandler TextUpdate {
			add { Events.AddHandler (TextUpdateEvent, value); }
			remove { Events.RemoveHandler (TextUpdateEvent, value); }
		}
		#endregion

		#region Private Methods
		private void HandleDropDown (object sender, EventArgs e)
		{
			OnDropDown (e);
		}

		private void HandleDropDownClosed (object sender, EventArgs e)
		{
			OnDropDownClosed (e);
		}
		
		private void HandleDropDownStyleChanged (object sender, EventArgs e)
		{
			OnDropDownStyleChanged (e);
		}

		private void HandleSelectedIndexChanged (object sender, EventArgs e)
		{
			OnSelectedIndexChanged (e);
		}

		private void HandleTextChanged (object sender, EventArgs e)
		{
			OnTextChanged (e);
		}
		
		private void HandleTextUpdate (object sender, EventArgs e)
		{
			OnTextUpdate (e);
		}
		#endregion

		private class ToolStripComboBoxControl : ComboBox
		{
			public ToolStripComboBoxControl () : base ()
			{
				this.border_style = BorderStyle.None;
				this.FlatStyle = FlatStyle.Popup;
			}
		}
	}
}
