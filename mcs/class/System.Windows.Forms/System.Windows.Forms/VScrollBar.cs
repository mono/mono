//
// System.Windows.Forms.StructFormat.cs
//
// Author:
//   John Rebbeck <john@rebent.com>
//   Dennis Hayes (dennish@Raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Drawing;

namespace System.Windows.Forms {

	public class VScrollBar : ScrollBar {

		private RightToLeft rightToLeft;
		// --- Properties ---
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				createParams.Caption = "";
				createParams.ClassName = "VSCROLL";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
  				
				//if (parent != null)
				//	createParams.Parent = parent.Handle;
				//else 
				createParams.Parent = (IntPtr) 0;
	  
				createParams.Style = (int) WindowStyles.WS_OVERLAPPEDWINDOW;
	  
				return createParams;
			}
		}

		[MonoTODO]
		protected override Size DefaultSize {
			get {
				//Set to Microsoft Default
				return new Size(16,80);
			}
		}

		[MonoTODO]
		public override RightToLeft RightToLeft {

			get {
				return rightToLeft;
			}
			set {
				rightToLeft = value;
				//FixMe: invalidate to force redraw.
				//Invalidate();
			}
		}

		// --- Constructor ---
		[MonoTODO]
		public VScrollBar()
		{
			rightToLeft = RightToLeft.Inherit;
		}
	}
}
