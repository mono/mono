//
// System.Windows.Forms.GridItem.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002/3 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public abstract class GridItem {
		bool expandable;
		bool expanded;
		//
		//  --- Public Properties
		//
		public virtual bool Expandable {
			get {
				return expandable;
			}
		}
		public virtual bool Expanded {
			get {
				return expanded;
			}
			set {
				expanded = value;
			}
		}


		public abstract GridItemCollection GridItems {
			get;
		}
		public abstract GridItemType GridItemType {
			get;
		}
		public abstract string Label {
			get;
		}
		public abstract GridItem Parent {
			get;
		}
		//public abstract PropertyDescriptor PropertyDescriptor {
		//	get;
		//}
		public abstract object Value {
			get;
		}

		//
		// --- Protected Constructor
		//
		[MonoTODO]
		protected GridItem()
		{
			
		}
	 }
}
