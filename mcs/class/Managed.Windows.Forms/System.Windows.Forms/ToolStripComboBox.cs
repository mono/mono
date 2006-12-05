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
#if NET_2_0
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms
{
	[DefaultProperty ("Items")]
	public class ToolStripComboBox : ToolStripControlHost
	{
		#region Public Constructors
		public ToolStripComboBox () : base (new ToolStripComboBoxControl ())
		{
		}

		public ToolStripComboBox (Control c) : base (c)
		{
			throw new NotSupportedException ();
		}

		public ToolStripComboBox (string name) : this ()
		{
			base.Control.Name = name;
		}
		#endregion

		#region Public Properties
		[Browsable (false)]
		public ComboBox ComboBox {
			get { return (ComboBox)base.Control; }
		}

		[MonoTODO ("Stub, will not actually affect anything.")]
		public int DropDownHeight {
			get { return 50; }
			set { }
		}
		
		[DefaultValue (ComboBoxStyle.DropDown)]
		public ComboBoxStyle DropDownStyle {
			get { return this.ComboBox.DropDownStyle; }
			set { this.ComboBox.DropDownStyle = value; }
		}

		public int DropDownWidth {
			get { return this.ComboBox.DropDownWidth; }
			set { this.ComboBox.DropDownWidth = value; }
		}

		[Browsable (false)]
		public bool DroppedDown {
			get { return this.ComboBox.DroppedDown; }
			set { this.ComboBox.DroppedDown = value; }
		}

		[Localizable (true)]
		[DefaultValue (true)]
		public bool IntegralHeight {
			get { return this.ComboBox.IntegralHeight; }
			set { this.ComboBox.IntegralHeight = value; }
		}

		[Localizable (true)]
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
		public int SelectedIndex {
			get { return this.ComboBox.SelectedIndex; }
			set { this.ComboBox.SelectedIndex = value; }
		}

		[Bindable (true)]
		[Browsable (false)]
		public Object SelectedItem {
			get { return this.ComboBox.SelectedItem; }
			set { this.ComboBox.SelectedItem = value; }
		}

		[Browsable (false)]
		public string SelectedText {
			get { return this.ComboBox.SelectedText; }
			set { this.ComboBox.SelectedText = value; }
		}

		[Browsable (false)]
		public int SelectionLength {
			get { return this.ComboBox.SelectionLength; }
			set { this.ComboBox.SelectionLength = value; }
		}

		[Browsable (false)]
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
		protected internal override Padding DefaultMargin { get { return new Padding (2); } }
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
			if (DropDown != null) DropDown (this, e);
		}

		protected virtual void OnDropDownClosed (EventArgs e)
		{
			if (DropDownClosed != null) DropDownClosed (this, e);
		}

		protected virtual void OnDropDownStyleChanged (EventArgs e)
		{
			if (DropDownStyleChanged != null) DropDownStyleChanged (this, e);
		}

		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			if (SelectedIndexChanged != null) SelectedIndexChanged (this, e);
		}

		protected virtual void OnSelectionChangeCommitted (EventArgs e)
		{
		}

		[MonoTODO ("Needs to hook into DropDownClosed and TextUpdate when ComboBox 2.0 has them.")]
		protected override void OnSubscribeControlEvents (Control control)
		{
			base.OnSubscribeControlEvents (control);

			this.ComboBox.DropDown += new EventHandler (HandleDropDown);
			this.ComboBox.DropDownStyleChanged += new EventHandler (HandleDropDownStyleChanged);
			this.ComboBox.SelectedIndexChanged += new EventHandler (HandleSelectedIndexChanged);
		}

		protected virtual void OnTextUpdate (EventArgs e)
		{
			if (TextUpdate != null) TextUpdate (this, e);
		}

		protected override void OnUnsubscribeControlEvents (Control control)
		{
			base.OnUnsubscribeControlEvents (control);
		}
		#endregion

		#region Public Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick;
		public event EventHandler DropDown;
		public event EventHandler DropDownClosed;
		public event EventHandler DropDownStyleChanged;
		public event EventHandler SelectedIndexChanged;
		public event EventHandler TextUpdate;
		#endregion

		#region Private Methods
		private void HandleDropDown (object sender, EventArgs e)
		{
			OnDropDown (e);
		}

		private void HandleDropDownStyleChanged (object sender, EventArgs e)
		{
			OnDisplayStyleChanged (e);
		}

		private void HandleSelectedIndexChanged (object sender, EventArgs e)
		{
			OnSelectedIndexChanged (e);
		}
		#endregion

		private class ToolStripComboBoxControl : ComboBox
		{
			public ToolStripComboBoxControl () : base ()
			{
				this.border_style = BorderStyle.None;
			}
		}
	}
}
#endif
