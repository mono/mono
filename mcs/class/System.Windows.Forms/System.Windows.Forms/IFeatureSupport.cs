//
// System.Windows.Forms.IFeatureSupport.cs
//
// Author:
// William Lamb (wdlamb@notwires.com)
// Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

namespace System.Windows.Forms {

	public interface IFeatureSupport {

		Version GetVersionPresent(object feature);
		bool IsPresent(object feature);
		bool IsPresent(object feature, Version minimumVersion);
	}
}
