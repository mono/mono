//
// System.ComponentModel.Design.ArrayEditor
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design
{
	public class ArrayEditor : CollectionEditor
	{
		[MonoTODO]
		public ArrayEditor (Type type) : base (type)
		{
		}

		[MonoTODO]
		protected override Type CreateCollectionItemType()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override object[] GetItems (object editValue)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override object SetItems (object editValue, 
						    object[] value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~ArrayEditor ()
		{
		}
	}
}
