//
// System.Windows.Forms.GridItemCollection.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//
// (C) 2002 Ximian, Inc
//
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
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
		public virtual bool Equals(object o)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static bool Equals(object o1, object o2)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException ();
		}
	 }
}
