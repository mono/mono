//
// System.Windows.Forms.BaseCollection
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis Hayes (dennish@raytek.com)
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

//		ArrayList list;
//		
//		
//		// --- Constructor ---
//		public BaseCollection()
//		{
//			this.list=null;
//		}



		// --- public and protected Properties ---
		//virtual 
		int ICollection.Count {
			get {
				throw new NotImplementedException ();
				//return list.Count; 
			}
		}
		
		public bool IsReadOnly {
			get { return false; }
		}
		
		[MonoTODO]
		public bool IsSynchronized {
			// FIXME: should return true if object is synchronized
			get { return false; }
		}
		
		protected virtual ArrayList List {
			get { 
				throw new NotImplementedException ();
				//return list; 
			}
		}
		
		[MonoTODO]
		public object SyncRoot {
			// FIXME: should return object that can be used with the C# lock keyword
			get { return this; }
		}
		
		
		
		// --- public Methods ---
		public void CopyTo (Array ar, int index) 
		{
			throw new NotImplementedException ();
			//list.CopyTo(ar, index);
		}
		
		public IEnumerator GetEnumerator() 
		{
			throw new NotImplementedException ();
			//return list.GetEnumerator();
		}
		
	}
}
