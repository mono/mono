//
// System.Web.UI.PartialCachingControl.cs
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
	public class PartialCachingControl : BasePartialCachingControl
	{

		private Type controlType;
		private Control createdControl;

		internal PartialCachingControl (Type createCachedControlType)
		{
			controlType = createCachedControlType;
		}

		[MonoTODO ("Implement")]
		internal override Control CreateControl()
		{
			createdControl = null;
			throw new NotImplementedException ();
		}

		public Control CachedControl {
			get {return createdControl;} 
		}
	}
}
