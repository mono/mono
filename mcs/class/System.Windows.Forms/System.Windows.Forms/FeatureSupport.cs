//
// System.Windows.Forms.FeatureSupport.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public abstract class FeatureSupport : IFeatureSupport {

		//
		//  --- Public Methods
		//
		//IFeatureSupport

		[MonoTODO]
		Version IFeatureSupport.GetVersionPresent(object feature) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		bool IFeatureSupport.IsPresent(object feature){
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		bool IFeatureSupport.IsPresent(object feature, Version minimumVersion){
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public abstract Version GetVersionPresent(object o);

		//
		// --- Protected Constructors
		//
		[MonoTODO]
		protected FeatureSupport()
		{
		}
	 }
}
