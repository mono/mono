//
// System.Windows.Forms.FeatureSupport.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public abstract class FeatureSupport : IFeatureSupport {

		//
		//  --- Public Methods
		//
		//IFeatureSupport

		Version IFeatureSupport.GetVersionPresent(object feature) {
			throw new NotImplementedException ();
		}
		bool IFeatureSupport.IsPresent(object feature){
			throw new NotImplementedException ();
		}
		bool IFeatureSupport.IsPresent(object feature, Version minimumVersion){
			throw new NotImplementedException ();
		}
		
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

		[MonoTODO]
		public abstract Version GetVersionPresent(object o);

		//[MonoTODO]
		//public virtual bool IsPresent(object o)
		//{
		//	throw new NotImplementedException ();
		//}

		//
		// --- Protected Constructors
		//
		[MonoTODO]
		protected FeatureSupport()
		{
			throw new NotImplementedException ();
		}
	 }
}
