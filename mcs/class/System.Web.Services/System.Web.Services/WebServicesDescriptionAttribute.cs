// 
// System.Web.Services.WebServicesDescriptionAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services {
	[AttributeUsage(AttributeTargets.All)]
	internal sealed class WebServicesDescriptionAttribute : Attribute {
		#region Fields

		string description;

		#endregion // Fields

		#region Constructors

		internal WebServicesDescriptionAttribute (string description)
		{
			this.description = description;
		}

		#endregion // Constructors

		#region Properties

		internal string Description {
			get { return description; }
			set { description = value; }
		}

		#endregion // Properties
	}
}
