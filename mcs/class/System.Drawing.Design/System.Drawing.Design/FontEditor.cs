//
// System.Drawing.Design.FontEditor.cs
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
using System.ComponentModel;using System.Windows.Forms;
namespace System.Drawing.Design
{
	public class FontEditor : UITypeEditor
	{

		private FontDialog fontEdit;

		public FontEditor()
		{
		}

		public override object EditValue (ITypeDescriptorContext context,
			IServiceProvider provider, object value)
		{
			fontEdit = new FontDialog ();
			if (value is Font)
				fontEdit.Font = (Font) value;
			else
				// Set default
				fontEdit.Font = new Drawing.Font (FontFamily.GenericSansSerif, 12);

			fontEdit.FontMustExist = true;
			DialogResult result = fontEdit.ShowDialog();

			if (result == DialogResult.OK)
				return fontEdit.Font;
			else
				return value;
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}
	}
}
