//
// System.Web.UI.PartialCachingAttribute.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.Class)]
	public sealed class PartialCachingAttribute : Attribute
	{
		int duration;
		string varyByControls;
		string varyByCustom;
		string varyByParams;
		
		public PartialCachingAttribute (int duration)
		{
			this.duration = duration;
		}

		public PartialCachingAttribute (int duration, string varyByParams,
						string varyByControls, string varyByCustom)
		{
			this.duration = duration;
			this.varyByParams = varyByParams;
			this.varyByControls = varyByControls;
			this.varyByCustom = varyByCustom;
		}

		public int Duration {
			get { return duration; }
		}

		public string VaryByParams {
			get { return varyByParams; }
		}

		public string VaryByControls {
			get { return varyByControls; }
		}

		public string VaryByCustom {
			get { return varyByCustom; }
		}
	}
}
