// 
// System.Web.Services.WebServicesDescriptionAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;

namespace System.Web.Services {
	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Event)]
	internal class WebServicesDescriptionAttribute : DescriptionAttribute {

		#region Constructors

		public WebServicesDescriptionAttribute (string description) 
			: base (description)
		{
		}

		#endregion // Constructors

		#region Properties

		public override string Description {
			get { return DescriptionValue; }
		}

		#endregion // Properties
	}
}
