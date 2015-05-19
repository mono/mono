//
// ToolStripProgressBar.cs
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

namespace System.Windows.Forms
{
	[DefaultProperty ("Value")]
	public class ToolStripProgressBar : ToolStripControlHost
	{
		#region Public Constructors
		public ToolStripProgressBar () : base (new ProgressBar ())
		{
		}

		public ToolStripProgressBar (string name) : this ()
		{
			this.Name = name;
		}
		#endregion

		#region Public Properties
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
		
		[DefaultValue (100)]
		public int MarqueeAnimationSpeed {
			get { return this.ProgressBar.MarqueeAnimationSpeed; }
			set { this.ProgressBar.MarqueeAnimationSpeed = value; }
		}
		
		[DefaultValue (100)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public int Maximum {
			get { return this.ProgressBar.Maximum; }
			set { this.ProgressBar.Maximum = value; }
		}

		[DefaultValue (0)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public int Minimum {
			get { return this.ProgressBar.Minimum; }
			set { this.ProgressBar.Minimum = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ProgressBar ProgressBar {
			get { return (ProgressBar)base.Control; }
		}

		[Localizable (true)]
		[DefaultValue (false)]
		public virtual bool RightToLeftLayout {
			get { return this.ProgressBar.RightToLeftLayout; }
			set { this.ProgressBar.RightToLeftLayout = value; }
		}
		
		[DefaultValue (10)]
		public int Step {
			get { return this.ProgressBar.Step; }
			set { this.ProgressBar.Step = value; }
		}

		[DefaultValue (ProgressBarStyle.Blocks)]
		public ProgressBarStyle Style {
			get { return this.ProgressBar.Style; }
			set { this.ProgressBar.Style = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Bindable (true)]
		[DefaultValue (0)]
		public int Value {
			get { return this.ProgressBar.Value; }
			set { this.ProgressBar.Value = value; }
		}
		#endregion

		#region Protected Properties
		protected internal override Padding DefaultMargin { get { return new Padding (1, 2, 1, 1); } }
		protected override Size DefaultSize { get { return new Size (100, 15); } }
		#endregion

		#region Public Methods
		public void Increment (int value)
		{
			this.ProgressBar.Increment (value);
		}

		public void PerformStep ()
		{
			this.ProgressBar.PerformStep ();
		}
		#endregion

		#region Protected Methods
		protected virtual void OnRightToLeftLayoutChanged (EventArgs e)
		{
		}
		
		protected override void OnSubscribeControlEvents (Control control)
		{
			base.OnSubscribeControlEvents (control);
		}

		protected override void OnUnsubscribeControlEvents (Control control)
		{
			base.OnUnsubscribeControlEvents (control);
		}
		#endregion

		#region Public Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyDown {
			add { base.KeyDown += value; }
			remove { base.KeyDown -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyPressEventHandler KeyPress {
			add { base.KeyPress += value; }
			remove { base.KeyPress -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyUp {
			add { base.KeyUp += value; }
			remove { base.KeyUp -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler LocationChanged {
			add { base.LocationChanged += value; }
			remove { base.LocationChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler OwnerChanged {
			add { base.OwnerChanged += value; }
			remove { base.OwnerChanged -= value; }
		}

		public event EventHandler RightToLeftLayoutChanged {
			add { ProgressBar.RightToLeftLayoutChanged += value; }
			remove { ProgressBar.RightToLeftLayoutChanged -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Validated {
			add { base.Validated += value; }
			remove { base.Validated -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event CancelEventHandler Validating {
			add { base.Validating += value; }
			remove { base.Validating -= value; }
		}
		#endregion
	}
}
