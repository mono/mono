//
// System.ComponentModel.Design.DesignerCollection.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

using System.Collections;

namespace System.ComponentModel.Design
{
	public class DesignerCollection : ICollection, IEnumerable
	{

		private ArrayList designers;

		public DesignerCollection (IDesignerHost[] designers)
		{
			this.designers = new ArrayList (designers);
		}

		public DesignerCollection (IList designers)
		{
			this.designers = new ArrayList (designers);
		}

		public int Count {
			get { return designers.Count; }
		}

		public virtual IDesignerHost this [int index] {
			get { return (IDesignerHost) designers [index]; }
		}

		public IEnumerator GetEnumerator()
		{
			return designers.GetEnumerator ();
		}

		bool ICollection.IsSynchronized {
			get { return designers.IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return designers.SyncRoot; }
		}

		void ICollection.CopyTo (Array array, int index)
		{
			designers.CopyTo (array, index);
		}
	}
}
