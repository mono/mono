// 
// System.Data/ResDescriptionAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.ComponentModel;

namespace System.Data {
	[AttributeUsage (AttributeTargets.All)]
	internal sealed class ResDescriptionAttribute : DescriptionAttribute
	{
		#region Fields

		string description;

		#endregion // Fields

		#region Constructors

		public ResDescriptionAttribute (string description)
			: base (description)
		{
			this.description = description; 
		}

		#endregion // Constructors

		#region Properties

		public override string Description {
			get { return description; }
		}

		#endregion // Properties
	}
}
