//
// System.Windows.Forms.GridItem.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public abstract class GridItem {
		bool expandable;
		bool expanded;
		//
		//  --- Public Properties
		//
		public bool Expandable {
			get {
				return expandable;
			}
		}
		public bool Expanded {
			get {
				return expanded;
			}
			set {
				expanded = value;
			}
		}
		//[MonoTODO]
		//public abstract GridItemCollection GridItems {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public abstract GridItemType GridItemType {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public abstract string Label {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public abstract GridItem Parent {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public abstract PropertyDescriptor PropertyDescriptor {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public abstract object Value {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}
		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}
		//
		// --- Protected Constructor
		//
		[MonoTODO]
		protected GridItem()
		{
			throw new NotImplementedException ();
		}
	 }
}
