//
// System.Windows.Forms.PrintPreviewControl
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;
using System.Drawing.Printing;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// The raw "preview" part of print previewing, without any dialogs or buttons. Most PrintPreviewControl objects are found on PrintPreviewDialog objects, but they do not have to be.
	///
	/// ToDo note:
	///  - nothing is implemented
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
			throw new NotImplementedException ();
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
			get { throw new NotImplementedException (); }
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
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
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
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnPaint(PaintEventArgs pevent) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnResize(EventArgs eventargs) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnStartPageChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void ResetBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void ResetForeColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) 
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		
		
		
		#region Events
		[MonoTODO]
		public event EventHandler StartPageChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		#endregion
	}
}
