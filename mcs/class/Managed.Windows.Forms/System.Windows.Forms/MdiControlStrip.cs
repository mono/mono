//
// MdiControlStrip.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.Layout;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;

namespace System.Windows.Forms
{
	internal class MdiControlStrip
	{
		public class SystemMenuItem : ToolStripMenuItem
		{
			private Form form;
			
			public SystemMenuItem (Form ownerForm)
			{
				form = ownerForm;
				
				base.AutoSize = false;
				base.Size = new Size (20, 20);
				base.Image = ownerForm.Icon.ToBitmap ();
				base.MergeIndex = int.MinValue;
				base.DisplayStyle = ToolStripItemDisplayStyle.Image;

				DropDownItems.Add ("&Restore", null, RestoreItemHandler);
				ToolStripMenuItem tsiMove = (ToolStripMenuItem)DropDownItems.Add ("&Move");
				tsiMove.Enabled = false;
				ToolStripMenuItem tsiSize = (ToolStripMenuItem)DropDownItems.Add ("&Size");
				tsiSize.Enabled = false;
				DropDownItems.Add ("Mi&nimize", null, MinimizeItemHandler);
				ToolStripMenuItem tsiMaximize = (ToolStripMenuItem)DropDownItems.Add ("Ma&ximize");
				tsiMaximize.Enabled = false;
				DropDownItems.Add ("-");
				ToolStripMenuItem tsiClose = (ToolStripMenuItem)DropDownItems.Add ("&Close", null, CloseItemHandler);
				tsiClose.ShortcutKeys = Keys.Control | Keys.F4;
				DropDownItems.Add ("-");
				ToolStripMenuItem tsiNext = (ToolStripMenuItem)DropDownItems.Add ("Nex&t", null, NextItemHandler);
				tsiNext.ShortcutKeys = Keys.Control | Keys.F6;
			}

			protected override void OnPaint (PaintEventArgs e)
			{
				// Can't render without an owner
				if (this.Owner == null)
					return;

				// If DropDown.ShowImageMargin is false, we don't display the image
				Image draw_image = this.Image;

				// Figure out where our text and image go
				Rectangle text_layout_rect;
				Rectangle image_layout_rect;

				this.CalculateTextAndImageRectangles (out text_layout_rect, out image_layout_rect);

				if (image_layout_rect != Rectangle.Empty)
					this.Owner.Renderer.DrawItemImage (new ToolStripItemImageRenderEventArgs (e.Graphics, this, draw_image, image_layout_rect));

				return;
			}

			public Form MdiForm {
				get { return form; }
				set { form = value; }
			}

			private void RestoreItemHandler (object sender, EventArgs e)
			{
				form.WindowState = FormWindowState.Normal;
			}

			private void MinimizeItemHandler (object sender, EventArgs e)
			{
				form.WindowState = FormWindowState.Minimized;
			}

			private void CloseItemHandler (object sender, EventArgs e)
			{
				form.Close ();
			}

			private void NextItemHandler (object sender, EventArgs e)
			{
				form.MdiParent.MdiContainer.ActivateNextChild ();
			}
		}
		
		public class ControlBoxMenuItem : ToolStripMenuItem
		{
			private Form form;
			private ControlBoxType type;
			
			public ControlBoxMenuItem (Form ownerForm, ControlBoxType type)
			{
				form = ownerForm;
				this.type = type;
				
				base.AutoSize = false;
				base.Alignment = ToolStripItemAlignment.Right;
				base.Size = new Size (20, 20);
				base.MergeIndex = int.MaxValue;
				base.DisplayStyle = ToolStripItemDisplayStyle.None;

				switch (type) {
					case ControlBoxType.Close:
						this.Click += new EventHandler(CloseItemHandler);
						break;
					case ControlBoxType.Min:
						this.Click += new EventHandler (MinimizeItemHandler);
						break;
					case ControlBoxType.Max:
						this.Click += new EventHandler (RestoreItemHandler);
						break;
				}
			}

			protected override void OnPaint (PaintEventArgs e)
			{
				base.OnPaint (e);
				Graphics g = e.Graphics;
				
				switch (type) {
					case ControlBoxType.Close:
						g.FillRectangle (Brushes.Black, 8, 8, 4, 4);
						g.FillRectangle (Brushes.Black, 6, 6, 2, 2);
						g.FillRectangle (Brushes.Black, 6, 12, 2, 2);
						g.FillRectangle (Brushes.Black, 12, 6, 2, 2);
						g.FillRectangle (Brushes.Black, 12, 12, 2, 2);
						g.DrawLine (Pens.Black, 8, 7, 8, 12);
						g.DrawLine (Pens.Black, 7, 8, 12, 8);
						g.DrawLine (Pens.Black, 11, 7, 11, 12);
						g.DrawLine (Pens.Black, 7, 11, 12, 11);
						break;
					case ControlBoxType.Min:
						g.DrawLine (Pens.Black, 6, 12, 11, 12);
						g.DrawLine (Pens.Black, 6, 13, 11, 13);
						break;
					case ControlBoxType.Max:
						g.DrawLines (Pens.Black, new Point[] {new Point (7, 8), new Point (7, 5), new Point (13, 5), new Point (13, 10), new Point (11, 10)});
						g.DrawLine (Pens.Black, 7, 6, 12, 6);
						
						g.DrawRectangle (Pens.Black, new Rectangle (5, 8, 6, 5));
						g.DrawLine (Pens.Black, 5, 9, 11, 9);
						
						break;
				}
			}

			public Form MdiForm {
				get { return form; }
				set { form = value; }
			}
			
			private void RestoreItemHandler (object sender, EventArgs e)
			{
				form.WindowState = FormWindowState.Normal;
			}

			private void MinimizeItemHandler (object sender, EventArgs e)
			{
				form.WindowState = FormWindowState.Minimized;
			}

			private void CloseItemHandler (object sender, EventArgs e)
			{
				form.Close ();
			}
		}
		
		public enum ControlBoxType
		{
			Close,
			Min,
			Max
		}
	}
}
