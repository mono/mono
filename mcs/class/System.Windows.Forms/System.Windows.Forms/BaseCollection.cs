//
// System.Windows.Forms.BaseCollection
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis hayes (dennish@raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System;
using System.Collections;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides the base functionality for creating data-related collections in the System.Windows.Forms namespace.
	/// ToDo note:
	///  - Synchronization is not implemented
	///  - MarshalByRefObject members not stubbed out
	/// </summary>
	
	[MonoTODO]
	public class BaseCollection : MarshalByRefObject, ICollection, IEnumerable {

		ArrayList list;
		
		
		// --- Constructor ---
		public BaseCollection()
		{
			this.list = null;
		}



		// --- public and protected Properties ---
//		public virtual int ICollection.Count {
		public virtual int Count {
			get {
				return list.Count; 
			}
		}
		
		public bool IsReadOnly {
			//always false as per spec.
			get { return false; }
		}
		
		[MonoTODO]
		public bool IsSynchronized {
			//always false as per spec.
			get { return false; }
		}
		
		protected virtual ArrayList List {
			get { 
				return list; 
			}
		}
		
		[MonoTODO]
		public object SyncRoot {
			get { return this; }
		}
		
		// --- public Methods ---
		public void CopyTo (Array ar, int index) 
		{
			list.CopyTo(ar, index);
		}
		
		public IEnumerator GetEnumerator() 
		{
			return list.GetEnumerator();
		}
		
	}
}
