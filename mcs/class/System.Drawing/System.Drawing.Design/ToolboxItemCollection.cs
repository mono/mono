//
// System.Drawing.Design.ToolboxItemCollection
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Collections;

namespace System.Drawing.Design
{
	public sealed class ToolboxItemCollection : ReadOnlyCollectionBase
	{
		[MonoTODO]
		public ToolboxItemCollection (ToolboxItem[] value)
		{
		}

		[MonoTODO]
		public ToolboxItemCollection (ToolboxItemCollection value)
		{
		}

		public ToolboxItem this [int index] {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public bool Contains (ToolboxItem value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void CopyTo (ToolboxItem[] array, int index)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public int IndexOf (ToolboxItem value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~ToolboxItemCollection()
		{
		}
	}
}
