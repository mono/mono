//
// System.Drawing.Design.IconEditor
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.ComponentModel;
using System.IO;

namespace System.Drawing.Design
{
	public class IconEditor : UITypeEditor
	{
		[MonoTODO]
		public IconEditor()
		{
		}

		[MonoTODO]
		protected static string CreateExtensionsString (string[] extensions,
								string sep)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected static string CreateFilterEntry (IconEditor e)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override object EditValue (ITypeDescriptorContext context,
						  IServiceProvider provider,
						  object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override UITypeEditorEditStyle GetEditStyle (
				ITypeDescriptorContext context)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual string[] GetExtensions()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual string GetFileDialogDescription()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override bool GetPaintValueSupported (
				     ITypeDescriptorContext context)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual Icon LoadFromStream (Stream stream)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void PaintValue (PaintValueEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
