//
// System.Web.UI.StaticPartialCachingControl.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class StaticPartialCachingControl : BasePartialCachingControl
	{
		BuildMethod buildMethod;
		string sqlDependency;

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

		public StaticPartialCachingControl (string ctrlID, string guid, int duration, string varyByParams,
						    string varyByControls, string varyByCustom, string sqlDependency,
						    BuildMethod buildMethod)
			: this (ctrlID, guid, duration, varyByParams, varyByControls, varyByCustom, buildMethod)
		{
			this.sqlDependency = sqlDependency;
		}
#if NET_4_0
		public StaticPartialCachingControl (string ctrlID, string guid, int duration, string varyByParams,
						    string varyByControls, string varyByCustom, string sqlDependency,
						    BuildMethod buildMethod, string providerName)
			: this (ctrlID, guid, duration, varyByParams, varyByControls, varyByCustom, sqlDependency, buildMethod)
		{
			ProviderName = providerName;
		}
#endif
		[MonoTODO("Consider sqlDependency parameter")]
		public static void BuildCachedControl (Control parent, string ctrlID, string guid,
						       int duration, string varyByParams, string varyByControls, string varyByCustom,
						       string sqlDependency, BuildMethod buildMethod)
		{
			StaticPartialCachingControl NewControl = new StaticPartialCachingControl (ctrlID, guid, duration,
												  varyByParams, varyByControls, varyByCustom,
												  sqlDependency, buildMethod);
			if (parent != null)
				parent.Controls.Add (NewControl);
		}

		public static void BuildCachedControl (Control parent, string ctrlID, string guid, int duration,
						       string varyByParams, string varyByControls, string varyByCustom,
						       BuildMethod buildMethod)
		{
			BuildCachedControl (parent, ctrlID, guid, duration, varyByParams, varyByControls, varyByCustom, null, buildMethod);
		}
#if NET_4_0
		public static void BuildCachedControl (Control parent, string ctrlID, string guid, int duration,
						       string varyByParams, string varyByControls, string varyByCustom,
						       string sqlDependency, BuildMethod buildMethod, string providerName)
		{
			var ctl = new StaticPartialCachingControl (ctrlID, guid, duration, varyByParams, varyByControls, varyByCustom,
								   sqlDependency, buildMethod, providerName);
			if (parent != null)
				parent.Controls.Add (ctl);
		}
#endif
		internal override Control CreateControl()
		{
		       return buildMethod ();
		}
	}
}
