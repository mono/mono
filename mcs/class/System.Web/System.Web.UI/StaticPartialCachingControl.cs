//
// System.Web.UI.StaticPartialCachingControl.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;

namespace System.Web.UI
{
	public class StaticPartialCachingControl : BasePartialCachingControl
	{

		private BuildMethod buildMethod;

		public StaticPartialCachingControl (string ctrlID, string guid, int duration,
				string varyByParams, string varyByControls, string varyByCustom,
				BuildMethod buildMethod)
		{
			CtrlID = ctrlID;
			Guid = guid;
			Duration = duration;
			VaryByParams = varyByParams;
			VaryByControls = varyByControls;
			VaryByCustom = varyByCustom;
			
			this.buildMethod = buildMethod;
		}

		public static void BuildCachedControl (Control parent, string ctrlID, string guid,
				int duration, string varyByParams, string varyByControls,
				string varyByCustom, BuildMethod buildMethod)
		{
			StaticPartialCachingControl NewControl =
				new StaticPartialCachingControl (ctrlID, guid, duration,
						varyByParams, varyByControls, varyByCustom,
						buildMethod);

			parent.Controls.Add (NewControl);
		}

		internal override Control CreateControl()
		{
		       return buildMethod ();
		}
	}
}
