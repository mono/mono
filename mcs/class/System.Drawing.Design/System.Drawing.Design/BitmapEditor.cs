//
// System.Drawing.Design
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.IO;

namespace System.Drawing.Design
{
	public class BitmapEditor : ImageEditor
	{
		[MonoTODO]
		public BitmapEditor()
		{
		}

		protected override string[] GetExtensions()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override string GetFileDialogDescription()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override Image LoadFromStream (Stream stream)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~BitmapEditor()
		{
		}
	}
}
