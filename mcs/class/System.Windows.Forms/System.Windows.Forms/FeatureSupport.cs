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
		public abstract Version GetVersionPresent(object o)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual bool IsPresent(object o)
		{
			throw new NotImplementedException ();
		}

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
