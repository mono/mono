//
// System.Drawing.Design.ColorEditor.cs
// 
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// 

using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace System.Drawing.Design
{
	public class ColorEditor : UITypeEditor
	{
		private ColorDialog colorEdit;

		public ColorEditor()
		{
		}

		public override object EditValue (ITypeDescriptorContext context,
			IServiceProvider provider, object value)
		{
			// TODO MS.Net is using a in place color editor. We just use the modal
			// Windows.Forms.ColorDialog to keep things simple for now
			// especially as Windows.Forms are not fully implemented right now

			colorEdit = new ColorDialog ();

			if (value is Color)
				colorEdit.Color = (Color) value;
			else
				// Set default
				colorEdit.Color = Color.White; // TODO set which color as default?

			colorEdit.FullOpen = true;
			DialogResult result = colorEdit.ShowDialog();

			if (result == DialogResult.OK)
				return colorEdit.Color;
			else
				return value;
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;

			// TODO UITypeEditorEditStyle.DropDown is returned by the MS.Net library
			// see EditValue why we use a modal window for now
			// return UITypeEditorEditStyle.DropDown;
		}

		public override bool GetPaintValueSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		public override void PaintValue (PaintValueEventArgs e)
		{
			Graphics G = e.Graphics;

			if (e.Value != null)
			{
				Color C = (Color) e.Value;
				G.FillRectangle (new SolidBrush (C), e.Bounds);
			}

			G.DrawRectangle (Pens.Black, e.Bounds);
		}
	}
}
