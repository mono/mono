//
// System.Windows.Forms.PrintPreviewControl
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

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

using System.Drawing;
using System.Drawing.Printing;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// The raw "preview" part of print previewing, without any dialogs or buttons. Most PrintPreviewControl objects are found on PrintPreviewDialog objects, but they do not have to be.
	///
	/// </summary>

	[MonoTODO]
	public class PrintPreviewControl : Control {

		#region Fields
		bool autoZoom;
		int columns;
		PrintDocument document;
		int rows;
		int startPage;
		bool useAntiAlias;
		double zoom;
		#endregion
		
		#region Constructors
		[MonoTODO]
		public PrintPreviewControl() 
		{
			autoZoom=true;
			columns=1;
			document=null;
			rows=0;
			startPage=0;
			zoom=1.0;
		}
		#endregion
		
		#region Properties
		public bool AutoZoom {
			get { return autoZoom; }
			set { autoZoom=value; }
		}
		
		public int Columns {
			get { return columns; }
			set { columns=value; }
		}

		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;

				createParams.ClassName = "PRINTPREVIEWCONTROL";
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE);

				return createParams;
			}		
		}

		public PrintDocument Document {
			get { return document; }
			set { document=value; }
		}
		
		public int Rows {
			get { return rows; }
			set { rows=value; }
		}
		
		public int StartPage {
			get { return startPage; }
			set { startPage=value; }
		}
		
		[MonoTODO]
		public override string Text {
			get { 
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}
		
		public bool UseAntiAlias {
			get { return useAntiAlias; }
			set { useAntiAlias=value; }
		}
		
		public double Zoom {
			get { return zoom; }
			set { zoom=value; }
		}
		#endregion
		
		#region Methods
		[MonoTODO]
		public void InvalidatePreview() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected override void OnPaint(PaintEventArgs pevent) 
		{
			//FIXME:
			base.OnPaint(pevent);
		}
		
		[MonoTODO]
		protected override void OnResize(EventArgs eventargs) 
		{
			//FIXME:
			base.OnResize(eventargs);
		}
		
		[MonoTODO]
		protected virtual void OnStartPageChanged(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public override void ResetBackColor() 
		{
			//FIXME:
			base.ResetBackColor();
		}
		
		[MonoTODO]
		public override void ResetForeColor() 
		{
			//FIXME:
			base.ResetForeColor();
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) 
		{
			//FIXME:
			base.WndProc(ref m);
		}
		#endregion
		
		#region Events
		[MonoTODO]
		public event EventHandler StartPageChanged;
		#endregion
	}
}
