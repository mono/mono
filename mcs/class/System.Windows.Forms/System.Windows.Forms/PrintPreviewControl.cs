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
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[DefaultPropertyAttribute("Document")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class PrintPreviewControl : Control {
		#region Local variables
		bool autozoom;
		int columns;
		int rows;
		int startPage;
		double zoom;
		int padding = ThemeEngine.Current.PrintPreviewControlPadding;
		PrintDocument document;
		internal PreviewPrintController controller;
		internal PreviewPageInfo[] page_infos;
		private VScrollBar vbar;
		private HScrollBar hbar;

		internal Rectangle ViewPort;
		internal Image[] image_cache;
		Size image_size;

		#endregion // Local variables

		#region Public Constructors
		public PrintPreviewControl() {
			autozoom = true;
			columns = 1;
			rows = 0;
			startPage = 1;

			this.BackColor = SystemColors.AppWorkspace;

			controller = new PreviewPrintController ();

			vbar = new ImplicitVScrollBar ();
			hbar = new ImplicitHScrollBar ();

			vbar.Visible = false;
			hbar.Visible = false;
			vbar.ValueChanged += new EventHandler (VScrollBarValueChanged);
			hbar.ValueChanged += new EventHandler (HScrollBarValueChanged);

			SuspendLayout ();
			Controls.AddImplicit (vbar);
			Controls.AddImplicit (hbar);
			ResumeLayout ();
		}
		#endregion // Public Constructors

		
		#region Public Instance Properties
		[DefaultValue(true)]
		public bool AutoZoom {
			get { return autozoom; }
			set {
				autozoom = value;
				InvalidateLayout ();
			}
		}
		[DefaultValue(1)]
		public int Columns {
			get { return columns; }
			set {
				columns = value;
				InvalidateLayout ();
			}
		}
		[DefaultValue(null)]
		public PrintDocument Document {
			get { return document; }
			set {
				document = value;
			}
		}

		[Localizable (true)]
		[AmbientValue (RightToLeft.Inherit)]
		public override RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set { base.RightToLeft = value; }
		}

		[DefaultValue(1)]
		public int Rows {
			get { return rows; }
			set {
				rows = value;
				InvalidateLayout ();
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

				int start = StartPage;
				startPage = value;
				if (start != startPage) {
					InvalidateLayout ();
					OnStartPageChanged (EventArgs.Empty);
				}
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

		[DefaultValue (0.3)]
		public double Zoom {
			get { return zoom; }
			set {
				if (value <= 0)
					throw new ArgumentException ("zoom");
				autozoom = false;
				zoom = value;
				InvalidateLayout ();				
			}
		}
		#endregion // Public Instance Properties

		
		#region Public Instance Methods
		internal void GeneratePreview ()
		{
			if (document == null)
				return;

			try {
				if (page_infos == null) {
					if (document.PrintController == null || !(document.PrintController is PrintControllerWithStatusDialog)) {
						document.PrintController = new PrintControllerWithStatusDialog (controller);
					}
					document.Print ();
					page_infos = controller.GetPreviewPageInfo ();
				}
				
				if (image_cache == null) {
					image_cache = new Image[page_infos.Length];

					if (page_infos.Length > 0) {
						image_size = ThemeEngine.Current.PrintPreviewControlGetPageSize (this);
						if (image_size.Width >= 0 && image_size.Width < page_infos[0].Image.Width
						    && image_size.Height >= 0 && image_size.Height < page_infos[0].Image.Height) {

							for (int i = 0; i < page_infos.Length; i ++) {
								image_cache[i] = new Bitmap (image_size.Width, image_size.Height);
								Graphics g = Graphics.FromImage (image_cache[i]);
								g.DrawImage (page_infos[i].Image, new Rectangle (new Point (0, 0), image_size), 0, 0, page_infos[i].Image.Width, page_infos[i].Image.Height, GraphicsUnit.Pixel);
								g.Dispose ();
							}
						}
					}
				}
				UpdateScrollBars();
			}
			catch (Exception e) {
				page_infos = new PreviewPageInfo[0];
				image_cache = new Image[0];
				MessageBox.Show (e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void InvalidatePreview()
		{
			if (page_infos != null) {
				for (int i = 0; i < page_infos.Length; i++) {
					if (page_infos[i].Image != null) {
						page_infos[i].Image.Dispose();
					}
				}
				page_infos = null;
			}
			InvalidateLayout();
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

		protected override void OnPaint(PaintEventArgs pevent)
		{
			if (page_infos == null || image_cache == null)
				GeneratePreview ();
			ThemeEngine.Current.PrintPreviewControlPaint (pevent, this, image_size);
		}

		protected override void OnResize(EventArgs eventargs)
		{
			InvalidateLayout ();
			base.OnResize (eventargs);
		}

		protected virtual void OnStartPageChanged(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [StartPageChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc (ref m);
		}

		#endregion // Protected Instance Methods

		static object StartPageChangedEvent = new object ();

		public event EventHandler StartPageChanged {
			add { Events.AddHandler (StartPageChangedEvent, value); }
			remove { Events.RemoveHandler (StartPageChangedEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		internal int vbar_value;
		internal int hbar_value;

		#region UIA Framework Property
		internal ScrollBar UIAVScrollBar {
			get { return vbar; }
		}

		internal ScrollBar UIAHScrollBar {
			get { return hbar; }
		}
		#endregion

		private void VScrollBarValueChanged (object sender, EventArgs e)
		{
			int pixels;

			if (vbar.Value > vbar_value)
				pixels = -1 * (vbar.Value - vbar_value);
			else
				pixels = vbar_value - vbar.Value;

			vbar_value = vbar.Value;
			XplatUI.ScrollWindow (Handle, ViewPort, 0, pixels, false);
		}


		private void HScrollBarValueChanged (object sender, EventArgs e)
		{
			int pixels;

			if (hbar.Value > hbar_value)
				pixels = -1 * (hbar.Value - hbar_value);
			else
				pixels = hbar_value - hbar.Value;

			hbar_value = hbar.Value;
			XplatUI.ScrollWindow (Handle, ViewPort, pixels, 0, false);
		}

		private void UpdateScrollBars ()
		{
			ViewPort = ClientRectangle;
			if (AutoZoom)
				return;

			int total_width, total_height;

			total_width = image_size.Width * Columns + (Columns + 1) * padding;
			total_height = image_size.Height * (Rows + 1) + (Rows + 2) * padding;

			bool vert = false;
			bool horz = false;

			if (total_width > ClientRectangle.Width) {
				/* we need the hbar */
				horz = true;
				ViewPort.Height -= hbar.Height;
			}
			if (total_height > ViewPort.Height) {
				/* we need the vbar */
				vert = true;
				ViewPort.Width -= vbar.Width;
			}
			if (!horz && total_width > ViewPort.Width) {
				horz = true;
				ViewPort.Height -= hbar.Height;
			}

			SuspendLayout ();

			if (vert) {
				vbar.SetValues (total_height, ViewPort.Height);

				vbar.Bounds = new Rectangle (ClientRectangle.Width - vbar.Width, 0, vbar.Width,
							     ClientRectangle.Height -
							     (horz ? SystemInformation.VerticalScrollBarWidth : 0));
				vbar.Visible = true;
				vbar_value = vbar.Value;
			}
			else {
				vbar.Visible = false;
			}

			if (horz) {
				hbar.SetValues (total_width, ViewPort.Width);

				hbar.Bounds = new Rectangle (0, ClientRectangle.Height - hbar.Height,
							     ClientRectangle.Width - (vert ?
										      SystemInformation.HorizontalScrollBarHeight : 0),
							     hbar.Height);
				hbar.Visible = true;
				hbar_value = hbar.Value;
			}
			else {
				hbar.Visible = false;
			}

			ResumeLayout (false);
		}

		private void InvalidateLayout() {
			if (image_cache != null) {
				for (int i = 0; i < image_cache.Length; i++) {
					if (image_cache[i] !=null)
						image_cache[i].Dispose();
				}
				image_cache = null;
			}
			Invalidate ();
		}
	}
}
