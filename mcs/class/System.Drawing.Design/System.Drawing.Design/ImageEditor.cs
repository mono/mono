//
// System.Drawing.Design.ImageEditor.cs
// 
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// 
using System;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace System.Drawing.Design
{
	public class ImageEditor : UITypeEditor
	{

		private OpenFileDialog openDialog;

		public ImageEditor()
		{
		}

		public override object EditValue (ITypeDescriptorContext context,
			IServiceProvider provider, object value)
		{
			openDialog = new OpenFileDialog();
			openDialog.Title = Locale.GetText("Open image file");
			openDialog.CheckFileExists = true;
			openDialog.CheckPathExists = true;
			openDialog.Filter = CreateFilterEntry (this);
			openDialog.Multiselect = false;

			// Show the dialog
			DialogResult result = openDialog.ShowDialog();

			// Check the result and create a new image from the file
			if (result == DialogResult.OK)
			{
				return LoadFromStream (openDialog.OpenFile());
			}
			else
				return value;	
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
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
				Image I = (Image) e.Value;
				G.DrawImage (I, e.Bounds);
			}
			G.DrawRectangle (Pens.Black, e.Bounds);
		}

		protected static string CreateExtensionsString (string[] extensions, string sep)
		{
			if (extensions.Length > 0)
			{
				string Ext = extensions[0];
				for (int x = 1; x < extensions.Length - 1; x++)
					Ext = string.Concat(Ext, sep, extensions[x]);
				return Ext;
			}
			else
			{
				return string.Empty;
			}
		}

		protected static string CreateFilterEntry (ImageEditor e)
		{
			string ExtStr = CreateExtensionsString (e.GetExtensions(), ";");
			string Desc = e.GetFileDialogDescription() + " (" + ExtStr + ")";
			return String.Concat (Desc, "|", ExtStr);
		}

		protected virtual string[] GetExtensions()
		{
			return new string[] {"*.bmp", "*.gif", "*.jpg", "*.jpeg", "*.png", "*.ico", "*.emf", "*.wmf"};
		}

		protected virtual string GetFileDialogDescription()
		{
			return Locale.GetText("All image files");
		}

		protected virtual Image LoadFromStream (Stream stream)
		{
			return new Bitmap (stream);
		}
	}
}
