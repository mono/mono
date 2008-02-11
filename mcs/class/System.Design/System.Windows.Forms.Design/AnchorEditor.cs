//
// System.Windows.Forms.Design.AnchorEditor.cs
//
//
// For creating type editors see the *great* tutorial:
//  "Walkthrough: implement a UI Type Editor"
//
// Author:
//   Dennis Hayes 
//   Miguel de Icaza (miguel@novell.com)
// (C) 2006 Novell, Inc.  http://www.ximian.com
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

using System;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace System.Windows.Forms.Design
{
	public sealed class AnchorEditor : UITypeEditor
	{
		#region Public Instance Constructors

		public AnchorEditor()
		{
		}

		#endregion Public Instance Constructors

		#region Override implementation of UITypeEditor

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, Object value)
		{
			IWindowsFormsEditorService editor_service = null;
				
			if (provider != null){
				editor_service = provider.GetService (typeof (IWindowsFormsEditorService))
					as IWindowsFormsEditorService;
			}

			if (editor_service != null){
				AnchorSelector anchor_selector = new AnchorSelector (editor_service, (AnchorStyles) value);
				editor_service.DropDownControl (anchor_selector);

				value = anchor_selector.AnchorStyles;
			}

			return value;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		#endregion Override implementation of UITypeEditor
	}

	internal class AnchorSelector : UserControl
	{
	        /// <summary> 
	        /// Required designer variable.
	        /// </summary>
	        private System.ComponentModel.IContainer components = null;
	
	        protected override void Dispose(bool disposing)
	        {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
	        }
	
	        private void InitializeComponent()
	        {
	            this.SuspendLayout();
	            this.Name = "AnchorSelector";
	            this.Size = new System.Drawing.Size (150, 120);
	            this.ResumeLayout(false);
	        }

		public AnchorStyles AnchorStyles {
			get {
				return styles;
			}
		}
		
	        AnchorStyles styles;
	
	        public AnchorSelector (IWindowsFormsEditorService editor_service, AnchorStyles startup)
	        {
			styles = startup;
			
			InitializeComponent();
			BackColor = Color.White;
	        }
	
	        void PaintState(Graphics g, int x1, int y1, int x2, int y2, AnchorStyles v)
	        {
			if ((styles & v) != 0)
				g.DrawLine(SystemPens.MenuText, x1, y1, x2, y2);
			else {
				int xf = (x1 == x2) ? 10 : 0;
				int yf = (y1 == y2) ? 10 : 0;
				
				g.DrawBezier(SystemPens.MenuText,
					     new Point(x1, y1),
					     new Point((x1+x2)/2+xf, (y1+y2)/2-yf),
					     new Point((x1+x2)/2-xf, (y1+y2)/2+yf),
					     new Point(x2, y2));
			}
	        }
	
	        protected override void OnPaint (PaintEventArgs e)
	        {
			Graphics g = e.Graphics;
			int w3 = Width / 3;
			int h3 = Height / 3;
			int w2 = Width/2;
			int h2 = Height/2;
			
			g.FillRectangle(Brushes.Black, new Rectangle(w3, h3, w3, h3));
			
			PaintState (g, 0, h2, w3, h2, AnchorStyles.Left);
			PaintState (g, w3 * 2, h2, Width, h2, AnchorStyles.Right);
			PaintState (g, w2, 0, w2, h3, AnchorStyles.Top);
			PaintState (g, w2, h3 * 2, w2, Height, AnchorStyles.Bottom);
	        }
		
	        protected override void OnClick (EventArgs ee)
	        {
			Point e = PointToClient (MousePosition);
			int w3 = Width / 3;
			int h3 = Height / 3;
			
			if (e.X <= w3 && e.Y > h3 && e.Y < h3 * 2)
				styles = styles ^ AnchorStyles.Left;
			else if (e.Y < h3 && e.X > w3 && e.X < w3 * 2)
				styles = styles ^ AnchorStyles.Top;
			else if (e.X > w3 * 2 && e.Y > h3 && e.Y < h3 * 2)
				styles = styles ^ AnchorStyles.Right;
			else if (e.Y > h3 * 2 && e.X > w3 && e.X < w3 * 2)
				styles = styles ^ AnchorStyles.Bottom;
			else
				base.OnClick(ee);
			Invalidate();
	        }

	        protected override void OnDoubleClick (EventArgs ee)
	        {
			OnClick (ee);
		}

	}
	
}
