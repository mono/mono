//
// System.Windows.Forms.OSFeature.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	// </summary>

        public class OSFeature : FeatureSupport {

		//
		//	 --- Public Fields
		//
		public static readonly object LayeredWindows;
		public static readonly object Themes;

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public static OSFeature Feature {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public override Version GetVersionPresent(object feature)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual bool IsPresent(object o)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual bool IsPresent(object o, Version v)
		{
			throw new NotImplementedException ();
		}
	 }
}
