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
		public abstract Version GetVersionPresent(object feature);

		[MonoTODO]
		public static Version GetVersionPresent(string featureClassName, string featureConstName){
			throw new NotImplementedException ();
		}


		[MonoTODO]
		public virtual bool IsPresent(object feature){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsPresent(object feature, Version minimumVersion){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsPresent(string featureClassName, string featureConstName){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsPresent(string featureClassName, string featureConstName, Version minimumVersion){
			throw new NotImplementedException ();
		}

		//
		// --- Protected Constructors
		//
		[MonoTODO]
		protected FeatureSupport()
		{
		}
	 }
}
