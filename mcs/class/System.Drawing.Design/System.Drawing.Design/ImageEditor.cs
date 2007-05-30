//
// System.Drawing.Design.ImageEditor.cs
// 
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

using System.Text;
using System.IO;
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
			openDialog.Title = Locale.GetText ("Open image file");
			openDialog.CheckFileExists = true;
			openDialog.CheckPathExists = true;
			openDialog.Filter = CreateFilterEntry (this);
			openDialog.Multiselect = false;

			// Show the dialog
			DialogResult result = openDialog.ShowDialog();

			// Check the result and create a new image from the file
			if (result == DialogResult.OK) {
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

			if (e.Value != null) {
				Image I = (Image) e.Value;
				G.DrawImage (I, e.Bounds);
			}

			G.DrawRectangle (Pens.Black, e.Bounds);
		}

		protected static string CreateExtensionsString (string[] extensions, string sep)
		{
			if (extensions.Length > 0) {
				StringBuilder sb = new StringBuilder();

				sb.Append (extensions[0]);
				for (int x = 1; x < extensions.Length - 1; x++) {
					sb.Append (sep);
					sb.Append (extensions[x]);
				}
				return sb.ToString();
			}
			else {
				return string.Empty;
			}
		}

		protected static string CreateFilterEntry (ImageEditor e)
		{
			StringBuilder sb = new StringBuilder();
			string ExtStr = CreateExtensionsString (e.GetExtensions(), ";");

			sb.Append (e.GetFileDialogDescription());
			sb.Append (" (" + ExtStr + ")" + "|");
			sb.Append (ExtStr);
			return sb.ToString();
		}

		protected virtual string[] GetExtensions()
		{
			return new string[] {"*.bmp", "*.gif", "*.jpg", "*.jpeg", "*.png", "*.ico", "*.emf", "*.wmf"};
		}

		protected virtual string GetFileDialogDescription()
		{
			return Locale.GetText ("All image files");
		}

		protected virtual Image LoadFromStream (Stream stream)
		{
			return new Bitmap (stream);
		}
#if NET_2_0
		[MonoTODO]
		protected virtual Type[] GetImageExtenders ()
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
