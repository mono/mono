//
// System.Windows.Forms.GridItemCollection.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public abstract class GridItemCollection : ICollection, IEnumerable {

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public int Count {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public GridItem this[string str] {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// ICollection Interface implmentation.
		/// </summary>
		int ICollection.Count{
			get{
				throw new NotImplementedException ();
			}
		}
		bool ICollection.IsSynchronized{
			get{
				throw new NotImplementedException ();
			}
		}
		object ICollection.SyncRoot{
			get{
				throw new NotImplementedException ();
			}
		}
		void ICollection.CopyTo(Array array, int index){
			throw new NotImplementedException ();
		}
		// End Of ICollection
	}
}
