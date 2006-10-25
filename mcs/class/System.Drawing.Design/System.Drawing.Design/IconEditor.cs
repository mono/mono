//
// System.Drawing.Design.IconEditor.cs
// 
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
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
using System.Text;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace System.Drawing.Design
{
	// LAMESPEC: Why isn't this class inherited from ImageEditor?
	public class IconEditor : UITypeEditor
	{

		private OpenFileDialog openDialog;

		public IconEditor()
		{
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

		protected static string CreateFilterEntry (IconEditor e)
		{
			StringBuilder sb = new StringBuilder();
			string ExtStr = CreateExtensionsString (e.GetExtensions(), ";");

			sb.Append (e.GetFileDialogDescription());
			sb.Append (" (" + ExtStr + ")" + "|");
			sb.Append (ExtStr);
			return sb.ToString();
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

		protected virtual string[] GetExtensions()
		{
			return new string[] {"*.ico"};
		}

		protected virtual string GetFileDialogDescription()
		{
			return Locale.GetText ("Icon files");
		}

		public override bool GetPaintValueSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		protected virtual Icon LoadFromStream (Stream stream)
		{
			return new Icon (stream);
		}

		public override void PaintValue (PaintValueEventArgs e)
		{
			Graphics G = e.Graphics;

			if (e.Value != null) {
				Icon i = (Icon) e.Value;
				Image img = i.ToBitmap ();
				G.DrawImage (img, e.Bounds);
				img.Dispose ();
			}

			G.DrawRectangle (Pens.Black, e.Bounds);
		}
	}
}
