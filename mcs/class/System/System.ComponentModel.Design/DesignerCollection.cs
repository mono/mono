//
// System.ComponentModel.Design.DesignerCollection
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Collections;

namespace System.ComponentModel.Design
{
	public class DesignerCollection : ICollection, IEnumerable
	{
		[MonoTODO]
		public DesignerCollection (IDesignerHost[] designers)
		{
		}

		[MonoTODO]
		public DesignerCollection (IList designers)
		{
		}

		public int Count {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public virtual IDesignerHost this [int index] {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public bool IsSynchronized {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public object SyncRoot {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public void CopyTo (Array array, int index)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~DesignerCollection()
		{
		}

	}
}
