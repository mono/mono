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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Reflection;

namespace System.Windows.Forms {
	[DefaultPropertyAttribute("Document")]
	public class PrintPreviewControl : Control {
		#region Local variables
		bool autozoom;
		int columns;
		int rows;
		int startPage;
		double zoom;
		PrintDocument document;
		internal PreviewPrintController controller;
		internal PreviewPageInfo[] page_infos;
		#endregion // Local variables

		#region Public Constructors
		public PrintPreviewControl() {
			autozoom = true;
			columns = 1;
			rows = 0;
			startPage = 1;

			controller = new PreviewPrintController ();

			this.BackColor = SystemColors.AppWorkspace;
		}
		#endregion // Public Constructors

		
		#region Public Instance Properties
		[DefaultValue(true)]
		public bool AutoZoom {
			get { return autozoom; }
			set {
				if (autozoom != value)
					Invalidate ();
				autozoom = value;
			}
		}
		[DefaultValue(1)]
		public int Columns {
			get { return columns; }
			set {
				if (columns != value)
					Invalidate ();
				columns = value;
			}
		}
		[DefaultValue(null)]
		public PrintDocument Document {
			get { return document; }
			set {
				document = value;
				document.PrintController = new PrintControllerWithStatusDialog (controller);
				InvalidatePreview ();
			}
		}
		[DefaultValue(1)]
		public int Rows {
			get { return rows; }
			set {
				if (rows != value)
					Invalidate ();
				rows = value;
			}
		}
		[DefaultValue(0)]
		public int StartPage {
			get { return startPage; }
			set {
				if (value < 1)
					return;
				if (document != null && value + (Rows + 1) * Columns > page_infos.Length + 1) {
					value = page_infos.Length + 1 - (Rows + 1) * Columns;
					if (value < 1)
						value = 1;
				}

				if (startPage != value)
					Invalidate ();
				startPage = value;
				OnStartPageChanged (EventArgs.Empty);
			}
		}

		[Bindable(false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		[DefaultValue(false)]
		public bool UseAntiAlias {
			get { return controller.UseAntiAlias; }
			set { controller.UseAntiAlias = value; }
		}

		public double Zoom {
			get { return zoom; }
			set {
				if (zoom < 0.0)
					throw new ArgumentException ("zoom");
				if (zoom != value)
					Invalidate ();
				zoom = value;
			}
		}
		#endregion // Public Instance Properties

		
		#region Public Instance Methods
		public void InvalidatePreview()
		{
			document.Print ();
			page_infos = controller.GetPreviewPageInfo ();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void ResetBackColor()
		{
			base.ResetBackColor();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void ResetForeColor()
		{
			base.ResetForeColor ();
		}
		#endregion // Public Instance Methods

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		#endregion // Protected Instance Methods

		#region Protected Instance Properties

		protected override void OnPaint(PaintEventArgs e)
		{
			Console.WriteLine ("OnPaint");
			ThemeEngine.Current.PrintPreviewControlPaint (e, this);
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize (e);
			Console.WriteLine ("OnResize");
			Invalidate ();
		}

		protected virtual void OnStartPageChanged(EventArgs e)
		{
			if (StartPageChanged != null)
				StartPageChanged(this, e);
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc (ref m);
		}

		#endregion // Protected Instance Methods

		public event EventHandler StartPageChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
	}
}
